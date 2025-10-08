using LinkCare_IT15.Models.AdminModel;
using LinkCare_IT15.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LinkCare_IT15.Data;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using System.Linq;
using LinkCare_IT15.Models;

namespace LinkCare_IT15.Controllers
{

    [Authorize(Roles = "Admin")] // restrict to admins



    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        RoleManager<IdentityRole> roleManager,
        IEmailSender emailSender,
        ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _logger = logger;
        }


        public async Task<IActionResult> AdminDashboard()
        {
            ViewData["ActivePage"] = "Dashboard";

            var today = DateTime.Today;

            // ✅ Count total patients (users in "Patient" role)
            var patientRoleId = await _context.Roles
                .Where(r => r.Name == "Patient")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var totalPatients = await _context.UserRoles
                .Where(ur => ur.RoleId == patientRoleId)
                .CountAsync();

            // ✅ Count total appointments
            var totalAppointments = await _context.Appointments.CountAsync();

            // ✅ Today's appointments
            var todayAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.StartDate.Date == today)
                .OrderBy(a => a.StartDate)
                .Select(a => new AdminAppointmentViewModel
                {
                    PatientName = a.Patient != null
                        ? $"{a.Patient.FirstName} {a.Patient.LastName}"
                        : a.WalkInName ?? "Walk-in Patient",
                    Title = a.Title,
                    Start = a.StartDate,
                    Status = a.Status.ToString()
                })
                .ToListAsync();

            // ✅ Accurate Monthly Revenue (sum of all billings this month)
            var monthlyRevenue = await _context.Billings
                .Where(b =>
                    b.BillingDate.Month == DateTime.Now.Month &&
                    b.BillingDate.Year == DateTime.Now.Year
                )
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            // ✅ Pending billings (not yet fully paid)
            var pendingBillsQuery = await _context.Billings
                .Include(b => b.Transactions)
                .ToListAsync();

            var pendingBills = pendingBillsQuery
                .GroupBy(b => new {
                    PatientID = b.PatientID ?? "",
                    WalkInName = b.WalkInName ?? "",
                    AppointmentId = b.AppointmentId ?? 0,
                    TotalAmount = b.TotalAmount
                })
                .Select(g => g.First())
                .Where(b =>
                    b.Transactions.Sum(t => t.AmountPaid) < b.TotalAmount // still unpaid
                )
                .ToList();

            var pendingAmount = pendingBills.Sum(b => b.TotalAmount - b.Transactions.Sum(t => t.AmountPaid));

            // ✅ Recent Activity (last 5 consultations)
            var recentActivities = await _context.Consultations
                .Include(c => c.Patient)
                .OrderByDescending(c => c.Date)
                .Take(5)
                .Select(c => new AdminActivityViewModel
                {
                    Label = "Consultation completed",
                    User = c.Patient != null
                        ? $"{c.Patient.FirstName} {c.Patient.LastName}"
                        : c.WalkInName ?? "Walk-in Patient"
                })
                .ToListAsync();

            // ✅ Create dashboard view model
            var model = new AdminDashboardModel
            {
                TotalAppointments = totalAppointments,
                ScheduledAppointments = todayAppointments.Count(a => a.Status == "Scheduled"),
                TotalPatients = totalPatients,
                MonthlyRevenue = monthlyRevenue, // ✅ use decimal, no cast to int
                RevenueGrowth = 12, // optional placeholder
                PendingBills = pendingBills.Count(),
                PendingAmount = pendingAmount,
                TodayAppointments = todayAppointments,
                RecentActivity = recentActivities
            };

            return View(model);
        }



        // ---------------- Appointments Page ----------------


        // ---------------- Patient Registration ----------------
        public IActionResult PatientRegistration()
        {
            return View(new PatientRegistrationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PatientRegistration(PatientRegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save to DB
                return RedirectToAction("Patients");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Reports(string reportType = "Overview")
        {
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // ===== 1️⃣ Revenue Data (Monthly Payments) =====
            var revenueData = await _context.Transactions
                .Where(t => t.TransactionType == "Payment")
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(t => t.AmountPaid)
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToListAsync();

            var revenues = revenueData.Select(r => new RevenueData
            {
                Month = new DateTime(r.Year, r.Month, 1).ToString("MMMM yyyy"),
                Amount = r.Amount
            }).ToList();
           
   

            // ===== 2️⃣ Patient Analytics =====
            var totalPatients = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where role.Name == "Patient"
                select user
            ).CountAsync();

            var newPatients = await _context.Consultations
                .Where(c => c.PatientId != null)
                .GroupBy(c => c.PatientId)
                .Where(g => g.Min(c => c.Date).Month == DateTime.Now.Month &&
                            g.Min(c => c.Date).Year == DateTime.Now.Year)
                .CountAsync();

            var returningPatients = await _context.Consultations
                .Where(c => c.PatientId != null)
                .GroupBy(c => c.PatientId)
                .Where(g => g.Count() > 1)
                .CountAsync();

            var followUps = await _context.Consultations
                .Where(c => c.Date >= DateTime.Now.AddDays(-30))
                .CountAsync();

            var patientAnalytics = new List<PatientAnalyticsData>
    {
        new PatientAnalyticsData { Category = "New Patients", Count = newPatients },
        new PatientAnalyticsData { Category = "Returning Patients", Count = returningPatients },
        new PatientAnalyticsData { Category = "Follow-ups", Count = followUps },
        new PatientAnalyticsData { Category = "Total Patients", Count = totalPatients }
    };

            // ===== 3️⃣ Financial Data =====
            // Total billings (sum of all bill amounts from Billings table)
            decimal totalBilling = await _context.Billings
                .Select(b => (decimal?)b.TotalAmount)
                .SumAsync() ?? 0m;

            // Actual revenue (payments made)
            var monthlyRevenue = await _context.Billings
            .Where(b =>
                b.BillingDate.Month == DateTime.Now.Month &&
                b.BillingDate.Year == DateTime.Now.Year
            )
            .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;


            // Equipment expenses
            decimal totalEquipmentCost = await _context.Equipments
                .Where(e => !e.IsArchived)
                .SumAsync(e => (decimal?)e.PurchaseCost * e.Quantity) ?? 0m;

            decimal totalConsumableCost = await _context.ConsumableBatches
                 .Where(b => !b.IsArchived) // only active batches
                 .SumAsync(b => (decimal?)b.Quantity * b.UnitCost) ?? 0m;

            // ✅ Compute Net Revenue accurately
            decimal netRevenue = monthlyRevenue - (totalEquipmentCost + totalConsumableCost);
            if (netRevenue < 0) netRevenue = 0;

            // ===== 4️⃣ Operational Stats =====
            var totalAppointments = await _context.Appointments.CountAsync();
            var totalConsultations = await _context.Consultations.CountAsync();
            var activeDoctors = await _context.Doctors.CountAsync(d => d.IsActive);


            var equipmentDetails = await _context.Equipments
                .Where(e => !e.IsArchived)
                .Select(e => new EquipmentDetailViewModel
                {
                    Name = e.EquipmentName,
                    Category = e.Category,
                    Quantity = e.Quantity,
                    PurchaseCost = e.PurchaseCost,
           
                
                }).ToListAsync();

            // ✅ Get Consumable Details (sum quantities from batches)
            var consumableDetails = await _context.Consumables
                .Where(c => !c.IsArchived)
                .Select(c => new ConsumableDetailViewModel
                {
                    Name = c.ConsumableName,
                    Category = c.Category,
                    UnitCost = c.UnitCost,
                    TotalQuantity = c.Batches.Sum(b => b.Quantity)
                }).ToListAsync();

            // ===== 5️⃣ Construct ViewModel =====
            var model = new ReportsViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                ReportType = reportType,
                Revenues = revenues,
                Patients = patientAnalytics,
                TotalBilling = totalBilling,
                TotalRevenue = monthlyRevenue,
                TotalEquipmentCost = totalEquipmentCost,
                TotalConsumableCost = totalConsumableCost,
                TotalAppointments = totalAppointments,
                TotalConsultations = totalConsultations,
                TotalPatients = totalPatients,
                ActiveDoctors = activeDoctors,
                Equipments = equipmentDetails,
                Consumables = consumableDetails
            };

            return View("Reports", model);
        }




        // ---------------- Online Appointments Page ----------------
        public IActionResult OnlineAppointment()
        {
            var requests = _context.ServiceRequests
                .Where(r => !r.IsArchived)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ServiceRequest
                {
                    RequestId = r.RequestId,
                    Patient = r.Patient,
                    Service = r.Service,
                    Notes = r.Notes,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Status = r.Status
                })
                .ToList();

            return View("~/Views/Admin/OnlineAppointment.cshtml", requests);
        }

        public async Task<IActionResult> DoctorManagement(string search = null)
        {
            var doctorsQuery = _context.Doctors.Include(d => d.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                doctorsQuery = doctorsQuery.Where(d =>
                    d.User.FirstName.Contains(search) ||
                    d.User.LastName.Contains(search) ||
                    d.User.Email.Contains(search) ||
                    d.Specialty.Contains(search) ||
                    d.LicenseNumber.Contains(search));
            }

            var model = new AdminDoctorsModel
            {
                Doctors = await doctorsQuery.ToListAsync(),
                SearchTerm = search ?? ""
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDoctorStatus(int id)
        {
            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.DoctorId == id);
            if (doctor == null)
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction("DoctorManagement");
            }

            doctor.IsActive = !doctor.IsActive;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Doctor {doctor.User.FullName} is now {(doctor.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("DoctorManagement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterDoctor(AdminDoctorsModel model)
        {
            var newDoctor = model.NewDoctor;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "⚠️ Please fill in all required fields.";
                var doctors = await _context.Doctors.Include(d => d.User).ToListAsync();
                return View("DoctorManagement", new AdminDoctorsModel { Doctors = doctors, NewDoctor = newDoctor });
            }

            try
            {
                // ✅ Ensure Doctor role exists
                if (!await _roleManager.RoleExistsAsync("Doctor"))
                    await _roleManager.CreateAsync(new IdentityRole("Doctor"));

                // ✅ Create user account (not yet confirmed)
                var user = new ApplicationUser
                {
                    UserName = newDoctor.Email,
                    Email = newDoctor.Email,
                    FirstName = newDoctor.FirstName,
                    LastName = newDoctor.LastName,
                    PhoneNumber = newDoctor.Phone,
                    Address = newDoctor.Address,
                    Gender = newDoctor.Gender,
                    EmailConfirmed = false
                };

                var createResult = await _userManager.CreateAsync(user, newDoctor.Password);
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    var doctors = await _context.Doctors.Include(d => d.User).ToListAsync();
                    return View("DoctorManagement", new AdminDoctorsModel { Doctors = doctors, NewDoctor = newDoctor });
                }

                await _userManager.AddToRoleAsync(user, "Doctor");

                // ✅ Encode extra info (Specialty, License) into URL parameters
                var specialtyEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(newDoctor.Specialty ?? ""));
                var licenseEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(newDoctor.LicenseNumber ?? ""));

                // ✅ Generate confirmation email
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new
                    {
                        area = "Identity",
                        userId = userId,
                        code = code,
                        sp = specialtyEncoded,
                        lic = licenseEncoded
                    },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Confirm your Doctor Account",
                    $"Dear Dr. {user.FirstName},<br><br>" +
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br><br>" +
                    $"Thank you,<br>LinkCare Admin Team."
                );

                TempData["Message"] = "✅ Doctor registered successfully! A confirmation email was sent. The account will activate after verification.";
                return RedirectToAction("DoctorManagement");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"⚠️ Unexpected error: {ex.InnerException?.Message ?? ex.Message}";
                var doctors = await _context.Doctors.Include(d => d.User).ToListAsync();
                return View("DoctorManagement", new AdminDoctorsModel { Doctors = doctors, NewDoctor = model.NewDoctor });
            }
        }

    }
}
