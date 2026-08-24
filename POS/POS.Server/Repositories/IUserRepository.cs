using POS.Shared.Entities;

namespace POS.Server.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
    Task SaveAsync();
    Task<bool> ExistsAsync(string username, string email, int? exceptId = null);
}
