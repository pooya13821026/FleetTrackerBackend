using FleetTracker.Simulator;

var builder = Host.CreateApplicationBuilder(args);

// ثبت HttpClient با آدرس پایه API
builder.Services.AddHttpClient<VehicleSimulatorWorker>();

builder.Services.AddHostedService<VehicleSimulatorWorker>();

var host = builder.Build();
host.Run();
