namespace DriverService.Contracts;

public record DriverResponse(Guid Id, string Name, string Status);
public record UpdateDriverStatusRequest(string Status);
public record RegisterDriverRequest(string Name);
public record AssignOrderToDriverRequest(Guid OrderId);
