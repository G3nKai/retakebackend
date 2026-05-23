namespace retakebackend.Contracts;

public record CreateOrderRequest(string ClientName, string PickupAddress, string DestinationAddress);
public record OrderResponse(Guid Id, string ClientName, string PickupAddress, string DestinationAddress, string Status, Guid DriverId, DateTime CreatedAtUtc);
