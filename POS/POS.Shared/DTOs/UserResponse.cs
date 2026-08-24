namespace POS.Shared.DTOs;

/// <summary>
/// Contrato público de usuario. Excluye PasswordHash y cualquier secreto de autenticación.
/// </summary>
public class UserResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
