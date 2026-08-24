using Microsoft.EntityFrameworkCore;
using POS.Server.Data;
using POS.Shared.DTOs;
using POS.Shared.Entities;

namespace POS.Server.Services;

public class ReceptionService(AppDbContext db) : IReceptionService
{
    public async Task<List<ReceptionResponse>> GetAllAsync() =>
        (await Query().AsNoTracking().OrderByDescending(x => x.Date).ToListAsync()).Select(Map).ToList();

    public async Task<ReceptionResponse?> GetByIdAsync(int id)
    {
        var entity = await Query().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return entity is null ? null : Map(entity);
    }

    public async Task<ReceptionResponse> CreateAsync(CreateReceptionRequest request, int authenticatedUserId)
    {
        ValidateRequest(request);
        var user = await db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == authenticatedUserId);
        if (user is null || !user.IsActive || user.Role is null || !user.Role.IsActive)
            throw new BusinessValidationException("El usuario autenticado no es válido o está inactivo.");
        var supplier = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == request.SupplierId);
        if (supplier is null) throw new BusinessValidationException("El proveedor no existe.");
        if (!supplier.IsActive) throw new BusinessValidationException("El proveedor está inactivo.");

        var ids = request.Details.Select(x => x.RawMaterialId).Distinct().ToList();
        var materials = await db.RawMaterials.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        ValidateMaterials(request.Details, materials);

        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var reception = new Reception
            {
                SupplierId = request.SupplierId,
                UserId = authenticatedUserId,
                Date = request.Date,
                Observation = request.Observation.Trim(),
                Status = request.Status,
                Details = request.Details.Select(x => new ReceptionDetail { RawMaterialId = x.RawMaterialId, Quantity = x.Quantity }).ToList()
            };
            db.Receptions.Add(reception);
            await db.SaveChangesAsync();

            if (reception.Status == ReceptionStatuses.Confirmed)
            {
                foreach (var detail in request.Details) materials[detail.RawMaterialId].CurrentStock += detail.Quantity;
                await db.SaveChangesAsync();
            }
            await transaction.CommitAsync();
            return (await GetByIdAsync(reception.Id))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ReceptionResponse?> ConfirmAsync(int id, int authenticatedUserId)
    {
        var user = await db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == authenticatedUserId);
        if (user is null || !user.IsActive || user.Role is null || !user.Role.IsActive)
            throw new BusinessValidationException("El usuario autenticado no es válido o está inactivo.");
        var reception = await Query().FirstOrDefaultAsync(x => x.Id == id);
        if (reception is null) return null;
        if (reception.Status == ReceptionStatuses.Confirmed) return Map(reception);
        if (reception.Status != ReceptionStatuses.Pending)
            throw new BusinessValidationException("Solo una recepción pendiente puede confirmarse.");
        if (reception.Supplier is null || !reception.Supplier.IsActive)
            throw new BusinessValidationException("El proveedor está inactivo.");
        if (reception.Details.Count == 0) throw new BusinessValidationException("La recepción no contiene detalles.");
        foreach (var detail in reception.Details)
        {
            if (detail.Quantity <= 0 || detail.RawMaterial is null || !detail.RawMaterial.IsActive)
                throw new BusinessValidationException("La recepción contiene una materia prima inválida o inactiva.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var detail in reception.Details) detail.RawMaterial!.CurrentStock += detail.Quantity;
            reception.Status = ReceptionStatuses.Confirmed;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return Map(reception);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private IQueryable<Reception> Query() => db.Receptions
        .Include(x => x.Supplier).Include(x => x.User)
        .Include(x => x.Details).ThenInclude(x => x.RawMaterial);

    private static void ValidateRequest(CreateReceptionRequest request)
    {
        if (request.SupplierId <= 0) throw new BusinessValidationException("El proveedor es obligatorio.");
        if (request.Date == default) throw new BusinessValidationException("La fecha es obligatoria.");
        if (request.Details.Count == 0) throw new BusinessValidationException("Debe agregar al menos una materia prima.");
        if (request.Details.Any(x => x.Quantity <= 0)) throw new BusinessValidationException("Las cantidades deben ser mayores que cero.");
        if (request.Details.GroupBy(x => x.RawMaterialId).Any(x => x.Count() > 1))
            throw new BusinessValidationException("No se permiten materias primas duplicadas.");
        if (request.Status is not ReceptionStatuses.Pending and not ReceptionStatuses.Confirmed)
            throw new BusinessValidationException("El estado de la recepción no es válido.");
    }

    private static void ValidateMaterials(List<ReceptionDetailRequest> details, Dictionary<int, RawMaterial> materials)
    {
        foreach (var detail in details)
        {
            if (!materials.TryGetValue(detail.RawMaterialId, out var material))
                throw new BusinessValidationException($"La materia prima {detail.RawMaterialId} no existe.");
            if (!material.IsActive) throw new BusinessValidationException($"La materia prima {material.Name} está inactiva.");
        }
    }

    private static ReceptionResponse Map(Reception x) => new()
    {
        Id = x.Id, SupplierId = x.SupplierId, SupplierName = x.Supplier?.Name ?? string.Empty,
        UserId = x.UserId, Username = x.User?.Username ?? string.Empty, Date = x.Date,
        Observation = x.Observation, Status = x.Status,
        Details = x.Details.Select(d => new ReceptionDetailResponse
        {
            Id = d.Id, RawMaterialId = d.RawMaterialId, RawMaterialName = d.RawMaterial?.Name ?? string.Empty,
            UnitOfMeasure = d.RawMaterial?.UnitOfMeasure ?? string.Empty, Quantity = d.Quantity
        }).ToList()
    };
}
