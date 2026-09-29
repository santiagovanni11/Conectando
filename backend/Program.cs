using Conectando.Api.Hubs;
using Conectando.Api.Data;
using Conectando.Api.Extensions;
using Conectando.Api.RateLimiting;
using Conectando.Api.Middleware;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.ConfigureApiErrorFormatting();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddRateLimiting();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddDevelopmentCors();

builder.Services.AddDbContext<ConectandoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(ServiceCollectionExtensions.DevCorsPolicy);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// El limitador va después de autenticar: separa por usuario, no por anonimo.
app.UseRateLimiter();

// La SPA se sirve desde acá. Va después de autenticación porque el
// preview de un post depende de quién mira: un crawler (sin sesión) solo
// ve contenido público, igual que en un navegador sin login.
app.UseMiddleware<SpaMetadataMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        // Los assets llevan hash en el nombre, así que se cachean siempre.
        // El index lo maneja el middleware de metadatos, con no-store.
        var path = context.File.Name;
        if (path.Contains('.'))
        {
            context.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
        }
    },
});

app.MapControllers();
app.MapHub<MessageHub>("/hubs/messages");

app.Run();