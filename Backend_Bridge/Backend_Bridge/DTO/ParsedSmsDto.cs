namespace Backend_Bridge.DTOs
{
    public class ParsedSmsDto
    {
        public decimal Amount { get; set; }

        public string PayerName { get; set; } = string.Empty;

        public string Reference { get; set; } = string.Empty;
    }
}