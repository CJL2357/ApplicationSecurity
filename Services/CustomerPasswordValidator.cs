using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Ensure this is included for ToListAsync
using WebApplication1.Model; // Adjust the namespace based on your project structure

public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
{
    private readonly AuthDbContext _context;

    public CustomPasswordValidator(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
    {
        // Get the last two password hashes from the history
        var passwordHistory = await _context.PasswordHistories
            .Where(ph => ph.UserId == user.Id)
            .OrderByDescending(ph => ph.CreatedAt)
            .Take(2)
            .ToListAsync();

        foreach (var history in passwordHistory)
        {
            // Verify the provided password against the hashed password in history
            if (manager.PasswordHasher.VerifyHashedPassword(user, history.PasswordHash, password) != PasswordVerificationResult.Failed)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Cannot reuse old password." });
            }
        }

        return IdentityResult.Success;
    }
}