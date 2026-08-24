using Microsoft.EntityFrameworkCore;
using POS.Server.Data;
using POS.Shared.DTOs;
using POS.Shared.Entities;
namespace POS.Server.Services;
public class DispatchService(AppDbContext db):IDispatchService
{
 public async Task<List<DispatchResponse>>GetAllAsync()=>(await Query().AsNoTracking().OrderByDescending(x=>x.Date).ToListAsync()).Select(Map).ToList();
 public async Task<DispatchResponse?>GetByIdAsync(int id){var x=await Query().AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id);return x is null?null:Map(x);}
 public async Task<DispatchResponse>CreateAsync(CreateDispatchRequest r,int userId)
 {
  await ValidateUser(userId);Validate(r);var ids=r.Details.Select(x=>x.ProductId).ToList();var products=await db.InventoryProducts.Where(x=>ids.Contains(x.Id)).ToDictionaryAsync(x=>x.Id);ValidateProducts(r.Details,products);
  await using var tx=await db.Database.BeginTransactionAsync();try{
   var d=new Dispatch{UserId=userId,Date=r.Date,Destination=r.Destination.Trim(),Observation=r.Observation.Trim(),Status=DispatchStatuses.Pending,Details=r.Details.Select(x=>new DispatchDetail{ProductId=x.ProductId,Quantity=x.Quantity}).ToList()};db.Dispatches.Add(d);await db.SaveChangesAsync();
   ApplyConfirmation(d,r.Details,products,userId);await db.SaveChangesAsync();await tx.CommitAsync();return(await GetByIdAsync(d.Id))!;
  }catch{await tx.RollbackAsync();throw;}
 }
 public async Task<DispatchResponse?>ConfirmAsync(int id,int userId)
 {
  await ValidateUser(userId);var d=await Query().FirstOrDefaultAsync(x=>x.Id==id);if(d is null)return null;if(d.Status==DispatchStatuses.Confirmed)return Map(d);if(d.Status!=DispatchStatuses.Pending)throw new BusinessValidationException("El despacho no puede confirmarse.");
  var products=d.Details.ToDictionary(x=>x.ProductId,x=>x.Product!);ValidateProducts(d.Details.Select(x=>new DispatchDetailRequest{ProductId=x.ProductId,Quantity=x.Quantity}).ToList(),products);
  await using var tx=await db.Database.BeginTransactionAsync();try{ApplyConfirmation(d,d.Details.Select(x=>new DispatchDetailRequest{ProductId=x.ProductId,Quantity=x.Quantity}).ToList(),products,userId);await db.SaveChangesAsync();await tx.CommitAsync();return Map(d);}catch{await tx.RollbackAsync();throw;}
 }
 private void ApplyConfirmation(Dispatch d,List<DispatchDetailRequest> details,Dictionary<int,InventoryProduct> products,int userId){foreach(var x in details){var p=products[x.ProductId];p.CurrentStock-=x.Quantity;db.InventoryMovements.Add(new(){ProductId=p.Id,UserId=userId,MovementType=InventoryMovementTypes.DispatchExit,Quantity=x.Quantity,Date=d.Date,Reference=$"DESPACHO:{d.Id}"});}d.Status=DispatchStatuses.Confirmed;}
 private async Task ValidateUser(int id){var u=await db.Users.Include(x=>x.Role).FirstOrDefaultAsync(x=>x.Id==id);if(u is null||!u.IsActive||u.Role is null||!u.Role.IsActive)throw new BusinessValidationException("El usuario autenticado no es válido o está inactivo.");}
 private static void Validate(CreateDispatchRequest r){if(r.Date==default)throw new BusinessValidationException("La fecha es obligatoria.");if(string.IsNullOrWhiteSpace(r.Destination))throw new BusinessValidationException("El destino es obligatorio.");if(r.Details.Count==0)throw new BusinessValidationException("Debe agregar al menos un producto.");if(r.Details.Any(x=>x.Quantity<=0))throw new BusinessValidationException("Las cantidades deben ser mayores que cero.");if(r.Details.GroupBy(x=>x.ProductId).Any(x=>x.Count()>1))throw new BusinessValidationException("No se permiten productos duplicados.");}
 private static void ValidateProducts(List<DispatchDetailRequest> details,Dictionary<int,InventoryProduct> products){foreach(var x in details){if(!products.TryGetValue(x.ProductId,out var p))throw new BusinessValidationException($"El producto {x.ProductId} no existe.");if(!p.IsActive)throw new BusinessValidationException($"El producto {p.Name} está inactivo.");if(x.Quantity>p.CurrentStock)throw new BusinessValidationException($"Stock insuficiente para {p.Name}.");}}
 private IQueryable<Dispatch>Query()=>db.Dispatches.Include(x=>x.User).Include(x=>x.Details).ThenInclude(x=>x.Product);
 private static DispatchResponse Map(Dispatch x)=>new(){Id=x.Id,UserId=x.UserId,Username=x.User?.Username??"",Date=x.Date,Destination=x.Destination,Observation=x.Observation,Status=x.Status,Details=x.Details.Select(d=>new DispatchDetailResponse{Id=d.Id,ProductId=d.ProductId,ProductName=d.Product?.Name??"",UnitOfMeasure=d.Product?.UnitOfMeasure??"",Quantity=d.Quantity}).ToList()};
}
