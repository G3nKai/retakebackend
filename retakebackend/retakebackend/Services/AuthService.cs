using retakebackend.Auth;

namespace retakebackend.Services;

public class AuthService(JwtTokenService tokenService)
{
    public string? Login(string username, string password)
    {
        if (username != "admin" || password != "admin")
        {
            return null;
        }

        return tokenService.CreateToken(username);
    }
}
