using System.ComponentModel.DataAnnotations;

namespace POS.Shared.DTOs;

public class UserUpdateRequest
{
    [Required, MaxLength(80)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string LastName { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string Username { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(100)] public string Email { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int RoleId { get; set; }
    public bool IsActive { get; set; }
    [MinLength(8)] public string? NewPassword { get; set; }
}
