namespace WebApplication1.Model
{
    public class PasswordHistory
    {
        public int Id { get; set; } // Primary key for the PasswordHistory table
        public string UserId { get; set; } // Foreign key to ApplicationUser  
        public string PasswordHash { get; set; } // Store the hashed password
        public DateTime CreatedAt { get; set; } // When the password was created

        public ApplicationUser User { get; set; } // Navigation property
    }
}
