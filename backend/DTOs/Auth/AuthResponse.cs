namespace Conectando.Api.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserResponse User { get; init; } = null!;
}