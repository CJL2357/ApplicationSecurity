using System;
using System.Threading.Tasks;
using WebApplication1.Model;

namespace WebApplication1.Services
{
    public class AuditService
    {
        private readonly AuthDbContext _context;

        public AuditService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task LogUserActivity(string userName, string action)
        {
            var auditLog = new AuditLog
            {
                UserName = userName,
                Action = action,
                Timestamp = DateTime.UtcNow // Use UTC for consistency
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}