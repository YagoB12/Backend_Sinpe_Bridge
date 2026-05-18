using Backend_Bridge.Data;
using Backend_Bridge.DTO;
using Backend_Bridge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("orders")]
public class OrderController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderDTO request)
    {
        if (request == null || request.Details == null || !request.Details.Any())
            return BadRequest("La orden debe tener productos.");

        //  EVITAR MÚLTIPLES ÓRDENES PENDING
        var existingOrder = _context.Orders
            .FirstOrDefault(o => o.CustomerName == request.CustomerName && o.Status == "PENDING");

        if (existingOrder != null)
            return BadRequest("El cliente ya tiene una orden pendiente.");

        decimal total = 0;
        var details = new List<OrderDetail>();

        foreach (var d in request.Details)
        {
            var product = _context.Products.Find(d.ProductId);

            if (product == null)
                return BadRequest($"Producto {d.ProductId} no existe.");

            if (product.Stock < d.Quantity)
                return BadRequest($"Stock insuficiente para {product.Name}");

            var detail = new OrderDetail
            {
                ProductId = product.Id,
                Quantity = d.Quantity,
                UnitPrice = product.Price
            };

            total += detail.Quantity * detail.UnitPrice;

            product.Stock -= d.Quantity;

            details.Add(detail);
        }

        var order = new Order
        {
            CustomerName = request.CustomerName,
            Phone = request.Phone,
            Amount = total,
            Status = "PENDING",
            CreatedAt = DateTime.Now,
            Details = details
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        return Ok(order);
    }
        // RF 09 conssulta rapida (Ver ordenes pedientes)

        [HttpGet("pending")]

        public IActionResult GetPendingOrders()
        {
            var orders = _context.Orders
                .Where(o => o.Status == "PENDING")
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)

                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return Ok(orders);
        }
    }




