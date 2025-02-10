using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebApplication1.Model;

public class TwoFactorAuthenticationModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TwoFactorAuthenticationModel> _logger;  // Injected logger

    public TwoFactorAuthenticationModel(UserManager<ApplicationUser> userManager, ILogger<TwoFactorAuthenticationModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public bool Is2faEnabled { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            _logger.LogInformation("[GET] User ID: {UserId}, Is2faEnabled: {Is2faEnabled}", user.Id, Is2faEnabled);
        }
        else
        {
            _logger.LogWarning("[GET] User not found.");
        }
    }

    public async Task<IActionResult> OnPostAsync(string enable2fa, string disable2fa)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            _logger.LogError("[POST] ERROR: User not found.");
            return Page();
        }

        IdentityResult result;
        bool newStatus = false;

        if (!string.IsNullOrEmpty(enable2fa))
        {
            result = await _userManager.SetTwoFactorEnabledAsync(user, true);
            newStatus = true;
            _logger.LogInformation("[POST] Enabling 2FA for User ID: {UserId}", user.Id);
        }
        else if (!string.IsNullOrEmpty(disable2fa))
        {
            result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            newStatus = false;
            _logger.LogInformation("[POST] Disabling 2FA for User ID: {UserId}", user.Id);
        }
        else
        {
            _logger.LogWarning("[POST] No action taken.");
            return Page();
        }

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                _logger.LogError("[POST] ERROR: {ErrorMessage}", error.Description);
            }
            return Page();
        }

        // Ensure the change is saved in the database
        await _userManager.UpdateAsync(user);

        // Verify the update
        user = await _userManager.GetUserAsync(User);
        Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

        _logger.LogInformation("[POST] 2FA Status Updated -> User ID: {UserId}, Is2faEnabled: {Is2faEnabled} (Expected: {ExpectedStatus})",
            user.Id, Is2faEnabled, newStatus);

        return RedirectToPage();
    }
}
