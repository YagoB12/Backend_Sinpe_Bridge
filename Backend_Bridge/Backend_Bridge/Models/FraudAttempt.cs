namespace Backend_Bridge.Models
{
    public class FraudAttempt
    {
        public int Id { get; set; }
        public string FraudType { get; set; }
        public string Reference { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}
