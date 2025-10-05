using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LinkCare_IT15.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [ForeignKey("Billing")]
        public int BillingId { get; set; }
        public Billing Billing { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash";
        // Options: Cash, GCash, Maya, PayMongo, etc.

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }
        // For PayMongo transaction IDs, etc.

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
        // Pending, Paid, Failed, Refunded, etc.

        [MaxLength(100)]
        public string TransactionType { get; set; } = "Consultation Payment";

        public DateTime TransactionDate { get; set; } = DateTime.Now;
    }
}
