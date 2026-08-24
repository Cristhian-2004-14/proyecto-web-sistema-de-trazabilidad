using System.ComponentModel.DataAnnotations;

namespace POS.Shared.DTOs;

public class CreateProductionLotRequest
{
    [Range(1, int.MaxValue)] public int ProductId { get; set; }
    [Required, MaxLength(50)] public string Code { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.Today;
    [Range(0.01, double.MaxValue)] public decimal PlannedQuantity { get; set; }
}

public class MaterialConsumptionItemRequest
{
    [Range(1, int.MaxValue)] public int RawMaterialId { get; set; }
    [Range(0.01, double.MaxValue)] public decimal Quantity { get; set; }
}

public class MaterialConsumptionRequest : IValidatableObject
{
    [MinLength(1)] public List<MaterialConsumptionItemRequest> Items { get; set; } = [];
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Items.Count == 0) yield return new("Debe agregar al menos una materia prima.", [nameof(Items)]);
        if (Items.GroupBy(x => x.RawMaterialId).Any(x => x.Count() > 1))
            yield return new("No se permiten materias primas duplicadas.", [nameof(Items)]);
    }
}

public class FinishProductionLotRequest
{
    [Range(0.01, double.MaxValue)] public decimal ProducedQuantity { get; set; }
}

public class ProductionLotMaterialResponse
{
    public int Id { get; set; }
    public int RawMaterialId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal QuantityUsed { get; set; }
}

public class ProductionLotResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductUnitOfMeasure { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal ProducedQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ProductionLotMaterialResponse> Materials { get; set; } = [];
}
