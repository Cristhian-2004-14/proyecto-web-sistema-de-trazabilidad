namespace POS.Server.Repositories;

public interface ICatalogRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetAsync(int id);
    Task<T> AddAsync(T entity);
    Task SaveAsync();
}
