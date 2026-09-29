using Conectando.Api.Data;
using Conectando.Api.DTOs.Auth;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class AuthService(ConectandoDbContext dbContext, IJwtService jwtService) : IAuthService
{
    private readonly ConectandoDbContext _dbContext = dbContext;
    private readonly IJwtService _jwtService = jwtService;

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var userName = request.UserName.Trim();
        var email = NormalizeEmail(request.Email);
        var displayName = ResolveDisplayName(request.DisplayName, userName);

        if (PasswordPolicy.Validate(request.Password) is { } reason)
        {
            throw new WeakPasswordException(reason);
        }

        if (await HasUserNameAsync(userName, cancellationToken))
        {
            throw new DuplicateUserNameException();
        }

        if (await HasEmailAsync(email, cancellationToken))
        {
            throw new DuplicateEmailException();
        }

        var now = DateTime.UtcNow;
        var user = new AppUser
        {
            UserName = userName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = displayName,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterResponse { User = MapToUserResponse(user) };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email.ToLower() == email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        // Una cuenta dada de baja queda anonimizada, con una contraseña
        // aleatoria: el chequeo de arriba casi nunca la deja pasar, pero
        // este filtro evita depender de ese detalle para algo tan sensible.
        if (user.DeletedAt is not null)
        {
            throw new InvalidCredentialsException();
        }

        return BuildAuthResponse(user);
    }

    public async Task<UserResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user is null
            ? throw new UserNotFoundException()
            : MapToUserResponse(user);
    }

    private async Task<bool> HasUserNameAsync(string userName, CancellationToken cancellationToken)
    {
        var normalized = userName.ToLowerInvariant();
        return await _dbContext.Users.AnyAsync(u => u.UserName.ToLower() == normalized, cancellationToken);
    }

    private async Task<bool> HasEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == email, cancellationToken);
    }

    private AuthResponse BuildAuthResponse(AppUser user)
    {
        var token = _jwtService.CreateToken(user, out var expiresAt);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapToUserResponse(user),
        };
    }

    private static UserResponse MapToUserResponse(AppUser user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        DisplayName = user.DisplayName,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
    };

    private static string ResolveDisplayName(string? displayName, string userName)
    {
        var trimmed = displayName?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? userName : trimmed;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}