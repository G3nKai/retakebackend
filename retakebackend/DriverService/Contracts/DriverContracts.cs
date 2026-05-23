namespace DriverService.Contracts;

public record AuthDriverResponse(Guid Id, string Name, string Status, string Token);
public record DriverResponse(Guid Id, string Name, string Status);
public record UpdateDriverStatusRequest(string Status);
public record RegisterDriverRequest(string Name, string Login, string Password);
public record LoginDriverRequest(string Login, string Password);
public record AssignOrderToDriverRequest(Guid OrderId);

public record DriverSummary(Guid Id, string Name, string Status);
public record GetAvailableDriverRpcRequest;
public record GetAvailableDriverRpcResponse(bool Found, DriverSummary? Driver);
public record AssignOrderRpcRequest(Guid DriverId, Guid OrderId);
public record AssignOrderRpcResponse(bool Success, string? DriverName, Guid? ClientId);
public record OrderAssignedNotificationEvent(Guid OrderId, Guid DriverId, string DriverName, Guid ClientId);
