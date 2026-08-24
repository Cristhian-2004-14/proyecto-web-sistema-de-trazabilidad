using System.ComponentModel.DataAnnotations;

namespace POS.Shared.DTOs;

public class ReceptionDetailRequest
{
    [Range(1, int.MaxValue)] public int RawMaterialId { get; set; }
    [Range(0.01, double.MaxValue)] public decimal Quantity { get; set; }
}

public class CreateReceptionRequest : IValidatableObject
{
    [Range(1, int.MaxValue)] public int SupplierId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    [MaxLength(300)] public string Observation { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Status { get; set; } = "Confirmada";
    [MinLength(1)] public List<ReceptionDetailRequest> Details { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Date == default) yield return new ValidationResult("La fecha es obligatoria.", [nameof(Date)]);
        if (Details.Count == 0) yield return new ValidationResult("Debe agregar al menos una materia prima.", [nameof(Details)]);
        if (Details.GroupBy(x => x.RawMaterialId).Any(x => x.Count() > 1))
            yield return new ValidationResult("No se permiten materias primas duplicadas.", [nameof(Details)]);
    }
}

public class ReceptionDetailResponse
{
    public int Id { get; set; }
    public int RawMaterialId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class ReceptionResponse
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Observation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ReceptionDetailResponse> Details { get; set; } = [];
}
