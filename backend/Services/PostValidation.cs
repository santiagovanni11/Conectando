using Conectando.Api.Exceptions;
using Conectando.Api.Settings;

namespace Conectando.Api.Services;

public static class PostValidation
{
    public static string? NormalizeContent(string? content) =>
        string.IsNullOrWhiteSpace(content) ? null : content.Trim();

    public static void ValidateOrThrow(string? content, int mediaCount)
    {
        if (mediaCount > PostConstants.MaxMediaPerPost)
        {
            throw new MediaLimitException($"Una publicación puede tener hasta {PostConstants.MaxMediaPerPost} fotos.");
        }

        if (string.IsNullOrEmpty(content) && mediaCount == 0)
        {
            throw new EmptyPostException();
        }

        if (content is not null && content.Length > PostConstants.ContentMaxLength)
        {
            throw new MediaLimitException($"El texto no puede superar los {PostConstants.ContentMaxLength} caracteres.");
        }
    }
}