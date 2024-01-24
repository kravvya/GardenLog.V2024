
using GardenLog.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Linq;

namespace GardenLog.SharedInfrastructure.MongoDB;

public interface IMongoDBContext
{
    T GetCollection<T, Y>(string collectionName);
    Task<int> ApplyChangesAsync(List<Func<Task>> commands);
}

public class MongoDbContext : IMongoDBContext
{
    private readonly ILogger<MongoDbContext> _logger;

    protected MongoClient? MongoClient { get; set; }
    protected IMongoDatabase? Database { get; set; }
    protected MongoSettings? Settings { get; set; }
    protected IClientSessionHandle? Session { get; set; }

   

    public MongoDbContext(IConfigurationService configurationService, ILogger<MongoDbContext> logger)
    {
        _logger = logger;
        Settings = configurationService.GetPlantCatalogMongoSettings();

        if(Settings == null || Settings.Server == null || Settings.DatabaseName == null || Settings.UserName==null)
        {
            _logger.LogCritical("Did not get MongoDB connection setting. ");
            throw new ArgumentNullException(nameof(configurationService));
        }

        OnConfiguring();
    }

    private void OnConfiguring()
    {
        _logger.LogInformation("Got connection string. Start with {server}", Settings!.Server);

        MongoUrlBuilder bldr = new()
        {
            Scheme = ConnectionStringScheme.MongoDBPlusSrv,
            UseTls = true,
            Server = new MongoServerAddress(Settings!.Server),
            Username = Settings.UserName,
            Password = Settings.Password
        };

        var settings = MongoClientSettings.FromUrl(bldr.ToMongoUrl());
        settings.RetryWrites = true;
        settings.WriteConcern = WriteConcern.WMajority;
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        settings.LinqProvider = LinqProvider.V3;

        MongoClient = new MongoClient(settings);
        _logger.LogInformation("Mongo Client is set up");

        Database = MongoClient.GetDatabase(Settings.DatabaseName);
        _logger.LogInformation("Mongo database is set up {db}", Settings.DatabaseName);

        try
        {
            var result = Database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public T GetCollection<T, Y>(string collectionName)
    {
        return (T)Database!.GetCollection<Y>(collectionName);
    }
     

    public async Task<int> ApplyChangesAsync(List<Func<Task>> commands)
    {
        using (Session = await MongoClient!.StartSessionAsync())
        {
            Session.StartTransaction();

            var commandTasks = commands.Select(c => c());

            await Task.WhenAll(commandTasks);

            await Session.CommitTransactionAsync();
        }

        return commands.Count; ;
    }
}