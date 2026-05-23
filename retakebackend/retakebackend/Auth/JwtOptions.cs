namespace retakebackend.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "retakebackend";
    public string Audience { get; set; } = "retakebackend-clients";
    public string Key { get; set; } = "this-is-a-very-long-dev-secret-key-please-change";
    public int ExpirationMinutes { get; set; } = 120;
}
