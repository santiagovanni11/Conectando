using Conectando.Api.Hubs;
using Conectando.Api.Data;
using Conectando.Api.Extensions;
using Conectando.Api.RateLimiting;
using Conectando.Api.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.ConfigureApiErrorFormatting();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddRateLimiting();
builder.Services.AddJwtAuthentication(builder.Configuration);
// La CORS solo hace falta en desarrollo, donde el frontend corre en otro
// puerto. En producción la SPA se sirve desde acá mismo, o sea que todo es
// del mismo origen y no hay nada que permitir.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDevelopmentCors();
}

builder.Services.AddDbContext<ConectandoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Render termina el HTTPS y le habla a este proceso por HTTP plano. Sin esto,
// ASP.NET cree que la peticion llego sin cifrar y responde con una redireccion
// a https; Render la vuelve a reenviar como http y el navegador queda en un
// bucle de redirecciones. Va primero: antes de que ningun middleware mire el
// esquema de la peticion.
var forwarded = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
// Por defecto el middleware solo confia en el proxy del loopback, y Render entra
// desde una red privada. Vaciar la lista le hace confiar en el proxy de verdad:
// el contenedor no esta expuesto a internet, solo Render llega a el.
forwarded.KnownNetworks.Clear();
forwarded.KnownProxies.Clear();
app.UseForwardedHeaders(forwarded);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseCors(ServiceCollectionExtensions.DevCorsPolicy);
}

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
