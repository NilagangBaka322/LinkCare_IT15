using LinkCare_IT15.Models.Entities;
using System.Collections.Generic;

namespace LinkCare_IT15.Models.DoctorModel
{
    public class DoctorConsultationViewModel
    {
        public List<Consultation> Consultations { get; set; } = new List<Consultation>();

        // This will hold the new consultation form
        public Consultation NewConsultation { get; set; } = new Consultation();
    }
}
