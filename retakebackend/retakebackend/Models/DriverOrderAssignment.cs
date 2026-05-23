namespace retakebackend.Models;

public class DriverOrderAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    public DateTime AssignedTime { get; set; } = DateTime.UtcNow;
}
