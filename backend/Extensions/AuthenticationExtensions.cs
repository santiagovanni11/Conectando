using System.Text;
using Conectando.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Conectando.Api.Extensions;

// Va aparte del registro de servicios: autenticar no es lo mismo que
// declarar que existe algo. Archivo propio porque juntos pasaban de 150 lineas.
public static partial class ServiceCollectionExtensions
{
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

