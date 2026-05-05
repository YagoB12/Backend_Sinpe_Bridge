namespace Backend_Bridge.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string SenderNumber { get; set; }
    }
}
