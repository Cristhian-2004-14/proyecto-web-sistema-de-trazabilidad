using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Shared.Entities;

public static class ProductionLotStatuses
{
    public const string Pending = "Pendiente";
    public const string InProgress = "EnProceso";
    public const string Finished = "Finalizado";
}

public class ProductionLot
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public InventoryProduct? Product { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    [Required, MaxLength(50)] public string Code { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PlannedQuantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ProducedQuantity { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = ProductionLotStatuses.Pending;
    public ICollection<ProductionLotMaterialDetail> Details { get; set; } = new List<ProductionLotMaterialDetail>();
}
