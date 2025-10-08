using Microsoft.AspNetCore.Mvc;
using LinkCare_IT15.Models;
using LinkCare_IT15.Data;

namespace LinkCare_IT15.Controllers
{
    [Route("Admin/[controller]")]
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/Admin/ServiceManagement")]
        public IActionResult ServiceManagement()
        {
            var services = _context.Services.ToList();
            return View("~/Views/Admin/ServiceManagement.cshtml", services);
        }

        [HttpPost("/Admin/ServiceManagement")]
        public IActionResult ServiceManagement(Service service, IFormFile? image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var fileName = Path.GetFileName(image.FileName);
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/services");

                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    var filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    service.ImagePath = "/uploads/services/" + fileName;
                }

                service.IsActive = true;
                _context.Services.Add(service);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Service added successfully!";
                return RedirectToAction("ServiceManagement");
            }

            var allServices = _context.Services.ToList();
            return View("~/Views/Admin/ServiceManagement.cshtml", allServices);
        }

        // ✅ Toggle Active / Inactive
        [HttpPost("ToggleStatus/{id}")]
        public IActionResult ToggleStatus(int id)
        {
            var service = _context.Services.Find(id);
            if (service == null)
                return NotFound();

            service.IsActive = !service.IsActive;
            _context.SaveChanges();

            return Json(new { success = true, isActive = service.IsActive });
        }

        // ✅ Get Service for Edit (AJAX)
        [HttpGet("GetService/{id}")]
        public IActionResult GetService(int id)
        {
            var service = _context.Services.Find(id);
            if (service == null)
                return NotFound();

            return Json(service);
        }

        // ✅ Update Service (POST)
        [HttpPost("UpdateService")]
        public IActionResult UpdateService(Service updatedService, IFormFile? image)
        {
            var service = _context.Services.Find(updatedService.ServiceId);
            if (service == null)
                return NotFound();

            service.ServiceName = updatedService.ServiceName;
            service.Description = updatedService.Description;
            service.DurationMinutes = updatedService.DurationMinutes;
            service.IsActive = updatedService.IsActive;

            if (image != null && image.Length > 0)
            {
                var fileName = Path.GetFileName(image.FileName);
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/services");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                service.ImagePath = "/uploads/services/" + fileName;
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Service updated successfully!";
            return RedirectToAction("ServiceManagement");
        }
    }
}
