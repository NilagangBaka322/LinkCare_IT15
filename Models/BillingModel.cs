using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LinkCare_IT15.Models
{
    public class Billing
    {
        [Key]
        public int BillingId { get; set; }

        [ForeignKey("Consultation")]
        public int ConsultationId { get; set; }
        public Consultation Consultation { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime BillingDate { get; set; } = DateTime.Now;

        // Track if it’s fully paid or pending
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // or "Paid"

        // Optional: For quick access, even for walk-ins
        [MaxLength(255)]
        public string? PatientName { get; set; }
    }
}
