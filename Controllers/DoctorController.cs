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
        public async Task<IActionResult> Index()
        {
            // Get patients only
            var patients = await (from u in _context.Users
                                  join ur in _context.UserRoles on u.Id equals ur.UserId
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  where r.Name == "Patient"
                                  select new SelectListItem
                                  {
                                      Value = u.Id,
                                      Text = string.IsNullOrEmpty(u.FirstName) && string.IsNullOrEmpty(u.LastName)
                                             ? u.UserName // fallback if names are empty
                                             : (u.FirstName + " " + u.LastName).Trim()
                                  }).ToListAsync();


            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => !a.IsArchived)
                .Select(a => new AppointmentViewModel
                {
                    PatientId = a.PatientId,
                    PatientName = a.Patient != null ? a.Patient.FirstName + " " + a.Patient.LastName : a.WalkInName,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Title = a.Title,
                    Notes = a.Notes,
                    Status = a.Status.ToString()
                }).ToListAsync();

            var vm = new DoctorAppointmentsModel
            {
                Schedule = new DoctorScheduleViewModel
                {
                    Appointments = appointments,
                    NewAppointment = new AppointmentViewModel(),
                    Patients = patients
                }
            };

            return View("DoctorAppointments", vm);
        }
        //======================
        // Doctor Dashboard
        //======================
        public IActionResult DoctorDashboard()
        {
            var model = new DoctorDashboardModel
            {
                DoctorName = User.Identity.Name,
                TodayAppointments = 5,
                PendingConsultations = 2,
                TotalPatients = 48,
                RecentActivity = new List<ActivityViewModel>
                {
                    new ActivityViewModel { Label="Consultation completed", User="John Doe", Ago=TimeSpan.FromHours(1)},
                    new ActivityViewModel { Label="Prescription added", User="Jane Smith", Ago=TimeSpan.FromHours(3)}
                }
            };
            return View(model);
        }

        //======================
        // Doctor Appointments
        //======================
        // In DoctorController
        public async Task<IActionResult> DoctorAppointments()
        {
            // Get current logged-in doctor
            var doctorId = _userManager.GetUserId(User);

            // Get all patients in role Patient
            var patientRoleId = await _context.Roles
                .Where(r => r.Name == "Patient")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var patients = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == patientRoleId))
                .Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = u.Id,
                    Text = u.FirstName + " " + u.LastName
                })
                .ToListAsync();

            // Load doctor's appointments
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && !a.IsArchived)
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    PatientName = a.Patient != null ? a.Patient.FirstName + " " + a.Patient.LastName : null,
                    WalkInName = a.WalkInName,
                    Status = a.Status.ToString(),
                    Notes = a.Notes
                })
                .ToListAsync();

            var model = new DoctorAppointmentsModel
            {
                Schedule = new DoctorScheduleViewModel
                {
                    Appointments = appointments,
                    Patients = patients
                }
            };

            return View(model);
        }

        // 🔍 Search Patients (for autocomplete)
        [HttpGet]
        public async Task<IActionResult> SearchPatients(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new { });

            var patientRoleId = await _context.Roles
                .Where(r => r.Name == "Patient")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var patients = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == patientRoleId)
                            && (u.FirstName + " " + u.LastName).Contains(query))
                .Select(u => new
                {
                    id = u.Id,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    contact = u.PhoneNumber
                })
                .Take(10)
                .ToListAsync();

            return Json(patients);
        }

        // ➕ Create Appointment
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            if (dto == null) return BadRequest();

            var doctorId = _userManager.GetUserId(User);

            var appointment = new Appointment
            {
                Title = dto.Title,
                StartDate = DateTime.Parse(dto.StartDate),
                EndDate = DateTime.Parse(dto.EndDate),
                DoctorId = doctorId,
                PatientId = string.IsNullOrEmpty(dto.PatientId) ? null : dto.PatientId,
                WalkInName = dto.WalkInName,
                Status = AppointmentStatus.Scheduled,
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var patientName = appointment.PatientId != null
                ? (await _context.Users.FindAsync(appointment.PatientId))?.FirstName + " " +
                  (await _context.Users.FindAsync(appointment.PatientId))?.LastName
                : null;

            return Json(new
            {
                id = appointment.Id,
                patientName,
                appointment.WalkInName,
                appointment.Status
            });
        }

        //======================
        // Doctor Consultation
        //======================
        public IActionResult DoctorConsultation()
        {
            return View(new List<ConsultationViewModel>());
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
                Prescriptions = model.Prescriptions != null ? string.Join(",", model.Prescriptions) : null,
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

        //======================
        // Doctor Patients
        //======================
        public IActionResult DoctorPatients()
        {
            return View(new DoctorPatientsModel { Patients = new List<DoctorPatientViewModel>() });
        }

        //======================
        // Doctor Medical Records
        //======================
        public IActionResult DoctorMedicalRecords()
        {
            return View(new List<ConsultationViewModel>());
        }
    }
}
