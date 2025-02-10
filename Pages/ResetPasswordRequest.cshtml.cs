using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApplication1.Model;

public class ResetPasswordRequestModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailSender _emailSender;

    public ResetPasswordRequestModel(UserManager<ApplicationUser> userManager, EmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
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

        // Create the reset link without the email
        var resetLink = Url.Page("/ResetPassword", null, new { token = token }, Request.Scheme);

        // Send the email
        await _emailSender.SendEmailAsync(Email, "Reset Password", $"Please reset your password by clicking here: <a href='{resetLink}'>Reset your password</a>");

        return RedirectToPage("ResetPasswordConfirmation");
    }
}