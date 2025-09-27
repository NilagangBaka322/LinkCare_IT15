using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LinkCare_IT15.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinkCare_IT15.Models
{
    public enum AppointmentStatus
    {
        Scheduled,
        Completed,
        Cancelled
    }

    // =======================
    // Appointment Entity
    // =======================
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        // Relationships
        [ForeignKey("Doctor")]
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        [ForeignKey("Patient")]
        public string? PatientId { get; set; }   // Nullable for walk-in
        public ApplicationUser? Patient { get; set; }

        // If not a registered patient
        public string? WalkInName { get; set; }

        // Core appointment details
        [Required]
        public string Title { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public string? Notes { get; set; }

        // Metadata
        public bool IsArchived { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    // =======================
    // Appointment ViewModel
    // =======================
    public class AppointmentViewModel
    {

        public int Id { get; set; } 

        [Required]
        public string PatientId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Title { get; set; }

        public string Notes { get; set; }

        // Walk-in patient support
        public string WalkInName { get; set; }

        // Combined patient name for display
        public string PatientName { get; set; }

        // Appointment status
        public string Status { get; set; } = "Scheduled";

        // For dropdowns
        public List<SelectListItem> Patients { get; set; } = new();
    }
    public class CreateAppointmentDto
    {
        public string Title { get; set; }
        public string StartDate { get; set; }   // coming in as string from JSON
        public string EndDate { get; set; }     // same here
        public string PatientId { get; set; }
        public string WalkInName { get; set; }
    }

}
