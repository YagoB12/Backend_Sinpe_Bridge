using Backend_Bridge.DTO;
using Backend_Bridge.Services.Orders;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("orders")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderDTO request)
    {
        var result = _orderService.CreateOrder(request);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result.Order);
    }

    [HttpGet("pending")]
    public IActionResult GetPendingOrder()
    {
        var order = _orderService.GetLatestPendingOrder();

        if (order == null)
            return NotFound("No hay órdenes pendientes.");

        return Ok(order);
    }

    [HttpGet]
    public IActionResult GetOrders()
    {
        return Ok(_orderService.GetOrders());
    }

    [HttpGet("search")]
    public IActionResult SearchOrders([FromQuery] string query)
    {
        return Ok(_orderService.SearchOrders(query));
    }
}
