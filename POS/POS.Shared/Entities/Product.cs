namespace POS.Shared.Entities;

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CurrentStock { get; set; }
    public int StockMinimum { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool IsActive { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
