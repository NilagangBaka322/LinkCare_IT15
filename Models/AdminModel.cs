using LinkCare_IT15.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkCare_IT15.Models.AdminModel
{
    public class AdminDashboardModel
    {
        public int TotalAppointments { get; set; }
        public int ScheduledAppointments { get; set; }
        public int TotalPatients { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int RevenueGrowth { get; set; }
        public int PendingBills { get; set; }
        public decimal PendingAmount { get; set; }

        public List<AdminAppointmentViewModel> TodayAppointments { get; set; } = new();
        public List<AdminActivityViewModel> RecentActivity { get; set; } = new();
    }

    public class AdminAppointmentViewModel
    {
        public string PatientName { get; set; }
        public string Title { get; set; } // Type of appointment
        public DateTime Start { get; set; } // Appointment Date & Time
        public string Status { get; set; } // scheduled, completed, cancelled
    }

    public class AdminActivityViewModel
    {
        public string Label { get; set; }
        public string User { get; set; }
    }

    public class PatientRegistrationViewModel
    {
        // Personal Info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
        public string Email { get; set; }

        // Medical Info
        public string MedicalHistory { get; set; }
        public string Allergies { get; set; }
        public string CurrentMedications { get; set; }
    }

    public class AdminAppointmentsModel
    {
        public List<AdminAppointmentViewModel> TodayAppointments { get; set; } = new();
        public List<AdminAppointmentViewModel> UpcomingAppointments { get; set; } = new();
    }

    public class AdminPatientViewModel
    {
        public string PatientName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int Age => DateTime.Today.Year - DateOfBirth.Year -
                         (DateOfBirth.Date > DateTime.Today.AddYears(-(DateTime.Today.Year - DateOfBirth.Year)) ? 1 : 0);

        // Contact Info
        public string Phone { get; set; }
        public string Email { get; set; }

        // Medical Info
        public List<string> MedicalHistory { get; set; } = new();
        public List<string> Allergies { get; set; } = new();
    }

    public class AdminPatientsModel
    {
        public List<AdminPatientViewModel> Patients { get; set; } = new();
        public int TotalPatients => Patients.Count;
    }
    public class AdminConsultationViewModel
    {
        public string PatientName { get; set; }
        public int RecordCount { get; set; }
        public DateTime LastVisit { get; set; }
    }

    public class AdminBillingModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal Pending { get; set; }
        public decimal Overdue { get; set; }
        public int TotalBills { get; set; }

        public List<AdminBillViewModel> Bills { get; set; } = new();
    }

    public class AdminBillViewModel
    {
        public string Id { get; set; }
        public string PatientName { get; set; }
        public DateTime Date { get; set; }
        public string Services { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } // "paid", "pending", "overdue"
    }

    public class ReportsViewModel
    {
        public string ReportType { get; set; } = "Overview";

        public List<RevenueData> Revenues { get; set; } = new List<RevenueData>();
        public List<PatientAnalyticsData> Patients { get; set; } = new List<PatientAnalyticsData>();
    }
   
    public class BillRecord
    {
        public string Id { get; set; }               // Bill ID (for actions like MarkPaid)
        public string PatientName { get; set; }  // Name of the patient
        public DateTime Date { get; set; }       // Date of the bill
        public string Services { get; set; }     // Description of services
        public decimal Amount { get; set; }      // Total amount of the bill
        public string Status { get; set; }       // "Paid", "Pending", "Overdue", etc.
    }
    
    public class RevenueData
    {
        public string Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class PatientAnalyticsData
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }

    public class NewDoctorViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Required]
        public string Specialty { get; set; }

        [Required]
        public string LicenseNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AdminDoctorsModel
    {
        public List<Doctor> Doctors { get; set; }
        public string SearchTerm { get; set; }

        public NewDoctorViewModel NewDoctor { get; set; } = new NewDoctorViewModel();
    }

}
