using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Model
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        // DbSet for AuditLog
        public DbSet<AuditLog> AuditLogs { get; set; } // Add this line

        // DbSet for PasswordHistory
        public DbSet<PasswordHistory> PasswordHistories { get; set; } // Add this line

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the AuditLog entity
            modelBuilder.Entity<AuditLog>()
                .Property(a => a.UserName)
                .IsRequired(); // Make UserName required

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Action)
                .IsRequired(); // Make Action required

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()"); // Set default value for Timestamp

            // Configure the PasswordHistory entity
            modelBuilder.Entity<PasswordHistory>()
                .HasOne(ph => ph.User)
                .WithMany(u => u.PasswordHistories)
                .HasForeignKey(ph => ph.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Configure delete behavior
        }
    }
}