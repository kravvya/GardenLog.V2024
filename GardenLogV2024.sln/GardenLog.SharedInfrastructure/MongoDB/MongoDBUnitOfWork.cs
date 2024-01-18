using DnsClient.Internal;
using GardenLog.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;

namespace GardenLog.SharedInfrastructure.MongoDB;

public class MongoDBUnitOfWork : IUnitOfWork
{
    private readonly List<Func<Task>> _commands;
    private readonly IMongoDBContext _context;
    private readonly ILogger<MongoDBUnitOfWork> _logger;
    private string? _rootHandler = null;

    public MongoDBUnitOfWork(IMongoDBContext context, ILogger<MongoDBUnitOfWork> logger)
    {
        _context = context;
        _logger = logger;

        // Every command will be stored and it'll be processed at SaveChanges
        _commands = new List<Func<Task>>();
    }

    public string Initialize(string handlerName)
    {
        if (string.IsNullOrWhiteSpace(_rootHandler))
        {
            _rootHandler = handlerName;
        }

        return _rootHandler;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await SaveChangesAsync(null);
    }

    public async Task<int> SaveChangesAsync(string? handlerName)
    {
        if (!string.IsNullOrWhiteSpace(_rootHandler))
        {
            if (string.IsNullOrWhiteSpace(handlerName) || handlerName != _rootHandler)
            {
                _logger.LogWarning("Request did not originate from root handler. Changes will not be saved.");
                return 0;
            }
        }

        int numberOfChanges = await _context.ApplyChangesAsync(_commands);
        _commands.Clear();

        _logger.LogInformation("Request originate from root handler. {0} changes were saved.", numberOfChanges);
        return numberOfChanges;
    }

    public void AddCommand(Func<Task> func)
    {
        _commands.Add(func);
    }

    public T GetCollection<T, Y>(string collectionName)
    {
        return _context.GetCollection<T, Y>(collectionName);
    }


}

