using GardenLog.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GardenLog.SharedInfrastructure.MongoDB
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class, IEntity
    {
        protected IMongoCollection<T> Collection { get; }
        private readonly ILogger<BaseRepository<T>> _logger;
        protected readonly IUnitOfWork _unitOfWork;

        public BaseRepository(IUnitOfWork unitOfWork, ILogger<BaseRepository<T>> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;

            //It is very importent to register models before getting Collection. Run into issues, when get collection was automatically mapping documents. 
            OnModelCreating();

            Collection = GetCollection();
        }

        public void Add(T entity)
        {
            AddCommand(() => Collection.InsertOneAsync(entity));
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

        protected void AddCommand(Func<Task> func)
        {
            _unitOfWork.AddCommand(func);
        }

        protected abstract IMongoCollection<T> GetCollection();
        protected abstract void OnModelCreating();
    }
}
