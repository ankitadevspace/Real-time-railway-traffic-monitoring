using Microsoft.AspNetCore.SignalR;
using RealTimeRailwayTrafficMonitoring.Api.Hubs;
using RealTimeRailwayTrafficMonitoring.Api.Models;

namespace RealTimeRailwayTrafficMonitoring.Api.Services;

public sealed class TrainSimulatorService : BackgroundService
{
    private readonly ITrainService _trainService;
    private readonly IHubContext<TrainHub> _hub;
    private readonly Random _random = new();

    private static readonly string[] Stations =
    {
        "KSR Bengaluru", "Kengeri", "Ramanagara", "Mandya", "Mysuru",
        "Baiyappanahalli", "Whitefield", "Yeshwanthpur", "Tumakuru",
        "Carmelaram", "Hosur", "Yelahanka", "Dodballapur"
    };

    public TrainSimulatorService(ITrainService trainService, IHubContext<TrainHub> hub)
    {
        _trainService = trainService;
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var train in _trainService.GetAll())
            {
                Simulate(train);
                _trainService.Update(train);
            }

            var snapshot = _trainService.GetAll();
            await _hub.Clients.All.SendAsync("TrainUpdates", snapshot, stoppingToken);
            await _hub.Clients.All.SendAsync("DashboardStats", BuildStats(snapshot), stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private void Simulate(Train train)
    {
        if (train.Status == TrainStatus.Stopped)
        {
            if (_random.NextDouble() < 0.35)
            {
                train.Status = TrainStatus.Running;
                train.SpeedKmph = _random.Next(45, 80);
            }
            else
            {
                train.SpeedKmph = 0;
            }
        }
        else
        {
            train.SpeedKmph = Math.Clamp(train.SpeedKmph + _random.Next(-8, 9), 25, 110);
            train.ProgressPercent += _random.NextDouble() * 2.8;

            if (train.ProgressPercent >= 100)
            {
                train.ProgressPercent = 0;
                train.CurrentStation = train.NextStation;
                train.NextStation = Stations[_random.Next(Stations.Length)];
            }

            if (_random.NextDouble() < 0.08)
            {
                train.Status = TrainStatus.Delayed;
                train.DelayMinutes = Math.Clamp(train.DelayMinutes + _random.Next(1, 4), 1, 20);
            }
            else if (_random.NextDouble() < 0.12)
            {
                train.Status = TrainStatus.Stopped;
                train.SpeedKmph = 0;
            }
            else
            {
                train.Status = TrainStatus.Running;
                train.DelayMinutes = Math.Max(0, train.DelayMinutes - 1);
            }
        }

        train.LastUpdatedUtc = DateTime.UtcNow;
    }

    private static object BuildStats(IReadOnlyCollection<Train> trains) => new
    {
        total = trains.Count,
        running = trains.Count(t => t.Status == TrainStatus.Running),
        delayed = trains.Count(t => t.Status == TrainStatus.Delayed),
        stopped = trains.Count(t => t.Status == TrainStatus.Stopped),
        averageSpeed = Math.Round(trains.Average(t => t.SpeedKmph), 1)
    };
}
