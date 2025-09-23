using System;
using System.Collections.Generic;

namespace LinkCare_IT15.Models.PatientModel
{
    public class PatientDashboardModel
    {
        public AppointmentViewModel NextAppointment { get; set; }
        public int TotalConsultations { get; set; }
        public decimal OutstandingBills { get; set; }
        public int PendingPayments { get; set; }
        public List<AppointmentViewModel> UpcomingAppointments { get; set; }
        public List<ActivityViewModel> RecentActivity { get; set; }
    }

    public class AppointmentViewModel
    {
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public string Doctor { get; set; }
    }

    public class ActivityViewModel
    {
        public string Label { get; set; }
        public string User { get; set; }
        public TimeSpan Ago { get; set; }
    }

    public class RecordViewModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime ConsultationDate { get; set; }
        public string ChiefComplaint { get; set; }
        public string Diagnosis { get; set; }
        public List<string> Prescriptions { get; set; }
        public string BloodPressure { get; set; }
        public int HeartRate { get; set; }
        public double Temperature { get; set; }
        public double Weight { get; set; }
        public string Notes { get; set; }

    }
    public class BillingViewModel
    {
        public string PatientName { get; set; }
        public DateTime BillDate { get; set; }
        public List<string> Services { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // Paid, Pending, Overdue
        public DateTime? PaymentDate { get; set; }
    }

    public class BillingSummaryViewModel
    {
        public decimal TotalPaid { get; set; }
        public decimal Pending { get; set; }
        public decimal Overdue { get; set; }
        public int TotalBills { get; set; }
    }
}
