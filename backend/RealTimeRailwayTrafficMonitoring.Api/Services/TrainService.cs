using System.Collections.Concurrent;
using RealTimeRailwayTrafficMonitoring.Api.Models;

namespace RealTimeRailwayTrafficMonitoring.Api.Services;

public sealed class TrainService : ITrainService
{
    private readonly ConcurrentDictionary<int, Train> _trains = new();

    public TrainService()
    {
        var initial = new[]
        {
            Create(1, "TR101", "Kaveri Express", "Bengaluru → Mysuru", "KSR Bengaluru", "Kengeri", 82, 0, TrainStatus.Running, 34),
            Create(2, "TR102", "Namma Metro Link", "Bengaluru → Whitefield", "Baiyappanahalli", "Whitefield", 45, 8, TrainStatus.Delayed, 61),
            Create(3, "TR103", "South Western Express", "Bengaluru → Tumakuru", "Yeshwanthpur", "Tumakuru", 72, 0, TrainStatus.Running, 22),
            Create(4, "TR104", "Cauvery Shuttle", "Mysuru → Bengaluru", "Mandya", "Ramanagara", 0, 14, TrainStatus.Stopped, 48),
            Create(5, "TR105", "City Intercity", "Bengaluru → Hosur", "Carmelaram", "Hosur", 68, 0, TrainStatus.Running, 73),
            Create(6, "TR106", "Karnataka Express", "Bengaluru → Hubballi", "Yelahanka", "Dodballapur", 91, 3, TrainStatus.Delayed, 18)
        };

        foreach (var train in initial)
            _trains[train.Id] = train;
    }

    public IReadOnlyCollection<Train> GetAll() =>
        _trains.Values.OrderBy(t => t.Id).ToArray();

    public Train? GetById(int id) =>
        _trains.TryGetValue(id, out var train) ? train : null;

    public void Update(Train train) => _trains[train.Id] = train;

    private static Train Create(
        int id, string number, string name, string route,
        string current, string next, double speed, int delay,
        TrainStatus status, double progress) =>
        new()
        {
            Id = id,
            TrainNumber = number,
            Name = name,
            Route = route,
            CurrentStation = current,
            NextStation = next,
            SpeedKmph = speed,
            DelayMinutes = delay,
            Status = status,
            ProgressPercent = progress,
            LastUpdatedUtc = DateTime.UtcNow
        };
}
