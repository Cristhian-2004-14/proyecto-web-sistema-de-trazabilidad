using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.Shared.Enums;

namespace POS.Shared.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public User User { get; set; } = null!;
        public int UserId { get; set; }
        public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
        public PaymentMethod PaymentMethod { get; set; }
    }
}
