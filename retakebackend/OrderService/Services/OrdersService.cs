using Microsoft.EntityFrameworkCore;
using OrderService.Contracts;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services;

public class OrdersService(OrderDbContext dbContext, DriverGatewayClient driverGateway)
{
    public async Task<(bool Success, OrderResponse? Order, string? Error)> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var availableDriver = await driverGateway.GetAvailableDriverAsync(cancellationToken);
        if (availableDriver is null)
        {
            return (false, null, "No available drivers on line");
        }

        var order = new Order
        {
            ClientId = request.ClientId,
            PickupAddress = request.PickupAddress,
            DestinationAddress = request.DestinationAddress,
            DriverId = availableDriver.Id,
            Status = OrderStatus.DriverAssigned
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        var assigned = await driverGateway.AssignOrderToDriverAsync(availableDriver.Id, order.Id, cancellationToken);
        if (!assigned)
        {
            return (false, null, "Unable to set driver to busy state");
        }

        return (true, ToResponse(order), null);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        return order is null ? null : ToResponse(order);
    }

    private static OrderResponse ToResponse(Order order) =>
        new(order.Id, order.ClientId, order.PickupAddress, order.DestinationAddress, order.Status.ToString(), order.DriverId, order.CreatedAtUtc);
}
