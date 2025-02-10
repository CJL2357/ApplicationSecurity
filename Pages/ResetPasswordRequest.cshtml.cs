using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WebApplication1.Model;

public class ResetPasswordRequestModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailSender _emailSender;
    private readonly AuthDbContext _context; // Add your DbContext

    public ResetPasswordRequestModel(UserManager<ApplicationUser> userManager, EmailSender emailSender, AuthDbContext context)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _context = context; // Initialize the DbContext
    }

    [BindProperty]
    public string Email { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Email))
        {
            ModelState.AddModelError(string.Empty, "Email is required.");
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            // Do not reveal that the user does not exist
            return RedirectToPage("ResetPasswordConfirmation");
        }

        // Generate the password reset token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Create a new PasswordResetToken instance
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpirationDate = DateTime.UtcNow.AddHours(1) // Set expiration time
        };

        // Save the token to the database
        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        // Create the reset link with the unique identifier
        var resetLink = Url.Page("/ResetPassword", null, new { id = resetToken.Id }, Request.Scheme);

        // Send the email
        await _emailSender.SendEmailAsync(Email, "Reset Password", $"Please reset your password by clicking here: <a href='{resetLink}'>Reset your password</a>");

        return RedirectToPage("ResetPasswordConfirmation");
    }
}