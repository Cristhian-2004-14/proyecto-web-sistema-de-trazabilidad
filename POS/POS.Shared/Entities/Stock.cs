using System.Reflection.Metadata;
using POS.Shared.Enums;

namespace POS.Shared.Entities
{
    public class Stock
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public DateTime MovementDate { get; set; }
        public MovementType MovementType { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public User User { get; set; } = null!;
        public int UserId { get; set; }

    }
}
