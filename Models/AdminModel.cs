using LinkCare_IT15.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
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
        public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateOfBirth.Date > DateTime.Today.AddYears(-(DateTime.Today.Year - DateOfBirth.Year)) ? 1 : 0);

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
        public string ReportType { get; set; }

        // === Summary Statistics ===
        public int TotalAppointments { get; set; }
        public int TotalConsultations { get; set; }
        public int TotalPatients { get; set; }
        public int ActiveDoctors { get; set; }

        // === Financial Overview ===
        public decimal TotalRevenue { get; set; }          // Total earnings from Billings
        public decimal TotalBilling { get; set; }          // For consistency and reporting
        public decimal TotalEquipmentCost { get; set; }    // Equipment expenses
        public decimal TotalConsumableCost { get; set; }   // Consumables (supplies, meds, etc.)

        // ✅ Corrected Net Revenue Formula
        public decimal NetRevenue => TotalRevenue - (TotalEquipmentCost + TotalConsumableCost);

        // === Charts / Analytics ===
        public List<RevenueData> Revenues { get; set; } = new();
        public List<PatientAnalyticsData> Patients { get; set; } = new();
        public List<ServicePerformanceData> TopServices { get; set; } = new();
        public List<DoctorPerformanceData> DoctorPerformances { get; set; } = new();

        // ✅ Equipment & Consumable details
        public List<EquipmentDetailViewModel> Equipments { get; set; } = new();
        public List<ConsumableDetailViewModel> Consumables { get; set; } = new();
        // === Date Filters ===
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }


    public class ServicePerformanceData
    {
        public string ServiceName { get; set; }
        public int TimesUsed { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DoctorPerformanceData
    {
        public string DoctorName { get; set; }
        public int PatientsSeen { get; set; }
        public int AppointmentsHandled { get; set; }
        public decimal RevenueGenerated { get; set; }
    }



    public class BillRecord
    {
        public string Id { get; set; } // Bill ID
        public string PatientName { get; set; }
        public DateTime Date { get; set; }
        public string Services { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } // "Paid", "Pending", "Overdue"
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
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required]
        public string Specialty { get; set; }

        [Required]
        public string LicenseNumber { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required, DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AdminDoctorsModel
    {
        [ValidateNever]
        public List<Doctor> Doctors { get; set; }

        [ValidateNever]
        public string SearchTerm { get; set; }

        public NewDoctorViewModel NewDoctor { get; set; } = new NewDoctorViewModel();
    }


    public class EquipmentModel
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string Category { get; set; }
        public decimal PurchaseCost { get; set; }
        public string Status { get; set; }
        public int Quantity { get; set; } = 1; // track multiple units
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public DateTime AcquiredDate { get; set; } = DateTime.UtcNow;
        public DateTime? DisposeDate { get; set; } // for leased equipment
        public bool IsArchived { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        [FileExtensions(Extensions = "jpg,jpeg,png,gif")]
        public IFormFile? ImageFile { get; set; }  // client uploads
        public byte[]? ImageData { get; set; }     // stored in DB

        public ICollection<MaintenanceLog>? MaintenanceLogs { get; set; }
    }
    public class MaintenanceLog
    {
        public int MaintenanceLogId { get; set; }

        // Foreign key to Equipment
        public int EquipmentId { get; set; }
        public EquipmentModel? Equipment { get; set; }

        // User who performed or logged the maintenance
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string EquipmentName { get; set; }
        public string? Remarks { get; set; }
        public decimal MaintenanceCost { get; set; }
        public DateTime MaintenanceDate { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; } = false;
    }

    public class ConsumableModel
    {
        public int ConsumableId { get; set; }
        public string ConsumableName { get; set; }
        public string Category { get; set; }
        public decimal UnitCost { get; set; }
        public string Status { get; set; }

        public bool IsArchived { get; set; } = false;
        [NotMapped]
        [DataType(DataType.Upload)]
        [FileExtensions(Extensions = "jpg,jpeg,png,gif")]
        public IFormFile? ImageFile { get; set; }

        public byte[]? ImageData { get; set; }

        // ✅ Navigation property — one consumable can have many batches
        public ICollection<ConsumableBatch>? Batches { get; set; }
    }

    public class ConsumableBatch
    {
        [Key]
        public int BatchId { get; set; }

        [ForeignKey("ConsumableModel")]
        public int ConsumableId { get; set; }
        public ConsumableModel? Consumable { get; set; }

        // User who performed or logged the batch addition
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string BatchNumber { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }

        public DateTime DateReceived { get; set; } = DateTime.UtcNow;

        // Shelf life in days (used to auto-calculate expiration)
        public int ShelfLifeDays { get; set; } = 0;
        public DateTime? ExpirationDate { get; set; }

        public bool IsArchived { get; set; } = false;

     
    }

    public class ConsumableBatchViewModel
    {
        public string BatchNumber { get; set; }
        public string ConsumableName { get; set; }
        public string ConsumableImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime? DateReceived { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string AddedBy { get; set; }
    }

    public class EquipmentDetailViewModel
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public decimal PurchaseCost { get; set; }
        public DateTime AcquiredDate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }

        public decimal TotalCost => Quantity * PurchaseCost;
    }

    public class ConsumableDetailViewModel
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int TotalQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost => UnitCost * TotalQuantity;
    }
    public class AdminInventoryModel
    {
        public List<EquipmentModel> Equipments { get; set; } = new();
        public List<ConsumableModel> Consumables { get; set; } = new();

        public EquipmentModel NewEquipment { get; set; } = new();
        public ConsumableModel NewConsumable { get; set; } = new();
      
    }
}
