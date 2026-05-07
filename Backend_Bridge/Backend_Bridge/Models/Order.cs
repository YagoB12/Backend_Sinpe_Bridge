namespace Backend_Bridge.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string Phone { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = "PENDING"; // PENDING, PAID, REJECTED

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}