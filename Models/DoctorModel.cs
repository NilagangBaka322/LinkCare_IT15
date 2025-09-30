// *DoctorModel.cs*
using System;
using System.Collections.Generic;

namespace LinkCare_IT15.Models.DoctorModel
{
    public class DoctorDashboardModel
    {
        public string DoctorName { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingConsultations { get; set; }
        public int TotalPatients { get; set; }
        public List<ActivityViewModel> RecentActivity { get; set; }
    }

    public class ActivityViewModel
    {
        public string Label { get; set; }
        public string User { get; set; }
        public TimeSpan Ago { get; set; }
    }

    public class DoctorPatientsModel
    {
        public List<DoctorPatientViewModel> Patients { get; set; } = new List<DoctorPatientViewModel>();
        public int TotalPatients => Patients?.Count ?? 0;
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
