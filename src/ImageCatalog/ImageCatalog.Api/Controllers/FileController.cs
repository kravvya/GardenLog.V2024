
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ImageCatalog.Api.Controllers;

[Route(ImageRoutes.FileCatalogBase)]
[ApiController]
[Authorize]
public class FileController : Controller
{
    private readonly ILogger<FileController> _logger;
    private readonly IFileCommandHandler _fileService;

    public FileController(ILogger<FileController> logger, IFileCommandHandler fileService)
    {
        _logger = logger;
        _fileService = fileService;
    }

    [HttpGet()]
    [ActionName("GenerateSasToken")]
    [Route(ImageRoutes.GenerateSasToken)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    public ActionResult GenerateSasUri(string fileName)
    {
        try
        {
            _logger.LogInformation("Generating SAS Uri for {user}", User.GetUserProfileId(Request.Headers));
            var results = _fileService.GenerateSasToken(fileName);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception getting images: {ex}", ex);
            return Problem(ex.Message);
        }
    }


    [HttpGet()]
    [ActionName("ResizeImageToThumbnail")]
    [Route(ImageRoutes.ResizeImageToThumbnail)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public ActionResult ResizeImageToThumbnail(string fileName)
    {
        try
        {
            _logger.LogInformation("Generating thumbnail image for {file}", fileName);
            _fileService.ResizeImageToThumbnail(fileName);
            return Accepted();
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception getting images: {ex}", ex);
            return Problem(ex.Message);
        }
    }

}
