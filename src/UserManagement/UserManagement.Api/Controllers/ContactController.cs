using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserManagement.CommandHandlers;
using UserManagement.Contract;

namespace UserManagement.Controllers;

[Route(GardenRoutes.GardenBase)]
[ApiController]
public class ContactController(IContactCommandHandler commadnHandler, ILogger<ContactController> logger) : Controller
{
    private readonly IContactCommandHandler _commadnHandler = commadnHandler;
    private readonly ILogger<ContactController> _logger = logger;

    #region Garden

    [HttpPost()]
    [ActionName("SendEmail")]
    [Route(UserProfileRoutes.SendEmail)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> SendEmail([FromBody] SendEmailCommand command)
    {
        var result =  await _commadnHandler.SendEmail(command);
        if (!result) return BadRequest();

        _logger.LogInformation("Email sent to {command.EmailAddress}.", command.EmailAddress);

       return Ok();

    }

    #endregion

 
}
