namespace Backend_Bridge.Models
{
    public class EmailNotificationLog
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
