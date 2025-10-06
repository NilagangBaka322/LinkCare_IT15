using LinkCare_IT15.Data;
using LinkCare_IT15.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LinkCare_IT15.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]")]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Billing
        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            // Get all consultations
            var consultations = await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Appointment)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            // Ensure each consultation has a billing
            foreach (var consultation in consultations)
            {
                // More reliable check — match by unique consultation Id
                bool billingExists = await _context.Billings
                    .AnyAsync(b =>
                        b.AppointmentId == consultation.AppointmentId &&
                        (b.PatientID == consultation.PatientId ||
                         (b.PatientID == null && b.WalkInName == consultation.WalkInName))
                    );

                if (!billingExists)
                {
                    var billing = new Billing
                    {
                        PatientID = consultation.PatientId,
                        WalkInName = string.IsNullOrEmpty(consultation.PatientId)
                            ? consultation.WalkInName ?? consultation.Appointment?.WalkInName
                            : null,
                        AppointmentId = consultation.AppointmentId,
                        TotalAmount = consultation.ConsultationFee,
                        BillingDate = consultation.Date
                    };

                    _context.Billings.Add(billing);
                }
            }

            await _context.SaveChangesAsync();

            // Load only distinct billings from DB
            var billings = await _context.Billings
                .Include(b => b.Patient)
                .Include(b => b.Transactions)
                .Include(b => b.Appointment)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();

            // Optional: Deduplicate by unique combination
            billings = billings
                .GroupBy(b => new
                {
                    PatientID = b.PatientID ?? "",
                    WalkInName = b.WalkInName ?? "",
                    AppointmentId = b.AppointmentId ?? 0,
                    TotalAmount = b.TotalAmount
                })
                .Select(g => g.First())
                .ToList();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                billings = billings.Where(b =>
                    (b.Patient != null &&
                     $"{b.Patient.FirstName} {b.Patient.LastName}".ToLower().Contains(search.ToLower()))
                    || (b.WalkInName != null && b.WalkInName.ToLower().Contains(search.ToLower()))
                ).ToList();

                ViewData["CurrentFilter"] = search;
            }

            return View("~/Views/Admin/Billing.cshtml", billings);
        }



        // POST: /Admin/Billing/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int billingId, decimal amountPaid, string paymentMethod, string? referenceNumber)
        {
            var bill = await _context.Billings
                .Include(b => b.Transactions)
                .FirstOrDefaultAsync(b => b.BillingID == billingId);

            if (bill == null)
                return NotFound();

            // Calculate total paid so far
            var totalPaid = bill.Transactions.Sum(t => t.AmountPaid);
            var newTotal = totalPaid + amountPaid;

            // Determine status and change
            string status;
            decimal change = 0;

            if (newTotal >= bill.TotalAmount)
            {
                status = "Paid";
                change = newTotal - bill.TotalAmount;
            }
            else
            {
                status = "Partial";
            }

            var transaction = new Transaction
            {
                BillingID = bill.BillingID,
                AmountPaid = amountPaid,
                Change = change,
                TransactionDate = DateTime.Now,
                TransactionType = "Payment",
                PaymentMethod = paymentMethod,
                ReferenceNumber = referenceNumber,
                Status = status
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment recorded successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
