using Microsoft.EntityFrameworkCore;
using POS.Server.Data;

namespace POS.Server.Repositories;

public class CatalogRepository<T>(AppDbContext context) : ICatalogRepository<T> where T : class
{
    public Task<List<T>> GetAllAsync() => context.Set<T>().AsNoTracking().ToListAsync();
    public Task<T?> GetAsync(int id) => context.Set<T>().FindAsync(id).AsTask();
    public async Task<T> AddAsync(T entity) { context.Set<T>().Add(entity); await context.SaveChangesAsync(); return entity; }
    public Task SaveAsync() => context.SaveChangesAsync();
}
