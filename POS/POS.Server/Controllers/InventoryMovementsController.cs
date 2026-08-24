using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;using POS.Server.Data;using POS.Shared.DTOs;
namespace POS.Server.Controllers;
[ApiController,Route("api/inventory/movements"),Authorize(Roles="Administrador,Almacén,Producción,Gerencia")]public class InventoryMovementsController(AppDbContext db):ControllerBase
{
 [HttpGet]public async Task<List<InventoryMovementResponse>>Get(int? productId,string? type,DateTime? from,DateTime? to,int? userId){var q=db.InventoryMovements.AsNoTracking().Include(x=>x.Product).Include(x=>x.User).AsQueryable();if(productId.HasValue)q=q.Where(x=>x.ProductId==productId);if(!string.IsNullOrWhiteSpace(type))q=q.Where(x=>x.MovementType==type);if(from.HasValue)q=q.Where(x=>x.Date>=from);if(to.HasValue)q=q.Where(x=>x.Date<to.Value.Date.AddDays(1));if(userId.HasValue)q=q.Where(x=>x.UserId==userId);return await q.OrderByDescending(x=>x.Date).Select(x=>new InventoryMovementResponse{Id=x.Id,ProductId=x.ProductId,ProductName=x.Product!.Name,UserId=x.UserId,Username=x.User!.Username,MovementType=x.MovementType,Quantity=x.Quantity,Date=x.Date,Reference=x.Reference}).ToListAsync();}
}
