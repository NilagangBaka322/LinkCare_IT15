using LinkCare_IT15.Models.AdminModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace LinkCare_IT15.Controllers
{

    [Authorize(Roles = "Admin")] // restrict to admins
    public class AdminController : Controller
    {
        public IActionResult AdminDashboard()
        {
            var model = new AdminDashboardModel
            {
                TotalAppointments = 6,
                ScheduledAppointments = 5,
                TotalPatients = 6,
                MonthlyRevenue = 22750,
                RevenueGrowth = 12,
                PendingBills = 1,
                PendingAmount = 17500,

                TodayAppointments = new List<AdminAppointmentViewModel>
                {
                    new AdminAppointmentViewModel { PatientName = "Patient User", Title = "Routine Checkup", Start = DateTime.Today.AddHours(9), Status = "scheduled" },
                    new AdminAppointmentViewModel { PatientName = "Sarah Smith", Title = "Follow-up", Start = DateTime.Today.AddHours(10), Status = "scheduled" },
                    new AdminAppointmentViewModel { PatientName = "Michael Johnson", Title = "Consultation", Start = DateTime.Today.AddHours(11), Status = "completed" },
                    new AdminAppointmentViewModel { PatientName = "Lisa Rodriguez", Title = "Vaccination", Start = DateTime.Today.AddHours(13.5), Status = "scheduled" },
                    new AdminAppointmentViewModel { PatientName = "David Chen", Title = "Emergency", Start = DateTime.Today.AddHours(14.5), Status = "scheduled" },
                    new AdminAppointmentViewModel { PatientName = "Maria Santos", Title = "Follow-up", Start = DateTime.Today.AddHours(15.5), Status = "scheduled" }
                },

                RecentActivity = new List<AdminActivityViewModel>
                {
                    new AdminActivityViewModel { Label = "Consultation completed", User = "Michael Johnson" },
                    new AdminActivityViewModel { Label = "Consultation completed", User = "Sarah Smith" },
                    new AdminActivityViewModel { Label = "New patient registered", User = "Sarah Smith" }
                }
            };

            return View(model);
        }

        // ---------------- Appointments Page ----------------


        // ---------------- Patient Registration ----------------
        public IActionResult PatientRegistration()
        {
            return View(new PatientRegistrationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PatientRegistration(PatientRegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save to DB
                return RedirectToAction("Patients");
            }

            return View(model);
        }

        // ---------------- Patients List ----------------


        public IActionResult Appointments()
        {
            var model = new AdminAppointmentsModel
            {
                TodayAppointments = new List<AdminAppointmentViewModel>
        {
            new AdminAppointmentViewModel { PatientName = "Patient User", Title = "Routine Checkup", Start = DateTime.Today.AddHours(9), Status = "scheduled" },
            new AdminAppointmentViewModel { PatientName = "Sarah Smith", Title = "Follow-up", Start = DateTime.Today.AddHours(10), Status = "scheduled" },
            new AdminAppointmentViewModel { PatientName = "Michael Johnson", Title = "Consultation", Start = DateTime.Today.AddHours(11), Status = "completed" },
        },

                UpcomingAppointments = new List<AdminAppointmentViewModel>
        {
            new AdminAppointmentViewModel { PatientName = "Lisa Rodriguez", Title = "Vaccination", Start = DateTime.Today.AddDays(1).AddHours(13.5), Status = "scheduled" },
            new AdminAppointmentViewModel { PatientName = "David Chen", Title = "Emergency", Start = DateTime.Today.AddDays(2).AddHours(14.5), Status = "scheduled" },
            new AdminAppointmentViewModel { PatientName = "Maria Santos", Title = "Follow-up", Start = DateTime.Today.AddDays(3).AddHours(15.5), Status = "scheduled" }
        }
            };

            return View(model);
        }

        // ---------------- Patients List ----------------
        public IActionResult Patients()
        {
            var model = new AdminPatientsModel
            {
                Patients = new List<AdminPatientViewModel>
        {
            new AdminPatientViewModel {
                PatientName = "Patient User",
                DateOfBirth = new DateTime(1985, 3, 15),
                Gender = "Male",
                Phone = "+63-917-123-4567",
                Email = "patient.user@email.com",
                MedicalHistory = new List<string>{ "Hypertension", "Diabetes Type 2" },
                Allergies = new List<string>{ "Penicillin", "Shellfish" }
            },
            new AdminPatientViewModel {
                PatientName = "Sarah Smith",
                DateOfBirth = new DateTime(1992, 7, 22),
                Gender = "Female",
                Phone = "+63-917-234-5678",
                Email = "sarahsmith@email.com",
                MedicalHistory = new List<string>{ "Asthma" },
                Allergies = new List<string>{ "Latex" }
            },
            new AdminPatientViewModel {
                PatientName = "Michael Johnson",
                DateOfBirth = new DateTime(1978, 11, 8),
                Gender = "Male",
                Phone = "+63-917-345-6789",
                Email = "michaeljohnson@email.com",
                MedicalHistory = new List<string>{ "Heart Disease", "High Cholesterol" },
                Allergies = new List<string>() // None
            },
            new AdminPatientViewModel {
                PatientName = "Lisa Rodriguez",
                DateOfBirth = new DateTime(1990, 5, 14),
                Gender = "Female",
                Phone = "+63-917-456-7890",
                Email = "lisa.rodriguez@email.com",
                MedicalHistory = new List<string>{ "None" },
                Allergies = new List<string>() // None
            },
            new AdminPatientViewModel {
                PatientName = "David Chen",
                DateOfBirth = new DateTime(1988, 12, 3),
                Gender = "Male",
                Phone = "+63-917-567-8901",
                Email = "david.chen@email.com",
                MedicalHistory = new List<string>{ "Migraine" },
                Allergies = new List<string>{ "Aspirin" }
            },
            new AdminPatientViewModel {
                PatientName = "Maria Santos",
                DateOfBirth = new DateTime(1995, 9, 20),
                Gender = "Female",
                Phone = "+63-917-678-9012",
                Email = "maria.santos@email.com",
                MedicalHistory = new List<string>{ "PCOS" },
                Allergies = new List<string>() // None
            }
        }
            };

            return View(model);
        }

        public IActionResult AdminConsultations()
        {
            ViewData["ActivePage"] = "Consultations";

            var consultations = new List<AdminConsultationViewModel>
    {
        new AdminConsultationViewModel
        {
            PatientName = "Patient User",
            RecordCount = 1,
            LastVisit = new DateTime(2025, 8, 20)
        },
        new AdminConsultationViewModel
        {
            PatientName = "Sarah Smith",
            RecordCount = 1,
            LastVisit = new DateTime(2025, 8, 15)
        }
    };

            return View(consultations);
        }

        public IActionResult Billing()
        {
            var model = new AdminBillingModel
            {
                TotalRevenue = 22750,
                Pending = 17500,
                Overdue = 35000,
                TotalBills = 3,
                Bills = new List<AdminBillViewModel>
        {
            new AdminBillViewModel {
                Id = "1",
                PatientName = "Patient User",
                Date = new DateTime(2025, 8, 20),
                Services = "Annual Physical Exam, Blood Work Panel",
                Amount = 22750,
                Status = "paid"
            },
            new AdminBillViewModel {
                Id = "2",
                PatientName = "Sarah Smith",
                Date = new DateTime(2025, 8, 15),
                Services = "Follow-up Visit, Pulmonary Function Test",
                Amount = 17500,
                Status = "pending"
            },
            new AdminBillViewModel {
                Id = "3",
                PatientName = "Michael Johnson",
                Date = new DateTime(2025, 8, 10),
                Services = "Cardiology Consultation, Stress Test",
                Amount = 35000,
                Status = "overdue"
            }
        }
            };

            return View(model);
        }

        public IActionResult Reports(string reportType = "Overview")
        {
            var model = new ReportsViewModel
            {
                ReportType = reportType,
                Revenues = new List<RevenueData>
                {
                    new RevenueData { Month = "January", Amount = 12000 },
                    new RevenueData { Month = "February", Amount = 15000 },
                    new RevenueData { Month = "March", Amount = 18000 }
                },
                Patients = new List<PatientAnalyticsData>
                {
                    new PatientAnalyticsData { Category = "New Patients", Count = 25 },
                    new PatientAnalyticsData { Category = "Returning Patients", Count = 40 },
                    new PatientAnalyticsData { Category = "Follow-ups", Count = 18 }
                }
            };

            // Dropdown options
            ViewBag.ReportOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Overview", Text = "Overview Report", Selected = (reportType == "Overview") },
                new SelectListItem { Value = "Revenue", Text = "Revenue Analysis", Selected = (reportType == "Revenue") },
                new SelectListItem { Value = "Patient", Text = "Patient Analytics", Selected = (reportType == "Patient") }
            };

            return View("Reports", model); // explicitly load Reports.cshtml
        }

        public IActionResult DoctorManagement(string search = null)
        {
            ViewData["ActivePage"] = "DoctorManagement";

            // static sample data (replace with DB in real app)
            var doctors = new List<DoctorViewModel>
    {
        new DoctorViewModel {
            Id = "1",
            FullName = "Dr. Emily Wilson",
            Email = "emily.wilson@linkcare.com",
            Phone = "+63-917-111-2222",
            Specialty = "Family Medicine",
            LicenseNumber = "MD-12345",
            IsActive = true,
            Registered = new DateTime(2024,1,15),
            LastLogin = new DateTime(2025,9,13)
        },
        new DoctorViewModel {
            Id = "2",
            FullName = "Dr. Robert Brown",
            Email = "robert.brown@linkcare.com",
            Phone = "+63-917-333-4444",
            Specialty = "Pulmonology",
            LicenseNumber = "MD-23456",
            IsActive = true,
            Registered = new DateTime(2024,2,20),
            LastLogin = new DateTime(2025,9,12)
        },
        new DoctorViewModel {
            Id = "3",
            FullName = "Dr. Maria Garcia",
            Email = "maria.garcia@linkcare.com",
            Phone = "+63-917-555-6666",
            Specialty = "Cardiology",
            LicenseNumber = "MD-34567",
            IsActive = false,
            Registered = new DateTime(2024,3,10),
            LastLogin = new DateTime(2025,8,30)
        },
        new DoctorViewModel {
            Id = "4",
            FullName = "Dr. James Lee",
            Email = "james.lee@linkcare.com",
            Phone = "+63-917-777-8888",
            Specialty = "Orthopedics",
            LicenseNumber = "MD-45678",
            IsActive = true,
            Registered = new DateTime(2024,4,18),
            LastLogin = new DateTime(2025,9,10)
        }
    };

            // optional: simple server-side filter (case-insensitive)
            if (!string.IsNullOrWhiteSpace(search))
            {
                doctors = doctors
                    .FindAll(d =>
                        d.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || d.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || d.Specialty.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || d.LicenseNumber.Contains(search, StringComparison.OrdinalIgnoreCase)
                    );
            }

            var model = new AdminDoctorsModel
            {
                Doctors = doctors,
                SearchTerm = search ?? ""
            };

            return View(model); // will load Views/Admin/DoctorManagement.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleDoctorStatus(int id)
        {
            // In a real app you'd update DB. Here we just send a temp message and redirect back.
            TempData["Message"] = $"(Demo) toggled status for doctor id {id}.";
            return RedirectToAction("DoctorManagement");
        }

        // Optional: simple Register pages (demo)
        public IActionResult RegisterDoctor()
        {
            ViewData["ActivePage"] = "DoctorManagement";
            return View(new DoctorViewModel { Registered = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterDoctor(DoctorViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // TODO: save to DB
            TempData["Message"] = $"(Demo) Registered doctor {model.FullName}.";
            return RedirectToAction("DoctorManagement");
        }


    }
}
