using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Server.Repositories;
using POS.Shared.DTOs;
using POS.Shared.Entities;

namespace POS.Server.Controllers;

[ApiController, Route("api/roles"), Authorize(Roles = "Administrador")]
public class RolesController(IRoleRepository repository) : ControllerBase
{
    [HttpGet] public Task<List<RoleEntity>> Get() => repository.GetAllAsync();
    [HttpGet("active")] public Task<List<RoleEntity>> GetActive() => repository.GetActiveAsync();
    [HttpPost] public async Task<ActionResult<RoleEntity>> Post(RoleEntity item)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        item.Name = item.Name.Trim();
        return Ok(await repository.AddAsync(item));
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Put(int id, RoleEntity item)
    {
        var current = await repository.GetByIdAsync(id); if (current is null) return NotFound();
        current.Name = item.Name.Trim(); current.Description = item.Description.Trim(); current.IsActive = item.IsActive;
        await repository.SaveAsync(); return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, RoleStatusRequest request)
    {
        var role = await repository.GetByIdAsync(id); if (role is null) return NotFound();
        role.IsActive = request.IsActive; await repository.SaveAsync(); return NoContent();
    }
}
