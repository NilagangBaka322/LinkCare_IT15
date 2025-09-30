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
using CreateConsultationDto = LinkCare_IT15.Models.ViewModels.CreateConsultationDto;

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

        // GET: /Doctor/Consultation/DoctorConsultation
        [HttpGet]
        [ActionName("DoctorConsultation")]
        public async Task<IActionResult> Index(int? appointmentId)
        {
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Load all consultations of this doctor
            var consultations = await _context.Consultations
                .Include(c => c.Patient)
                .Where(c => c.DoctorId == doctorId && !c.IsArchived)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            // Prepare new consultation DTO
            var newConsultation = new CreateConsultationDto();

            if (appointmentId.HasValue)
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId.Value);

                if (appointment != null)
                {
                    newConsultation.AppointmentId = appointment.Id;
                    newConsultation.PatientId = appointment.PatientId; // linked patient
                }
            }

            var viewModel = new DoctorConsultationVM
            {
                Consultations = consultations.Select(c => new ConsultationRecordVM
                {
                    Id = c.Id,
                    PatientId = c.PatientId ?? string.Empty,
                        
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
                Patients = _context.Users.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName
                }).ToList()
            };

            return View("~/Views/Doctor/DoctorConsultation.cshtml", viewModel);
        }

        // POST: /Doctor/Consultation/AddConsultation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsultation(DoctorConsultationVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                TempData["Error"] = "Invalid consultation: " + errors;
                return RedirectToAction("DoctorConsultation", new { appointmentId = model.NewConsultation.AppointmentId });
            }

            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Convert list of prescriptions to single string
            string? prescriptionsText = null;
            if (model.NewConsultation.Prescriptions != null && model.NewConsultation.Prescriptions.Any())
            {
                prescriptionsText = string.Join(", ",
                    model.NewConsultation.Prescriptions
                        .Where(p => !string.IsNullOrWhiteSpace(p)));
            }

            var newConsult = new Consultation
            {
                DoctorId = doctorId,
                PatientId = string.IsNullOrEmpty(model.NewConsultation.PatientId) ? null : model.NewConsultation.PatientId,

                AppointmentId = model.NewConsultation.AppointmentId,
                Date = DateTime.Now,
                ChiefComplaint = model.NewConsultation.ChiefComplaint,
                Diagnosis = model.NewConsultation.Diagnosis,
                Prescriptions = prescriptionsText,
                Notes = model.NewConsultation.Notes,
                BloodPressure = model.NewConsultation.BloodPressure,
                HeartRate = model.NewConsultation.HeartRate,
                Temperature = model.NewConsultation.Temperature,
                Weight = model.NewConsultation.Weight,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Consultations.Add(newConsult);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Consultation saved successfully!";
            return RedirectToAction("DoctorConsultation", new { appointmentId = model.NewConsultation.AppointmentId });
        }
    }
}
