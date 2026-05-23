namespace retakebackend.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClientId { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public Guid DriverId { get; set; }
    public Driver Driver { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}
