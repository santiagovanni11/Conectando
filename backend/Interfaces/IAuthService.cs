using Conectando.Api.DTOs.Auth;

namespace Conectando.Api.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
}