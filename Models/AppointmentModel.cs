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
        Cancelled,
        Rescheduled
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


        //Service and Request Linkage
        [ForeignKey("Service")]
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }
        public int? RequestId { get; set; }

        [ForeignKey("RequestId")]
        public ServiceRequest? ServiceRequest { get; set; }
    }

    // =======================
    // Appointment ViewModel
    // =======================
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        // For registered patients
        public string? PatientId { get; set; }

        // For walk-in patients
        public string? WalkInName { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }

        public string Status { get; set; } = "Scheduled";

        // For display
        public string? PatientName { get; set; }

        // Dropdown
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
    public class RescheduleDto
    {
        public string NewDate { get; set; }
    }

}