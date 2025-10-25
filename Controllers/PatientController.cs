using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkCare_IT15.Models.PatientModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using LinkCare_IT15.Data;
using LinkCare_IT15.Models;
using System.Security.Claims;

namespace LinkCare_IT15.Controllers
{
    [Authorize(Roles = "Patient")] 
    public class PatientController : Controller

    {
        private readonly ApplicationDbContext _context;
        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> PatientDashboard()
        {
            ViewData["ActivePage"] = "Dashboard";

            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ----------------------
            // NEXT APPOINTMENT
            // ----------------------
            var nextAppointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId &&
                            a.Status == AppointmentStatus.Scheduled &&
                            a.StartDate >= DateTime.Now)
                .OrderBy(a => a.StartDate)
                .Select(a => new LinkCare_IT15.Models.PatientModel.AppointmentViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Start = a.StartDate,
                    End = a.EndDate,
                    Doctor = "Dr. " + a.Doctor.FirstName + " " + a.Doctor.LastName
                })
                .FirstOrDefaultAsync();

            // ----------------------
            // UPCOMING APPOINTMENTS (Next 5)
            // ----------------------
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId &&
                            a.Status == AppointmentStatus.Scheduled &&
                            a.StartDate >= DateTime.Now)
                .OrderBy(a => a.StartDate)
                .Take(5)
                .Select(a => new LinkCare_IT15.Models.PatientModel.AppointmentViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Start = a.StartDate,
                    End = a.EndDate,
                    Doctor = "Dr. " + a.Doctor.FirstName + " " + a.Doctor.LastName
                })
                .ToListAsync();

            // ----------------------
            // MEDICAL RECORDS
            // Count completed consultations for this patient
            // ----------------------
            var completedConsultationsCount = await _context.Consultations
                .Where(c => c.PatientId == patientId)
                .CountAsync();

            // ----------------------
            // BILLING
            // Fetch pending bills (Status != "Paid") using in-memory filtering
            // ----------------------
            var pendingBills = _context.Billings
                .Include(b => b.Transactions)
                .Where(b => b.PatientID == patientId)
                .AsEnumerable() // switch to in-memory
                .Where(b => b.Status != "Paid") // now we can use Status
                .ToList();

            var totalPendingAmount = pendingBills.Sum(b => b.RemainingBalance);
            var pendingCount = pendingBills.Count;

            // ----------------------
            // PREPARE MODEL
            // ----------------------
            var model = new LinkCare_IT15.Models.PatientModel.PatientDashboardModel
            {
                NextAppointment = nextAppointment,
                UpcomingAppointments = upcomingAppointments ?? new List<LinkCare_IT15.Models.PatientModel.AppointmentViewModel>(),

                // Dynamic boxes
                TotalConsultations = completedConsultationsCount,
                OutstandingBills = totalPendingAmount,
                PendingPayments = pendingCount
            };

            return View(model);
        }



        public async Task<IActionResult> PatientAppointments()
        {
            ViewData["ActivePage"] = "Appointments";

            // Fetch confirmed & active doctors
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsActive && d.User.EmailConfirmed)
                .ToListAsync();

            return View(doctors); // This will stay largely the same
        }

        [HttpGet]
        public async Task<IActionResult> BookAppointment(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
                return NotFound();

            // Load booking form or redirect to appointment creation page
            return View("BookAppointment", doctor);
        }


        [HttpGet]
        public async Task<IActionResult> PatientRecords(string search)
        {
            ViewData["ActivePage"] = "Records"; // highlight sidebar
            ViewBag.SearchQuery = search;       // keep search value in input

            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch consultations for this patient
            var consultationsQuery = _context.Consultations
                .Include(c => c.Doctor)
                .Include(c => c.Appointment)
                .Where(c => c.PatientId == patientId);

            // Apply search filter (by Doctor or Diagnosis)
            if (!string.IsNullOrEmpty(search))
            {
                consultationsQuery = consultationsQuery.Where(c =>
                    (c.Doctor.FirstName + " " + c.Doctor.LastName).Contains(search) ||
                    c.Diagnosis.Contains(search));
            }

            // Execute query
            var consultations = await consultationsQuery
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            // Map to your RecordViewModel
            var records = consultations.Select(c => new RecordViewModel
            {
                PatientName = c.Patient != null ? $"{c.Patient.FirstName} {c.Patient.LastName}"
                                                : c.WalkInName ?? "Walk-in",
                DoctorName = c.Doctor != null ? $"Dr. {c.Doctor.FirstName} {c.Doctor.LastName}" : "N/A",
                ConsultationDate = c.Date,
                ChiefComplaint = c.ChiefComplaint,
                Diagnosis = c.Diagnosis,
                Prescriptions = string.IsNullOrEmpty(c.Prescriptions)
                                    ? new List<string>()
                                    : c.Prescriptions.Split(',').Select(p => p.Trim()).ToList(),
                BloodPressure = c.BloodPressure,
                HeartRate = string.IsNullOrEmpty(c.HeartRate) ? 0 : int.Parse(c.HeartRate),
                Temperature = string.IsNullOrEmpty(c.Temperature) ? 0 : double.Parse(c.Temperature),
                Weight = string.IsNullOrEmpty(c.Weight) ? 0 : double.Parse(c.Weight),
                Notes = c.Notes
            }).ToList();

            return View(records);
        }


        public async Task<IActionResult> PatientBilling(string search, string statusFilter)
        {
            ViewData["ActivePage"] = "Billing";

            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch all billings for this patient
            var billings = await _context.Billings
                .Include(b => b.Transactions)
                .Include(b => b.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Include(b => b.Patient)
                .Where(b => b.PatientID == patientId)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                billings = billings.Where(b =>
                    (b.Appointment?.Doctor != null &&
                     $"{b.Appointment.Doctor.FirstName} {b.Appointment.Doctor.LastName}".ToLower().Contains(search))
                    || (b.WalkInName != null && b.WalkInName.ToLower().Contains(search))
                ).ToList();

                ViewData["CurrentSearch"] = search;
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                billings = billings.Where(b => b.Status.ToLower() == statusFilter.ToLower()).ToList();
                ViewData["CurrentStatus"] = statusFilter;
            }

            // Map to view model
            var billsVM = billings.Select(b => new BillingViewModel
            {
                BillId = b.BillingID,
                DoctorName = b.Appointment?.Doctor != null
                    ? $"Dr. {b.Appointment.Doctor.FirstName} {b.Appointment.Doctor.LastName}"
                    : "N/A",
                BillDate = b.BillingDate,
                Services = new List<string> { "Consultation Fee" }, // Optionally fetch services if you have them
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                PaymentDate = b.Transactions.OrderByDescending(t => t.TransactionDate).FirstOrDefault()?.TransactionDate
            }).ToList();

            // Billing summary
            var summary = new BillingSummaryViewModel
            {
                TotalPaid = billings.Sum(b => Math.Min(b.AmountPaid, b.TotalAmount)),
                Pending = billings.Sum(b => b.RemainingBalance),
                TotalBills = billings.Count
            };

            ViewBag.Summary = summary;
            return View(billsVM);
        }

    }
}
