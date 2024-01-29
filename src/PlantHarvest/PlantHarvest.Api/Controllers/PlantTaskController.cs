using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantHarvest.Contract.Query;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlantHarvest.Api.Controllers;

[Route(HarvestRoutes.PlantTaskBase)]
[ApiController]
[Authorize]
public class PlantTaskController : Controller
{
    private readonly IPlantTaskCommandHandler _handler;
    private readonly IPlantTaskQueryHandler _queryHandler;
    private readonly ILogger<PlantTaskController> _logger;

    public PlantTaskController(IPlantTaskCommandHandler handler, IPlantTaskQueryHandler queryHandler, ILogger<PlantTaskController> logger)
    {
        _handler = handler;
        _queryHandler = queryHandler;
        _logger = logger;
    }

    #region Plant Task

    [HttpGet()]
    [ActionName("GetTasks")]
    [Route(HarvestRoutes.GetTasks)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PlantTaskViewModel>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IReadOnlyCollection<PlantTaskViewModel>>> GetTasks()
    {
         return Ok(await _queryHandler.GetPlantTasks());
    }

    [HttpGet()]
    [ActionName("GetActiveTasks")]
    [Route(HarvestRoutes.GetActiveTasks)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PlantTaskViewModel>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IReadOnlyCollection<PlantTaskViewModel>>> GetActiveTasks()
    {
        return Ok(await _queryHandler.GetActivePlantTasks());
    }

    [HttpPost()]
    [ActionName("SearchTasks")]
    [Route(HarvestRoutes.SearchTasks)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PlantTaskViewModel>), (int)HttpStatusCode.OK)]
    [Produces("application/json", "text/html")]
    public async Task<ActionResult<IReadOnlyCollection<PlantTaskViewModel>>> SearchTasks([FromBody] PlantTaskSearch search, string format)
    {
        var data = await _queryHandler.SearchPlantTasks(search);

        if (format == "html")
        {
            return View(( data, search.IsPastDue)); // return JSON if the format parameter is "json"
        }
       
        return Ok(data);
    }

    [HttpPost]
    [Route(HarvestRoutes.CreateTask)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateTask([FromBody] CreatePlantTaskCommand command)
    {
        try
        {
            string result = await _handler.CreatePlantTask(command);

            if (!string.IsNullOrWhiteSpace(result))
            {
                return Ok(result);
            }
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName!, ex.Message);
            return BadRequest(ModelState);
        }

        return BadRequest();
    }

    [HttpPut()]
    [Route(HarvestRoutes.UpdateTask)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateTask([FromBody] UpdatePlantTaskCommand command)
    {
        try
        {
            string result = await _handler.UpdatePlantTask(command);

            if (!string.IsNullOrWhiteSpace(result))
            {
                return Ok(result);
            }
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName!, ex.Message);
            return BadRequest(ModelState);
        }

        return BadRequest();
    }

    [HttpPut()]
    [Route(HarvestRoutes.CompleteTask)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CompleteTask([FromBody] UpdatePlantTaskCommand command)
    {

        string result = await _handler.CompletePlantTask(command);

        if (!string.IsNullOrWhiteSpace(result))
        {
            return Ok(true);
        }


        return BadRequest();
    }

    [HttpGet()]
    [ActionName("GetCompleteTaskCount")]
    [Route(HarvestRoutes.GetCompleteTaskCount)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PlantTaskViewModel>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IReadOnlyCollection<PlantTaskViewModel>>> GetCompleteTaskCount(string harvestId)
    {
        return Ok(await _queryHandler.GetCompletedTaskCount(harvestId));
    }
    #endregion
}
