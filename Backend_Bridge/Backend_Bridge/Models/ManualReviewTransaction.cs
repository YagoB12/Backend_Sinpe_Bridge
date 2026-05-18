namespace Backend_Bridge.Models
{
    public class ManualReviewTransaction
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public string ActionType { get; set; }// Aprobar, rechazar y SUSPECTED 
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
