using POS.Shared.Entities;

namespace POS.Server.Repositories;

public interface IRoleRepository
{
    Task<List<RoleEntity>> GetAllAsync();
    Task<List<RoleEntity>> GetActiveAsync();
    Task<RoleEntity?> GetByIdAsync(int id);
    Task<RoleEntity> AddAsync(RoleEntity role);
    Task SaveAsync();
}
