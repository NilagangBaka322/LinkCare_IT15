using LinkCare_IT15.Models.Entities; // ✅ so ApplicationUser is recognized
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System;

namespace LinkCare_IT15.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager; // ✅ Add this
        private readonly ILogger<LoginModel> _logger;
        private readonly IConfiguration _configuration;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager, // ✅ Add this
        ILogger<LoginModel> logger,
        IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager; 
            _logger = logger; 
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public string RecaptchaToken { get; set; }

        public string ReturnUrl { get; set; }

        // Brute force control
        public int RemainingAttempts { get; set; }
        public int? CooldownRemaining { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName, // still use UserName here
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Get roles of the signed-in user
                var roles = await _userManager.GetRolesAsync(user);

                // Redirect based on role
                if (roles.Contains("Admin"))
                    return RedirectToAction("AdminDashboard", "Admin");
                if (roles.Contains("Doctor"))
                    return RedirectToAction("DoctorDashboard", "Doctor");
                if (roles.Contains("Patient"))
                    return RedirectToAction("PatientDashboard", "Patient");

                // fallback
                return LocalRedirect(ReturnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }


    }
}
