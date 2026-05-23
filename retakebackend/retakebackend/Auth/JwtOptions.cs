namespace retakebackend.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "retakebackend";
    public string Audience { get; set; } = "retakebackend-clients";
    public string Key { get; set; } = "super-secret-dev-key-change-me";
    public int ExpirationMinutes { get; set; } = 120;
}
