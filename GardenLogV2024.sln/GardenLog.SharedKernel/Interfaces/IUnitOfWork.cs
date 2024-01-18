namespace GardenLog.SharedKernel.Interfaces;

public interface IUnitOfWork
{
    string Initialize(string handlerName);
    void AddCommand(Func<Task> func);
   T GetCollection<T, Y>(string collectionName);
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(string? handlerName);
}
