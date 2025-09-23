using Microsoft.AspNetCore.Mvc;
using LinkCare_IT15.Data;
using LinkCare_IT15.Models;
using System.Linq;
using LinkCare_IT15.Models.Entities;

namespace LinkCare_IT15.Controllers
{
    public class ConsultationController : Controller   // ✅ must inherit from Controller
    {
        private readonly ApplicationDbContext _context;

        public ConsultationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Consultation/Create
        public IActionResult Create()
        {
            return View();  // ✅ Now works
        }

        // POST: Consultation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Consultations model)
        {
            if (ModelState.IsValid)  // ✅ Now works
            {
                _context.Consultations.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");  // ✅ Now works
            }
            return View(model);
        }

        // GET: Consultation/Index
        public IActionResult Index()
        {
            var consultations = _context.Consultations.ToList();
            return View(consultations);
        }
    }
}
