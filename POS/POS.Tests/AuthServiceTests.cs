using Microsoft.Extensions.Configuration;
using POS.Server.Repositories;
using POS.Server.Services;
using POS.Server.Controllers;
using POS.Shared.DTOs;
using POS.Shared.Entities;
using System.Text.Json;
using Xunit;

namespace POS.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndRole()
    {
        var service = CreateService(new User { Id=1, Username="admin", Email="a@a.bo", Name="Ana", LastName="Rojas", IsActive=true, PasswordHash=BCrypt.Net.BCrypt.HashPassword("Segura123!"), Role=new RoleEntity{Id=1, Name="Administrador", IsActive=true} });
        var result = await service.LoginAsync(new LoginRequest { Username="admin", Password="Segura123!" });
        Assert.NotNull(result); Assert.Equal("Administrador", result.Role); Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsNull()
    {
        var service = CreateService(new User { Username="admin", IsActive=true, PasswordHash=BCrypt.Net.BCrypt.HashPassword("Correcta123!"), Role=new RoleEntity{Id=1, Name="Administrador", IsActive=true} });
        Assert.Null(await service.LoginAsync(new LoginRequest { Username="admin", Password="incorrecta" }));
    }

    [Fact]
    public async Task Login_WithInactiveAccount_ReturnsNull()
    {
        var service = CreateService(new User { Username="admin", IsActive=false, PasswordHash=BCrypt.Net.BCrypt.HashPassword("Correcta123!") });
        Assert.Null(await service.LoginAsync(new LoginRequest { Username="admin", Password="Correcta123!" }));
    }

    [Fact]
    public async Task Register_StoresBcryptHash_NotPlainText()
    {
        var repository = new FakeUserRepository(); var service = CreateService(null, repository);
        await service.RegisterAsync(new RegisterRequest { Name="Eva", LastName="López", Username="eva", Email="eva@pv.bo", Password="Secreta123!", RoleId=1 });
        Assert.NotEqual("Secreta123!", repository.Saved!.PasswordHash); Assert.True(BCrypt.Net.BCrypt.Verify("Secreta123!", repository.Saved.PasswordHash));
    }

    [Fact]
    public async Task Login_WithInactiveRole_ReturnsNull()
    {
        var service = CreateService(new User { Username="admin", IsActive=true, PasswordHash=BCrypt.Net.BCrypt.HashPassword("Correcta123!"), Role=new RoleEntity{Id=1, Name="Administrador", IsActive=false} });
        Assert.Null(await service.LoginAsync(new LoginRequest { Username="admin", Password="Correcta123!" }));
    }

    [Fact]
    public async Task Register_WithInactiveRole_IsRejected()
    {
        var repository = new FakeUserRepository();
        var roles = new FakeRoleRepository(new RoleEntity { Id=9, Name="Inactivo", IsActive=false });
        var service = CreateService(null, repository, roles);
        var result = await service.RegisterAsync(new RegisterRequest { Name="Eva", LastName="López", Username="eva", Email="eva@pv.bo", Password="Secreta123!", RoleId=9 });
        Assert.Null(result); Assert.Null(repository.Saved);
    }

    [Fact]
    public async Task UsersApiResponse_DoesNotExposePasswordHash()
    {
        var user = new User { Id=1, Name="Ana", LastName="Rojas", Username="admin", Email="admin@pv.bo", PasswordHash="hash-muy-sensible", RoleId=1, Role=new RoleEntity{Id=1, Name="Administrador", IsActive=true}, IsActive=true };
        var repository = new FakeUserRepository { Existing=user };
        var controller = new UsersController(repository, new FakeRoleRepository(user.Role), new FakeAuthService());
        var response = await controller.Get();
        var json = JsonSerializer.Serialize(response);
        Assert.DoesNotContain("PasswordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash-muy-sensible", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Administrador", response.Single().RoleName);
    }

    private static AuthService CreateService(User? user, FakeUserRepository? repository=null, FakeRoleRepository? roleRepository=null)
    {
        repository ??= new FakeUserRepository { Existing=user };
        roleRepository ??= new FakeRoleRepository(user?.Role ?? new RoleEntity { Id=1, Name="Administrador", IsActive=true });
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["JwtSettings:SecretKey"]="Clave-de-pruebas-Palma-Verde-2026-muy-segura", ["JwtSettings:Issuer"]="Tests", ["JwtSettings:Audience"]="Tests", ["JwtSettings:ExpirationMinutes"]="10" }).Build();
        return new AuthService(repository, roleRepository, config);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? Existing { get; init; } public User? Saved { get; private set; }
        public Task<List<User>> GetAllAsync()=>Task.FromResult(Existing is null ? new List<User>() : new List<User> { Existing });
        public Task<User?> GetByIdAsync(int id)=>Task.FromResult(Saved ?? Existing);
        public Task<User?> GetByUsernameAsync(string username)=>Task.FromResult(Existing?.Username==username?Existing:null);
        public Task<User> CreateAsync(User user){user.Id=1;user.Role=new RoleEntity{Id=user.RoleId,Name="Administrador",IsActive=true};Saved=user;return Task.FromResult(user);}
        public Task SaveAsync()=>Task.CompletedTask;
        public Task<bool> ExistsAsync(string username,string email,int? exceptId=null)=>Task.FromResult(false);
    }

    private sealed class FakeRoleRepository(RoleEntity? role) : IRoleRepository
    {
        public Task<List<RoleEntity>> GetAllAsync()=>Task.FromResult(role is null ? new List<RoleEntity>() : new List<RoleEntity>{role});
        public Task<List<RoleEntity>> GetActiveAsync()=>Task.FromResult(role is {IsActive:true} ? new List<RoleEntity>{role} : new List<RoleEntity>());
        public Task<RoleEntity?> GetByIdAsync(int id)=>Task.FromResult(role?.Id==id ? role : null);
        public Task<RoleEntity> AddAsync(RoleEntity item)=>Task.FromResult(item);
        public Task SaveAsync()=>Task.CompletedTask;
    }

    private sealed class FakeAuthService : IAuthService
    {
        public Task<AuthResponse?> LoginAsync(LoginRequest request)=>Task.FromResult<AuthResponse?>(null);
        public Task<AuthResponse?> RegisterAsync(RegisterRequest request)=>Task.FromResult<AuthResponse?>(null);
    }
}
