using GardenLog.SharedKernel.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCatalog.Contract;
using PlantHarvest.Contract.Enum;
using PlantHarvest.Domain.WorkLogAggregate.Events.Meta;
using System.Net;

namespace PlantHarvest.Api.Controllers;

[Route(HarvestRoutes.WorkLogBase)]
[ApiController]
[Authorize]
public class WorkLogController : Controller
{
    private readonly IWorkLogCommandHandler _handler;
    private readonly IWorkLogQueryHandler _queryHandler;
    private readonly ILogger<WorkLogController> _logger;

    public WorkLogController(IWorkLogCommandHandler handler, IWorkLogQueryHandler queryHandler, ILogger<WorkLogController> logger)
    {
        _handler = handler;
        _queryHandler = queryHandler;
        _logger = logger;
    }

    #region Work Log

    [HttpGet()]
    [ActionName("GetAllWorkLogs")]
    [Route(HarvestRoutes.GetWorkLogs)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(IReadOnlyCollection<WorkLogViewModel>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IReadOnlyCollection<WorkLogViewModel>>> GetAllWorkLogs(string entityType, string entityId)
    {
        RelatedEntityTypEnum type = Enum.Parse<RelatedEntityTypEnum>(entityType);
        return Ok(await _queryHandler.GetWorkLogs(type, entityId));

    }

    [HttpPost]
    [Route(HarvestRoutes.CreateWorkLog)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> PostWorkLogAsync([FromBody] CreateWorkLogCommand command)
    {
        try
        {
            string result = await _handler.CreateWorkLog(command);

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
    [Route(HarvestRoutes.UpdateWorkLog)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> PutWorkLogAsync([FromBody] UpdateWorkLogCommand command)
    {
        try
        {
            string result = await _handler.UpdateWorkLog(command);

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

    [HttpDelete()]
    [Route(HarvestRoutes.DeleteWorkLog)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> DeleteWorkLogAsync(string id)
    {

        string result = await _handler.DeleteWorkLog(id);

        if (!string.IsNullOrWhiteSpace(result))
        {
            return Ok(true);
        }


        return BadRequest();
    }
    #endregion
}
