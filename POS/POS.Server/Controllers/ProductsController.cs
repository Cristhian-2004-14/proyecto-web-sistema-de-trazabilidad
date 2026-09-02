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
        var query = db.InventoryProducts.AsNoTracking().Include(x => x.Recipe).ThenInclude(x => x.RawMaterial).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search.Trim()));
        return (await query.OrderBy(x => x.Name).ToListAsync()).Select(Map).ToList();
    }

    [HttpGet("{id:int}"), Authorize(Roles = "Administrador,Almacén,Producción,Gerencia")]
    public async Task<ActionResult<ProductResponse>> Get(int id)
    {
        var item = await db.InventoryProducts.AsNoTracking().Include(x => x.Recipe).ThenInclude(x => x.RawMaterial).FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : Ok(Map(item));
    }

    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<ActionResult<ProductResponse>> Post(ProductRequest request)
    {
        var validation = await ValidateRecipe(request); if (validation is not null) return BadRequest(new { message = validation });
        var item = new InventoryProduct(); Apply(item, request);
        item.Recipe = request.Recipe.Select(x => new ProductRecipeItem { RawMaterialId=x.RawMaterialId, QuantityPerUnit=x.QuantityPerUnit }).ToList();
        db.InventoryProducts.Add(item); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = item.Id }, await LoadResponse(item.Id));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Put(int id, ProductRequest request)
    {
        var validation = await ValidateRecipe(request); if (validation is not null) return BadRequest(new { message = validation });
        var item = await db.InventoryProducts.Include(x=>x.Recipe).FirstOrDefaultAsync(x=>x.Id==id); if (item is null) return NotFound();
        Apply(item, request); db.ProductRecipeItems.RemoveRange(item.Recipe);
        item.Recipe = request.Recipe.Select(x => new ProductRecipeItem { RawMaterialId=x.RawMaterialId, QuantityPerUnit=x.QuantityPerUnit }).ToList();
        await db.SaveChangesAsync(); return NoContent();
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
        CurrentStock=x.CurrentStock, MinimumStock=x.MinimumStock, IsActive=x.IsActive,
        Recipe=x.Recipe.OrderBy(r=>r.RawMaterial?.Name).Select(MapRecipe).ToList()
    };
    private static ProductRecipeItemResponse MapRecipe(ProductRecipeItem x) => new()
    {
        RawMaterialId=x.RawMaterialId, RawMaterialName=x.RawMaterial?.Name??string.Empty,
        UnitOfMeasure=x.RawMaterial?.UnitOfMeasure??string.Empty, QuantityPerUnit=x.QuantityPerUnit
    };
    private async Task<ProductResponse> LoadResponse(int id) => Map(await db.InventoryProducts.AsNoTracking()
        .Include(x=>x.Recipe).ThenInclude(x=>x.RawMaterial).SingleAsync(x=>x.Id==id));
    private async Task<string?> ValidateRecipe(ProductRequest request)
    {
        if(request.CurrentStock<0||request.MinimumStock<0) return "El stock no puede ser negativo.";
        if(request.Recipe.Count==0) return "Debe agregar al menos una materia prima a la receta.";
        if(request.Recipe.Any(x=>x.RawMaterialId<=0||x.QuantityPerUnit<=0)) return "Cada ingrediente debe tener una materia prima y una cantidad por unidad mayor que cero.";
        if(request.Recipe.GroupBy(x=>x.RawMaterialId).Any(x=>x.Count()>1)) return "No se permiten materias primas duplicadas en la receta.";
        var ids=request.Recipe.Select(x=>x.RawMaterialId).ToList();
        var count=await db.RawMaterials.CountAsync(x=>ids.Contains(x.Id)&&x.IsActive);
        return count==ids.Count?null:"La receta contiene materias primas inexistentes o inactivas.";
    }
}
