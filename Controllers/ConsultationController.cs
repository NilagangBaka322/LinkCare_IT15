
using LinkCare_IT15.Data;
using LinkCare_IT15.Models;
using LinkCare_IT15.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace LinkCare_IT15.Controllers
{
    [Authorize(Roles = "Doctor")]
    [Route("Doctor/[controller]")]
    public class ConsultationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConsultationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET
        [HttpGet]
        [ActionName("DoctorConsultation")]
        public async Task<IActionResult> Index(int? appointmentId)
        {
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var consultations = await _context.Consultations
                .Include(c => c.Patient)
                .Where(c => c.DoctorId == doctorId && !c.IsArchived)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            var newConsultation = new CreateConsultationDto();

            if (appointmentId.HasValue)
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId.Value);

                if (appointment != null)
                {
                    newConsultation.AppointmentId = appointment.Id;
                    newConsultation.PatientId = appointment.PatientId;
                }
            }

            var vm = new DoctorConsultationVM
            {
                Consultations = consultations.Select(c => new ConsultationEntityVM
                {
                    Id = c.Id,
                    PatientId = c.PatientId ?? string.Empty,
                    PatientName = c.Patient?.FullName ?? "Walk-in",
                    ChiefComplaint = c.ChiefComplaint,
                    Diagnosis = c.Diagnosis,
                    Prescriptions = c.Prescriptions ?? string.Empty,
                    Notes = c.Notes ?? string.Empty,
                    BloodPressure = c.BloodPressure ?? string.Empty,
                    HeartRate = c.HeartRate ?? string.Empty,
                    Temperature = c.Temperature ?? string.Empty,
                    Weight = c.Weight ?? string.Empty,
                    Date = c.Date
                }).ToList(),

                NewConsultation = newConsultation,

                Patients = (from user in _context.Users
                            join ur in _context.UserRoles on user.Id equals ur.UserId
                            join role in _context.Roles on ur.RoleId equals role.Id
                            where role.Name == "Patient"
                            select new SelectListItem
                            {
                                Value = user.Id,
                                Text = user.FullName
                            }).ToList()
            };

            return View("~/Views/Doctor/DoctorConsultation.cshtml", vm);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsultation(DoctorConsultationVM vm)
        {
            var model = vm.NewConsultation;

            // DEBUG: log all posted values
            Console.WriteLine("=== FORM DEBUG ===");
            Console.WriteLine($"ChiefComplaint: {model?.ChiefComplaint}");
            Console.WriteLine($"Diagnosis: {model?.Diagnosis}");
            Console.WriteLine($"PatientId: {model?.PatientId}");
            Console.WriteLine($"AppointmentId: {model?.AppointmentId}");
            Console.WriteLine($"Prescriptions: {string.Join(", ", model?.Prescriptions ?? new List<string>())}");
            Console.WriteLine($"Notes: {model?.Notes}");
            Console.WriteLine($"BP: {model?.BloodPressure}");
            Console.WriteLine($"HR: {model?.HeartRate}");
            Console.WriteLine($"Temp: {model?.Temperature}");
            Console.WriteLine($"Weight: {model?.Weight}");
            Console.WriteLine("==================");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                Console.WriteLine("Validation errors: " + string.Join(" | ", errors));

                // repopulate Patients
                vm.Patients = (from user in _context.Users
                               join ur in _context.UserRoles on user.Id equals ur.UserId
                               join role in _context.Roles on ur.RoleId equals role.Id
                               where role.Name == "Patient"
                               select new SelectListItem
                               {
                                   Value = user.Id,
                                   Text = user.FullName
                               }).ToList();

                return View("~/Views/Doctor/DoctorConsultation.cshtml", vm);
            }

            // if everything looks good, save
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var consultation = new Consultation
            {
                DoctorId = doctorId,
                PatientId = string.IsNullOrEmpty(model.PatientId) ? null : model.PatientId,
                AppointmentId = model.AppointmentId,
                Date = DateTime.Now,
                ChiefComplaint = model.ChiefComplaint,
                Diagnosis = model.Diagnosis,
                Prescriptions = string.Join(", ", model.Prescriptions ?? new List<string>()),
                BloodPressure = model.BloodPressure,
                HeartRate = model.HeartRate,
                Temperature = model.Temperature,
                Weight = model.Weight,
                Notes = model.Notes
            };

            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Consultation saved successfully!";
            return RedirectToAction("DoctorConsultation", new { appointmentId = model.AppointmentId });
        }

    }



}

