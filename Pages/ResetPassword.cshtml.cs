using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApplication1.Model;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetPasswordModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public string Token { get; set; }

    [BindProperty]
    public string NewPassword { get; set; }

    [BindProperty]
    public string ConfirmPassword { get; set; } // Add ConfirmPassword property

    public void OnGet(string token)
    {
        Token = token; // Store the token for use in the form
    }

    public async Task<IActionResult> OnPostAsync()
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

        // Find the user by iterating through all users
        ApplicationUser user = null;
        var users = _userManager.Users; // Get all users

        foreach (var u in users)
        {
            // Validate the token for each user
            var isValidToken = await _userManager.VerifyUserTokenAsync(u, "Default", "ResetPassword", Token);
            if (isValidToken)
            {
                user = u; // Found the user with a valid token
                break;
            }
        }

        if (user == null)
        {
            // Handle user not found or invalid token
            ModelState.AddModelError(string.Empty, "Invalid token.");
            return Page();
        }

        // Reset the password
        var result = await _userManager.ResetPasswordAsync(user, Token, NewPassword);
        if (result.Succeeded)
        {
            return RedirectToPage("ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}