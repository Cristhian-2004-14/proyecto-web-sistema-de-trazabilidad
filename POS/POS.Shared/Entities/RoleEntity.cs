using System.ComponentModel.DataAnnotations;

namespace POS.Shared.Entities;

public class RoleEntity
{
    public int Id { get; set; }
    [Required, MaxLength(50)] public string Name { get; set; } = string.Empty;
    [MaxLength(150)] public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<User> Users { get; set; } = new List<User>();
}
