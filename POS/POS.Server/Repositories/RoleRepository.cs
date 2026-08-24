using Microsoft.EntityFrameworkCore;
using POS.Server.Data;
using POS.Shared.Entities;

namespace POS.Server.Repositories;

public class RoleRepository(AppDbContext context) : IRoleRepository
{
    public Task<List<RoleEntity>> GetAllAsync() => context.Roles.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    public Task<List<RoleEntity>> GetActiveAsync() => context.Roles.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
    public Task<RoleEntity?> GetByIdAsync(int id) => context.Roles.FirstOrDefaultAsync(x => x.Id == id);
    public async Task<RoleEntity> AddAsync(RoleEntity role) { context.Roles.Add(role); await context.SaveChangesAsync(); return role; }
    public Task SaveAsync() => context.SaveChangesAsync();
}
