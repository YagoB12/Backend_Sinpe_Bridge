using Backend_Bridge.Constants;
using Backend_Bridge.Data;
using Backend_Bridge.DTO;
using Backend_Bridge.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Bridge.Services.Orders
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public (bool IsSuccess, string Message, Order? Order) CreateOrder(CreateOrderDTO request)
        {
            if (request == null || request.Details == null || !request.Details.Any())
                return (false, "La orden debe tener productos.", null);

            var existingOrder = _context.Orders
                .FirstOrDefault(o => o.CustomerName == request.CustomerName && o.Status == OrderStatuses.Pending);

            if (existingOrder != null)
                return (false, "El cliente ya tiene una orden pendiente.", null);

            var details = new List<OrderDetail>();
            decimal total = 0;

            foreach (var detailRequest in request.Details)
            {
                var product = _context.Products.Find(detailRequest.ProductId);

                if (product == null)
                    return (false, $"Producto {detailRequest.ProductId} no existe.", null);

                if (product.Stock < detailRequest.Quantity)
                    return (false, $"Stock insuficiente para {product.Name}", null);

                var detail = new OrderDetail
                {
                    ProductId = product.Id,
                    Quantity = detailRequest.Quantity,
                    UnitPrice = product.Price
                };

                total += detail.Quantity * detail.UnitPrice;
                product.Stock -= detailRequest.Quantity;
                details.Add(detail);
            }

            var order = new Order
            {
                CustomerName = request.CustomerName,
                Phone = request.Phone,
                Amount = total,
                Status = OrderStatuses.Pending,
                CreatedAt = DateTime.Now,
                Details = details
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            AssociateAdvancedPaymentIfExists(order);

            return (true, "Orden creada correctamente.", order);
        }

        public object? GetLatestPendingOrder()
        {
            return _context.Orders
                .Where(o => o.Status == OrderStatuses.Pending)
                .Include(o => o.Details)
                .ThenInclude(d => d.Product)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerName,
                    o.Phone,
                    o.Amount,
                    o.Status,
                    o.CreatedAt
                })
                .FirstOrDefault();
        }

        public object GetOrders()
        {
            return _context.Orders
                .Include(o => o.Details)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerName,
                    o.Phone,
                    o.Amount,
                    o.Status,
                    o.CreatedAt
                })
                .ToList();
        }

        public object SearchOrders(string query)
        {
            return _context.Orders
                .Include(o => o.Details)
                .Where(o =>
                    o.CustomerName.Contains(query) ||
                    o.Phone.Contains(query)
                )
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerName,
                    o.Phone,
                    o.Amount,
                    o.Status,
                    o.CreatedAt
                })
                .ToList();
        }

        private void AssociateAdvancedPaymentIfExists(Order order)
        {
            var advancedPayment = _context.Payments
                .Where(p =>
                    p.Status == PaymentStatuses.PendingAssociation &&
                    p.Amount == order.Amount &&
                    p.SenderNumber == order.Phone &&
                    p.OrderId == null)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            if (advancedPayment == null)
                return;

            advancedPayment.OrderId = order.Id;
            advancedPayment.Status = PaymentStatuses.Approved;
            advancedPayment.VerificationResult = "Pago adelantado asociado automáticamente";

            order.Status = OrderStatuses.Paid;

            _context.SaveChanges();
        }
    }
}
