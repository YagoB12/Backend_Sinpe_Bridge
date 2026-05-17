namespace Backend_Bridge.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Reference { get; set; }

        public int? OrderId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}