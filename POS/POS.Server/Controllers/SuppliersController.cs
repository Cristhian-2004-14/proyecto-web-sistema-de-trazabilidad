using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Server.Repositories;
using POS.Shared.Entities;

namespace POS.Server.Controllers;

[ApiController, Route("api/suppliers"), Authorize]
public class SuppliersController(ICatalogRepository<Supplier> repository) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Administrador,Almacén")] public Task<List<Supplier>> Get() => repository.GetAllAsync();
    [HttpPost, Authorize(Roles = "Administrador")] public async Task<ActionResult<Supplier>> Post(Supplier item) => Ok(await repository.AddAsync(item));
    [HttpPut("{id:int}"), Authorize(Roles = "Administrador")] public async Task<IActionResult> Put(int id, Supplier item)
    {
        var x = await repository.GetAsync(id); if (x is null) return NotFound();
        x.Name = item.Name.Trim(); x.Nit = item.Nit.Trim(); x.Phone = item.Phone.Trim(); x.Address = item.Address.Trim(); x.IsActive = item.IsActive;
        await repository.SaveAsync(); return NoContent();
    }
}
