using System.Text;
using System.Threading.Tasks;
using LinkCare_IT15.Models.Entities;
using LinkCare_IT15.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace LinkCare_IT15.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code, string sp, string lic)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                // ✅ Decode specialty and license
                var specialty = string.IsNullOrEmpty(sp) ? "" : Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(sp));
                var license = string.IsNullOrEmpty(lic) ? "" : Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(lic));

                // ✅ Only add to Doctors table if not existing
                var exists = await _context.Doctors.AnyAsync(d => d.UserId == user.Id);
                if (!exists)
                {
                    var doctor = new LinkCare_IT15.Models.Entities.Doctor
                    {
                        UserId = user.Id,
                        Specialty = specialty,
                        LicenseNumber = license,
                        Phone = user.PhoneNumber,
                        IsActive = true,
                        Registered = DateTime.Now
                    };


                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync();
                }

                StatusMessage = "✅ Thank you for confirming your email. Your account is now active.";
            }
            else
            {
                StatusMessage = "❌ Error confirming your email.";
            }

            return Page();
        }
    }
}
