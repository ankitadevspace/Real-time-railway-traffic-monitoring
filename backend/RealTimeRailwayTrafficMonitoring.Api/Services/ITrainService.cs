using RealTimeRailwayTrafficMonitoring.Api.Models;

namespace RealTimeRailwayTrafficMonitoring.Api.Services;

public interface ITrainService
{
    IReadOnlyCollection<Train> GetAll();
    Train? GetById(int id);
    void Update(Train train);
}
