namespace retakebackend.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ClientName { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public Guid DriverId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum OrderStatus
{
    Created = 0,
    DriverAssigned = 1
}
