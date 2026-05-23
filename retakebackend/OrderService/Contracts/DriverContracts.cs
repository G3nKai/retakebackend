namespace OrderService.Contracts;

public record DriverSummary(Guid Id, string Name, string Status);
public record AssignOrderToDriverRequest(Guid OrderId);
