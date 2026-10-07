using Microsoft.AspNetCore.HttpOverrides;
using RealTimeRailwayTrafficMonitoring.Api.Hubs;
using RealTimeRailwayTrafficMonitoring.Api.Services;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "Frontend";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddSingleton<ITrainService, TrainService>();
builder.Services.AddHostedService<TrainSimulatorService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost:", StringComparison.OrdinalIgnoreCase) ||
                origin.StartsWith("https://localhost:", StringComparison.OrdinalIgnoreCase))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsPolicy);

app.MapControllers();
app.MapHub<TrainHub>("/hubs/train");

app.MapGet("/", () => Results.Ok(new
{
    service = "Real-Time Railway Traffic Monitoring API",
    status = "Running",
    signalRHub = "/hubs/train"
}));

app.Run();
