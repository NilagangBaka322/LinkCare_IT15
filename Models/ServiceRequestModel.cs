using LinkCare_IT15.Models.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkCare_IT15.Models
{
    public class ServiceRequest
    {
        [Key]
        public int RequestId { get; set; }

        [ForeignKey("Patient")]
        public string? PatientId { get; set; }  // FK to AspNetUsers.Id (ApplicationUser)
        public ApplicationUser? Patient { get; set; } // Navigation property

        [ForeignKey("Doctor")]
        public string? DoctorId { get; set; }

        public ApplicationUser? Doctor { get; set; }


        [Required]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public bool IsArchived { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
