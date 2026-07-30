using System.Text;
using FleetTracker.Application.Interfaces;
using FleetTracker.Application.Services;
using FleetTracker.Api.Hubs;
using FleetTracker.Api.Realtime;
using FleetTracker.Infrastructure.Caching;
using FleetTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog ---
builder.Host.UseSerilog((ctx, services, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

// --- EF Core (SQL Server) ---
builder.Services.AddDbContext<FleetTrackerDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ثبت DbContext به‌عنوان پیاده‌سازی IFleetTrackerDbContext برای لایه‌ی Application
builder.Services.AddScoped<IFleetTrackerDbContext>(sp =>
    sp.GetRequiredService<FleetTrackerDbContext>());

// --- Redis ---
var useRedis = builder.Configuration.GetValue("UseRedis", true);
if (useRedis)
{
    var redisConn = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrWhiteSpace(redisConn))
    {
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConn!));
        builder.Services.AddScoped<ICacheService, RedisCacheService>();
    }
    else
    {
        builder.Services.AddScoped<ICacheService, NullCacheService>();
    }
}
else
{
    builder.Services.AddScoped<ICacheService, NullCacheService>();
}

// --- Application Services ---
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IGeofenceService, GeofenceService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();

// --- SignalR + Controllers ---
builder.Services.AddSignalR();
builder.Services.AddControllers();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FleetTracker API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "توکن JWT را با پیشوند 'Bearer ' وارد کنید."
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// --- JWT Authentication ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // پشتیبانی از توکن JWT در کوئری استرینگ برای اتصال SignalR (WebSocket نمی‌تواند هدر سفارشی بفرستد)
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hub"))
                {
                    ctx.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// --- CORS ---
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();
builder.Services.AddCors(opt => opt.AddPolicy("frontend", p =>
    p.WithOrigins(allowedOrigins)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

var app = builder.Build();

// --- Middleware Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TrackingHub>("/hub/tracking");

// در محیط توسعه، schema دیتابیس را به‌صورت خودکار اعمال کن و داده‌های اولیه را وارد کن.
// اگر دیتابیس (SQL Server) در دسترس نبود، هشدار می‌دهیم ولی برنامه را متوقف نمی‌کنیم
// تا بتوان API را حتی بدون دیتابیس بالا آورد (مثلاً برای بررسی Swagger).
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FleetTrackerDbContext>();
    try
    {
        db.Database.EnsureCreated();
        app.Logger.LogInformation("دیتابیس با موفقیت ایجاد/بررسی شد.");

        // --- Seed Data ---
        if (!db.Vehicles.Any())
        {
            var driver1 = new FleetTracker.Domain.Entities.Driver
            {
                Id = Guid.NewGuid(),
                FullName = "علی محمدی",
                PhoneNumber = "09121234567"
            };
            var driver2 = new FleetTracker.Domain.Entities.Driver
            {
                Id = Guid.NewGuid(),
                FullName = "رضا احمدی",
                PhoneNumber = "09129876543"
            };

            var vehicle1 = new FleetTracker.Domain.Entities.Vehicle
            {
                Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                PlateNumber = "22الف123",
                Model = "سمند",
                Status = FleetTracker.Domain.Enums.VehicleStatus.Active,
                DriverId = driver1.Id,
                Driver = driver1
            };
            var vehicle2 = new FleetTracker.Domain.Entities.Vehicle
            {
                Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                PlateNumber = "11ب456",
                Model = "پراید",
                Status = FleetTracker.Domain.Enums.VehicleStatus.Idle,
                DriverId = driver2.Id,
                Driver = driver2
            };
            var vehicle3 = new FleetTracker.Domain.Entities.Vehicle
            {
                Id = Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa"),
                PlateNumber = "77ت789",
                Model = "دنا",
                Status = FleetTracker.Domain.Enums.VehicleStatus.Offline
            };

            db.Vehicles.AddRange(vehicle1, vehicle2, vehicle3);
            db.SaveChanges();
            app.Logger.LogInformation("سه وسیله نقلیه نمونه به دیتابیس اضافه شد.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex,
            "اتصال به دیتابیس ناموفق بود. API همچنان اجرا می‌شود ولی عملیات دیتابیس خطا خواهد داد. " +
            "برای اجرای کامل، SQL Server را روی localhost بالا بیاورید یا از docker compose استفاده کنید.");
    }
}

app.Run();
