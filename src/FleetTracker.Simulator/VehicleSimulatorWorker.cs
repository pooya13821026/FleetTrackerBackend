using System.Net.Http.Json;

namespace FleetTracker.Simulator;

/// <summary>
/// شبیه‌ساز GPS: هر چند ثانیه یک موقعیت تخیلی برای یک وسیله‌ی نقلیه تولید کرده
/// و به API می‌فرستد. مسیر به‌صورت حلقوی پیمایش می‌شود و نویز کوچکی برای واقعی‌تر شدن اضافه می‌شود.
/// </summary>
public class VehicleSimulatorWorker : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleSimulatorWorker> _logger;
    private readonly IConfiguration _config;

    // مسیر نمونه (تهران) — در محیط واقعی از مسیرهای واقعی استفاده می‌شود
    private readonly List<RoutePoint> _route = new()
    {
        new(35.7219, 51.3347),
        new(35.7250, 51.3400),
        new(35.7300, 51.3450),
        new(35.7350, 51.3500),
        new(35.7400, 51.3450),
        new(35.7350, 51.3400),
        new(35.7300, 51.3350),
        new(35.7250, 51.3300)
    };

    private int _index = 0;
    private readonly Random _random = new();

    public VehicleSimulatorWorker(HttpClient httpClient, ILogger<VehicleSimulatorWorker> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // شناسه‌ی وسیله‌ی نقلیه از تنظیمات خوانده می‌شود (در داکر با env override می‌شود)
        var vehicleIdStr = _config["Simulator:VehicleId"];
        if (string.IsNullOrWhiteSpace(vehicleIdStr) || !Guid.TryParse(vehicleIdStr, out var vehicleId))
        {
            _logger.LogError("Simulator:VehicleId تنظیم نشده یا نامعتبر است. شبیه‌ساز متوقف می‌شود.");
            return;
        }

        var apiUrl = _config["Simulator:ApiUrl"] ?? "http://api:8080";
        var intervalSec = _config.GetValue("Simulator:IntervalSeconds", 3);
        var ingestUrl = $"{apiUrl.TrimEnd('/')}/api/locations";

        _logger.LogInformation("شبیه‌ساز برای وسیله {VehicleId} شروع شد. ارسال به {Url} هر {Sec} ثانیه",
            vehicleId, ingestUrl, intervalSec);

        while (!stoppingToken.IsCancellationRequested)
        {
            var point = _route[_index % _route.Count];
            _index++;

            var payload = new
            {
                VehicleId = vehicleId,
                Latitude = point.Lat + (_random.NextDouble() - 0.5) * 0.0005,   // نویز کوچک
                Longitude = point.Lng + (_random.NextDouble() - 0.5) * 0.0005,
                SpeedKmh = _random.Next(20, 60)
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(ingestUrl, payload, stoppingToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API کد {Code} برگرداند", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ارسال موقعیت به API با خطا مواجه شد");
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

        _logger.LogInformation("شبیه‌ساز متوقف شد.");
    }
}
