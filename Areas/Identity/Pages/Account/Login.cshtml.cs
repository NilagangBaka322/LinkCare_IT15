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
        private readonly SignInManager<ApplicationUser> _signInManager;  // ✅ switched to ApplicationUser
        private readonly ILogger<LoginModel> _logger;
        private readonly IConfiguration _configuration;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, IConfiguration configuration)
        {
            _signInManager = signInManager;
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

            const int MaxAttempts = 5;
            const int CooldownSeconds = 15;

            var failedAttempts = HttpContext.Session.GetInt32("FailedAttempts") ?? 0;
            var lockoutEnd = HttpContext.Session.GetString("LockoutEnd");

            // ✅ Check cooldown with local time
            if (!string.IsNullOrEmpty(lockoutEnd) && DateTime.TryParse(lockoutEnd, out var lockoutTime))
            {
                if (DateTime.Now < lockoutTime)
                {
                    CooldownRemaining = (int)(lockoutTime - DateTime.Now).TotalSeconds;
                    RemainingAttempts = 0;
                    ModelState.AddModelError(string.Empty, $"Too many failed attempts. Try again in {CooldownRemaining} seconds.");
                    return Page();
                }
                else
                {
                    // reset lockout
                    HttpContext.Session.Remove("LockoutEnd");
                    failedAttempts = 0;
                    HttpContext.Session.SetInt32("FailedAttempts", failedAttempts);
                }
            }

            if (ModelState.IsValid)
            {
                // ✅ Sign in using ApplicationUser’s UserName/Email
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    HttpContext.Session.SetInt32("FailedAttempts", 0);
                    return LocalRedirect(ReturnUrl);
                }

                // login failed
                failedAttempts++;
                HttpContext.Session.SetInt32("FailedAttempts", failedAttempts);

                if (failedAttempts >= MaxAttempts)
                {
                    // ✅ Use DateTime.Now instead of UtcNow
                    var cooldownEnd = DateTime.Now.AddSeconds(CooldownSeconds);
                    HttpContext.Session.SetString("LockoutEnd", cooldownEnd.ToString("O"));
                    CooldownRemaining = CooldownSeconds;
                    RemainingAttempts = 0;
                    ModelState.AddModelError(string.Empty, $"Too many failed attempts. Please wait {CooldownSeconds} seconds before trying again.");
                }
                else
                {
                    RemainingAttempts = MaxAttempts - failedAttempts;
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return Page();
        }
    }
}
