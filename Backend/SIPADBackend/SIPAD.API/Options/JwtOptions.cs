namespace SIPAD.API.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SIPAD.API";
    public string Audience { get; set; } = "SIPAD.Frontend";
    public string Key { get; set; } = "change_this_super_secret_key_with_at_least_32_chars";
    public int ExpirationMinutes { get; set; } = 120;
}
