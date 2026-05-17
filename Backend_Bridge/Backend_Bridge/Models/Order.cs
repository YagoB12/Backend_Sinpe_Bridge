namespace Backend_Bridge.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }   // NUEVO
        public string Phone { get; set; }          // ahora sí teléfono real

        public decimal Amount { get; set; }

        public string Status { get; set; } = "PENDING";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<OrderDetail> Details { get; set; }
    }
}