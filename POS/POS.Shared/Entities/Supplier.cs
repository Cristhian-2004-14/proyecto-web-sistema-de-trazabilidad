using System.ComponentModel.DataAnnotations;

namespace POS.Shared.Entities;

public class Supplier
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(30)] public string Nit { get; set; } = string.Empty;
    [MaxLength(30)] public string Phone { get; set; } = string.Empty;
    [MaxLength(150)] public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Reception> Receptions { get; set; } = new List<Reception>();
}
