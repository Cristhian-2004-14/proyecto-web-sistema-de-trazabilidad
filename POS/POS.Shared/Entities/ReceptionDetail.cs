using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Shared.Entities;

public class ReceptionDetail
{
    public int Id { get; set; }
    public int ReceptionId { get; set; }
    public Reception? Reception { get; set; }
    public int RawMaterialId { get; set; }
    public RawMaterial? RawMaterial { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Quantity { get; set; }
}
