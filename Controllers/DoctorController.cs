using LinkCare_IT15.Data;
using LinkCare_IT15.Models.DoctorModel;
using LinkCare_IT15.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkCare_IT15.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ======================
        // Doctor Dashboard
        // ======================
        public IActionResult DoctorDashboard()
        {
            var model = new DoctorDashboardModel
            {
                DoctorName = User.Identity.Name,
                TodayAppointments = 5,
                PendingConsultations = 2,
                TotalPatients = 48,
                UpcomingAppointments = new List<DoctorAppointmentViewModel>
                {
                    new DoctorAppointmentViewModel { PatientName="John Doe", Title="Routine Checkup", Status="Scheduled"},
                    new DoctorAppointmentViewModel { PatientName="Jane Smith", Title="Follow-up", Status="Scheduled"}
                },
                RecentActivity = new List<ActivityViewModel>
                {
                    new ActivityViewModel { Label="Consultation completed", User="John Doe", Ago=TimeSpan.FromHours(1)},
                    new ActivityViewModel { Label="Prescription added", User="Jane Smith", Ago=TimeSpan.FromHours(3)},
                }
            };
            return View(model);
        }

        // ======================
        // Doctor Appointments
        // ======================
        public IActionResult DoctorAppointments()
        {
            // STATIC DATA for demo
            var model = new DoctorAppointmentsModel
            {
                SelectedDate = DateTime.Today,
                Appointments = new List<DoctorAppointmentViewModel>
                {
                    new DoctorAppointmentViewModel {
                        PatientName = "Patient User",
                        DoctorName = User.Identity.Name,
                        AppointmentDate = DateTime.Today.AddHours(9),
                        Title = "Routine Checkup",
                        Status = "Scheduled"
                    },
                    new DoctorAppointmentViewModel {
                        PatientName = "Sarah Smith",
                        DoctorName = User.Identity.Name,
                        AppointmentDate = DateTime.Today.AddHours(10),
                        Title = "Follow-up",
                        Status = "Scheduled"
                    }
                }
            };

            return View(model);
        }

        // ======================
        // Doctor Consultation
        // ======================
        public IActionResult DoctorConsultation()
        {
            // STATIC DATA for demo
            var consultations = new List<ConsultationViewModel>
            {
                new ConsultationViewModel {
                    ConsultationId = 1,
                    Date = DateTime.Now.AddDays(-1),
                    PatientName = "Patient User",
                    DoctorName = User.Identity.Name,
                    ChiefComplaint = "Routine checkup",
                    Diagnosis = "Healthy",
                    Notes = "No issues",
                    BloodPressure = "120/80",
                    HeartRate = "72",
                    Temperature = "36.7",
                    Weight = "70",
                    PatientId = null
                }
            };

            return View(consultations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsultation(ConsultationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var consultation = new Consultation
            {
                PatientId = string.IsNullOrEmpty(model.PatientId) ? null : model.PatientId,
                DoctorId = doctorId,
                ChiefComplaint = model.ChiefComplaint,
                Diagnosis = model.Diagnosis,
                Prescriptions = model.Prescriptions != null
                                ? string.Join(",", model.Prescriptions)
                                : null,
                Notes = model.Notes,
                BloodPressure = model.BloodPressure,
                HeartRate = model.HeartRate,
                Temperature = model.Temperature,
                Weight = model.Weight,
                Date = DateTime.Now
            };

            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();

            return RedirectToAction("DoctorConsultation");
        }

        // ======================
        // Doctor Patients
        // ======================
        public IActionResult DoctorPatients()
        {
            var model = new DoctorPatientsModel
            {
                Patients = new List<DoctorPatientViewModel>
                {
                    new DoctorPatientViewModel {
                        PatientName = "Patient User",
                        Age = 30,
                        Gender = "Male",
                        Contact = "0912-345-6789",
                        Status = "Active",
                        LastVisit = DateTime.Today.AddDays(-3)
                    },
                    new DoctorPatientViewModel {
                        PatientName = "Sarah Smith",
                        Age = 27,
                        Gender = "Female",
                        Contact = "0917-222-3333",
                        Status = "Active",
                        LastVisit = DateTime.Today.AddDays(-10)
                    },
                    new DoctorPatientViewModel {
                        PatientName = "Michael Johnson",
                        Age = 45,
                        Gender = "Male",
                        Contact = "0918-555-9999",
                        Status = "Inactive",
                        LastVisit = DateTime.Today.AddDays(-60)
                    },
                    new DoctorPatientViewModel {
                        PatientName = "Lisa Rodriguez",
                        Age = 35,
                        Gender = "Female",
                        Contact = "0917-123-4567",
                        Status = "Active",
                        LastVisit = DateTime.Today.AddDays(-1)
                    }
                }
            };

            return View(model);
        }

        // ======================
        // Doctor Medical Records
        // ======================
        public IActionResult DoctorMedicalRecords()
        {
            var records = new List<ConsultationViewModel>
            {
                new ConsultationViewModel {
                    PatientName = "Patient User",
                    DoctorName = User.Identity.Name,
                    Date = DateTime.Now.AddDays(-20),
                    ChiefComplaint = "Annual physical examination",
                    Diagnosis = "Hypertension - well controlled, Diabetes Type 2 - stable",
                    Prescriptions = new List<string> { "Metformin 500mg twice daily", "Lisinopril 10mg daily" },
                    BloodPressure = "128/82",
                    HeartRate = "72",
                    Temperature = "37.0",
                    Weight = "82",
                    Notes = "Patient reports good adherence to medications. Blood pressure well controlled."
                },
                new ConsultationViewModel {
                    PatientName = "Sarah Smith",
                    DoctorName = User.Identity.Name,
                    Date = DateTime.Now.AddDays(-25),
                    ChiefComplaint = "Shortness of breath",
                    Diagnosis = "Asthma exacerbation",
                    Prescriptions = new List<string> { "Albuterol as needed", "Prednisone 20mg daily for 5 days" },
                    BloodPressure = "118/75",
                    HeartRate = "88",
                    Temperature = "36.9",
                    Weight = "61",
                    Notes = "Patient experienced increased symptoms during high pollen season."
                }
            };

            return View(records);
        }

        // ======================
        // Register Doctor (Admin)
        // ======================
        //[HttpGet]
        //public IActionResult RegisterDoctor() => View();

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegisterDoctor(Doctor model)
        //{
        //    // If you don't have a Doctor entity or DbSet, skip DB save
        //    // _context.Doctors.Add(model);
        //    // await _context.SaveChangesAsync();

        //    // For demo purposes, just redirect
        //    return RedirectToAction("DoctorDashboard");
        //}

        // ======================
        // Search Patients API
        // ======================
        [HttpGet]
        public async Task<JsonResult> SearchPatients(string query)
        {
            var patients = new List<ApplicationUser>();
            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, "Patient"))
                    patients.Add(user);
            }

            var results = patients
                .Where(p => (p.FirstName + " " + p.LastName).Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(p => new { name = p.FirstName + " " + p.LastName, contact = p.PhoneNumber })
                .ToList();

            return Json(results);
        }

        // ======================
        // Create Appointment API
        // ======================
        public class CreateAppointmentDto
        {
            public string PatientName { get; set; }
            public string Title { get; set; }
            public string StartDate { get; set; } // ISO string
            public string EndDate { get; set; }   // ISO string
        }

        [HttpPost]
        [Route("Doctor/CreateAppointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.PatientName) || string.IsNullOrWhiteSpace(dto.Title)
                || string.IsNullOrWhiteSpace(dto.StartDate) || string.IsNullOrWhiteSpace(dto.EndDate))
            {
                return BadRequest("Invalid appointment data.");
            }

            if (!DateTime.TryParse(dto.StartDate, out var startDate) ||
                !DateTime.TryParse(dto.EndDate, out var endDate))
            {
                return BadRequest("Invalid date format.");
            }

            return Json(new
            {
                id = Guid.NewGuid().ToString(),
                title = $"{dto.Title}: {dto.PatientName}",
                startDate = startDate.ToString("s"),
                endDate = endDate.ToString("s"),
                patientName = dto.PatientName
            });
        }
    }
}
