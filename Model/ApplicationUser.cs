using Microsoft.AspNetCore.Identity;
using System;

namespace WebApplication1.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string NRIC { get; set; } // This will be encrypted later
        public DateTime DateOfBirth { get; set; }
        public string WhoAmI { get; set; }
        public string CurrentSessionId { get; set; } = string.Empty; // Store the current session ID

        public virtual ICollection<PasswordHistory> PasswordHistories { get; set; }

        public DateTime LastPasswordChangeDate { get; set; } = DateTime.UtcNow; // Default to now
        public DateTime PasswordExpirationDate { get; set; } = DateTime.UtcNow.AddDays(90); // Default to 90 days from now
    }
}
