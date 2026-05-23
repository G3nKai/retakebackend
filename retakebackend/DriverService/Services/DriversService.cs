using DriverService.Contracts;
using DriverService.Data;
using DriverService.Models;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Services;

public class DriversService(DriverDbContext dbContext)
{
    public async Task<DriverResponse> RegisterAsync(RegisterDriverRequest request)
    {
        var driver = new Driver { Name = request.Name, Status = DriverStatus.Offline };
        dbContext.Drivers.Add(driver);
        await dbContext.SaveChangesAsync();
        return ToResponse(driver);
    }

    public async Task<DriverResponse?> GetAvailableAsync()
    {
        var driver = await dbContext.Drivers.AsNoTracking().OrderBy(d => d.Id).FirstOrDefaultAsync(d => d.Status == DriverStatus.Available);
        return driver is null ? null : ToResponse(driver);
    }

    public async Task<(bool Found, bool ValidStatus, DriverResponse? Driver)> UpdateManualStatusAsync(Guid id, string status)
    {
        var driver = await dbContext.Drivers.FindAsync(id);
        if (driver is null) return (false, true, null);

        if (!TryParseManualStatus(status, out var nextStatus)) return (true, false, null);

        driver.Status = nextStatus;
        await dbContext.SaveChangesAsync();
        return (true, true, ToResponse(driver));
    }

    public async Task<bool> AssignOrderAsync(Guid driverId, Guid orderId)
    {
        var driver = await dbContext.Drivers.FindAsync(driverId);
        if (driver is null || driver.Status != DriverStatus.Available) return false;

        driver.Status = DriverStatus.Busy;
        dbContext.DriverOrderAssignments.Add(new DriverOrderAssignment { DriverId = driverId, OrderId = orderId });
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<(bool DriverExists, List<Guid> OrderIds)> GetDriverOrdersAsync(Guid driverId)
    {
        var exists = await dbContext.Drivers.AnyAsync(x => x.Id == driverId);
        if (!exists) return (false, []);

        var orderIds = await dbContext.DriverOrderAssignments.AsNoTracking()
            .Where(x => x.DriverId == driverId)
            .OrderByDescending(x => x.AssignedAtUtc)
            .Select(x => x.OrderId)
            .ToListAsync();

        return (true, orderIds);
    }

    private static DriverResponse ToResponse(Driver driver) => new(driver.Id, driver.Name, driver.Status.ToString());

    private static bool TryParseManualStatus(string status, out DriverStatus nextStatus)
    {
        switch (status.Trim().ToLowerInvariant())
        {
            case "on_line":
                nextStatus = DriverStatus.Available;
                return true;
            case "off_line":
                nextStatus = DriverStatus.Offline;
                return true;
            default:
                nextStatus = default;
                return false;
        }
    }
}
