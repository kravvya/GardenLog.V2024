using ImageCatalog.Api.CommandHandlers;
using ImageCatalog.Api.QueryHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ImageCatalog.Api.Controllers;

[Route(ImageRoutes.ImageCatalogBase)]
[ApiController]
[Authorize]
public class ImageController : ControllerBase
{
    private readonly ILogger<ImageController> _logger;
    private readonly IImageCommandHandler _handler;
    private readonly IImageQueryHandler _queryHandler;

    public ImageController(ILogger<ImageController> logger,  IImageCommandHandler handler, IImageQueryHandler queryHandler)
    {
        _logger = logger;
        _handler = handler;
        _queryHandler = queryHandler;
    }

    [HttpPost()]
    [ActionName("Search")]
    [Route(ImageRoutes.Search)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(IReadOnlyCollection<ImageViewModel>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> SearchAsync([FromBody] GetImagesByRelatedEntity request)
    {
        try
        {
            var results = await _queryHandler.GetImagesByRelatedEntityAsync(request, User.GetUserProfileId(Request.Headers));

            if (results == null) return NotFound();

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception getting images: {ex}", ex);
            return Problem(ex.Message);
        }
    }

    [HttpPost()]
    [ActionName("SearchBatch")]
    [Route(ImageRoutes.SearchBatch)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(IReadOnlyCollection<ImageViewModel>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> SearchBatchAsync([FromBody] GetImagesByRelatedEntities request)
    {
        try
        {
            var results = await _queryHandler.GetImagesByRelatedEntitiesAsync(request, User.GetUserProfileId(Request.Headers));

            if (results == null) return NotFound();

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception getting images: {ex}", ex);
            return Problem(ex.Message);
        }
    }

    [HttpPost]
    [ActionName("CrerateImage")]
    [Route(ImageRoutes.CrerateImage)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> PostImageAsync(CreateImageCommand createCommand)
    {
        try
        {
            var results = await _handler.CreateImageAsync(User.GetUserProfileId(Request.Headers), createCommand);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception creating Image: {ex}", ex);
            return Problem(ex.Message);
        }
    }

    [HttpPut]
    [ActionName("UpdateImage")]
    [Route(ImageRoutes.UpdateImage)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> PutImageAsync(UpdateImageCommand updateCommand)
    {
        try
        {
            var result = await _handler.UpdateImageAsync(updateCommand);

            return result ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception updating Image: {ex}", ex);
            return Problem(ex.Message);
        }
    }

    [HttpDelete]
    [ActionName("DeleteImage")]
    [Route(ImageRoutes.DeleteImage)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> DeleteImageAsync(string imageId)
    {
        try
        {
            var result = await _handler.DeleteImageAsync(imageId);

            return result ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception deleting Image: {ex}", ex);
            return Problem(ex.Message);
        }
    }
}
