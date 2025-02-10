using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApplication1.Model;

public class VerifyTwoFactorModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public VerifyTwoFactorModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public string Code { get; set; }

    [BindProperty]
    public string Email { get; set; }

    public void OnGet(string email)
    {
        Email = email;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            return RedirectToPage("/Account/Login"); // Redirect to login if user not found
        }

        // Verify the 2FA code
        var result = await _signInManager.TwoFactorSignInAsync("Email", Code, isPersistent: false, rememberClient: false);
        if (result.Succeeded)
        {
            return RedirectToPage("/Index"); // Redirect to the home page or dashboard
        }

        ModelState.AddModelError(string.Empty, "Invalid code. Please try again.");
        return Page();
    }
}