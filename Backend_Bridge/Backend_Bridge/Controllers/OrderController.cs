using Backend_Bridge.Data;
using Backend_Bridge.Models;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult CreateOrder([FromBody] Order order)
    {
        order.Status = "PENDING";

        _context.Orders.Add(order);
        _context.SaveChanges();

        return Ok(order);
    }
}