using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinkCare_IT15.Models.ViewModels
{
    public class DoctorConsultationVM
    {
        public List<ConsultationRecordVM> Consultations { get; set; } = new();
        public CreateConsultationDto NewConsultation { get; set; } = new();
        public List<SelectListItem> Patients { get; set; } = new();
    }

    public class ConsultationRecordVM
    {
        public int Id { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string ChiefComplaint { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Prescriptions { get; set; }
        public string? Notes { get; set; }
        public string? BloodPressure { get; set; }
        public string? HeartRate { get; set; }
        public string? Temperature { get; set; }
        public string? Weight { get; set; }
        public DateTime Date { get; set; }
    }

    public class CreateConsultationDto
    {
        public int? AppointmentId { get; set; }
        public string? PatientId { get; set; }
        public string? WalkInName { get; set; } // nullable
        public string ChiefComplaint { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public List<string> Prescriptions { get; set; } = new();
        public string? Notes { get; set; }
        public string? BloodPressure { get; set; }
        public string? HeartRate { get; set; }
        public string? Temperature { get; set; }
        public string? Weight { get; set; }
    }

    public class ConsultationEntityVM
    {
        public int Id { get; set; }
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string ChiefComplaint { get; set; }
        public string Diagnosis { get; set; }
        public string Prescriptions { get; set; }
        public string Notes { get; set; }
        public string BloodPressure { get; set; }
        public string HeartRate { get; set; }
        public string Temperature { get; set; }
        public string Weight { get; set; }
        public DateTime Date { get; set; }
    }
}
