using System.ComponentModel.DataAnnotations;

namespace POS.Shared.Entities;

public class Reception
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime Date { get; set; }
    [MaxLength(300)] public string Observation { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Status { get; set; } = ReceptionStatuses.Pending;
    public ICollection<ReceptionDetail> Details { get; set; } = new List<ReceptionDetail>();
}

public static class ReceptionStatuses
{
    public const string Pending = "Pendiente";
    public const string Confirmed = "Confirmada";
    public const string Cancelled = "Anulada";
    public static readonly string[] Valid = [Pending, Confirmed, Cancelled];
}
