using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Shared.Entities;

public class RawMaterial
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string UnitOfMeasure { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)"), Range(0, double.MaxValue)] public decimal CurrentStock { get; set; }
    [Column(TypeName = "decimal(18,2)"), Range(0, double.MaxValue)] public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ReceptionDetail> ReceptionDetails { get; set; } = new List<ReceptionDetail>();
    public ICollection<ProductionLotMaterialDetail> ProductionLotDetails { get; set; } = new List<ProductionLotMaterialDetail>();
    public ICollection<ProductRecipeItem> ProductRecipes { get; set; } = new List<ProductRecipeItem>();
}
