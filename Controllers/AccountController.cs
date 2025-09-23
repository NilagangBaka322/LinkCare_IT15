using Microsoft.AspNetCore.Mvc;
using LinkCare_IT15.Models;
using LinkCare_IT15.Models.AdminModel;

namespace LinkCare_IT15.Controllers
{
    public class AccountController : Controller
    {
        // Show Register Page
        public IActionResult Register()
        {
            return View();
        }

        // Handle Register POST
        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save user to DB (e.g., using EF Core)
                // Redirect to login or dashboard
                return RedirectToAction("Login");
            }

            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

      

    }


}
