using Microsoft.AspNetCore.Mvc;
using retakebackend.Contracts;
using retakebackend.Services;

namespace retakebackend.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(OrdersService ordersService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var result = await ordersService.CreateAsync(request);
        if (!result.Success || result.Order is null)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Order.Id }, result.Order);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id)
    {
        var order = await ordersService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }
}
