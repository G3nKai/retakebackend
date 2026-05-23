using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using retakebackend.Contracts;
using retakebackend.Data;
using retakebackend.Models;

namespace retakebackend.Controllers;

[ApiController]
[Route("drivers")]
public class DriversController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DriverResponse>> Register(RegisterDriverRequest request)
    {
        var driver = new Driver
        {
            Name = request.Name,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Status = DriverStatus.Offline
        };

        dbContext.Drivers.Add(driver);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = driver.Id }, ToResponse(driver));
    }

    [HttpGet("available")]
    public async Task<ActionResult<DriverResponse>> GetAvailable()
    {
        var driver = await dbContext.Drivers.AsNoTracking()
            .OrderBy(d => d.Id)
            .FirstOrDefaultAsync(d => d.Status == DriverStatus.Available);

        return driver is null ? NotFound() : Ok(ToResponse(driver));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DriverResponse>> GetById(Guid id)
    {
        var driver = await dbContext.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        return driver is null ? NotFound() : Ok(ToResponse(driver));
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateDriverStatusRequest request)
    {
        var driver = await dbContext.Drivers.FindAsync(id);
        if (driver is null) return NotFound();

        if (!TryParseManualStatus(request.Status, out var nextStatus))
        {
            return BadRequest("Разрешены только статусы: on_line, off_line.");
        }

        driver.Status = nextStatus;
        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(driver));
    }

    [HttpGet("{id:guid}/orders")]
    public async Task<ActionResult<IEnumerable<Guid>>> GetDriverOrders(Guid id)
    {
        var driverExists = await dbContext.Drivers.AnyAsync(d => d.Id == id);
        if (!driverExists) return NotFound();

        var orderIds = await dbContext.DriverOrderAssignments.AsNoTracking()
            .Where(a => a.DriverId == id)
            .OrderByDescending(a => a.AssignedAtUtc)
            .Select(a => a.OrderId)
            .ToListAsync();

        return Ok(orderIds);
    }

    private static bool TryParseManualStatus(string status, out DriverStatus driverStatus)
    {
        driverStatus = status.Trim().ToLowerInvariant() switch
        {
            "on_line" => DriverStatus.Available,
            "off_line" => DriverStatus.Offline,
            _ => driverStatus
        };

        return status.Trim().ToLowerInvariant() is "on_line" or "off_line";
    }

    private static DriverResponse ToResponse(Driver driver) =>
        new(driver.Id, driver.Name, driver.Latitude, driver.Longitude, driver.Status.ToString());
}
