using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Server.Data;
using POS.Shared.DTOs;
using POS.Shared.Entities;

namespace POS.Server.Controllers;

[ApiController, Route("api/products"), Authorize]
public class ProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Administrador,Almacén,Producción,Gerencia")]
    public async Task<List<ProductResponse>> Get([FromQuery] string? search = null)
    {
        var query = db.InventoryProducts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search.Trim()));
        return await query.OrderBy(x => x.Name).Select(x => new ProductResponse
        {
            Id=x.Id, Name=x.Name, Description=x.Description, UnitOfMeasure=x.UnitOfMeasure,
            CurrentStock=x.CurrentStock, MinimumStock=x.MinimumStock, IsActive=x.IsActive
        }).ToListAsync();
    }

    [HttpGet("{id:int}"), Authorize(Roles = "Administrador,Almacén,Producción,Gerencia")]
    public async Task<ActionResult<ProductResponse>> Get(int id)
    {
        var item = await db.InventoryProducts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : Ok(Map(item));
    }

    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ProductResponse>> Post(ProductRequest request)
    {
        if (request.CurrentStock < 0 || request.MinimumStock < 0) return BadRequest(new { message = "El stock no puede ser negativo." });
        var item = new InventoryProduct(); Apply(item, request); db.InventoryProducts.Add(item); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = item.Id }, Map(item));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Put(int id, ProductRequest request)
    {
        if (request.CurrentStock < 0 || request.MinimumStock < 0) return BadRequest(new { message = "El stock no puede ser negativo." });
        var item = await db.InventoryProducts.FindAsync(id); if (item is null) return NotFound();
        Apply(item, request); await db.SaveChangesAsync(); return NoContent();
    }

    [HttpPatch("{id:int}/status"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Status(int id, ProductStatusRequest request)
    {
        var item = await db.InventoryProducts.FindAsync(id); if (item is null) return NotFound();
        item.IsActive = request.IsActive; await db.SaveChangesAsync(); return NoContent();
    }

    private static void Apply(InventoryProduct item, ProductRequest request)
    {
        item.Name = request.Name.Trim(); item.Description = request.Description.Trim();
        item.UnitOfMeasure = request.UnitOfMeasure.Trim(); item.CurrentStock = request.CurrentStock;
        item.MinimumStock = request.MinimumStock; item.IsActive = request.IsActive;
    }
    private static ProductResponse Map(InventoryProduct x) => new()
    {
        Id=x.Id, Name=x.Name, Description=x.Description, UnitOfMeasure=x.UnitOfMeasure,
        CurrentStock=x.CurrentStock, MinimumStock=x.MinimumStock, IsActive=x.IsActive
    };
}
