//*Entities

using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Numerics;

namespace LinkCare_IT15.Models.Entities
{
    public class Consultations
    {
        [Key]
        public int ConsultationId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }

        public string ChiefComplaint { get; set; }
        public string Diagnosis { get; set; }
        public string Prescription { get; set; } // stored as one string
        public string Notes { get; set; }

        // vitals
        public string BloodPressure { get; set; }
        public string HeartRate { get; set; }
        public string Temperature { get; set; }
        public string Weight { get; set; }

        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }

    }

    public class Patient
    {
        public int PatientId { get; set; }   // Primary Key
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }

        // Navigation Property
        public virtual ICollection<Consultations> Consultations { get; set; }
    }

    public class Doctor
    {
        public int DoctorId { get; set; }   // Primary Key
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialty { get; set; }
        public string ContactNumber { get; set; }

        // Navigation Property
        public virtual ICollection<Consultations> Consultation { get; set; }
    }
}
