using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Shared.Entities;

public class InventoryProduct
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(250)] public string Description { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string UnitOfMeasure { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)"), Range(0, double.MaxValue)] public decimal CurrentStock { get; set; }
    [Column(TypeName = "decimal(18,2)"), Range(0, double.MaxValue)] public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ProductionLot> ProductionLots { get; set; } = new List<ProductionLot>();
    public ICollection<DispatchDetail> DispatchDetails { get; set; } = new List<DispatchDetail>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
