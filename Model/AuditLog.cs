namespace WebApplication1.Model
{
    public class AuditLog
    {
        public int Id { get; set; } // Primary key
        public string UserName { get; set; } // The username of the user performing the action
        public string Action { get; set; } // Description of the action performed
        public DateTime Timestamp { get; set; } // When the action occurred
    }
}