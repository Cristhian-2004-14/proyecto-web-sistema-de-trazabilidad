using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Server.Repositories;
using POS.Server.Services;
using POS.Shared.DTOs;
using POS.Shared.Entities;

namespace POS.Server.Controllers;

[ApiController, Route("api/users"), Authorize(Roles = "Administrador")]
public class UsersController(IUserRepository users, IRoleRepository roles, IAuthService auth) : ControllerBase
{
    [HttpGet]
    public async Task<List<UserResponse>> Get() => (await users.GetAllAsync()).Select(ToResponse).ToList();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await users.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    [HttpPost] public async Task<ActionResult<AuthResponse>> Post(RegisterRequest request)
    {
        var role = await roles.GetByIdAsync(request.RoleId);
        if (role is null || !role.IsActive) return BadRequest(new { message = "El rol seleccionado no existe o está inactivo." });
        var result = await auth.RegisterAsync(request);
        return result is null ? Conflict(new { message = "El usuario o correo ya está registrado." }) : Ok(result);
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Put(int id, UserUpdateRequest request)
    {
        var user = await users.GetByIdAsync(id); if (user is null) return NotFound();
        var role = await roles.GetByIdAsync(request.RoleId);
        if (role is null || !role.IsActive) return BadRequest(new { message = "El rol seleccionado no existe o está inactivo." });
        if (await users.ExistsAsync(request.Username.Trim(), request.Email.Trim(), id)) return Conflict(new { message = "El usuario o correo ya está registrado." });
        user.Name = request.Name.Trim(); user.LastName = request.LastName.Trim(); user.Username = request.Username.Trim();
        user.Email = request.Email.Trim(); user.RoleId = request.RoleId; user.IsActive = request.IsActive;
        if (!string.IsNullOrWhiteSpace(request.NewPassword)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await users.SaveAsync(); return NoContent();
    }

    private static UserResponse ToResponse(User user) => new()
    {
        Id = user.Id, Name = user.Name, LastName = user.LastName, Username = user.Username,
        Email = user.Email, RoleId = user.RoleId, RoleName = user.Role?.Name ?? string.Empty, IsActive = user.IsActive
    };
}
