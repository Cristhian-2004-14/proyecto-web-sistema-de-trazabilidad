using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Shared.Entities;

public class ProductionLotMaterialDetail
{
    public int Id { get; set; }
    public int ProductionLotId { get; set; }
    public ProductionLot? ProductionLot { get; set; }
    public int RawMaterialId { get; set; }
    public RawMaterial? RawMaterial { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal QuantityUsed { get; set; }
    public ICollection<ProductionLotMaterialOrigin> Origins { get; set; } = new List<ProductionLotMaterialOrigin>();
}
