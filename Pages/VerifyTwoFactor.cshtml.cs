using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging; // Add this using directive
using System.Threading.Tasks;
using WebApplication1.Model;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

public class VerifyTwoFactorModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<VerifyTwoFactorModel> _logger; // Add logger field

    public VerifyTwoFactorModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<VerifyTwoFactorModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger; // Initialize the logger
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
            _logger.LogWarning("User  not found for email: {Email}", Email);
            return RedirectToPage("/Login");
        }

        // Retrieve the expected code and expiration from TempData
        var expectedCode = TempData["TwoFactorCode"] as string;
        var expiration = TempData["TwoFactorCodeExpiration"] as DateTime?;

        // Log the expected and entered codes
        _logger.LogInformation("Expected 2FA code for user {Email}: {ExpectedCode}", Email, expectedCode);
        _logger.LogInformation("User  entered code: {Code}", Code);

        // Check if the code has expired
        if (expiration == null || DateTime.UtcNow > expiration)
        {
            ModelState.AddModelError(string.Empty, "The code has expired. Please request a new code.");
            _logger.LogWarning("2FA code expired for user {Email}.", Email);
            return Page();
        }

        // Verify the OTP
        if (expectedCode == Code)
        {
            _logger.LogInformation("User  {Email} successfully verified 2FA code.", Email);

            // Update the user's session ID
            var newSessionId = Guid.NewGuid().ToString();
            user.CurrentSessionId = newSessionId; // Update the user's session ID

            // Save changes to the database
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to update session ID.");
                return Page();
            }

            // Create the security context
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Department", "HR"),
                new Claim("SessionId", newSessionId) // Optionally add session ID to claims
            };

            // Store user information in session
            HttpContext.Session.SetString("User Name", user.UserName);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("SessionId", newSessionId); // Store session ID in session

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

            return RedirectToPage("/Index"); // Redirect to the home page or dashboard
        }

        ModelState.AddModelError(string.Empty, "Invalid code. Please try again.");
        _logger.LogWarning("Invalid 2FA code entered for user {Email}.", Email);
        return Page();
    }
}