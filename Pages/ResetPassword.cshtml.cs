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
    public string Email { get; set; }
    [BindProperty]
    public string Token { get; set; }
    [BindProperty]
    public string NewPassword { get; set; }
    [BindProperty]
    public string ConfirmPassword { get; set; }

    public void OnGet(string email, string token)
    {
        Email = email;
        Token = token;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (NewPassword != ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            return RedirectToPage("ResetPasswordConfirmation");
        }

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