namespace Backend_Bridge.DTO
{
    public class EmailNotificationDto
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
