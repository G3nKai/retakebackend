namespace retakebackend.Contracts;

public record CreateOrderRequest(Guid ClientId, string PickupAddress, string DestinationAddress);
public record OrderResponse(Guid Id, Guid ClientName, string PickupAddress, string DestinationAddress, string Status, Guid DriverId, DateTime CreatedAtUtc);
