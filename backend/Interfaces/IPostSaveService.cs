using Conectando.Api.DTOs.Posts;

namespace Conectando.Api.Interfaces;

public interface IPostSaveService
{
    /// <summary>Guarda o quita la publicación. Devuelve el estado resultante.</summary>
    Task<PostSaveStatusDto> ToggleAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);

    /// <summary>Las publicaciones guardadas por el usuario, de más nueva a más vieja.</summary>
    Task<PostListDto> ListSavedAsync(Guid userId, PostListQuery query, CancellationToken cancellationToken = default);
}