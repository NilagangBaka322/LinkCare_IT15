using LinkCare_IT15.Data;
using LinkCare_IT15.Models.DoctorModel;
using LinkCare_IT15.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkCare_IT15.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class ConsultationController : Controller
    {
           private readonly ApplicationDbContext _context;
            private readonly UserManager<ApplicationUser> _userManager;

            public ConsultationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _userManager = userManager;
            }
        

        // ✅ Fetch consultations for logged-in doctor
        public async Task<IActionResult> Index()
        {
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var consultationsData = await _context.Consultations
                .Include(c => c.Doctor)
                .Include(c => c.Patient)
                .Where(c => c.DoctorId == doctorId)
                .ToListAsync();

            var consultations = consultationsData.Select(c => new ConsultationViewModel
            {
                ConsultationId = c.ConsultationId,
                PatientId = c.PatientId,
                DoctorId = c.DoctorId,
                Date = c.Date,
                ChiefComplaint = c.ChiefComplaint,
                Diagnosis = c.Diagnosis,
                Prescriptions = string.IsNullOrEmpty(c.Prescriptions)
                    ? new List<string>()
                    : c.Prescriptions.Split(',').ToList(),
                Notes = c.Notes,
                BloodPressure = c.BloodPressure,
                HeartRate = c.HeartRate,
                Temperature = c.Temperature,
                Weight = c.Weight,
                PatientName = c.Patient != null ? c.Patient.FullName : "Walk-in",
                DoctorName = c.Doctor?.FullName ?? "Unknown"
            }).ToList();

            var model = new DoctorConsultationPageViewModel
            {
                Consultations = consultations
            };

            return View("~/Views/Doctor/Consultations.cshtml", model);
        }

        // ✅ Save consultation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsultation(DoctorConsultationPageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid consultation details.";
                return RedirectToAction("Index");
            }

            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var newConsult = new Consultation
            {
                DoctorId = doctorId, // ✅ always from logged-in doctor
                PatientId = string.IsNullOrEmpty(model.NewConsultation.PatientId) ? null : model.NewConsultation.PatientId,
                Date = DateTime.Now,
                ChiefComplaint = model.NewConsultation.ChiefComplaint,
                Diagnosis = model.NewConsultation.Diagnosis,
                Prescriptions = model.NewConsultation.Prescriptions != null
                    ? string.Join(",", model.NewConsultation.Prescriptions)
                    : null,
                Notes = model.NewConsultation.Notes,
                BloodPressure = model.NewConsultation.BloodPressure,
                HeartRate = model.NewConsultation.HeartRate,
                Temperature = model.NewConsultation.Temperature,
                Weight = model.NewConsultation.Weight
            };

            _context.Consultations.Add(newConsult);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Consultation saved successfully!";
            return RedirectToAction("Index");
        }
    }
}
