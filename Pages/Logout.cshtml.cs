using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;

namespace WebApplication1.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager; // Use ApplicationUser   

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            // Sign out the user
            await signInManager.SignOutAsync();

            // Clear session data
            HttpContext.Session.Clear();

            // Redirect to the login page
            return RedirectToPage("Login");
        }

        public async Task<IActionResult> OnPostDontLogoutAsync()
        {
            // Redirect to the Index page without logging out
            return RedirectToPage("Index");
        }

        public void OnGet()
        {
        }
    }
}