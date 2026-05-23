namespace OrderService.Contracts;

public record DriverSummary(Guid Id, string Name, string Status);
public record AssignOrderToDriverRequest(Guid OrderId);

public record GetAvailableDriverRpcRequest;
public record GetAvailableDriverRpcResponse(bool Found, DriverSummary? Driver);
public record AssignOrderRpcRequest(Guid DriverId, Guid OrderId);
public record AssignOrderRpcResponse(bool Success);
