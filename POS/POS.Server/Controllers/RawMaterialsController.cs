using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Server.Repositories;
using POS.Shared.Entities;

namespace POS.Server.Controllers;

[ApiController, Route("api/raw-materials"), Authorize(Roles = "Administrador")]
public class RawMaterialsController(ICatalogRepository<RawMaterial> repository) : ControllerBase
{
    [HttpGet] public Task<List<RawMaterial>> Get() => repository.GetAllAsync();
    [HttpPost] public async Task<ActionResult<RawMaterial>> Post(RawMaterial item)
    {
        if (item.CurrentStock < 0 || item.MinimumStock < 0) return BadRequest(new { message = "El stock no puede ser negativo." });
        return Ok(await repository.AddAsync(item));
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Put(int id, RawMaterial item)
    {
        if (item.CurrentStock < 0 || item.MinimumStock < 0) return BadRequest(new { message = "El stock no puede ser negativo." });
        var x = await repository.GetAsync(id); if (x is null) return NotFound();
        x.Name = item.Name.Trim(); x.UnitOfMeasure = item.UnitOfMeasure.Trim(); x.CurrentStock = item.CurrentStock; x.MinimumStock = item.MinimumStock; x.IsActive = item.IsActive;
        await repository.SaveAsync(); return NoContent();
    }
}
