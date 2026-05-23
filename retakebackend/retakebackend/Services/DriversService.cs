using Microsoft.EntityFrameworkCore;
using retakebackend.Contracts;
using retakebackend.Data;
using retakebackend.Models;

namespace retakebackend.Services;

public class DriversService(AppDbContext dbContext)
{
    public async Task<DriverResponse> RegisterAsync(RegisterDriverRequest request)
    {
        var driver = new Driver
        {
            Name = request.Name,
            Status = DriverStatus.Offline
        };

        dbContext.Drivers.Add(driver);
        await dbContext.SaveChangesAsync();

        return ToResponse(driver);
    }

    public async Task<DriverResponse?> GetAvailableAsync()
    {
        var driver = await dbContext.Drivers.AsNoTracking()
            .OrderBy(d => d.Id)
            .FirstOrDefaultAsync(d => d.Status == DriverStatus.Available);

        return driver is null ? null : ToResponse(driver);
    }

    public async Task<DriverResponse?> GetByIdAsync(Guid id)
    {
        var driver = await dbContext.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        return driver is null ? null : ToResponse(driver);
    }

    public async Task<(bool Found, bool ValidStatus, DriverResponse? Driver)> UpdateStatusAsync(Guid id, string status)
    {
        var driver = await dbContext.Drivers.FindAsync(id);
        if (driver is null)
        {
            return (false, true, null);
        }

        if (!TryParseManualStatus(status, out var nextStatus))
        {
            return (true, false, null);
        }

        driver.Status = nextStatus;
        await dbContext.SaveChangesAsync();

        return (true, true, ToResponse(driver));
    }

    public async Task<(bool DriverExists, List<Guid> OrderIds)> GetDriverOrdersAsync(Guid id)
    {
        var driverExists = await dbContext.Drivers.AnyAsync(d => d.Id == id);
        if (!driverExists)
        {
            return (false, []);
        }

        var orderIds = await dbContext.DriverOrderAssignments.AsNoTracking()
            .Where(a => a.DriverId == id)
            .OrderByDescending(a => a.AssignedTime)
            .Select(a => a.OrderId)
            .ToListAsync();

        return (true, orderIds);
    }

    private static DriverResponse ToResponse(Driver driver) => new(driver.Id, driver.Name, driver.Status.ToString());

    private static bool TryParseManualStatus(string status, out DriverStatus driverStatus)
    {
        string normalizedStatus = status.Trim().ToLowerInvariant();

        switch (normalizedStatus)
        {
            case "online":
                driverStatus = DriverStatus.Available;
                return true;
            case "offline":
                driverStatus = DriverStatus.Offline;
                return true;
            default:
                driverStatus = default;
                return false;
        }
    }
}
