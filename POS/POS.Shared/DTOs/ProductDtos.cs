using System.ComponentModel.DataAnnotations;

namespace POS.Shared.DTOs;

public class ProductRequest
{
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(250)] public string Description { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string UnitOfMeasure { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal CurrentStock { get; set; }
    [Range(0, double.MaxValue)] public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; }
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
