using Microsoft.AspNetCore.Mvc;
using retakebackend.Auth;

namespace retakebackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(JwtTokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        if (request.Username != "admin" || request.Password != "admin")
        {
            return Unauthorized();
        }

        var token = tokenService.CreateToken(request.Username);
        return Ok(new { accessToken = token });
    }
}
