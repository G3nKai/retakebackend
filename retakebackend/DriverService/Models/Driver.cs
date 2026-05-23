namespace DriverService.Models;

public class Driver
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DriverStatus Status { get; set; } = DriverStatus.Offline;
    public ICollection<DriverOrderAssignment> Assignments { get; set; } = [];
}
