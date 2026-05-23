using Microsoft.AspNetCore.Mvc;
using retakebackend.Auth;
using retakebackend.Services;

namespace retakebackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var token = authService.Login(request.Username, request.Password);
        if (token is null)
        {
            return Unauthorized();
        }

        return Ok(new { accessToken = token });
    }
}
