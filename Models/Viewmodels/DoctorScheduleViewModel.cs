using System.Collections.Generic;
using LinkCare_IT15.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LinkCare_IT15.Models.ViewModels
{
    public class DoctorScheduleViewModel
    {
        public List<AppointmentViewModel> Appointments { get; set; } = new();
        public AppointmentViewModel NewAppointment { get; set; } = new AppointmentViewModel();

        // This is what feeds the search/dropdown
        public List<SelectListItem> Patients { get; set; } = new();
    }
}
