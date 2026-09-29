using System.Text;
using Conectando.Api.Hubs.Broadcasting;
using Conectando.Api.Interfaces;
using Conectando.Api.Services;
using Conectando.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Conectando.Api.Extensions;

public static partial class ServiceCollectionExtensions
{
    public const string DevCorsPolicy = "dev";

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Con default en la clase: si la sección no está, el tope sigue siendo
        // el de 200 MB y la app no necesita configuración para arrancar.
        services.AddOptions<StorageQuotaSettings>()
                .Bind(configuration.GetSection(StorageQuotaSettings.SectionName))
                .Validate(settings => settings.MaxBytesPerUser > 0,
                    "StorageQuota:MaxBytesPerUser tiene que ser mayor a cero.")
                .ValidateOnStart();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IProfileCountService, ProfileCountService>();
services.AddScoped<IUserDiscoveryService, UserDiscoveryService>();
services.AddScoped<IUserConnectionsService, UserConnectionsService>();
        services.AddScoped<IConversationService, ConversationService>();
        // El hub la necesita para constructor. Sin registrarla, SignalR no
        // puede activar el hub y cierra la conexion sin explicar por que: la
        // app entera anda y solo el tiempo real queda muerto.
        services.AddScoped<ConversationBroadcaster>();
        // El que manda los contadores al hub. Scoped como el resto: depende
        // del DbContext, que es por request.
        services.AddScoped<INavCountsBroadcaster, NavCountsBroadcaster>();
services.AddScoped<INavCountService, NavCountService>();
        services.AddScoped<IFriendRequestService, FriendRequestService>();
        services.AddScoped<IFriendshipService, FriendshipService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddScoped<IBlockService, BlockService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISocialStatusService, SocialStatusService>();
services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPostReadService, PostReadService>();
        services.AddScoped<IFeedService, FeedReadService>();
        services.AddScoped<FeedAuthorResolver>();
        services.AddScoped<IPostWriteService, PostWriteService>();
        services.AddScoped<IPostPatchService, PostPatchService>();
        services.AddScoped<PostMediaUpdater>();
        services.AddScoped<IPostMediaService, PostMediaService>();
        services.AddScoped<ILikeService, LikeService>();
        services.AddScoped<IPostSaveService, PostSaveService>();
services.AddScoped<IPagePreviewService, PagePreviewService>();
        services.AddScoped<ICommentService, CommentWriteService>();
        services.AddScoped<PostVisibilityService>();
        services.AddScoped<SearchService>();
        services.AddScoped<IStorageQuotaService, StorageQuotaService>();
        services.AddScoped<IMediaCleaner, MediaCleaner>();
        services.AddSingleton<IMediaStorage, CloudinaryMediaStorage>();
        return services;
    }
}

