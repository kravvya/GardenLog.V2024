using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserManagement.CommandHandlers;
using UserManagement.Contract;
using UserManagement.QueryHandlers;

namespace UserManagement.Controllers;


[Route(UserProfileRoutes.UserProfileBase)]
[ApiController]
[Authorize]
public class UserProfileController(IUserProfileCommandHandler commadnHandler, IUserProfileQueryHandler queryHandler, ILogger<UserProfileController> logger) : Controller
{
    private readonly IUserProfileCommandHandler _commadnHandler = commadnHandler;
    private readonly IUserProfileQueryHandler _queryHandler = queryHandler;
    private readonly ILogger<UserProfileController> _logger = logger;

    [HttpGet()]
    [ActionName("GetUserProfile")]
    [Route(UserProfileRoutes.GetUserProfile)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(UserProfileViewModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<UserProfileViewModel>> GetUserProfile()
    {
        try
        {
            var results = await _queryHandler.GetUserProfile();

            if (results == null) return NotFound();

            return Ok(results);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet()]
    [ActionName("GetUserProfileById")]
    [Route(UserProfileRoutes.GetUserProfileById)]
    [Authorize(Policy = "admin_or_tester")]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(UserProfileViewModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<UserProfileViewModel>> GetUserProfileById(string userProfileId)
    {
        try
        {
            var results = await _queryHandler.GetUserProfile(userProfileId);

            if (results == null) return NotFound();

            return Ok(results);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPost()]
    [ActionName("SearchUserProfile")]
    [Route(UserProfileRoutes.SearchUserProfile)]
    [Authorize(Policy = "admin_or_tester")]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(UserProfileViewModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<UserProfileViewModel>> SearchUserProfile([FromBody] SearchUserProfiles search)
    {
        try
        {
            var results = await _queryHandler.SearchForUserProfile(search);

            if (results == null) return NotFound();

            return Ok(results);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPost()]
    [ActionName("CreateUserProfile")]
    [AllowAnonymous]
    [Route(UserProfileRoutes.CreateUserProfile)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<UserProfileViewModel>> CreateUserProfile([FromBody] CreateUserProfileCommand command)
    {
        try
        {
            var results = await _commadnHandler.CreateUserProfile(command);
            _logger.LogInformation("New User created: {result}", results);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName!, ex.Message);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPut()]
    [ActionName("UpdateUserProfile")]
    [Route(UserProfileRoutes.UpdateUserProfile)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<UserProfileViewModel>> UpdateUserProfile([FromBody] UpdateUserProfileCommand command)
    {
        try
        {
            var results = await _commadnHandler.UpdateUserProfile(command);

            return results == 0 ? NotFound() : Ok(results);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName!, ex.Message);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpDelete()]
    [ActionName("DeleteUserProfile")]
    [Authorize(Policy = "admin_or_tester")]
    [Route(UserProfileRoutes.DeleteUserProfile)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<UserProfileViewModel>> DeleteUserProfile(string userProfileId)
    {
        try
        {
            var results = await _commadnHandler.DeleteUserProfile(userProfileId);

            return results == 0 ? NotFound() : Ok(results);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
