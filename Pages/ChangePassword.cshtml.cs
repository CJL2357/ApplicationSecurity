using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApplication1.Model;
using WebApplication1.ViewModels;
using Microsoft.EntityFrameworkCore; // For DbContext

namespace WebApplication1.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context; // Add this line

        public ChangePasswordModel(UserManager<ApplicationUser> userManager, AuthDbContext context)
        {
            _userManager = userManager;
            _context = context; // Initialize the DbContext
        }

        [BindProperty]
        public ChangePassword Model { get; set; }

        private const int MinimumPasswordChangeIntervalMinutes = 0; // Minimum time before changing password
        private const int MaximumPasswordAgeDays = 90; // Maximum password age in days

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Check if the password can be changed based on the last change date
            if (DateTime.UtcNow < user.LastPasswordChangeDate.AddMinutes(MinimumPasswordChangeIntervalMinutes))
            {
                ModelState.AddModelError(string.Empty, $"You must wait at least {MinimumPasswordChangeIntervalMinutes} minutes before changing your password again.");
                return Page();
            }

            // Check if the password has expired
            if (DateTime.UtcNow > user.PasswordExpirationDate)
            {
                ModelState.AddModelError(string.Empty, "Your password has expired. Please change your password.");
                return Page();
            }

            // Check password strength
            if (!IsPasswordStrong(Model.NewPassword))
            {
                ModelState.AddModelError(string.Empty, "Password must be at least 12 characters long and include a mix of upper-case, lower-case, numbers, and special characters.");
                return Page();
            }

            // Check if the new password is in the password history
            var passwordHistory = await _context.PasswordHistories
                .Where(ph => ph.UserId == user.Id)
                .OrderByDescending(ph => ph.CreatedAt)
                .Take(2)
                .ToListAsync();

            foreach (var history in passwordHistory)
            {
                if (_userManager.PasswordHasher.VerifyHashedPassword(user, history.PasswordHash, Model.NewPassword) != PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(string.Empty, "Cannot reuse old password.");
                    return Page();
                }
            }

            // Change the password
            var result = await _userManager.ChangePasswordAsync(user, Model.CurrentPassword, Model.NewPassword);
            if (result.Succeeded)
            {
                // Update the last password change date and expiration date
                user.LastPasswordChangeDate = DateTime.UtcNow;
                user.PasswordExpirationDate = DateTime.UtcNow.AddDays(MaximumPasswordAgeDays);
                await _userManager.UpdateAsync(user); // Save changes to the user

                // Store the new password in password history
                var passwordHistoryEntry = new PasswordHistory
                {
                    UserId = user.Id,
                    PasswordHash = _userManager.PasswordHasher.HashPassword(user, Model.NewPassword),
                    CreatedAt = DateTime.UtcNow
                };
                _context.PasswordHistories.Add(passwordHistoryEntry);
                await _context.SaveChangesAsync(); // Save the password history entry

                return RedirectToPage("Index"); // Redirect to a success page or home
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        private bool IsPasswordStrong(string password)
        {
            // Define the strong password pattern
            var strongPasswordPattern = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$");
            return strongPasswordPattern.IsMatch(password);
        }
    }
}