using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using retakebackend.Contracts;
using retakebackend.Data;
using retakebackend.Models;

namespace retakebackend.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request)
    {
        var availableDriver = await dbContext.Drivers
            .OrderBy(d => d.Id)
            .FirstOrDefaultAsync(d => d.Status == DriverStatus.Available);

        if (availableDriver is null)
        {
            return BadRequest("No available drivers");
        }

        var order = new Order
        {
            ClientId = request.ClientId,
            PickupAddress = request.PickupAddress,
            DestinationAddress = request.DestinationAddress,
            DriverId = availableDriver.Id,
            Status = OrderStatus.DriverAssigned
        };

        availableDriver.Status = DriverStatus.Busy;

        dbContext.Orders.Add(order);
        dbContext.DriverOrderAssignments.Add(new DriverOrderAssignment
        {
            DriverId = availableDriver.Id,
            OrderId = order.Id
        });

        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToResponse(order));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id)
    {
        var order = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? NotFound() : Ok(ToResponse(order));
    }

    private static OrderResponse ToResponse(Order order) =>
        new(order.Id, order.ClientId, order.PickupAddress, order.DestinationAddress, order.Status.ToString(), order.DriverId, order.CreatedTime);
}
