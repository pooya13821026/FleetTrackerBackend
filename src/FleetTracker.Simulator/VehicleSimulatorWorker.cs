using System.Net.Http.Json;

namespace FleetTracker.Simulator;

/// <summary>
/// شبیه‌ساز GPS: برای همه‌ی وسایل نقلیه به‌صورت همزمان موقعیت تولید و ارسال می‌کند.
/// هر وسیله مسیر و سرعت متفاوتی دارد.
/// </summary>
public class VehicleSimulatorWorker(
    HttpClient httpClient,
    ILogger<VehicleSimulatorWorker> logger,
    IConfiguration config)
    : BackgroundService
{
    // ۱۲ مسیر متفاوت در تهران — هر خودرو یک مسیر اختصاصی دارد
    private static readonly List<List<RoutePoint>> Routes = new()
    {
        // مسیر ۱: مرکز تهران — میدان انقلاب
        new()
        {
            new(35.7219, 51.3347),
            new(35.7250, 51.3400),
            new(35.7300, 51.3450),
            new(35.7350, 51.3500),
            new(35.7400, 51.3450),
            new(35.7350, 51.3400),
            new(35.7300, 51.3350),
            new(35.7250, 51.3300),
        },
        // مسیر ۲: شمال تهران — ولنجک
        new()
        {
            new(35.7500, 51.3800),
            new(35.7550, 51.3850),
            new(35.7600, 51.3900),
            new(35.7650, 51.3850),
            new(35.7600, 51.3800),
            new(35.7550, 51.3750),
        },
        // مسیر ۳: جنوب تهران — شاهپور
        new()
        {
            new(35.6800, 51.3200),
            new(35.6850, 51.3250),
            new(35.6900, 51.3300),
            new(35.6950, 51.3250),
            new(35.6900, 51.3200),
            new(35.6850, 51.3150),
        },
        // مسیر ۴: شرق تهران — تهرانپارس
        new()
        {
            new(35.7300, 51.4200),
            new(35.7350, 51.4250),
            new(35.7400, 51.4300),
            new(35.7450, 51.4250),
            new(35.7400, 51.4200),
            new(35.7350, 51.4150),
        },
        // مسیر ۵: غرب تهران — صادقیه
        new()
        {
            new(35.7200, 51.2800),
            new(35.7250, 51.2850),
            new(35.7300, 51.2900),
            new(35.7350, 51.2850),
            new(35.7300, 51.2800),
            new(35.7250, 51.2750),
        },
        // مسیر ۶: جنوب‌غرب — شهرک‌ارم
        new()
        {
            new(35.7000, 51.2700),
            new(35.7050, 51.2750),
            new(35.7100, 51.2800),
            new(35.7150, 51.2750),
            new(35.7100, 51.2700),
            new(35.7050, 51.2650),
        },
        // مسیر ۷: شمال‌شرق — مجیدیه
        new()
        {
            new(35.7450, 51.4050),
            new(35.7500, 51.4100),
            new(35.7550, 51.4150),
            new(35.7600, 51.4100),
            new(35.7550, 51.4050),
            new(35.7500, 51.4000),
        },
        // مسیر ۸: جنوب‌شرق — نازی‌آباد
        new()
        {
            new(35.6900, 51.3700),
            new(35.6950, 51.3750),
            new(35.7000, 51.3800),
            new(35.7050, 51.3750),
            new(35.7000, 51.3700),
            new(35.6950, 51.3650),
        },
        // مسیر ۹: شمال‌غرب — پونک
        new()
        {
            new(35.7400, 51.2900),
            new(35.7450, 51.2950),
            new(35.7500, 51.3000),
            new(35.7550, 51.2950),
            new(35.7500, 51.2900),
            new(35.7450, 51.2850),
        },
        // مسیر ۱۰: مرکز‌غرب — آریاشهر
        new()
        {
            new(35.7150, 51.3100),
            new(35.7200, 51.3150),
            new(35.7250, 51.3200),
            new(35.7300, 51.3150),
            new(35.7250, 51.3100),
            new(35.7200, 51.3050),
        },
        // مسیر ۱۱: شرق — فرجام
        new()
        {
            new(35.7400, 51.4400),
            new(35.7450, 51.4450),
            new(35.7500, 51.4500),
            new(35.7550, 51.4450),
            new(35.7500, 51.4400),
            new(35.7450, 51.4350),
        },
        // مسیر ۱۲: جنوب — جوادیه
        new()
        {
            new(35.6700, 51.3500),
            new(35.6750, 51.3550),
            new(35.6800, 51.3600),
            new(35.6850, 51.3550),
            new(35.6800, 51.3500),
            new(35.6750, 51.3450),
        },
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiUrl = config["Simulator:ApiUrl"] ?? "http://api:8080";
        var intervalSec = config.GetValue("Simulator:IntervalSeconds", 3);
        var ingestUrl = $"{apiUrl.TrimEnd('/')}/api/locations";

        // دریافت لیست شناسه خودروها از تنظیمات
        var vehicleIdsConfig = config["Simulator:VehicleIds"];
        var vehicleIds = new List<Guid>();

        if (!string.IsNullOrWhiteSpace(vehicleIdsConfig))
        {
            // جدا کردن شناسه‌ها با کاما
            foreach (var part in vehicleIdsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Guid.TryParse(part.Trim(), out var id))
                    vehicleIds.Add(id);
            }
        }

        // اگه لیست تنظیم نشده، از VehicleId قدیمی استفاده کن
        if (vehicleIds.Count == 0)
        {
            var singleId = config["Simulator:VehicleId"];
            if (!string.IsNullOrWhiteSpace(singleId) && Guid.TryParse(singleId, out var id))
                vehicleIds.Add(id);
        }

        if (vehicleIds.Count == 0)
        {
            logger.LogError("هیچ شناسه خودرویی تنظیم نشده است. شبیه‌ساز متوقف می‌شود.");
            return;
        }

        logger.LogInformation("شبیه‌ساز برای {Count} خودرو شروع شد. ارسال به {Url} هر {Sec} ثانیه",
            vehicleIds.Count, ingestUrl, intervalSec);

        // وضعیت هر خودرو — هر خودرو مسیر اختصاصی خودش را دارد
        var states = vehicleIds.Select((vid, i) => new VehicleState
        {
            VehicleId = vid,
            RouteIndex = 0,
            Route = Routes[i % Routes.Count],
            Random = new Random(vid.GetHashCode()),
        }).ToList();

        while (!stoppingToken.IsCancellationRequested)
        {
            var tasks = new List<Task>();

            foreach (var state in states)
            {
                var point = state.Route[state.RouteIndex % state.Route.Count];
                state.RouteIndex++;

                var payload = new
                {
                    VehicleId = state.VehicleId,
                    Latitude = point.Lat + (state.Random.NextDouble() - 0.5) * 0.0008,
                    Longitude = point.Lng + (state.Random.NextDouble() - 0.5) * 0.0008,
                    SpeedKmh = state.Random.Next(15, 70),
                };

                tasks.Add(SendLocation(ingestUrl, payload, state.VehicleId, stoppingToken));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch
            {
                // خطاها در هر درخواست جداگانه لاگ می‌شوند
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(intervalSec), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("شبیه‌ساز متوقف شد.");
    }

    private async Task SendLocation(string url, object payload, Guid vehicleId, CancellationToken ct)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(url, payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("ارسال موقعیت خودرو {VehicleId} — API کد {Code} برگرداند",
                    vehicleId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ارسال موقعیت خودرو {VehicleId} با خطا مواجه شد", vehicleId);
        }
    }

    private class VehicleState
    {
        public Guid VehicleId { get; set; }
        public int RouteIndex { get; set; }
        public List<RoutePoint> Route { get; set; } = new();
        public Random Random { get; set; } = new();
    }
}
