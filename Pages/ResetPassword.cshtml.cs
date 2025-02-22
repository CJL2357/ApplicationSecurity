using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using WebApplication1.Model;
using Microsoft.EntityFrameworkCore; // For DbContext operations

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthDbContext _context; // Add your DbContext

    public ResetPasswordModel(UserManager<ApplicationUser> userManager, AuthDbContext context)
    {
        _userManager = userManager;
        _context = context; // Initialize the DbContext
    }

    [BindProperty]
    public string Token { get; set; }

    [BindProperty]
    public string NewPassword { get; set; }

    [BindProperty]
    public string ConfirmPassword { get; set; } // Add ConfirmPassword property

    public void OnGet(int id) // Change to accept the unique identifier
    {
        // Retrieve the token from the database using the unique identifier
        var resetToken = _context.PasswordResetTokens.Find(id);

        if (resetToken == null || resetToken.ExpirationDate < DateTime.UtcNow)
        {
            // Handle invalid or expired token
            ModelState.AddModelError(string.Empty, "Invalid or expired token.");
            return;
        }

        Token = resetToken.Token; // Store the token for use in the form
    }

    public async Task<IActionResult> OnPostAsync(int id) // Accept the unique identifier
    {
        if (string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(Token))
        {
            ModelState.AddModelError(string.Empty, "Password and token are required.");
            return Page();
        }

        if (NewPassword != ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "The password and confirmation password do not match.");
            return Page();
        }

        // Find the token using the unique identifier
        var resetToken = await _context.PasswordResetTokens.FindAsync(id);
        if (resetToken == null)
        {
            // Handle user not found or invalid token
            ModelState.AddModelError(string.Empty, "Invalid token.");
            return Page();
        }

        // Find the user by ID
        var user = await _userManager.FindByIdAsync(resetToken.UserId);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User  not found.");
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
            if (_userManager.PasswordHasher.VerifyHashedPassword(user, history.PasswordHash, NewPassword) != PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Cannot reuse old password.");
                return Page();
            }
        }

        // Reset the password
        var result = await _userManager.ResetPasswordAsync(user, resetToken.Token, NewPassword);
        if (result.Succeeded)
        {
            // Store the new password in password history
            var passwordHistoryEntry = new PasswordHistory
            {
                UserId = user.Id,
                PasswordHash = _userManager.PasswordHasher.HashPassword(user, NewPassword),
                CreatedAt = DateTime.UtcNow
            };
            _context.PasswordHistories.Add(passwordHistoryEntry);
            await _context.SaveChangesAsync(); // Save the password history entry

            // Optionally, remove the token from the database after successful reset
            _context.PasswordResetTokens.Remove(resetToken);
            await _context.SaveChangesAsync();

            return RedirectToPage("ResetConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}