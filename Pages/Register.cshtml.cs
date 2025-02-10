using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using WebApplication1.Model;
using WebApplication1.ViewModels;
using WebApplication1.Services; // Add this using directive for your EncryptionService

namespace WebApplication1.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager; // Use ApplicationUser    
        private readonly SignInManager<ApplicationUser> signInManager; // Use ApplicationUser    
        private readonly ILogger<RegisterModel> logger;
        private readonly PasswordHasher<ApplicationUser> passwordHasher; // Password hasher
        private readonly EncryptionService encryptionService; // Add EncryptionService

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<RegisterModel> logger, EncryptionService encryptionService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.passwordHasher = new PasswordHasher<ApplicationUser>(); // Initialize the password hasher
            this.encryptionService = encryptionService; // Inject EncryptionService
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser() // Use ApplicationUser  instead of IdentityUser     
                {
                    UserName = RModel.Email,
                    Email = RModel.Email,
                    FirstName = RModel.FirstName, // Set additional properties
                    LastName = RModel.LastName,
                    Gender = RModel.Gender,
                    // Encrypt the NRIC before saving
                    NRIC = encryptionService.Encrypt(RModel.NRIC),
                    DateOfBirth = RModel.DateOfBirth,
                    WhoAmI = RModel.WhoAmI,
                    CurrentSessionId = string.Empty // Initialize the session ID
                };

                // Hash the password with salt
                user.PasswordHash = passwordHasher.HashPassword(user, RModel.Password);

                // Create the user
                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    // Handle file upload for the resume
                    if (RModel.Resume != null && RModel.Resume.Length > 0)
                    {
                        var uploadsFolder = Path.Combine("wwwroot", "uploads");
                        // Ensure the uploads directory exists
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filePath = Path.Combine(uploadsFolder, RModel.Resume.FileName);
                        // Optional: Handle file name conflicts
                        if (System.IO.File.Exists(filePath))
                        {
                            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(RModel.Resume.FileName);
                            var fileExtension = Path.GetExtension(RModel.Resume.FileName);
                            var counter = 1;
                            while (System.IO.File.Exists(filePath))
                            {
                                filePath = Path.Combine(uploadsFolder, $"{fileNameWithoutExtension}_{counter++}{fileExtension}");
                            }
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await RModel.Resume.CopyToAsync(stream);
                        }
                    }

                    // Redirect to the login page instead of the home page
                    return RedirectToPage("/Login"); // Change this line to redirect to the login page
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                        logger.LogError("Registration error: {Error}", error.Description); // Log the error
                    }
                }
            }
            return Page();
        }
    }
}