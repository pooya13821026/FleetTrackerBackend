# Fleet Tracker — بک‌اند (.NET 8)

سیستم ردیابی لحظه‌ای ناوگان خودرو با معماری Clean Architecture.

---

## پروژه چیست؟

یک سیستم مانیتورینگ خودروها به‌صورت لحظه‌ای روی نقشه. هر خودرو یک شبیه‌ساز GPS دارد که هر چند ثانیه موقعیتش را به سرور می‌فرستد. سرور موقعیت را ذخیره، کش و به همه کلاینت‌ها از طریق SignalR اعلام می‌کند. وضعیت خودروها سه حالت دارد:

| وضعیت | رنگ مارکر | رفتار |
|--------|-----------|-------|
| **Active** (فعال) | سبز | شبیه‌ساز موقعیت را به‌روز می‌کند → حرکت روی نقشه |
| **Idle** (توقف) | نارنجی | موقعیت ثابت از دیتابیس → بدون حرکت |
| **Offline** (آفلاین) | خاکستری | موقعیت ثابت از دیتابیس → بدون حرکت |

قابلیت‌ها:
- ردیابی لحظه‌ای خودروها روی نقشه (Leaflet + OpenStreetMap)
- پلاک خودرو به فرمت ایرانی (`[پرچم+شهر] | [۳ رقم] | [حرف] | [۲ رقم]`)
- نمایش اطلاعات راننده (نام، کد ملی، شماره تماس)
- فیلتر خودروها بر اساس وضعیت
- انتخاب خودرو از لیست → زوم روی آن + نمایش اطلاعات راننده
- هشدار خروج از محدوده مجاز (Geofence)
- داشبورد آماری

---

## تکنولوژی‌ها

| بخش | تکنولوژی |
|------|----------|
| فریم‌ورک | .NET 8 (ASP.NET Core) |
| دیتابیس | SQL Server (EF Core 8) |
| کش | Redis (آخرین موقعیت) |
| Real-time | SignalR |
| احراز هویت | JWT Bearer |
| لاگ | Serilog |
| مستندسازی | Swagger / OpenAPI |
| تست | xUnit + FluentAssertions + Moq |

---

## ساختار Solution

```
FleetTracker.sln
├── src/
│   ├── FleetTracker.Domain           # Entityها و Enumها (بدون وابستگی)
│   ├── FleetTracker.Application      # بیزینس منطق، DTO، Interface، Service
│   ├── FleetTracker.Infrastructure   # EF Core، Redis Cache، پیاده‌سازی واقعی
│   ├── FleetTracker.Api              # Controller، Hub، Program.cs، JWT، SeedData
│   └── FleetTracker.Simulator        # شبیه‌ساز GPS (BackgroundService)
├── tests/
│   └── FleetTracker.Tests            # تست‌های واحد (29 تست)
└── docker-compose.yml                # اجرای کامل با Docker
```

### جریان وابستگی
```
Domain  ←  Application  ←  Infrastructure  ←  Api
```
- `Domain` به هیچ لایه‌ای وابسته نیست
- `Application` فقط به Interfaceها وابسته است
- `Infrastructure` پیاده‌سازی واقعی (SQL Server, Redis) را ارائه می‌دهد

---

## پیش‌نیازها

### روش ۱: Docker (توصیه‌شده)
- Docker Desktop نصب باشد

### روش ۲: اجرای محلی
- .NET 8 SDK
- SQL Server (LocalDB یا نسخه کامل)
- Redis (اختیاری — اگر نباشد از حافظه in-memory استفاده می‌شود)

---

## راه‌اندازی با Docker

```bash
cd D:\Monitoring
docker compose up --build
```

سرویس‌ها:
| سرویس | پورت | توضیح |
|--------|------|-------|
| api | `5000` | API اصلی + Swagger |
| simulator | — | شبیه‌ساز GPS |
| sqlserver | `1433` | SQL Server 2022 |
| redis | `6379` | Redis Cache |

Swagger: http://localhost:5000/swagger

---

## راه‌اندازی محلی (بدون Docker)

### ۱. دیتابیس و Redis
- SQL Server: اگر LocalDB دارید، خودکار ساخته می‌شود
- Redis: اگر ندارید، در `appsettings.Development.json` مقدار `UseRedis` را `false` کنید

### ۲. اجرای API
```bash
cd D:\Monitoring
dotnet run --project src/FleetTracker.Api
```

### ۳. اجرای شبیه‌ساز
```bash
dotnet run --project src/FleetTracker.Simulator
```

