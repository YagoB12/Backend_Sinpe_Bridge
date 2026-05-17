namespace Backend_Bridge.DTO
{
    public class CreateOrderDTO
    {
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public List<CreateOrderDetailDTO> Details { get; set; }
    }

    public class CreateOrderDetailDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
