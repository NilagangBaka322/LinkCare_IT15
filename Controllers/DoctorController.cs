using LinkCare_IT15.Data;
using LinkCare_IT15.Models;
using LinkCare_IT15.Models.DoctorModel;
using LinkCare_IT15.Models.Entities;
using LinkCare_IT15.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

         //======================
         //Doctor Dashboard
         //======================
        public IActionResult DoctorDashboard()
        {
            var model = new DoctorDashboardModel
            {
                DoctorName = User.Identity.Name,
                TodayAppointments = 5,
                PendingConsultations = 2,
                TotalPatients = 48,
                //UpcomingAppointments = new List<DoctorAppointmentViewModel>
                //{
                //    new DoctorAppointmentViewModel { PatientName="John Doe", Title="Routine Checkup", Status="Scheduled"},
                //    new DoctorAppointmentViewModel { PatientName="Jane Smith", Title="Follow-up", Status="Scheduled"}
                //},
                RecentActivity = new List<ActivityViewModel>
                {
                    new ActivityViewModel { Label="Consultation completed", User="John Doe", Ago=TimeSpan.FromHours(1)},
                    new ActivityViewModel { Label="Prescription added", User="Jane Smith", Ago=TimeSpan.FromHours(3)},
                }
            };
            return View(model);
        }
        //======================
        //Doctor Appointments
        //======================
        public async Task<IActionResult> DoctorAppointments()
        {
            var doctorId = _userManager.GetUserId(User);
            var today = DateTime.Today;

            // All appointments for this doctor
            var allAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Include(a => a.Patient)
                .OrderBy(a => a.StartDate)
                .ToListAsync();

            // Today's appointments for schedule card
            var todayAppointments = allAppointments
                .Where(a => a.StartDate.Date == today)
                .ToList();

            // Map to AppointmentViewModel for Razor
            var scheduleVm = new DoctorScheduleViewModel
            {
                Appointments = todayAppointments.Select(a => new AppointmentViewModel
                {
                    PatientId = a.PatientId,
                    PatientName = a.Patient != null
                        ? $"{a.Patient.FirstName} {a.Patient.LastName}"
                        : a.WalkInName,
                    WalkInName = a.WalkInName,
                    Title = a.Title,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Notes = a.Notes,
                    Status = a.Status.ToString()
                }).ToList(),
                NewAppointment = new AppointmentViewModel()
            };

            // Map appointments for FullCalendar
            var calendarAppointments = allAppointments.Select(a => new
            {
                id = a.Id,
                title = $"{a.Title} - {(a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : a.WalkInName)}",
                start = a.StartDate.ToString("MM/dd/yyyy HH:mm"),
                end = a.EndDate.ToString("MM/dd/yyyy HH:mm"),
                status = a.Status.ToString()
            }).ToList();

            var model = new DoctorAppointmentsModel
            {
                Appointments = allAppointments,
                Schedule = scheduleVm
            };

            // Pass JSON to ViewBag for FullCalendar
            ViewBag.CalendarAppointments = calendarAppointments;

            return View(model);
        }


        // ======================
        // New Appointment
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewAppointment(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await DoctorAppointments(); // reload with errors
            }

            var doctorId = _userManager.GetUserId(User);

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = string.IsNullOrEmpty(model.PatientId) ? null : model.PatientId,
                WalkInName = string.IsNullOrEmpty(model.PatientId) ? model.PatientName : null,
                Title = model.Title,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = AppointmentStatus.Scheduled,
                Notes = model.Notes,
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DoctorAppointments));
        }

        // ======================
        // Create Appointment API
        // ======================
        [HttpPost]
        [Route("Doctor/CreateAppointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title)
                || string.IsNullOrWhiteSpace(dto.StartDate) || string.IsNullOrWhiteSpace(dto.EndDate))
            {
                return BadRequest("Invalid appointment data.");
            }

            if (!DateTime.TryParse(dto.StartDate, out var startDate) ||
                !DateTime.TryParse(dto.EndDate, out var endDate))
            {
                return BadRequest("Invalid date format.");
            }

            var doctorId = _userManager.GetUserId(User);

            string patientName = dto.WalkInName;
            string patientId = null;

            if (!string.IsNullOrEmpty(dto.PatientId))
            {
                var patient = await _userManager.FindByIdAsync(dto.PatientId);
                if (patient == null)
                    return BadRequest("Invalid PatientId.");

                patientId = patient.Id;
                patientName = $"{patient.FirstName} {patient.LastName}";
            }

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = patientId,
                WalkInName = patientId == null ? dto.WalkInName : null,
                Title = dto.Title,
                StartDate = startDate,
                EndDate = endDate,
                Status = AppointmentStatus.Scheduled,
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                id = appointment.Id,
                title = $"{appointment.Title}: {patientName}",
                startDate = appointment.StartDate.ToString("s"),
                endDate = appointment.EndDate.ToString("s"),
                patientName
            });
        }

        // ======================
        // Doctor Consultation
        // ======================
        public IActionResult DoctorConsultation()
        {
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
                }
            };

            return View(records);
        }

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
    }
}
