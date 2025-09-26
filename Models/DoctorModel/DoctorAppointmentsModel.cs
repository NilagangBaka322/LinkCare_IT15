using LinkCare_IT15.Models.ViewModels;
using System.Collections.Generic;

namespace LinkCare_IT15.Models.DoctorModel
{
    public class DoctorAppointmentsModel
    {
        // For full calendar appointments
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();

        public DoctorScheduleViewModel Schedule { get; set; }
    }
}
