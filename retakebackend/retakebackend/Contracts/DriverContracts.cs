namespace retakebackend.Contracts;

public record DriverResponse(Guid Id, string Name, string Status);
public record UpdateDriverStatusRequest(string Status);
public record RegisterDriverRequest(string Name, double Latitude, double Longitude);
