# Fleet Tracker — بک‌اند (.NET 8)

سیستم ردیابی ناوگان (Mini Fleet Tracker) ساخته‌شده با **.NET 8** و معماری لایه‌ای تمیز (Clean / Layered Architecture).

> این پروژه فقط بخش **بک‌اند** را شامل می‌شود (بدون فرانت‌اند).

---

## تکنولوژی‌ها

| بخش | تکنولوژی |
|------|----------|
| فریم‌ورک | .NET 8 (ASP.NET Core) |
| دیتابیس | SQL Server (EF Core 8) |
| کش | Redis |
| Realtime | SignalR |
| احراز هویت | JWT Bearer |
| لاگ | Serilog |
| مستندسازی | Swagger / OpenAPI |
| تست | xUnit + FluentAssertions |

---

## ساختار Solution

```
FleetTracker.sln
├── src/
│   ├── FleetTracker.Domain          # Entityها و Enumها (بدون وابستگی)
│   ├── FleetTracker.Application     # منطق بیزینس، DTO، Interface، Service
│   ├── FleetTracker.Infrastructure  # EF Core DbContext، Migration، Redis Cache
│   ├── FleetTracker.Api             # Controller، Hub، Program.cs، JWT
│   └── FleetTracker.Simulator       # شبیه‌ساز GPS (Worker Service)
└── tests/
    └── FleetTracker.Tests           # تست‌های واحد
```

### جریان وابستگی
```
Domain  ←  Application  ←  Infrastructure  ←  Api
                                  ↑
                          Application (دسترسی به داده از طریق IFleetTrackerDbContext)
```
- `Domain` به هیچ لایه‌ای وابسته نیست (فقط POCO).
- `Application` فقط به Interfaceها وابسته است (نه EF Core مستقیم).
- `Infrastructure` پیاده‌سازی واقعی را ارائه می‌دهد (SQL Server، Redis، SignalR).

---

## Endpointهای API

احراز هویت (بدون توکن):
- `POST /api/auth/login` — ورود و دریافت JWT

وسایل نقلیه:
- `GET /api/vehicles` — لیست همه وسایل
- `GET /api/vehicles/{id}` — جزئیات یک وسیله
- `POST /api/vehicles` — ثبت وسیله جدید
- `GET /api/vehicles/{id}/locations/last` — آخرین موقعیت (از Redis)
- `POST /api/vehicles/{id}/geofence` — تعریف حوزه‌ی مجاز

موقعیت:
- `POST /api/locations` — ثبت موقعیت (hot-path برای دستگاه‌ها/شبیه‌ساز)

هشدارها:
- `GET /api/alerts/open` — هشدارهای باز
- `GET /api/alerts/vehicle/{vehicleId}` — هشدارهای یک وسیله
- `POST /api/alerts/{id}/resolve` — رفع هشدار

داشبورد:
- `GET /api/dashboard/summary` — خلاصه‌ی وضعیت ناوگان

Realtime:
- `Hub /hub/tracking` — رویدادهای `locationUpdated` و `alertCreated`

---

## اجرا با Docker

```bash
docker compose up --build
```

این دستور سرویس‌های زیر را راه‌اندازی می‌کند:
- **api** روی پورت `5000` (Swagger: http://localhost:5000/swagger)
- **simulator** — شروع به ارسال موقعیت می‌کند (پس از تعیین VehicleId)
- **sqlserver** روی پورت `1433`
- **redis** روی پورت `6379`

### اعمال Migration (پس از اولین اجرا)
```bash
dotnet ef database update \
  --project src/FleetTracker.Infrastructure \
  --startup-project src/FleetTracker.Api
```

---

## اجرای محلی (بدون Docker)

1. SQL Server و Redis را روی `localhost` داشته باشید.
2. Environment را روی `Development` بگذارید (`UseRedis=false` در `appsettings.Development.json` تنظیم شده — کش in-memory استفاده می‌شود).
3. API را اجرا کنید:
   ```bash
   dotnet run --project src/FleetTracker.Api
   ```
4. برای شبیه‌ساز، `Simulator:ApiUrl` را به `http://localhost:5000` تغییر دهید.

### ورود پیش‌فرض
- یوزر: `admin`
- پسورد: `Admin@123456`

---

## نکات طراحی مهم

- **جداکردن hot-data از cold-data:** آخرین موقعیت فقط از Redis خوانده می‌شود؛ SQL فقط برای تاریخچه است.
- **تشخیص گذار وضعیت Geofence:** state قبلی (داخل/خارج) صریحاً روی `Geofence.IsCurrentlyInside` نگه‌داری می‌شود تا هشدار تکراری تولید نشود.
- **ایندکس حیاتی:** `IX_LocationLog_Vehicle_RecordedAt` برای کوئری تاریخچه‌ی موقعیت روی جدولی که به‌سرعت رشد می‌کند.
- **انتزاع DbContext:** `IFleetTrackerDbContext` در Application قرار دارد تا وابستگی دایره‌ای به Infrastructure برقرار نشود.
- **پشتیبانی از توکن در WebSocket:** JWT از query string برای اتصال SignalR پشتیبانی می‌کند.

---

## تست‌ها

```bash
dotnet test tests/FleetTracker.Tests
```

۱۱ تست واحد شامل:
- محاسبات Geofence (Haversine، حالت‌های مرزی)
- سرویس هشدار (ایجاد، رفع، فیلتر)
- سرویس کش in-memory
