using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Shared.Entities;

public class InventoryProduct
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(250)] public string Description { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string UnitOfMeasure { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int CurrentStock { get; set; }
    [Range(0, int.MaxValue)] public int MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ProductionLot> ProductionLots { get; set; } = new List<ProductionLot>();
    public ICollection<DispatchDetail> DispatchDetails { get; set; } = new List<DispatchDetail>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    public ICollection<ProductRecipeItem> Recipe { get; set; } = new List<ProductRecipeItem>();
}
