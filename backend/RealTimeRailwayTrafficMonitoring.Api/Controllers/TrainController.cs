using Microsoft.AspNetCore.Mvc;
using RealTimeRailwayTrafficMonitoring.Api.Models;
using RealTimeRailwayTrafficMonitoring.Api.Services;

namespace RealTimeRailwayTrafficMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TrainController : ControllerBase
{
    private readonly ITrainService _trainService;

    public TrainController(ITrainService trainService) => _trainService = trainService;

    [HttpGet]
    public ActionResult<IReadOnlyCollection<Train>> GetAll() => Ok(_trainService.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<Train> GetById(int id)
    {
        var train = _trainService.GetById(id);
        return train is null ? NotFound() : Ok(train);
    }
}
