using Microsoft.AspNetCore.Mvc;
using OrderService.Contracts;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(OrdersService ordersService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await ordersService.CreateAsync(request, cancellationToken);
        if (!result.Success || result.Order is null)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Order.Id }, result.Order);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await ordersService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}
