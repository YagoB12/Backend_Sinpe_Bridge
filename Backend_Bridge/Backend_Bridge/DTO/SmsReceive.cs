public class SmsReceive
{
    public string Message { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public DateTime ReceivedAt { get; set; }
}
