using GardenLog.SharedInfrastructure.MongoDB;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace PlantHarvest.Infrastructure.Data.Repositories;

public class UserProfileRepository : BaseRepository<UserProfile>, IUserProfileRepository
{
    private const string USER_COLLECTION_NAME = "UserProfile-Collection";

    public UserProfileRepository(IUnitOfWork unitOfWork, ILogger<UserProfileRepository> logger)
        : base(unitOfWork, logger)
    {
    }

    protected override IMongoCollection<UserProfile> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<UserProfile>, UserProfile>(USER_COLLECTION_NAME);
    }

    protected override void OnModelCreating()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(UserProfile)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<UserProfile>(p =>
        {
            p.AutoMap();
            //ignore elements not int he document 
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("user-profile");

        });

        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonClassMap.RegisterClassMap<BaseEntity>(p =>
            {
                p.AutoMap();
                //p.MapIdMember(c => c.Id).SetIdGenerator(MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
                //p.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                p.SetIgnoreExtraElements(true);
                p.UnmapMember(m => m.DomainEvents);
            });
        }

        BsonClassMap.RegisterClassMap<UserProfileBase>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);

        });

        BsonClassMap.RegisterClassMap<UserProfileViewModel>(p =>
        {
            p.AutoMap();
            //ignore elements not in the document 
            p.SetIgnoreExtraElements(true);
            //p.MapMember(m => m.UserProfileId).SetElementName("_id");
        });

    }

    public new async Task<UserProfile> GetByIdAsync(string id)
    {
        var data = await Collection.FindAsync(Builders<UserProfile>.Filter.Eq("UserProfileId", id));
        return data.SingleOrDefault();
    }

    public async Task<UserProfileViewModel> GetUserProfile(string userProfileId)
    {
        var data = await Collection
         .Find<UserProfile>(Builders<UserProfile>.Filter.Eq("UserProfileId", userProfileId))
        .As<UserProfileViewModel>()
        .FirstOrDefaultAsync();

        return data;
    }

    public async Task<UserProfileViewModel> SearchForUserProfile(SearchUserProfiles searchCriteria)
    {
        List<FilterDefinition<UserProfile>> filters = [];
        if (!string.IsNullOrWhiteSpace(searchCriteria.UserName))
        {
            filters.Add(Builders<UserProfile>.Filter.Eq("UserName", searchCriteria.UserName));
        }
       
        if (!string.IsNullOrWhiteSpace(searchCriteria.Email))
        {
            filters.Add(Builders<UserProfile>.Filter.Eq("Email", searchCriteria.Email));
        }

        var data = await Collection
            .Find<UserProfile>(Builders<UserProfile>.Filter.Or(filters))
            .As<UserProfileViewModel>()
            .FirstOrDefaultAsync();

       
        return data;
    }

    public async Task<IReadOnlyCollection<UserProfileViewModel>> GetAllUserProfiles()
    {
        var data = await Collection
           .Find<UserProfile>(_ => true)
           .As<UserProfileViewModel>()
           .ToListAsync();

        return data;
    }
}
