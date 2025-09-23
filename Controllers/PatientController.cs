using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkCare_IT15.Models.PatientModel;
using System;
using System.Collections.Generic;

namespace LinkCare_IT15.Controllers
{
    [Authorize(Roles = "Patient")] 
    public class PatientController : Controller
    {
        public IActionResult PatientDashboard()
        {
            ViewData["ActivePage"] = "Dashboard"; 

            var model = new PatientDashboardModel
            {
                NextAppointment = new AppointmentViewModel
                {
                    Title = "Routine Checkup",
                    Start = new DateTime(2025, 8, 28, 9, 0, 0),
                    Doctor = "Dr. Emily Wilson"
                },
                TotalConsultations = 12,
                OutstandingBills = 17500,
                PendingPayments = 1,
                UpcomingAppointments = new List<AppointmentViewModel>
                {
                    new AppointmentViewModel { Title = "Routine Checkup", Start = new DateTime(2025,8,28,9,0,0), Doctor="Dr. Emily Wilson" },
                    new AppointmentViewModel { Title = "Blood Work Review", Start = new DateTime(2025,8,31,8,30,0), Doctor="Dr. Sarah Smith" }
                },
                RecentActivity = new List<ActivityViewModel>
                {
                    new ActivityViewModel { Label="Consultation completed", User="Michael Johnson", Ago=TimeSpan.FromHours(3)},
                    new ActivityViewModel { Label="Consultation completed", User="Sarah Smith", Ago=TimeSpan.FromHours(3)},
                    new ActivityViewModel { Label="New patient registered", User="Sarah Smith", Ago=TimeSpan.FromHours(2)},
                    new ActivityViewModel { Label="Payment received ₱22,750", User="Patient User", Ago=TimeSpan.FromHours(5)},
                    new ActivityViewModel { Label="Appointment scheduled", User="Patient User", Ago=TimeSpan.FromHours(1)}
                }
            };

            return View(model);
        }

        public IActionResult PatientAppointments()
        {
            {
                ViewData["ActivePage"] = "Appointments"; // highlight appointments

                // 👇 FIX: must declare type
                var appointments = new List<PatientAppointmentsModel>
            {
                new PatientAppointmentsModel
                {
                    PatientName = "Patient User",
                    DoctorName = "Dr. Emily Wilson",
                    Start = new DateTime(2025, 8, 28, 9, 0, 0),
                    Type = "Routine Checkup",
                    Status = "Scheduled"
                },
                new PatientAppointmentsModel
                {
                    PatientName = "Patient User",
                    DoctorName = "Dr. Sarah Smith",
                    Start = new DateTime(2025, 8, 31, 8, 30, 0),
                    Type = "Blood Work Review",
                    Status = "Scheduled"
                },
                new PatientAppointmentsModel
                {
                    PatientName = "Patient User",
                    DoctorName = "Dr. Michael Johnson",
                    Start = new DateTime(2025, 8, 15, 14, 0, 0),
                    Type = "Follow-up",
                    Status = "Completed"
                },
                new PatientAppointmentsModel
                {
                    PatientName = "Patient User",
                    DoctorName = "Dr. Sarah Smith",
                    Start = new DateTime(2025, 8, 10, 10, 0, 0),
                    Type = "Consultation",
                    Status = "Cancelled"
                }
            };

                return View(appointments); // 👈 pass appointments to the view
            }
        }
        public IActionResult PatientRecords()
        {
            ViewData["ActivePage"] = "Records"; // highlight sidebar

            var records = new List<RecordViewModel>
    {
        new RecordViewModel
        {
            PatientName = "Patient User",
            DoctorName = "Dr. Emily Wilson",
            ConsultationDate = new DateTime(2025, 8, 28),
            ChiefComplaint = "Headache and fatigue",
            Diagnosis = "Migraine",
            Prescriptions = new List<string> { "Paracetamol 500mg", "Ibuprofen 200mg" },
            BloodPressure = "120/80",
            HeartRate = 72,
            Temperature = 36.8,
            Weight = 65,
            Notes = "Patient advised to rest and reduce screen time."
        },
        new RecordViewModel
        {
            PatientName = "Patient User",
            DoctorName = "Dr. Sarah Smith",
            ConsultationDate = new DateTime(2025, 8, 15),
            ChiefComplaint = "Cough and fever",
            Diagnosis = "Flu",
            Prescriptions = new List<string> { "Cough Syrup", "Vitamin C" },
            BloodPressure = "118/76",
            HeartRate = 80,
            Temperature = 38.2,
            Weight = 64.5,
            Notes = "Advised to drink plenty of fluids and take medications as prescribed."
        }
    };

            return View(records);
        }

        public IActionResult PatientBilling()
        {
            ViewData["ActivePage"] = "Billing";

            // Summary
            var summary = new BillingSummaryViewModel
            {
                TotalPaid = 22750,
                Pending = 0,
                Overdue = 0,
                TotalBills = 1
            };

            // Bills
            var bills = new List<BillingViewModel>
    {
        new BillingViewModel
        {
            PatientName = "Patient User",
            BillDate = new DateTime(2025, 8, 20),
            Services = new List<string> { "Annual Physical Exam", "Blood Work Panel", "X-Ray" },
            TotalAmount = 22750,
            Status = "Paid",
            PaymentDate = new DateTime(2025, 8, 20)
        }
    };

            ViewBag.Summary = summary; // pass summary separately
            return View(bills);
        }

    }
}
