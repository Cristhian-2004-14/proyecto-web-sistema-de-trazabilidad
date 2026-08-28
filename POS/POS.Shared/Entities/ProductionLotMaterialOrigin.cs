using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Shared.Entities;

public class ProductionLotMaterialOrigin
{
    public int Id { get; set; }
    public int ProductionLotMaterialDetailId { get; set; }
    public ProductionLotMaterialDetail? ProductionLotMaterialDetail { get; set; }
    public int ReceptionDetailId { get; set; }
    public ReceptionDetail? ReceptionDetail { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Quantity { get; set; }
}
