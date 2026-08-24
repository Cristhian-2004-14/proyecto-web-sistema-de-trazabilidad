using Microsoft.IdentityModel.Tokens;
using POS.Server.Repositories;
using POS.Shared.DTOs;
using POS.Shared.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace POS.Server.Services;

public class AuthService(IUserRepository users, IRoleRepository roles, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await users.GetByUsernameAsync(request.Username.Trim());
        if (user is null || !user.IsActive || user.Role is null || !user.Role.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;
        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await users.ExistsAsync(request.Username.Trim(), request.Email.Trim())) return null;
        var role = await roles.GetByIdAsync(request.RoleId);
        if (role is null || !role.IsActive) return null;
        var user = new User
        {
            Name = request.Name.Trim(), LastName = request.LastName.Trim(), Username = request.Username.Trim(),
            Email = request.Email.Trim(), PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = request.RoleId, IsActive = request.IsActive
        };
        await users.CreateAsync(user);
        user = (await users.GetByIdAsync(user.Id))!;
        return GenerateAuthResponse(user);
    }

    private AuthResponse GenerateAuthResponse(User user) => new()
    {
        Token = GenerateToken(user), UserId = user.Id, Email = user.Email, Username = user.Username,
        FullName = $"{user.Name} {user.LastName}", Role = user.Role?.Name ?? string.Empty
    };

    private string GenerateToken(User user)
    {
        var settings = configuration.GetSection("JwtSettings");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role?.Name ?? string.Empty)
        };
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims), Expires = DateTime.UtcNow.AddMinutes(double.Parse(settings["ExpirationMinutes"]!)),
            Issuer = settings["Issuer"], Audience = settings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings["SecretKey"]!)), SecurityAlgorithms.HmacSha256Signature)
        };
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
