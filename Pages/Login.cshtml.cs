using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging; // For logging
using Microsoft.Extensions.Configuration; // For configuration
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Model;
using WebApplication1.Services; // For EncryptionService
using WebApplication1.ViewModels;
using System.Net.Http; // For reCAPTCHA
using System.Text.Json; // For reCAPTCHA

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly EmailSender emailSender;
        private readonly EncryptionService encryptionService;
        private readonly AuditService auditService;
        private readonly IHttpClientFactory _httpClientFactory; // For reCAPTCHA
        private readonly string _recaptchaSecret; // Secret key from configuration
        public readonly string _recaptchaSiteKey;
        private readonly ILogger<LoginModel> _logger; // Logger

        public LoginModel(SignInManager<ApplicationUser> signInManager, EmailSender emailSender, EncryptionService encryptionService, AuditService auditService, IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger, IConfiguration configuration)
        {
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.encryptionService = encryptionService;
            this.auditService = auditService;
            _httpClientFactory = httpClientFactory; // Initialize the IHttpClientFactory
            _logger = logger; // Initialize the logger

            // Read the secret key from the configuration
            _recaptchaSecret = configuration["Recaptcha:SecretKey"];
            _recaptchaSiteKey = configuration["Recaptcha:SiteKey"];

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var recaptchaResponse = Request.Form["g-recaptcha-response"];
                var isValidCaptcha = await ValidateCaptcha(recaptchaResponse);
                if (!isValidCaptcha)
                {
                    ModelState.AddModelError("", "Please complete the reCAPTCHA.");
                    return Page();
                }

                // Retrieve the user to check for lockout
                var user = await signInManager.UserManager.FindByEmailAsync(LModel.Email);
                if (user != null && await signInManager.UserManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError("", "Your account is locked out. Please try again later.");
                    return Page();
                }

                // Attempt to sign in the user
                var identityResult = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, false);
                if (identityResult.Succeeded)
                {
                    // Check if the user has 2FA enabled
                    if (user != null && await signInManager.UserManager.GetTwoFactorEnabledAsync(user))
                    {
                        var code = await signInManager.UserManager.GenerateTwoFactorTokenAsync(user, "Email");
                        await emailSender.SendEmailAsync(LModel.Email, "Your 2FA Code", $"Your code is: {code}");
                        return RedirectToPage("VerifyTwoFactor", new { email = LModel.Email });
                    }

                    // Log successful login
                    await auditService.LogUserActivity(user.UserName, "User  logged in successfully.");

                    // Generate a new session ID
                    var newSessionId = Guid.NewGuid().ToString();
                    user.CurrentSessionId = newSessionId; // Update the user's session ID

                    // Save changes to the database
                    var updateResult = await signInManager.UserManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        ModelState.AddModelError("", "Failed to update session ID.");
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
                    HttpContext.Session.SetString("User  Name", user.UserName);
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("SessionId", newSessionId); // Store session ID in session

                    var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

                    return RedirectToPage("Index");
                }
                else
                {
                    // Increment the failed access count
                    if (user != null)
                    {
                        await signInManager.UserManager.AccessFailedAsync(user);
                        await auditService.LogUserActivity(user.UserName, "Failed login attempt.");
                    }
                    ModelState.AddModelError("", "Invalid email or password. Please try again.");
                }
            }
            return Page();
        }

        private async Task<bool> ValidateCaptcha(string captchaResponse)
        {
            _logger.LogInformation("ValidateCaptcha called."); // Log method execution

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={_recaptchaSecret}&response={captchaResponse}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error calling reCAPTCHA API.");
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("reCAPTCHA API Response: " + jsonResponse); // Log the full response

            RecaptchaResponse captchaResult;
            try
            {
                captchaResult = JsonSerializer.Deserialize<RecaptchaResponse>(jsonResponse);
            }
            catch (JsonException)
            {
                _logger.LogError("Error deserializing reCAPTCHA response.");
                return false;
            }

            // Log the score and success status
            _logger.LogInformation($"reCAPTCHA Success: {captchaResult.success}, Score: {captchaResult.score}");

            // Use the score to determine if the login should proceed
            return captchaResult.success && captchaResult.score >= 0.5; // Adjust score threshold as needed
        }

        private class RecaptchaResponse
        {
            public bool success { get; set; }
            public float score { get; set; }
            public string action { get; set; }
            public string challenge_ts { get; set; }
            public string hostname { get; set; }
        }

        public void OnGet()
        {
        }
    }
}