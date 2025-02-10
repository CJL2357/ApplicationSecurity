using System;

namespace WebApplication1.Model
{
    public class PasswordResetToken
    {
        public int Id { get; set; } // Primary key
        public string UserId { get; set; } // Foreign key to ApplicationUser 
        public string Token { get; set; } // The reset token
        public DateTime ExpirationDate { get; set; } // Expiration date of the token

        // Navigation property
        public ApplicationUser User { get; set; }
    }
}