//Todo - look for different Toast implemenation
using Blazored.Toast.Services;

namespace GardenLogWeb.Shared.Services;

public interface IGardenLogToastService
{
    void ShowToast(string message, GardenLogToastLevel level);
}

public class GardenLogToastService : IGardenLogToastService
{
    private readonly ILogger<GardenLogToastService> _logger;
    private readonly IToastService _service;
    public GardenLogToastService(ILogger<GardenLogToastService> logger, IToastService service)
    {
        _logger = logger;
        _service = service;
    }

    public void ShowToast(string message, GardenLogToastLevel level)
    {
        _logger.LogInformation(message, level);

        switch (level)
        {
            case GardenLogToastLevel.Error:
                _service.ShowError(message);
                break;
            case GardenLogToastLevel.Warning:
                _service.ShowWarning(message);
                break;
            case GardenLogToastLevel.Success:
                _service.ShowSuccess(message);
                break;
            case GardenLogToastLevel.Info:
                _service.ShowInfo(message);
                break;
            default:
                _service.ShowInfo(message);
                break;
        }
    }
}
public enum GardenLogToastLevel
{
    Info,
    Success,
    Warning,
    Error
}
