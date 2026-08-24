using System.ComponentModel.DataAnnotations;

namespace POS.Shared.Entities;

public class User
{
    public int Id { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string LastName { get; set; } = string.Empty;
    [Required, MaxLength(100), EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string Username { get; set; } = string.Empty;
    [Required, MaxLength(225)] public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public RoleEntity? Role { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Reception> Receptions { get; set; } = new List<Reception>();
    public ICollection<ProductionLot> ProductionLots { get; set; } = new List<ProductionLot>();
    public ICollection<Dispatch> Dispatches { get; set; } = new List<Dispatch>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
