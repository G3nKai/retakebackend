using Microsoft.AspNetCore.Mvc;
using retakebackend.consumer.Auth;

namespace retakebackend.consumer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(JwtTokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        if (request.Username != "admin" || request.Password != "admin") return Unauthorized();
        return Ok(new { accessToken = tokenService.CreateToken(request.Username) });
    }
}
