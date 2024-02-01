
using GardenLog.SharedInfrastructure.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace ImageCatalog.Api.Data;

public interface IImageRepository : IRepository<Model.Image>
{
    Task<IReadOnlyCollection<ImageViewModel>> GetImagesByRelatedEntitiesAsync(GetImagesByRelatedEntities request, string userProfileId);
    Task<IReadOnlyCollection<ImageViewModel>> GetImagesByRelatedEntityAsync(GetImagesByRelatedEntity request, string userProfileId);
}

public class ImageRepository : BaseRepository<Model.Image>, IImageRepository
{
    private const string IMAGE_COLLECTION_NAME = "ImageCatalog-Collection";
    private readonly ILogger<ImageRepository> _logger;

    public ImageRepository(IUnitOfWork unitOfWork, ILogger<ImageRepository> logger)
       : base(unitOfWork, logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<ImageViewModel>> GetImagesByRelatedEntityAsync(GetImagesByRelatedEntity request, string userProfileId)
    {

        string userFilter = request.FilterUserOnly ? userProfileId : string.Empty;

        return await SearchForImages(request.RelatedEntityType, request.RelatedEntityId, userFilter);
    }

    public async Task<IReadOnlyCollection<ImageViewModel>> GetImagesByRelatedEntitiesAsync(GetImagesByRelatedEntities request, string userProfileId)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        List<ImageViewModel> images = new List<ImageViewModel>();
        foreach (var entity in request.Requests!)
        {
            images.AddRange(await GetImagesByRelatedEntityAsync(entity, userProfileId));
        }

        return images.AsReadOnly();
    }

    private async Task<IReadOnlyCollection<ImageViewModel>> SearchForImages(RelatedEntityTypEnum relatedEntityType, string? relatedEntityId, string userFilter)
    {
        List<FilterDefinition<Model.Image>> filters = new();

        filters.Add(Builders<Model.Image>.Filter.Eq("RelatedEntityType", relatedEntityType.ToString()));

        if (!string.IsNullOrWhiteSpace(userFilter))
        {
            filters.Add(Builders<Model.Image>.Filter.Eq("UserProfileId", userFilter));
        }

        if (!string.IsNullOrWhiteSpace(relatedEntityId))
        {
            filters.Add(Builders<Model.Image>.Filter.Eq("RelatedEntityId", relatedEntityId));
        }


        var filter = Builders<Model.Image>.Filter.And(filters);

        var data = await Collection
          .Find<Model.Image>(filter)
          .As<ImageViewModel>()
          .ToListAsync();

        return data.AsReadOnly();

    }

    protected override IMongoCollection<Model.Image> GetCollection()
    {
        return _unitOfWork.GetCollection<IMongoCollection<Model.Image>, Model.Image>(IMAGE_COLLECTION_NAME);
    }

   
}
