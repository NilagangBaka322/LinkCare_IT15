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
        public async Task<IActionResult> DoctorDashboard()
        {
            var doctorId = _userManager.GetUserId(User);
            var doctor = await _userManager.Users
                .Where(u => u.Id == doctorId)
                .Select(u => new ApplicationUser
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();

            var today = DateTime.Today;

            // 1️⃣ Today's scheduled appointments
            var todayAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.StartDate.Date == today &&
                            a.Status == AppointmentStatus.Scheduled)
                .CountAsync();

            // 2️⃣ Pending appointments (not completed or cancelled)
            var pendingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.Status != AppointmentStatus.Completed &&
                            a.Status != AppointmentStatus.Cancelled)
                .CountAsync();

            // 3️⃣ Patients under care
            var patientsUnderCare = await _context.Consultations
                .Where(c => c.DoctorId == doctorId && !c.IsArchived)
                .Select(c => c.PatientId)
                .Distinct()
                .CountAsync();

            // 4️⃣ Recent consultations (today)
            var recentConsultations = await _context.Consultations
                .Include(c => c.Patient)
                .Where(c => c.DoctorId == doctorId && c.Date.Date == today)
                .OrderByDescending(c => c.Date)
                .Take(5)
                .Select(c => new ActivityViewModel
                {
                    Label = "Consultation completed",
                    User = c.Patient != null
                        ? $"{c.Patient.FirstName} {c.Patient.LastName}"
                        : (c.WalkInName ?? "Walk-in Patient"),
                    Ago = DateTime.Now - c.Date
                })
                .ToListAsync();

            // 5️⃣ Upcoming appointments — show all SCHEDULED appointments for today
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId &&
                            a.Status == AppointmentStatus.Scheduled &&
                            a.StartDate.Date == today)
                .OrderBy(a => a.StartDate)
                .Select(a => new UpcomingAppointmentViewModel
                {
                    PatientName = a.Patient != null
                        ? $"{a.Patient.FirstName} {a.Patient.LastName}"
                        : (a.WalkInName ?? "Walk-in Patient"),
                    Time = a.StartDate.ToString("hh:mm tt"),
                    Title = a.Title ?? "Consultation",
                    Status = a.Status.ToString()
                })
                .ToListAsync();

            var model = new DoctorDashboardModel
            {
                Doctor = doctor,
                TodayAppointments = todayAppointments,
                PendingConsultations = pendingAppointments,
                TotalPatients = patientsUnderCare,
                RecentActivity = recentConsultations,
                UpcomingAppointments = upcomingAppointments
            };

            return View(model);
        }

        // ======================
        // Doctor Appointments
        // ======================
        public async Task<IActionResult> DoctorAppointments(string filter = null)
        {
            var doctorId = _userManager.GetUserId(User);

            var appointmentsQuery = _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && !a.IsArchived)
                .AsQueryable();

            // Apply filter if requested
            if (!string.IsNullOrEmpty(filter))
            {
                if (filter == "pending")
                {
                    appointmentsQuery = appointmentsQuery.Where(a =>
                        a.Status != AppointmentStatus.Completed && a.Status != AppointmentStatus.Cancelled);
                }
                else if (filter == "today")
                {
                    appointmentsQuery = appointmentsQuery.Where(a => a.StartDate.Date == DateTime.Today);
                }
            }

            var appointments = await appointmentsQuery
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    PatientName = a.Patient != null ? a.Patient.FirstName + " " + a.Patient.LastName : a.WalkInName,
                    Status = a.Status.ToString(),
                    Notes = a.Notes
                })
                .ToListAsync();

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

        [HttpPost]
        
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, status = appointment.Status.ToString() });
        }


        // ======================
        // Doctor Consultation
        //======================


        // Doctor Patients
        // ======================
        public async Task<IActionResult> DoctorPatients()
        {
            var doctorId = _userManager.GetUserId(User);

            // Step 1: Fetch appointments and related patients for this doctor
            var appointmentData = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && !a.IsArchived)
                .ToListAsync(); // Query executes here

            // Step 2: Group and project in memory
            var patients = appointmentData
                .GroupBy(a => new
                {
                    Id = a.Patient?.Id,
                    Name = a.Patient != null
                        ? $"{a.Patient.FirstName} {a.Patient.LastName}"
                        : (a.WalkInName ?? "Walk-in Patient")
                })
                .Select(g =>
                {
                    // Determine latest or next appointment for this patient
                    var nextAppointment = g
                        .Where(a => a.StartDate >= DateTime.Now)
                        .OrderBy(a => a.StartDate)
                        .FirstOrDefault();

                    // Determine prompt based on appointment statuses
                    string prompt;
                    if (g.Any(a => a.Status == AppointmentStatus.Completed))
                        prompt = "Completed";
                    else if (g.Any(a => a.Status == AppointmentStatus.Rescheduled))
                        prompt = "Rescheduled";
                    else if (g.Any(a => a.Status == AppointmentStatus.Scheduled))
                        prompt = "Scheduled";
                    else if (g.Any(a => a.Status == AppointmentStatus.Cancelled))
                        prompt = "Cancelled";
                    else
                        prompt = "Pending";

                    return new DoctorPatientViewModel
                    {
                        PatientName = g.Key.Name,
                        Status = prompt,
                        LastVisit = nextAppointment != null
                            ? nextAppointment.StartDate
                            : g.Max(a => a.StartDate)
                    };
                })
                .OrderByDescending(p => p.LastVisit)
                .ToList();

            var model = new DoctorPatientsModel
            {
                TotalPatients = patients.Count,
                Patients = patients
            };

            return View(model);
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
