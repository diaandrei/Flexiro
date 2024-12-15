using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flexiro.Application.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}