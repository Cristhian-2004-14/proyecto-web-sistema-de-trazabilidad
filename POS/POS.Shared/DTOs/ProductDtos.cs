using System.ComponentModel.DataAnnotations;

namespace POS.Shared.DTOs;

public class ProductRequest
{
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(250)] public string Description { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string UnitOfMeasure { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int CurrentStock { get; set; }
    [Range(0, int.MaxValue)] public int MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
    [MinLength(1, ErrorMessage = "Debe agregar al menos una materia prima a la receta.")]
    public List<ProductRecipeItemRequest> Recipe { get; set; } = [];
}

public class ProductRecipeItemRequest
{
    [Range(1, int.MaxValue)] public int RawMaterialId { get; set; }
    [Range(0.0001, double.MaxValue)] public decimal QuantityPerUnit { get; set; }
}

public class ProductRecipeItemResponse
{
    public int RawMaterialId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal QuantityPerUnit { get; set; }
}

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public bool IsActive { get; set; }
    public List<ProductRecipeItemResponse> Recipe { get; set; } = [];
}

public class ProductStatusRequest { public bool IsActive { get; set; } }

public class RawMaterialInventoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock => CurrentStock <= MinimumStock;
}
