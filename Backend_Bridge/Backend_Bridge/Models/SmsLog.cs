namespace Backend_Bridge.Models
{
    public class SmsLog
    {
        public int Id { get; set; }
        public string SenderNumber { get; set; }
        public string MessageBody { get; set; }
        public DateTime ReceivedAt { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsValidOrigin { get; set; }
    }
}
