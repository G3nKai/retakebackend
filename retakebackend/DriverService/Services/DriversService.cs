using DriverService.Contracts;
using DriverService.Data;
using DriverService.Models;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Services;

public class DriversService(DriverDbContext dbContext, TokenService tokenService)
{
    public async Task<AuthDriverResponse> RegisterAsync(RegisterDriverRequest request)
    {
        var login = request.Login.Trim();
        if (string.IsNullOrWhiteSpace(login))
        {
            throw new ArgumentException("Логин обязателен.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Пароль обязателен.");
        }

        var exists = await dbContext.Drivers.AnyAsync(d => d.Login.ToLower() == login.ToLower());
        if (exists)
        {
            throw new InvalidOperationException("Водитель с таким логином уже существует.");
        }

        var driver = new Driver
        {
            Name = request.Name,
            Login = login,
            Password = request.Password,
            Status = DriverStatus.Offline
        };
        dbContext.Drivers.Add(driver);
        await dbContext.SaveChangesAsync();
        return ToAuthResponse(driver, tokenService.GenerateDriverToken(driver));
    }

    public async Task<AuthDriverResponse?> LoginAsync(LoginDriverRequest request)
    {
        var login = request.Login.Trim();
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var driver = await dbContext.Drivers.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Login.ToLower() == login.ToLower() && d.Password == request.Password);

        return driver is null ? null : ToAuthResponse(driver, tokenService.GenerateDriverToken(driver));
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


    public async Task<DriverResponse?> GetByIdAsync(Guid id)
    {
        var driver = await dbContext.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        return driver is null ? null : ToResponse(driver);
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

    private static AuthDriverResponse ToAuthResponse(Driver driver, string token) =>
        new(driver.Id, driver.Name, driver.Status.ToString(), token);

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
