using Conectando.Api.Data.Configuration;

namespace Conectando.Api.Settings;

public static class PostConstants
{
    public const int MaxMediaPerPost = 8;
    public const int MaxPendingMediaPerUser = 16;
    public const int ContentMaxLength = PostConfiguration.MaxContentLength;
    public const long MaxUploadRequestBodyBytes = MaxMediaPerPost * PostFileValidator.MaxFileBytes;
    public static readonly TimeSpan PendingMediaTtl = TimeSpan.FromHours(24);
}