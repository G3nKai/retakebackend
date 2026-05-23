namespace DriverService.Contracts;

public record DriverResponse(Guid Id, string Name, string Status);
public record UpdateDriverStatusRequest(string Status);
public record RegisterDriverRequest(string Name);
public record AssignOrderToDriverRequest(Guid OrderId);

public record DriverSummary(Guid Id, string Name, string Status);
public record GetAvailableDriverRpcRequest;
public record GetAvailableDriverRpcResponse(bool Found, DriverSummary? Driver);
public record AssignOrderRpcRequest(Guid DriverId, Guid OrderId);
public record AssignOrderRpcResponse(bool Success);
