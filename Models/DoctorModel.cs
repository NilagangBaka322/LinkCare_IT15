//*DoctorModel.cs*
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkCare_IT15.Models.DoctorModel
{
    public class DoctorDashboardModel
    {
        public string DoctorName { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingConsultations { get; set; }
        public int TotalPatients { get; set; }
        public List<DoctorAppointmentViewModel> UpcomingAppointments { get; set; }
        public List<ActivityViewModel> RecentActivity { get; set; }
    }

    public class ActivityViewModel
    {
        public string Label { get; set; }
        public string User { get; set; }
        public TimeSpan Ago { get; set; }
    }
    public class ConsultationViewModel
    {
        public int ConsultationId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public string ChiefComplaint { get; set; }
        public string Diagnosis { get; set; }
        public List<string> Prescriptions { get; set; } = new List<string>();
        public string Notes { get; set; }

        // vitals
        public string BloodPressure { get; set; }
        public string HeartRate { get; set; }
        public string Temperature { get; set; }
        public string Weight { get; set; }

        public string PatientName { get; set; }
        public string DoctorName { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactNumber { get; set; }
    }

    public class MedicalRecordViewModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime Date { get; set; }
        public string ChiefComplaint { get; set; }
        public string Diagnosis { get; set; }
        public List<string> Prescriptions { get; set; }
        public string BloodPressure { get; set; }
        public int HeartRate { get; set; }
        public double Temperature { get; set; }
        public double Weight { get; set; }
        public string Notes { get; set; }
    }

    public class DoctorMedicalRecordsModel
    {
        public List<MedicalRecordViewModel> Records { get; set; }
    }

    public class DoctorAppointmentsModel
    {
        public List<DoctorAppointmentViewModel> Appointments { get; set; } = new List<DoctorAppointmentViewModel>();
        public DateTime SelectedDate { get; set; } = DateTime.Today;
    }

    public class DoctorAppointmentViewModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime EndDate { get; set; }   // ✅ add this
        public string Title { get; set; }
        public string Status { get; set; }
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
