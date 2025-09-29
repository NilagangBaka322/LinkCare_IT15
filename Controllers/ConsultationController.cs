using LinkCare_IT15.Data;
using LinkCare_IT15.Models.Entities;
using LinkCare_IT15.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LinkCare_IT15.Controllers
{
    [Authorize(Roles = "Doctor")]
    [Route("Doctor/[controller]/[action]")]   // 👈 Adds /Doctor/ prefix
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
        public async Task<IActionResult> Index()
        {
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var consultations = await _context.Consultations
                .Include(c => c.Patient)
                .Where(c => c.DoctorId == doctorId)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            var viewModel = new DoctorConsultationViewModel
            {
                Consultations = consultations
            };

            return View("~/Views/Doctor/DoctorConsultation.cshtml", viewModel);
        }

        // POST: /Doctor/Consultation/AddConsultation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsultation(DoctorConsultationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid consultation details.";
                return RedirectToAction("DoctorConsultation");
            }

            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var newConsult = new Consultation
            {
                DoctorId = doctorId,
                PatientId = string.IsNullOrEmpty(model.NewConsultation.PatientId) ? null : model.NewConsultation.PatientId,
                Date = DateTime.Now,
                ChiefComplaint = model.NewConsultation.ChiefComplaint,
                Diagnosis = model.NewConsultation.Diagnosis,
                Prescriptions = model.NewConsultation.Prescriptions,
                Notes = model.NewConsultation.Notes,
                BloodPressure = model.NewConsultation.BloodPressure,
                HeartRate = model.NewConsultation.HeartRate,
                Temperature = model.NewConsultation.Temperature,
                Weight = model.NewConsultation.Weight
            };

            _context.Consultations.Add(newConsult);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Consultation saved successfully!";
            return RedirectToAction("DoctorConsultation");
        }
    }

}
