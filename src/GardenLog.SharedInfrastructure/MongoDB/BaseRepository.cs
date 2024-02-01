using GardenLog.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GardenLog.SharedInfrastructure.MongoDB
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class, IEntity
    {
        public IMongoCollection<T> Collection { get; }
        private IMongoCollection<BsonDocument>? BsonDocumentCollection { get; set; }
        private readonly ILogger<BaseRepository<T>> _logger;
        protected readonly IUnitOfWork _unitOfWork;

        public BaseRepository(IUnitOfWork unitOfWork, ILogger<BaseRepository<T>> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
                       
            Collection = GetCollection();
        }

      

        public void Add(T entity)
        {
            AddCommand(() => Collection.InsertOneAsync(entity));
        }

        public void Add(BsonDocument bsonDocument)
        {
            AddCommand(() => GetBsonCollection().InsertOneAsync(bsonDocument));
        }

        public void Delete(string id)
        {
            AddCommand(() => Collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id)));
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var data = await Collection.FindAsync(Builders<T>.Filter.Eq("_id", id));
            return data.SingleOrDefault();
        }

        public void Update(T entity)
        {
            AddCommand(() => Collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", entity.Id), entity));
        }

        public void Update(BsonDocument bsonDocument)
        {
            AddCommand(() => GetBsonCollection().ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("_id", bsonDocument["_id"]), bsonDocument));
        }


        protected void AddCommand(Func<Task> func)
        {
            _unitOfWork.AddCommand(func);
        }

        protected abstract IMongoCollection<T> GetCollection();

        protected IMongoCollection<BsonDocument> GetBsonCollection()
        {
            if (BsonDocumentCollection == null)
                BsonDocumentCollection = _unitOfWork.GetCollection<IMongoCollection<BsonDocument>, BsonDocument>(Collection.CollectionNamespace.CollectionName);

            return BsonDocumentCollection;
        }
    }
}
