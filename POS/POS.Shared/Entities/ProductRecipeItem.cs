using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Shared.Entities;

public class ProductRecipeItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public InventoryProduct? Product { get; set; }
    public int RawMaterialId { get; set; }
    public RawMaterial? RawMaterial { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal QuantityPerUnit { get; set; }
}
