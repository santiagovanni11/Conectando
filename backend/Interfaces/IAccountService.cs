using Conectando.Api.DTOs.Account;

namespace Conectando.Api.Interfaces;

public interface IAccountService
{
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task DeleteAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken = default);
}
