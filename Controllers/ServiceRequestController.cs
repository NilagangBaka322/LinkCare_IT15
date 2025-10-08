using LinkCare_IT15.Data;
using LinkCare_IT15.Models;
using LinkCare_IT15.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LinkCare_IT15.Controllers
{
    [Authorize]
    [Route("Patient/[action]")]
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===============================
        //  PATIENT SERVICES (UI + REQUEST)
        // ===============================
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> PatientServices()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var services = await _context.Services
                .Where(s => s.IsActive)
                .ToListAsync();

            var myRequests = await _context.ServiceRequests
                .Include(r => r.Service)
                .Where(r => r.PatientId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.MyRequests = myRequests;
            return View("~/Views/Patient/PatientServices.cshtml", services);
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestService(int serviceId, DateTime startDate, DateTime endDate, string notes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (serviceId <= 0 || startDate == default || endDate == default)
            {
                TempData["ErrorMessage"] = "Invalid request details. Please fill in all required fields.";
                return RedirectToAction(nameof(PatientServices));
            }

            var serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == serviceId);
            if (!serviceExists)
            {
                TempData["ErrorMessage"] = "The selected service does not exist.";
                return RedirectToAction(nameof(PatientServices));
            }

            var request = new ServiceRequest
            {
                PatientId = user.Id,
                ServiceId = serviceId,
                StartDate = startDate,
                EndDate = endDate,
                Notes = notes,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                IsArchived = false
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your service request has been submitted successfully!";
            return RedirectToAction(nameof(PatientServices));
        }

        // ===============================
        //  ADMIN: VIEW ALL REQUESTS + ASSIGN DOCTOR
        // ===============================
        [Authorize(Roles = "Admin")]
        [Route("/Admin/OnlineAppointment")]
        public async Task<IActionResult> OnlineAppointment()
        {
            var requests = await _context.ServiceRequests
                .Include(r => r.Service)
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Load all active doctors
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            ViewBag.Doctors = doctors;

            return View("~/Views/Admin/OnlineAppointment.cshtml", requests);
        }

        // ===============================
        //  ADMIN: UPDATE STATUS
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var req = await _context.ServiceRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.Status = status;
            req.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Request status updated successfully.";
            return RedirectToAction(nameof(OnlineAppointment));
        }

        // ===============================
        //  ADMIN: ASSIGN DOCTOR
        // ===============================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AssignDoctor(int requestId, string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId))
            {
                TempData["ErrorMessage"] = "Please select a doctor.";
                return RedirectToAction("OnlineAppointment");
            }

            var req = await _context.ServiceRequests
                .Include(s => s.Patient)
                .Include(s => s.Service)
                .FirstOrDefaultAsync(s => s.RequestId == requestId);

            if (req == null)
            {
                TempData["ErrorMessage"] = "Service request not found.";
                return RedirectToAction("OnlineAppointment");
            }

            // Assign doctor
            req.DoctorId = doctorId;
            req.Status = "Approved";
            _context.Update(req);

            // Create appointment automatically
            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = req.PatientId,
                WalkInName = null, // could be req.WalkInName if applicable
                Title = req.Service?.ServiceName ?? "Service",
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                Status = AppointmentStatus.Scheduled, // enum
                Notes = req.Notes,
                IsArchived = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ServiceId = req.ServiceId,
                RequestId = req.RequestId
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Doctor assigned and appointment created.";
            return RedirectToAction("OnlineAppointment");
        }

    }
}
