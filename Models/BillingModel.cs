using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LinkCare_IT15.Models.Entities;

namespace LinkCare_IT15.Models
{
    public class Billing
    {
        [Key]
        public int BillingID { get; set; }

        public string? PatientID { get; set; } // Nullable for walk-ins
        public string? WalkInName { get; set; } // Name for walk-ins
        public int? AppointmentId { get; set; } // Nullable if no appointment

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime BillingDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("PatientID")]
        public virtual ApplicationUser? Patient { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        // Calculated properties
        [NotMapped]
        public decimal AmountPaid => Transactions?.Sum(t => t.AmountPaid) ?? 0;

        [NotMapped]
        public decimal RemainingBalance => Math.Max(TotalAmount - AmountPaid, 0);


        [NotMapped]
        public string Status
        {
            get
            {
                if (RemainingBalance <= 0) return "Paid";
                return AmountPaid > 0 ? "Partial" : "Pending";
            }
        }
    }
}
