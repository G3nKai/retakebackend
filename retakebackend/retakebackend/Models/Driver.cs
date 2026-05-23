namespace retakebackend.Models;

public class Driver
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DriverStatus Status { get; set; } = DriverStatus.Offline;
    public ICollection<DriverOrderAssignment> Assignments { get; set; } = [];
}

public enum DriverStatus
{
    Offline = 0,
    Available = 1,
    Busy = 2
}
