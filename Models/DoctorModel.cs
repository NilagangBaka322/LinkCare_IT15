// *DoctorModel.cs*
using LinkCare_IT15.Models.Entities;
using System;
using System.Collections.Generic;

namespace LinkCare_IT15.Models.DoctorModel
{
    public class DoctorDashboardModel
    {
        public string DoctorName { get; set; }
        public ApplicationUser? Doctor { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingConsultations { get; set; }
        public int TotalPatients { get; set; }
        public List<ActivityViewModel> RecentActivity { get; set; }
        public List<UpcomingAppointmentViewModel> UpcomingAppointments { get; set; }
      
    }
    public class UpcomingAppointmentViewModel
    {
        public string Title { get; set; }
        public string PatientName { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
    } 
    public class ActivityViewModel
    {
        public string Label { get; set; }
        public string User { get; set; }
        public TimeSpan Ago { get; set; }
    }

    public class DoctorPatientsModel
    {
        public int TotalPatients { get; set; }
        public List<DoctorPatientViewModel> Patients { get; set; }
    }

    public class DoctorPatientViewModel
    {
        public string PatientName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Contact { get; set; }
        public string Status { get; set; }
        public DateTime LastVisit { get; set; }
    }
}
