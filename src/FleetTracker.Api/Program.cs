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
builder.Services.AddScoped<IDriverService, DriverService>();
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
                FullName = "حسین رضاخانی",
                NationalCode = "0012345678",
                PhoneNumber = "09121234567"
            };
            var driver2 = new FleetTracker.Domain.Entities.Driver
            {
                Id = Guid.NewGuid(),
                FullName = "رضا احمدی",
                NationalCode = "0023456789",
                PhoneNumber = "09129876543"
            };
            var driver3 = new FleetTracker.Domain.Entities.Driver
            {
                Id = Guid.NewGuid(),
                FullName = "علی محمدی",
                NationalCode = "0034567890",
                PhoneNumber = "09351112233"
            };
            var driver4 = new FleetTracker.Domain.Entities.Driver
            {
                Id = Guid.NewGuid(),
                FullName = "محمد حسینی",
                NationalCode = "0045678901",
                PhoneNumber = "09193334455"
            };
            var driver5 = new FleetTracker.Domain.Entities.Driver
            {
                Id = Guid.NewGuid(),
                FullName = "امیر کاظمی",
                NationalCode = "0056789012",
                PhoneNumber = "09367778899"
            };
            var driver6 = new FleetTracker.Domain.Entities.Driver
            {
                Id = Guid.NewGuid(),
                FullName = "سعید نوری",
                NationalCode = "0067890123",
                PhoneNumber = "09125556677"
            };
            var driver7 = new FleetTracker.Domain.Entities.Driver
            {
                Id = Guid.NewGuid(),
                FullName = "مرتضی عباسی",
                NationalCode = "0078901234",
                PhoneNumber = "09214445566"
            };
            var driver8 = new FleetTracker.Domain.Entities.Driver
            {
                Id = Guid.NewGuid(),
                FullName = "بهروز شریفی",
                NationalCode = "0089012345",
                PhoneNumber = "09132223344"
            };

            db.Drivers.AddRange(driver1, driver2, driver3, driver4, driver5, driver6, driver7, driver8);

            var vehicles = new FleetTracker.Domain.Entities.Vehicle[]
            {
                new()
                {
                    Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                    PlateNumber = "21ب54367",
                    Model = "سمند EF7",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Active,
                    DriverId = driver1.Id,
                    Driver = driver1
                },
                new()
                {
                    Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                    PlateNumber = "12الف32145",
                    Model = "پراید 151",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Active,
                    DriverId = driver2.Id,
                    Driver = driver2
                },
                new()
                {
                    Id = Guid.Parse("22222222-3333-4444-5555-666666666666"),
                    PlateNumber = "33جی78912",
                    Model = "دنا پلاس",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Idle,
                    DriverId = driver3.Id,
                    Driver = driver3
                },
                new()
                {
                    Id = Guid.Parse("33333333-4444-5555-6666-777777777777"),
                    PlateNumber = "44د23456",
                    Model = "تارا",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Active,
                    DriverId = driver4.Id,
                    Driver = driver4
                },
                new()
                {
                    Id = Guid.Parse("44444444-5555-6666-7777-888888888888"),
                    PlateNumber = "55س65432",
                    Model = "کی‌ام‌سی J5",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Active,
                    DriverId = driver5.Id,
                    Driver = driver5
                },
                new()
                {
                    Id = Guid.Parse("55555555-6666-7777-8888-999999999999"),
                    PlateNumber = "67ص87654",
                    Model = "رانا پلاس",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Idle,
                    DriverId = driver6.Id,
                    Driver = driver6
                },
                new()
                {
                    Id = Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa"),
                    PlateNumber = "78ق34567",
                    Model = "ساینا S",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Offline,
                    DriverId = driver7.Id,
                    Driver = driver7
                },
                new()
                {
                    Id = Guid.Parse("77777777-8888-9999-aaaa-bbbbbbbbbbbb"),
                    PlateNumber = "89ل98765",
                    Model = "کوییک R",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Active,
                    DriverId = driver8.Id,
                    Driver = driver8
                },
                new()
                {
                    Id = Guid.Parse("88888888-9999-aaaa-bbbb-cccccccccccc"),
                    PlateNumber = "11م11223",
                    Model = "هایما S7",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Active
                },
                new()
                {
                    Id = Guid.Parse("99999999-aaaa-bbbb-cccc-dddddddddddd"),
                    PlateNumber = "22ن44556",
                    Model = "چری آریزو 5",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Idle
                },
                new()
                {
                    Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-111111111111"),
                    PlateNumber = "33و77889",
                    Model = "MVM 315",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Offline
                },
                new()
                {
                    Id = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-222222222222"),
                    PlateNumber = "44ه33445",
                    Model = "لیفان X60",
                    Status = FleetTracker.Domain.Enums.VehicleStatus.Active
                },
            };

            db.Vehicles.AddRange(vehicles);
            db.SaveChanges();
            app.Logger.LogInformation("دوازده وسیله نقلیه و هشت راننده نمونه به دیتابیس اضافه شد.");
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
