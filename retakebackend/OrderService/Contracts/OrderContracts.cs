namespace OrderService.Contracts;

public record CreateOrderRequest(string PickupAddress, string DestinationAddress);
public record OrderResponse(Guid Id, Guid ClientId, string PickupAddress, string DestinationAddress, string Status, Guid DriverId, DateTime CreatedAtUtc);
