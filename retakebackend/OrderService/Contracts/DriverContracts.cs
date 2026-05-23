namespace OrderService.Contracts;

public record DriverSummary(Guid Id, string Name, string Status);
public record AssignOrderToDriverRequest(Guid OrderId);

public record GetAvailableDriverRpcRequest;
public record GetAvailableDriverRpcResponse(bool Found, DriverSummary? Driver);
public record AssignOrderRpcRequest(Guid DriverId, Guid OrderId, Guid ClientId);
public record AssignOrderRpcResponse(bool Success, string? DriverName, Guid? ClientId);
public record OrderAssignedNotificationEvent(Guid OrderId, Guid DriverId, string DriverName, Guid ClientId);
