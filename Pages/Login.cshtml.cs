using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Model;
using WebApplication1.Services; // Add this for EncryptionService
using WebApplication1.ViewModels;
// using System.Net.Http; // Commented out for reCAPTCHA
// using System.Text.Json; // Commented out for reCAPTCHA

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly EmailSender emailSender; // Inject EmailSender
        private readonly EncryptionService encryptionService; // Inject EncryptionService
        private readonly AuditService auditService; // Inject AuditService
        // private readonly IHttpClientFactory _httpClientFactory; // Commented out for reCAPTCHA
        // private readonly string _recaptchaSecret = "6Lc9xNIqAAAAALYRKkqFGzyU7ycI5KCBvkrf8dJ5"; // Commented out for reCAPTCHA

        public LoginModel(SignInManager<ApplicationUser> signInManager, EmailSender emailSender, EncryptionService encryptionService, AuditService auditService /*, IHttpClientFactory httpClientFactory */)
        {
            this.signInManager = signInManager;
            this.emailSender = emailSender; // Initialize EmailSender
            this.encryptionService = encryptionService; // Initialize EncryptionService
            this.auditService = auditService; // Initialize AuditService
            // _httpClientFactory = httpClientFactory; // Commented out for reCAPTCHA
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Commented out reCAPTCHA validation
                // var recaptchaResponse = Request.Form["g-recaptcha-response"];
                // var isValidCaptcha = await ValidateCaptcha(recaptchaResponse);
                // if (!isValidCaptcha)
                // {
                //     ModelState.AddModelError("", "Please complete the reCAPTCHA.");
                //     return Page();
                // }

                // Retrieve the user to check for lockout
                var user = await signInManager.UserManager.FindByEmailAsync(LModel.Email);
                if (user != null)
                {
                    // Check if the user is locked out
                    if (await signInManager.UserManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", "Your account is locked out. Please try again later.");
                        return Page();
                    }
                }

                // Attempt to sign in the user
                var identityResult = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, false);
                if (identityResult.Succeeded)
                {
                    // Check if the user has 2FA enabled
                    if (user != null && await signInManager.UserManager.GetTwoFactorEnabledAsync(user))
                    {
                        // Generate the 2FA code and send it via email
                        var code = await signInManager.UserManager.GenerateTwoFactorTokenAsync(user, "Email");
                        await emailSender.SendEmailAsync(LModel.Email, "Your 2FA Code", $"Your code is: {code}");

                        // Redirect to the 2FA verification page
                        return RedirectToPage("VerifyTwoFactor", new { email = LModel.Email });
                    }

                    // Log successful login
                    if (user != null)
                    {
                        await auditService.LogUserActivity(user.UserName, "User  logged in successfully.");
                    }

                    // Generate a new session ID
                    var newSessionId = Guid.NewGuid().ToString();
                    user.CurrentSessionId = newSessionId; // Update the user's session ID

                    // Save changes to the database
                    var updateResult = await signInManager.UserManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        // Handle update failure (e.g., log the error)
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

        // Commented out reCAPTCHA validation method
        // private async Task<bool> ValidateCaptcha(string captchaResponse)
        // {
        //     //     var client = _httpClientFactory.CreateClient();
        //     var response = await client.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={_recaptchaSecret}&response={captchaResponse}");
        //     var jsonResponse = await response.Content.ReadAsStringAsync();
        //     var captchaResult = JsonSerializer.Deserialize<RecaptchaResponse>(jsonResponse);
        //
        //     return captchaResult.Success && captchaResult.Score >= 0.5; // Adjust score threshold as needed
        // }

        // private class RecaptchaResponse
        // {
        //     public bool Success { get; set; }
        //     public float Score { get; set; }
        //     public string Action { get; set; }
        //     public string ChallengeTs { get; set; }
        //     public string Hostname { get; set; }
        // }

        public void OnGet()
        {
        }
    }
}