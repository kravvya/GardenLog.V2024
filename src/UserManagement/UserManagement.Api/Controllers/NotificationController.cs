using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserManagement.CommandHandlers;
using UserManagement.Contract;

namespace UserManagement.Controllers;

[Route(GardenRoutes.GardenBase)]
[ApiController]
public class NotificationController(INotificationCommandHandler commadnHandler, ILogger<NotificationController> logger) : Controller
{
    private readonly INotificationCommandHandler _commadnHandler= commadnHandler;
    private readonly ILogger<NotificationController> _logger= logger;
    
    #region Garden

    [HttpGet()]
    [ActionName("WeeklyTasks")]
    [Route(UserProfileRoutes.WeeklyTasks)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> WeeklyTasks()
    {
        var result =  await _commadnHandler.PublishWeeklyTasks();
        if (!result) return BadRequest();

        _logger.LogInformation("Weekly Tasks Published: {result}", result);

       return Ok();

    }

    [HttpGet()]
    [ActionName("PastDueTasks")]
    [Route(UserProfileRoutes.PastDueTasks)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> PastDueTasks()
    {
        var result = await _commadnHandler.PublishPastDueTasks();
        if (!result) return BadRequest();

        _logger.LogInformation("PastDueTasks: {result}", result);

        return Ok();

    }

    #endregion


}
