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
        if (request.StartDate == default) throw new BusinessValidationException("La fecha de inicio es obligatoria.");
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

        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            lot.Details = request.Items.Select(x => new ProductionLotMaterialDetail { RawMaterialId = x.RawMaterialId, QuantityUsed = x.Quantity }).ToList();
            await db.SaveChangesAsync();
            foreach (var item in request.Items) materials[item.RawMaterialId].CurrentStock -= item.Quantity;
            lot.Status = ProductionLotStatuses.InProgress;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return (await GetByIdAsync(lot.Id))!;
        }
        catch { await transaction.RollbackAsync(); throw; }
    }

    public async Task<ProductionLotResponse?> FinishAsync(int id, FinishProductionLotRequest request, int authenticatedUserId)
    {
        await ValidateUserAsync(authenticatedUserId);
        var lot = await Query().FirstOrDefaultAsync(x => x.Id == id);
        if (lot is null) return null;
        if (lot.Status == ProductionLotStatuses.Finished) return Map(lot);
        if (lot.Status != ProductionLotStatuses.InProgress) throw new BusinessValidationException("Solo un lote en proceso puede finalizarse.");
        if (request.ProducedQuantity <= 0) throw new BusinessValidationException("La cantidad producida debe ser mayor que cero.");
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
        .Include(x => x.Details).ThenInclude(x => x.RawMaterial);

    private static ProductionLotResponse Map(ProductionLot x) => new()
    {
        Id = x.Id, ProductId = x.ProductId, ProductName = x.Product?.Name ?? string.Empty,
        ProductUnitOfMeasure = x.Product?.UnitOfMeasure ?? string.Empty, UserId = x.UserId,
        Username = x.User?.Username ?? string.Empty, Code = x.Code, StartDate = x.StartDate,
        EndDate = x.EndDate, PlannedQuantity = x.PlannedQuantity, ProducedQuantity = x.ProducedQuantity,
        Status = x.Status, Materials = x.Details.Select(d => new ProductionLotMaterialResponse
        { Id = d.Id, RawMaterialId = d.RawMaterialId, RawMaterialName = d.RawMaterial?.Name ?? string.Empty,
          UnitOfMeasure = d.RawMaterial?.UnitOfMeasure ?? string.Empty, QuantityUsed = d.QuantityUsed }).ToList()
    };
}
