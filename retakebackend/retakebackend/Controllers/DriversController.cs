using Microsoft.AspNetCore.Mvc;
using retakebackend.Contracts;
using retakebackend.Services;

namespace retakebackend.Controllers;

[ApiController]
[Route("drivers")]
public class DriversController(DriversService driversService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DriverResponse>> Register(RegisterDriverRequest request)
    {
        var driver = await driversService.RegisterAsync(request);
        return Ok(driver);
    }

    [HttpGet("available")]
    public async Task<ActionResult<DriverResponse>> GetAvailable()
    {
        var driver = await driversService.GetAvailableAsync();
        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DriverResponse>> GetById(Guid id)
    {
        var driver = await driversService.GetByIdAsync(id);
        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateDriverStatusRequest request)
    {
        var result = await driversService.UpdateStatusAsync(id, request.Status);
        if (!result.Found)
        {
            return NotFound();
        }

        if (!result.ValidStatus)
        {
            return BadRequest("Разрешены только статусы: on_line, off_line.");
        }

        return Ok(result.Driver);
    }

    [HttpGet("{id:guid}/orders")]
    public async Task<ActionResult<IEnumerable<Guid>>> GetDriverOrders(Guid id)
    {
        var result = await driversService.GetDriverOrdersAsync(id);
        return !result.DriverExists ? NotFound() : Ok(result.OrderIds);
    }
}
