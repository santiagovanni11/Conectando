using System.Text;
using Conectando.Api.Interfaces;
using Conectando.Api.Services;
using Conectando.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Conectando.Api.Extensions;

public static class ServiceCollectionExtensions
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

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SiteSettings>()
.Bind(configuration.GetSection(SiteSettings.SectionName))
.Validate(settings => !string.IsNullOrWhiteSpace(settings.Url),
"Site:Url no está configurada. Definí la variable de entorno Site__Url.")
.ValidateOnStart();

services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Secret),
                "JwtSettings:Secret no está configurado. Definí la variable de entorno JwtSettings__Secret.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings?.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings?.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(settings?.Secret ?? string.Empty)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5),
                };

                // El navegador no puede mandar headers en un WebSocket, así
                // que SignalR tiene que leer el token del query string.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },

                    // Cierra las sesiones viejas: cuenta dada de baja, o sello
                    // que ya no es el vigente porque se cambió la contraseña.
                    OnTokenValidated = AccountTokenValidation.RejectStaleSessionsAsync,
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddDevelopmentCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DevCorsPolicy, policy =>
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        return services;
    }

    public static IServiceCollection ConfigureApiErrorFormatting(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var message = context.ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault() ?? "Datos inválidos.";

                return new BadRequestObjectResult(new { message });
            };
        });

        return services;
    }
}
