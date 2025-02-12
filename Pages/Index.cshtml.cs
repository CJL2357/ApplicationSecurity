using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebApplication1.Model;
using WebApplication1.Services; // Add this using directive for your EncryptionService

namespace WebApplication1.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager; // Inject UserManager
        private readonly SignInManager<ApplicationUser> _signInManager; // Inject SignInManager
        private readonly EncryptionService _encryptionService; // Inject EncryptionService

        // Properties to hold user data
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string DecryptedNRIC { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string WhoAmI { get; set; }

        public string ResumeFilePath { get; set; } // New property for resume file path

        public IndexModel(ILogger<IndexModel> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, EncryptionService encryptionService)
        {
            _logger = logger;
            _userManager = userManager; // Initialize UserManager
            _signInManager = signInManager; // Initialize SignInManager
            _encryptionService = encryptionService; // Initialize EncryptionService
        }

        public async Task OnGetAsync()
        {
            // Check if the session is active
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                // Session has expired or user is not logged in
                _logger.LogWarning("Session expired or user not logged in.");
                RedirectToPage("/Login");
                return;
            }

            // Retrieve the current user
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Check if the session ID matches the one stored in the database
                if (user.CurrentSessionId != sessionId)
                {
                    // Session ID does not match, indicating a new login from another device
                    _logger.LogWarning("Session ID mismatch for user {User Name}. Signing out.", user.UserName);
                    await _signInManager.SignOutAsync(); // Sign out the current user
                    RedirectToPage("/Login"); // Redirect to login page
                    return;
                }

                // Decrypt the NRIC
                try
                {
                    DecryptedNRIC = _encryptionService.Decrypt(user.NRIC);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error decrypting NRIC for user {User Name}: {Message}", user.UserName, ex.Message);
                    // Handle decryption error (e.g., set DecryptedNRIC to a default value or show an error message)
                }

                // Assign other properties
                FirstName = user.FirstName;
                LastName = user.LastName;
                Gender = user.Gender;
                DateOfBirth = user.DateOfBirth;
                WhoAmI = user.WhoAmI;
                ResumeFilePath = user.ResumeFilePath; // Get the resume file path
            }
            else
            {
                _logger.LogWarning("User  not found.");
                RedirectToPage("/Login"); // Redirect if user is not found
            }
        }
    }
}