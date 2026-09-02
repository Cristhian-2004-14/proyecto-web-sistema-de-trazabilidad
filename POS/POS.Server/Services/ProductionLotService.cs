using Microsoft.EntityFrameworkCore;
using POS.Server.Data;
using POS.Shared.DTOs;
using POS.Shared.Entities;

namespace POS.Server.Services;

public class ProductionLotService(AppDbContext db) : IProductionLotService
{
    public async Task<List<ProductionLotResponse>> GetAllAsync() =>
        (await Query().AsNoTracking().OrderByDescending(x => x.StartDate).ToListAsync()).Select(Map).ToList();

    public async Task<ProductionLotResponse?> GetByIdAsync(int id)
    {
        var lot = await Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return lot is null ? null : Map(lot);
    }

    public async Task<ProductionLotResponse> CreateAsync(CreateProductionLotRequest request, int authenticatedUserId)
    {
        await ValidateUserAsync(authenticatedUserId);
        if (request.ProductId <= 0) throw new BusinessValidationException("El producto es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Code)) throw new BusinessValidationException("El código de lote es obligatorio.");
        if (request.StartDate == default) throw new BusinessValidationException("La fecha programada es obligatoria.");
        if (request.StartDate.Date < DateTime.Today) throw new BusinessValidationException("La fecha programada no puede estar en el pasado.");
        if (request.PlannedQuantity <= 0) throw new BusinessValidationException("La cantidad planificada debe ser mayor que cero.");
        var code = request.Code.Trim();
        if (await db.ProductionLots.AnyAsync(x => x.Code == code)) throw new BusinessValidationException("El código de lote ya existe.");
        var product = await db.InventoryProducts.FindAsync(request.ProductId);
        if (product is null) throw new BusinessValidationException("El producto no existe.");
        if (!product.IsActive) throw new BusinessValidationException("El producto está inactivo.");
        var lot = new ProductionLot { ProductId = product.Id, UserId = authenticatedUserId, Code = code,
            StartDate = request.StartDate, PlannedQuantity = request.PlannedQuantity, ProducedQuantity = 0,
            Status = ProductionLotStatuses.Pending };
        db.ProductionLots.Add(lot);
        await db.SaveChangesAsync();
        return (await GetByIdAsync(lot.Id))!;
    }

    public async Task<ProductionLotResponse?> ConsumeAsync(int id, MaterialConsumptionRequest request, int authenticatedUserId)
    {
        await ValidateUserAsync(authenticatedUserId);
        var lot = await Query().FirstOrDefaultAsync(x => x.Id == id);
        if (lot is null) return null;
        if (lot.Status == ProductionLotStatuses.InProgress || lot.Status == ProductionLotStatuses.Finished) return Map(lot);
        if (lot.Status != ProductionLotStatuses.Pending) throw new BusinessValidationException("El estado del lote no permite registrar consumo.");
        if (request.Items.Count == 0) throw new BusinessValidationException("Debe agregar al menos una materia prima.");
        if (request.Items.Any(x => x.Quantity <= 0)) throw new BusinessValidationException("Las cantidades deben ser mayores que cero.");
        if (request.Items.GroupBy(x => x.RawMaterialId).Any(x => x.Count() > 1)) throw new BusinessValidationException("No se permiten materias primas duplicadas.");

        var ids = request.Items.Select(x => x.RawMaterialId).ToList();
        var materials = await db.RawMaterials.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        foreach (var item in request.Items)
        {
            if (!materials.TryGetValue(item.RawMaterialId, out var material)) throw new BusinessValidationException($"La materia prima {item.RawMaterialId} no existe.");
            if (!material.IsActive) throw new BusinessValidationException($"La materia prima {material.Name} está inactiva.");
            if (item.Quantity > material.CurrentStock) throw new BusinessValidationException($"Stock insuficiente para {material.Name}.");
        }

        var receptionDetails = await db.ReceptionDetails
            .Include(x => x.Reception).ThenInclude(x => x!.Supplier)
            .Include(x => x.Reception).ThenInclude(x => x!.User)
            .Include(x => x.ProductionOrigins)
            .Where(x => ids.Contains(x.RawMaterialId) && x.Reception!.Status == ReceptionStatuses.Confirmed)
            .OrderBy(x => x.Reception!.Date).ThenBy(x => x.Id)
            .ToListAsync();
        foreach (var item in request.Items)
        {
            var traceableAvailable = receptionDetails.Where(x => x.RawMaterialId == item.RawMaterialId)
                .Sum(x => x.Quantity - x.ProductionOrigins.Sum(o => o.Quantity));
            if (traceableAvailable < item.Quantity)
                throw new BusinessValidationException($"La materia prima {materials[item.RawMaterialId].Name} tiene stock físico, pero no suficiente stock asociado a recepciones confirmadas. Registre una recepción para mantener la trazabilidad.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            lot.Details = request.Items.Select(item =>
            {
                var remaining = item.Quantity;
                var detail = new ProductionLotMaterialDetail { RawMaterialId = item.RawMaterialId, QuantityUsed = item.Quantity };
                foreach (var source in receptionDetails.Where(x => x.RawMaterialId == item.RawMaterialId))
                {
                    var available = source.Quantity - source.ProductionOrigins.Sum(x => x.Quantity);
                    if (available <= 0) continue;
                    var assigned = Math.Min(remaining, available);
                    detail.Origins.Add(new ProductionLotMaterialOrigin { ReceptionDetailId = source.Id, Quantity = assigned });
                    remaining -= assigned;
                    if (remaining == 0) break;
                }
                return detail;
            }).ToList();
            await db.SaveChangesAsync();
            foreach (var item in request.Items) materials[item.RawMaterialId].CurrentStock -= item.Quantity;
            lot.Status = ProductionLotStatuses.InProgress;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (await GetByIdAsync(lot.Id))!;
        }
        catch { await transaction.RollbackAsync(); throw; }
    }

    public async Task<ProductionLotResponse?> StartAsync(int id, int authenticatedUserId)
    {
        await ValidateUserAsync(authenticatedUserId);
        var lot = await db.ProductionLots.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (lot is null) return null;
        if (lot.Status == ProductionLotStatuses.InProgress || lot.Status == ProductionLotStatuses.Finished)
            return await GetByIdAsync(id);
        if (lot.StartDate.Date > DateTime.Today)
            throw new BusinessValidationException($"Esta producción está programada para el {lot.StartDate:dd/MM/yyyy} y todavía no puede iniciarse.");
        var recipe = await db.ProductRecipeItems.AsNoTracking().Include(x => x.RawMaterial)
            .Where(x => x.ProductId == lot.ProductId).ToListAsync();
        if (recipe.Count == 0) throw new BusinessValidationException("El producto no tiene una receta configurada.");
        if (recipe.Any(x => x.RawMaterial is null || !x.RawMaterial.IsActive))
            throw new BusinessValidationException("La receta contiene una materia prima inexistente o inactiva.");
        var request = new MaterialConsumptionRequest
        {
            Items = recipe.Select(x => new MaterialConsumptionItemRequest
            {
                RawMaterialId = x.RawMaterialId,
                Quantity = x.QuantityPerUnit * lot.PlannedQuantity
            }).ToList()
        };
        return await ConsumeAsync(id, request, authenticatedUserId);
    }

    public async Task<ProductionLotResponse?> FinishAsync(int id, FinishProductionLotRequest request, int authenticatedUserId)
    {
        await ValidateUserAsync(authenticatedUserId);
        var lot = await Query().FirstOrDefaultAsync(x => x.Id == id);
        if (lot is null) return null;
        if (lot.Status == ProductionLotStatuses.Finished) return Map(lot);
        if (lot.Status != ProductionLotStatuses.InProgress) throw new BusinessValidationException("Solo un lote en proceso puede finalizarse.");
        if (request.ProducedQuantity <= 0) throw new BusinessValidationException("La cantidad producida debe ser un número entero mayor que cero.");
        if (lot.Product is null || !lot.Product.IsActive) throw new BusinessValidationException("El producto no existe o está inactivo.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            lot.ProducedQuantity = request.ProducedQuantity;
            lot.EndDate = DateTime.Now;
            lot.Status = ProductionLotStatuses.Finished;
            await db.SaveChangesAsync();
            lot.Product.CurrentStock += request.ProducedQuantity;
            db.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = lot.ProductId, UserId = authenticatedUserId,
                MovementType = InventoryMovementTypes.ProductionEntry,
                Quantity = request.ProducedQuantity, Date = lot.EndDate.Value,
                Reference = $"LOTE:{lot.Code}"
            });
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (await GetByIdAsync(lot.Id))!;
        }
        catch { await transaction.RollbackAsync(); throw; }
    }

    private async Task ValidateUserAsync(int id)
    {
        var user = await db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
        if (user is null || !user.IsActive || user.Role is null || !user.Role.IsActive)
            throw new BusinessValidationException("El usuario autenticado no es válido o está inactivo.");
    }

    private IQueryable<ProductionLot> Query() => db.ProductionLots.Include(x => x.Product).Include(x => x.User)
        .Include(x => x.Details).ThenInclude(x => x.RawMaterial)
        .Include(x => x.Details).ThenInclude(x => x.Origins).ThenInclude(x => x.ReceptionDetail).ThenInclude(x => x!.Reception).ThenInclude(x => x!.Supplier)
        .Include(x => x.Details).ThenInclude(x => x.Origins).ThenInclude(x => x.ReceptionDetail).ThenInclude(x => x!.Reception).ThenInclude(x => x!.User)
        .Include(x => x.DispatchDetails).ThenInclude(x => x.Dispatch);

    private static ProductionLotResponse Map(ProductionLot x) => new()
    {
        Id = x.Id, ProductId = x.ProductId, ProductName = x.Product?.Name ?? string.Empty,
        ProductUnitOfMeasure = x.Product?.UnitOfMeasure ?? string.Empty, UserId = x.UserId,
        Username = x.User?.Username ?? string.Empty, Code = x.Code, StartDate = x.StartDate,
        EndDate = x.EndDate, PlannedQuantity = x.PlannedQuantity, ProducedQuantity = x.ProducedQuantity,
        DispatchedQuantity = x.DispatchDetails.Where(d => d.Dispatch?.Status == DispatchStatuses.Confirmed).Sum(d => d.Quantity),
        Status = x.Status, Materials = x.Details.Select(d => new ProductionLotMaterialResponse
        { Id = d.Id, RawMaterialId = d.RawMaterialId, RawMaterialName = d.RawMaterial?.Name ?? string.Empty,
          UnitOfMeasure = d.RawMaterial?.UnitOfMeasure ?? string.Empty, QuantityUsed = d.QuantityUsed,
          Origins = d.Origins.Select(o => new ProductionLotMaterialOriginResponse
          {
              ReceptionId = o.ReceptionDetail?.ReceptionId ?? 0, ReceptionDetailId = o.ReceptionDetailId,
              ReceptionDate = o.ReceptionDetail?.Reception?.Date ?? default,
              SupplierId = o.ReceptionDetail?.Reception?.SupplierId ?? 0,
              SupplierName = o.ReceptionDetail?.Reception?.Supplier?.Name ?? string.Empty,
              SupplierNit = o.ReceptionDetail?.Reception?.Supplier?.Nit ?? string.Empty,
              ReceivedByUserId = o.ReceptionDetail?.Reception?.UserId ?? 0,
              ReceivedByUsername = o.ReceptionDetail?.Reception?.User?.Username ?? string.Empty,
              ReceivedByFullName = $"{o.ReceptionDetail?.Reception?.User?.Name} {o.ReceptionDetail?.Reception?.User?.LastName}".Trim(),
              Quantity = o.Quantity
          }).ToList() }).ToList()
    };
}
