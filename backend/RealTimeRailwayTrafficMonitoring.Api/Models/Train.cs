namespace RealTimeRailwayTrafficMonitoring.Api.Models;

public sealed class Train
{
    public int Id { get; init; }
    public string TrainNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string CurrentStation { get; set; } = string.Empty;
    public string NextStation { get; set; } = string.Empty;
    public double SpeedKmph { get; set; }
    public int DelayMinutes { get; set; }
    public TrainStatus Status { get; set; }
    public double ProgressPercent { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
}

public enum TrainStatus
{
    Running,
    Delayed,
    Stopped
}
