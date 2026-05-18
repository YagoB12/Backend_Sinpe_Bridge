namespace Backend_Bridge.Models
{
    public class DeviceHeartbeat
    {
        public int Id { get; set; }
        public string DeviceName { get; set; } = "Teléfono Principal POS";
        public DateTime LastCommunication { get; set; }
        public bool IsConnected { get; set; }
    }
}
