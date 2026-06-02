namespace Backend_Bridge.DTO
{
    public class PaymentRiskDto
    {
        public int PaymentId { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public string RiskLevel { get; set; }
        public string RiskDescription { get; set; }
    }
}
