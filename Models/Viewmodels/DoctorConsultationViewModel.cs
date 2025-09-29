using System.Collections.Generic;
using LinkCare_IT15.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinkCare_IT15.Models.ViewModels
{
    public class DoctorConsultationViewModel
    {
        // List of consultations for the doctor
        public List<Consultation> Consultations { get; set; } = new();

        // New consultation input model
        public ConsultationInputModel NewConsultation { get; set; } = new();

        // ✅ Dropdown for selecting patients
        public List<SelectListItem> Patients { get; set; } = new();
    }

    public class ConsultationInputModel
    {
        public string PatientId { get; set; }
        public string ChiefComplaint { get; set; }
        public string Diagnosis { get; set; }
        public string Prescriptions { get; set; }
        public string Notes { get; set; }
        public string BloodPressure { get; set; }
        public string HeartRate { get; set; }
        public string Temperature { get; set; }
        public string Weight { get; set; }
    }
}
