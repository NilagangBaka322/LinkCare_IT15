using System.Collections.Generic;
using LinkCare_IT15.Models;

namespace LinkCare_IT15.Models.ViewModels
{
    public class DoctorScheduleViewModel
    {
        // Use the ViewModel here, not the entity
        public List<AppointmentViewModel> Appointments { get; set; } = new List<AppointmentViewModel>();
        public AppointmentViewModel NewAppointment { get; set; } = new AppointmentViewModel();
    }
}
