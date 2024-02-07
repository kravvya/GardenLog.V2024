
using GrowConditions.Contract.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GrowConditions.Api.Controllers;

[Route(WeatherRoutes.WeatherBase)]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly ILogger<WeatherController> _logger;
    private readonly IWeatherCommandHandler _weatherCommandHandler;
    private readonly IWeatherQueryHandler _weatherQueryHandler;

    public WeatherController(ILogger<WeatherController> logger, IWeatherCommandHandler weatherCommandHandler, IWeatherQueryHandler weatherQueryHandler)
    {
        _logger = logger;
        _weatherCommandHandler = weatherCommandHandler;
        _weatherQueryHandler = weatherQueryHandler;
    }

    [Authorize]
    [HttpGet(WeatherRoutes.GetLastWeatherUpdate)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(WeatherUpdateViewModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetLastWeatherUpdate(string gardenId)
    {
        var weather = await _weatherQueryHandler.GetLastWeatherUpdate(gardenId);
        if (weather == null)
        {
            return NotFound();
        }
        return Ok(weather);
    }

    [Authorize]
    [HttpGet(WeatherRoutes.GetHistoryOfWeatherUpdates)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(IList<WeatherUpdateViewModel>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetHistoryOfWeatherUpdates(string gardenId, int numberOfDays)
    {
        var weather = await _weatherQueryHandler.GetHistoryOfWeatherUpdates(gardenId, numberOfDays);
        if (weather == null)
        {
            return NotFound();
        }
        return Ok(weather);
    }


    [HttpGet(WeatherRoutes.Run)]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public ActionResult Run()
    {
        try
        {
            _logger.LogInformation("Received request to run weatherStation cycle");
             _weatherCommandHandler.GetWeatherUpdates().GetAwaiter().GetResult();
            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception running weatherStation cycle time: {ex}", ex);
            return Problem(ex.Message);
        }
    }

    [Authorize]
    [HttpGet(WeatherRoutes.GetWeatherStation)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(WeatherUpdateViewModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetWeatherStation(decimal lat, decimal lon)
    {
        var weatherStation = await _weatherQueryHandler.GetWeatherStation(lat, lon);
        if (weatherStation == null)
        {
            return NotFound();
        }
        return Ok(weatherStation);
    }

    [Authorize]
    [HttpGet(WeatherRoutes.GetForecast)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(WeatherUpdateViewModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetForecast(string gardenId)
    {
        var weatherStation = await _weatherQueryHandler.GetForecast(gardenId);
        if (weatherStation == null)
        {
            return NotFound();
        }
        return Ok(weatherStation);
    }
}
