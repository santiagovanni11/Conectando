namespace Conectando.Api.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Conectando.Api";
    public string Audience { get; set; } = "Conectando.Frontend";
    public int AccessTokenExpirationMinutes { get; set; } = 60;
}