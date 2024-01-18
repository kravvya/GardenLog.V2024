namespace GardenLog.SharedKernel.Interfaces;

public interface IRepository<T>  where T : class, IEntity
{
    void Add(T entity);
    Task<T> GetByIdAsync(string id);
    void Update(T entity);
    void Delete(string id);
}