### ۴. ریست دیتابیس (اختیاری)
اگر می‌خواهید داده‌های نمونه دوباره ساخته شوند:
```bash
dotnet ef database drop --project src/FleetTracker.Infrastructure --startup-project src/FleetTracker.Api
```
سپس API را دوباره اجرا کنید — خودکار seed می‌شود.

---

## اطلاعات ورود

| فیلد | مقدار |
|------|-------|
| نام کاربری | `admin` |
| رمز عبور | `Admin@123456` |

---

## داده‌های نمونه (Seed)

با اولین اجرای API در حالت Development، ۱۲ خودرو و ۸ راننده نمونه ساخته می‌شوند:

### خودروها

| پلاک | مدل | وضعیت |
|------|------|--------|
| 21ب54367 | سمند EF7 | ✅ Active |
| 12الف32145 | پراید 151 | ✅ Active |
| 33جی78912 | دنا پلاس | ⏸ Idle |
| 44د23456 | تارا | ✅ Active |
| 55س65432 | کی‌ام‌سی J5 | ✅ Active |
| 67ص87654 | رانا پلاس | ⏸ Idle |
| 78ق34567 | ساینا S | 🔴 Offline |
| 89ل98765 | کوییک R | ✅ Active |
| 11م11223 | هایما S7 | ✅ Active |
| 22ن44556 | چری آریزو 5 | ⏸ Idle |
| 33و77889 | MVM 315 | 🔴 Offline |
| 44ه33445 | لیفان X60 | ✅ Active |

- خودروهای **Active**: توسط شبیه‌ساز حرکت می‌کنند (۱۲ مسیر مختلف در تهران)
- خودروهای **Idle** و **Offline**: موقعیت ثابت دارند و روی نقشه نمایش داده می‌شوند

---

## Endpointهای API

### احراز هویت
| Method | Endpoint | توضیح |
|--------|----------|-------|
| POST | `/api/auth/login` | ورود و دریافت JWT |

### وسایل نقلیه
| Method | Endpoint | توضیح |
|--------|----------|-------|
| GET | `/api/vehicles` | لیست همه خودروها + آخرین موقعیت |
| GET | `/api/vehicles/{id}` | جزئیات یک خودرو |
| POST | `/api/vehicles` | ثبت خودروی جدید |

### رانندگان
| Method | Endpoint | توضیح |
|--------|----------|-------|
| GET | `/api/drivers` | لیست رانندگان |
| GET | `/api/drivers/{id}` | جزئیات راننده |
| POST | `/api/drivers` | ثبت راننده جدید |
| PUT | `/api/drivers/{id}` | ویرایش راننده |
| DELETE | `/api/drivers/{id}` | حذف راننده |

### موقعیت
| Method | Endpoint | توضیح |
|--------|----------|-------|
| POST | `/api/locations` | ثبت موقعیت (برای شبیه‌ساز) |
| GET | `/api/locations/last` | آخرین موقعیت همه خودروها |

### هشدارها
| Method | Endpoint | توضیح |
|--------|----------|-------|
| GET | `/api/alerts/open` | هشدارهای باز |
| GET | `/api/alerts/vehicle/{id}` | هشدارهای یک وسیله |
| POST | `/api/alerts/{id}/resolve` | رفع هشدار |

### داشبورد
| Method | Endpoint | توضیح |
|--------|----------|-------|
| GET | `/api/dashboard/summary` | خلاصه وضعیت ناوگان |

### Real-time
| نوع | مسیر | رویدادها |
|-----|------|----------|
| SignalR | `/hub/tracking` | `locationUpdated`, `alertCreated` |

---

## تست‌ها

```bash
dotnet test tests/FleetTracker.Tests
```

**29 تست** شامل:
- محاسبات Geofence (Haversine، حالت‌های مرزی)
- سرویس هشدار (ایجاد، رفع، فیلتر)
- سرویس خودرو (CRUD + آخرین موقعیت)
- سرویس راننده (CRUD + اعتبارسنجی)
- سرویس کش in-memory

---

## نکات طراحی مهم

- **جداکردن hot-data از cold-data:** آخرین موقعیت فقط از Redis خوانده می‌شود؛ SQL فقط برای تاریخچه
- **تشخیص گذار وضعیت Geofence:** state قبلی صریحاً نگه‌داری می‌شود تا هشدار تکراری تولید نشود
- **فقط Active:** شبیه‌ساز فقط برای خودروهای Active موقعیت ارسال می‌کند
- **موقعیت اولیه:** همه خودروها (شامل Idle/Offline) موقعیت ثابت از seed دارند
- **پشتیبانی از توکن در WebSocket:** JWT از query string برای SignalR پشتیبانی می‌کند
