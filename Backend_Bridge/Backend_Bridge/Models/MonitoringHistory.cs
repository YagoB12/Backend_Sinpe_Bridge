namespace Backend_Bridge.Models
{
    public class MonitoringHistory
    {
        public int Id { get; set; }
        public DateTime DisconnectedAt { get; set; }
        public string Message { get; set; }
        public bool IsResolved { get; set; }
    }
}
