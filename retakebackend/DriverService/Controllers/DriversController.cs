using DriverService.Contracts;
using DriverService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DriverService.Controllers;

[ApiController]
[Route("drivers")]
public class DriversController(DriversService driversService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthDriverResponse>> Register(RegisterDriverRequest request)
    {
        try
        {
            return Ok(await driversService.RegisterAsync(request));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost]
    public Task<ActionResult<AuthDriverResponse>> RegisterLegacy(RegisterDriverRequest request)
        => Register(request);

    [HttpPost("login")]
    public async Task<ActionResult<AuthDriverResponse>> Login(LoginDriverRequest request)
    {
        var driver = await driversService.LoginAsync(request);
        return driver is null ? Unauthorized() : Ok(driver);
    }

    [HttpGet("available")]
    public async Task<ActionResult<DriverResponse>> GetAvailable()
    {
        var driver = await driversService.GetAvailableAsync();
        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateDriverStatusRequest request)
    {
        var result = await driversService.UpdateManualStatusAsync(id, request.Status);
        if (!result.Found) return NotFound();
        if (!result.ValidStatus) return BadRequest("Разрешены только статусы: on_line, off_line.");
        return Ok(result.Driver);
    }

    [HttpPost("{id:guid}/assign-order")]
    public async Task<IActionResult> AssignOrder(Guid id, AssignOrderToDriverRequest request)
        => await driversService.AssignOrderAsync(id, request.OrderId) ? Ok() : BadRequest("Driver is not available");

    [HttpGet("{id:guid}/orders")]
    public async Task<ActionResult<IEnumerable<Guid>>> GetDriverOrders(Guid id)
    {
        var result = await driversService.GetDriverOrdersAsync(id);
        return result.DriverExists ? Ok(result.OrderIds) : NotFound();
    }
}
