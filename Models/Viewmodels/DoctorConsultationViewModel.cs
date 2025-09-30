using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace LinkCare_IT15.Models.ViewModels
{
    public class DoctorConsultationVM
    {
        public List<ConsultationEntityVM> Consultations { get; set; }
        public CreateConsultationDto NewConsultation { get; set; } = new CreateConsultationDto();
        public IEnumerable<SelectListItem> Patients { get; set; }
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
