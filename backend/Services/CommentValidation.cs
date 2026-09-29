using Conectando.Api.Exceptions;

namespace Conectando.Api.Services;

public static class CommentValidation
{
    public const int MaxLength = 1000;

    public static void ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new EmptyCommentException();
        }

        if (content.Length > MaxLength)
        {
            throw new CommentTooLongException();
        }
    }
}