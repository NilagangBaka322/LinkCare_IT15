using LinkCare_IT15.Data;
using LinkCare_IT15.Models;
using LinkCare_IT15.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

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

            // Get consultations by doctor
            var consultations = await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Appointment)
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
                    newConsultation.PatientId = appointment.PatientId;
                    newConsultation.WalkInName = appointment.WalkInName;
                }
            }

            var viewModel = new DoctorConsultationVM
            {
                Consultations = consultations.Select(c => new ConsultationRecordVM
                {
                    Id = c.Id,
                    AppointmentId = c.AppointmentId,
                    PatientId = c.PatientId,
                    PatientName = c.Patient != null
                        ? $"{c.Patient.FirstName} {c.Patient.LastName}"
                        : (c.Appointment != null ? c.Appointment.WalkInName ?? "Walk-in" : "Walk-in"),
                    WalkInName = c.Appointment?.WalkInName,
                    ChiefComplaint = c.ChiefComplaint,
                    Diagnosis = c.Diagnosis,
                    Prescriptions = c.Prescriptions,
                    Notes = c.Notes,
                    BloodPressure = c.BloodPressure,
                    HeartRate = c.HeartRate,
                    Temperature = c.Temperature,
                    Weight = c.Weight,
                    ConsultationFee = c.ConsultationFee,
                    Date = c.Date
                }).ToList(),

                NewConsultation = newConsultation,
                Patients = _context.Users.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName}"
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
                TempData["Error"] = "Invalid consultation data.";
                return RedirectToAction("DoctorConsultation", new { appointmentId = model.NewConsultation.AppointmentId });
            }

            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Join prescriptions list
            string? prescriptionsText = null;
            if (model.NewConsultation.Prescriptions != null && model.NewConsultation.Prescriptions.Any())
            {
                prescriptionsText = string.Join(", ",
                    model.NewConsultation.Prescriptions.Where(p => !string.IsNullOrWhiteSpace(p)));
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
                ConsultationFee = model.NewConsultation.ConsultationFee,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // Add walk-in name if applicable
            if (!string.IsNullOrWhiteSpace(model.NewConsultation.WalkInName))
            {
                newConsult.Appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == model.NewConsultation.AppointmentId);

                if (newConsult.Appointment != null)
                {
                    newConsult.Appointment.WalkInName = model.NewConsultation.WalkInName;
                }
            }

            _context.Consultations.Add(newConsult);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Consultation saved successfully!";
            return RedirectToAction("DoctorConsultation", new { appointmentId = model.NewConsultation.AppointmentId });
        }
    }
}
