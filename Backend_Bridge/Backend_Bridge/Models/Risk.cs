namespace Backend_Bridge.Models
{
    public class Risk
    {
        public int Id { get; set; }

        public string Level { get; set; }

        public string Description { get; set; }

        public ICollection<Payment> Payment { get; set; }
            = new List<Payment>();
    }
}
