using Microsoft.EntityFrameworkCore;
using POS.Server.Data;
using POS.Shared.Entities;

namespace POS.Server.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<List<User>> GetAllAsync() => context.Users.Include(x => x.Role).AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    public Task<User?> GetByIdAsync(int id) => context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
    public Task<User?> GetByUsernameAsync(string username) => context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Username == username);
    public async Task<User> CreateAsync(User user) { context.Users.Add(user); await context.SaveChangesAsync(); return user; }
    public Task SaveAsync() => context.SaveChangesAsync();
    public Task<bool> ExistsAsync(string username, string email, int? exceptId = null) =>
        context.Users.AnyAsync(x => (x.Username == username || x.Email == email) && (!exceptId.HasValue || x.Id != exceptId));
}
