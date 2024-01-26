using UserManagement.Api.Data.ApiClients;

namespace UserManagement.CommandHandlers;

public interface IContactCommandHandler
{
    Task<bool> SendEmail(SendEmailCommand request);
}

public class ContactCommandHandler : IContactCommandHandler
{
    private readonly IEmailClient? _emailClient;
    private readonly ILogger<GardenCommandHandler>? _logger;

    public ContactCommandHandler(IEmailClient emailClient, ILogger<GardenCommandHandler> logger)
    {        
        _emailClient = emailClient;
        _logger = logger;
    }

    public async Task<bool> SendEmail(SendEmailCommand request)
    {
        if (_emailClient == null) return false;

        return await _emailClient.SendEmail(request);
    }
}
