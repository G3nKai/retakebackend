namespace retakebackend.Models;

public class DriverOrderAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = null!;
    public Guid OrderId { get; set; }
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
}
