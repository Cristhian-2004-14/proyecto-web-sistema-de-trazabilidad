using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Server.Data;
using POS.Shared.DTOs;

namespace POS.Server.Controllers;

[ApiController, Route("api/inventory/raw-materials"), Authorize(Roles = "Administrador,Almacén,Producción,Gerencia")]
public class InventoryController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<RawMaterialInventoryResponse>> Get([FromQuery] string? search = null, [FromQuery] bool? isActive = null)
    {
        var query = db.RawMaterials.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search.Trim()));
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        return await query.OrderBy(x => x.Name).Select(x => new RawMaterialInventoryResponse
        {
            Id = x.Id, Name = x.Name, UnitOfMeasure = x.UnitOfMeasure,
            CurrentStock = x.CurrentStock, MinimumStock = x.MinimumStock, IsActive = x.IsActive
        }).ToListAsync();
    }
}
