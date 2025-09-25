using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace LinkCare_IT15.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // Navigation properties
        public ICollection<Consultation> ConsultationsAsDoctor { get; set; } = new List<Consultation>();
        public ICollection<Consultation> ConsultationsAsPatient { get; set; } = new List<Consultation>();

        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }

    public class Consultation
    {
        public int ConsultationId { get; set; }

        // Foreign keys to Identity users
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        public string? PatientId { get; set; }
        public ApplicationUser? Patient { get; set; }

        public DateTime Date { get; set; }
        public string ChiefComplaint { get; set; }
        public string Diagnosis { get; set; }
        public string Prescriptions { get; set; }
        public string Notes { get; set; }

        // Vital signs
        public string BloodPressure { get; set; }
        public string HeartRate { get; set; }
        public string Temperature { get; set; }
        public string Weight { get; set; }
    }
}
