using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Server.Data;
using POS.Shared.DTOs;

namespace POS.Server.Controllers;

[ApiController, Route("api/traceability/lots"), Authorize(Roles = "Administrador,Almacén,Producción,Gerencia")]
public class TraceabilityController(AppDbContext db) : ControllerBase
{
    [HttpGet("{code}")]
    public async Task<ActionResult<LotTraceabilityResponse>> Get(string code)
    {
        var lot = await db.ProductionLots.AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.User).ThenInclude(x => x!.Role)
            .Include(x => x.Details).ThenInclude(x => x.RawMaterial)
            .Include(x => x.Details).ThenInclude(x => x.Origins).ThenInclude(x => x.ReceptionDetail).ThenInclude(x => x!.Reception).ThenInclude(x => x!.Supplier)
            .Include(x => x.Details).ThenInclude(x => x.Origins).ThenInclude(x => x.ReceptionDetail).ThenInclude(x => x!.Reception).ThenInclude(x => x!.User)
            .FirstOrDefaultAsync(x => x.Code == code);
        if (lot is null) return NotFound();

        var dispatches = await db.DispatchDetails.AsNoTracking()
            .Where(x => x.ProductionLotId == lot.Id)
            .Include(x => x.Dispatch).ThenInclude(x => x!.User)
            .OrderBy(x => x.Dispatch!.Date)
            .Select(x => new LotDispatchResponse
            {
                DispatchId = x.DispatchId, Date = x.Dispatch!.Date,
                Destination = x.Dispatch.Destination, Observation = x.Dispatch.Observation,
                Status = x.Dispatch.Status, Quantity = x.Quantity,
                UserId = x.Dispatch.UserId, Username = x.Dispatch.User!.Username,
                ResponsibleFullName = (x.Dispatch.User.Name + " " + x.Dispatch.User.LastName).Trim()
            }).ToListAsync();

        var dispatchReferences = dispatches.Select(x => $"DESPACHO:{x.DispatchId}").ToList();
        var lotReference = $"LOTE:{lot.Code}";
        var movements = await db.InventoryMovements.AsNoTracking()
            .Include(x => x.Product).Include(x => x.User)
            .Where(x => x.Reference == lotReference || dispatchReferences.Contains(x.Reference))
            .OrderBy(x => x.Date)
            .Select(x => new InventoryMovementResponse
            {
                Id = x.Id, ProductId = x.ProductId, ProductName = x.Product!.Name,
                UserId = x.UserId, Username = x.User!.Username, MovementType = x.MovementType,
                Quantity = x.Quantity, Date = x.Date, Reference = x.Reference
            }).ToListAsync();

        var dispatchedQuantity = dispatches.Where(x => x.Status == "Confirmado").Sum(x => x.Quantity);
        return Ok(new LotTraceabilityResponse
        {
            Lot = new ProductionLotResponse
            {
                Id = lot.Id, ProductId = lot.ProductId, ProductName = lot.Product!.Name,
                ProductUnitOfMeasure = lot.Product.UnitOfMeasure, UserId = lot.UserId,
                Username = lot.User!.Username, Code = lot.Code, StartDate = lot.StartDate,
                EndDate = lot.EndDate, PlannedQuantity = lot.PlannedQuantity,
                ProducedQuantity = lot.ProducedQuantity, DispatchedQuantity = dispatchedQuantity,
                Status = lot.Status,
                Materials = lot.Details.Select(x => new ProductionLotMaterialResponse
                {
                    Id = x.Id, RawMaterialId = x.RawMaterialId,
                    RawMaterialName = x.RawMaterial!.Name,
                    UnitOfMeasure = x.RawMaterial.UnitOfMeasure, QuantityUsed = x.QuantityUsed,
                    Origins = x.Origins.Select(o => new ProductionLotMaterialOriginResponse
                    {
                        ReceptionId = o.ReceptionDetail!.ReceptionId,
                        ReceptionDetailId = o.ReceptionDetailId,
                        ReceptionDate = o.ReceptionDetail.Reception!.Date,
                        SupplierId = o.ReceptionDetail.Reception.SupplierId,
                        SupplierName = o.ReceptionDetail.Reception.Supplier!.Name,
                        SupplierNit = o.ReceptionDetail.Reception.Supplier.Nit,
                        ReceivedByUserId = o.ReceptionDetail.Reception.UserId,
                        ReceivedByUsername = o.ReceptionDetail.Reception.User!.Username,
                        ReceivedByFullName = $"{o.ReceptionDetail.Reception.User.Name} {o.ReceptionDetail.Reception.User.LastName}".Trim(),
                        Quantity = o.Quantity
                    }).ToList()
                }).ToList()
            },
            ProductionOrderedBy = new LotResponsibleResponse
            {
                UserId = lot.UserId, Username = lot.User.Username,
                FullName = $"{lot.User.Name} {lot.User.LastName}".Trim(),
                Role = lot.User.Role?.Name ?? string.Empty
            },
            Dispatches = dispatches,
            Movements = movements,
            HasUntracedMaterialOrigins = lot.Details.Any(x => x.Origins.Sum(o => o.Quantity) < x.QuantityUsed)
        });
    }
}
