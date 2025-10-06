using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LinkCare_IT15.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required]
        public int BillingID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }  // Actual amount received

        [Column(TypeName = "decimal(18,2)")]
        public decimal Change { get; set; }      // Change to give back (if cash)

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        public string TransactionType { get; set; } // e.g., "Payment"

        [Required]
        public string PaymentMethod { get; set; }   // e.g., "Cash", "PayMongo"

        public string? ReferenceNumber { get; set; } // Optional (for PayMongo)

        [Required]
        public string Status { get; set; }          // "Paid", "Partial", "Pending"

        // Navigation property
        [ForeignKey("BillingID")]
        public virtual Billing Billing { get; set; }
    }
}
