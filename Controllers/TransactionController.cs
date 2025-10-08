//TransactionController.cs

using LinkCare_IT15.Data;
using LinkCare_IT15.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkCare_IT15.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // View generated receipt
        [HttpGet]
        public async Task<IActionResult> Receipt(string referenceNumber)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Billing)
                .ThenInclude(b => b.Patient)
                .FirstOrDefaultAsync(t => t.ReferenceNumber == referenceNumber);

            if (transaction == null)
                return NotFound();

            return View("~/Views/Transaction/Receipt.cshtml", transaction);
        }
    }
}
