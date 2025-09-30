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

        // ======================
        // Index / Schedule
        // ======================
        public async Task<IActionResult> Index()
        {
            var patients = await (from u in _context.Users
                                  join ur in _context.UserRoles on u.Id equals ur.UserId
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  where r.Name == "Patient"
                                  select new SelectListItem
                                  {
                                      Value = u.Id,
                                      Text = string.IsNullOrEmpty(u.FirstName) && string.IsNullOrEmpty(u.LastName)
                                             ? u.UserName
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
                RecentActivity = new List<ActivityViewModel>
                {
                    new ActivityViewModel { Label="Consultation completed", User="John Doe", Ago=TimeSpan.FromHours(1)},
                    new ActivityViewModel { Label="Prescription added", User="Jane Smith", Ago=TimeSpan.FromHours(3)}
                }
            };
            return View(model);
        }

        // ======================
        // Doctor Appointments
        // ======================
        public async Task<IActionResult> DoctorAppointments()
        {
            var doctorId = _userManager.GetUserId(User);

            var patientRoleId = await _context.Roles
                .Where(r => r.Name == "Patient")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var patients = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == patientRoleId))
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FirstName + " " + u.LastName
                })
                .ToListAsync();

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
        // ======================
        // Appointment Actions
        // ======================
        public async Task<IActionResult> RescheduleAppointment(int id, [FromBody] RescheduleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewDate))
                return BadRequest("Invalid date.");

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            appointment.StartDate = DateTime.Parse(dto.NewDate);
            appointment.Status = AppointmentStatus.Rescheduled;
            appointment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Json(new { success = true, newStatus = appointment.Status.ToString() });
        }

        public IActionResult CancelAppointment(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt == null)
                return Json(new { success = false, message = "Appointment not found" });

            appt.Status = AppointmentStatus.Cancelled;
            _context.SaveChanges();
            return Json(new { success = true });
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

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.StartDate) ||
                string.IsNullOrWhiteSpace(dto.EndDate))
            {
                return BadRequest("Missing required fields.");
            }

            if (string.IsNullOrWhiteSpace(dto.PatientId) && string.IsNullOrWhiteSpace(dto.WalkInName))
                return BadRequest("Either PatientId or WalkInName must be provided.");

            var appointment = new Appointment
            {
                Title = dto.Title.Trim(),
                StartDate = DateTime.Parse(dto.StartDate),
                EndDate = DateTime.Parse(dto.EndDate),
                DoctorId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Status = AppointmentStatus.Scheduled
            };

            if (!string.IsNullOrEmpty(dto.PatientId))
                appointment.PatientId = dto.PatientId;
            else
                appointment.WalkInName = dto.WalkInName?.Trim();

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var savedAppointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            return Json(new
            {
                id = savedAppointment.Id,
                title = savedAppointment.Title,
                patientName = savedAppointment.Patient != null
                                ? (savedAppointment.Patient.FirstName + " " + savedAppointment.Patient.LastName).Trim()
                                : null,
                walkInName = savedAppointment.WalkInName,
                startDate = savedAppointment.StartDate.ToString("s"),
                endDate = savedAppointment.EndDate.ToString("s"),
                status = savedAppointment.Status.ToString()
            });
        }

        // ======================
        // Doctor Consultation
        //======================
        

        // ======================
        // Doctor Patients
        // ======================
        public IActionResult DoctorPatients()
        {
            return View(new DoctorPatientsModel { Patients = new List<DoctorPatientViewModel>() });
        }

        // ======================
        // Doctor Medical Records
        // ======================
        public async Task<IActionResult> DoctorMedicalRecords()
        {
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var consultations = await _context.Consultations
                .Include(c => c.Patient)
                .Where(c => c.DoctorId == doctorId)
                .ToListAsync();

            return View(consultations);
        }
    }
}
