namespace Conectando.Api.DTOs.Social;

public class UserSummaryDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }

    /// <summary>
    /// La cuenta fue dada de baja.
    /// </summary>
    /// <remarks>
    /// <see cref="UserName"/> viene vacío y <see cref="ProfileImageUrl"/> en
    /// null cuando está en true: no hay nada que|linkear ni que mostrar. La
    /// decisión de qué se dibuja en su lugar la toma
    /// <c>UserPresentation</c> antes de armar el DTO, no cada componente.
    /// </remarks>
    public bool IsDeleted { get; init; }
}