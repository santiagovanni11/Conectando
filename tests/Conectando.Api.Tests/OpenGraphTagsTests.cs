using Conectando.Api.DTOs.Previews;
using Conectando.Api.Settings;
using Conectando.Api.Services;

namespace Conectando.Api.Tests;

public class OpenGraphTagsTests
{
    private static readonly SiteSettings Site = new()
    {
        Url = "https://conectando.app",
        Name = "Conectando",
        Description = "La red social donde las personas se encuentran.",
    };

    private static PagePreviewDto Preview() => new()
    {
        Title = "Ana en Conectando",
        Description = "Un post muy interesante",
        CanonicalUrl = "/posts/123",
        Type = "article",
        ImageUrl = "https://cdn.conectando.app/1.jpg",
    };

    [Fact]
    public void Build_IncludesTheEssentials()
    {
        var html = OpenGraphTags.Build(Preview(), Site);

        Assert.Contains("<meta property=\"og:title\" content=\"Ana en Conectando\" />", html);
        Assert.Contains("<meta property=\"og:description\" content=\"Un post muy interesante\" />", html);
        Assert.Contains("<meta property=\"og:image\"", html);
        Assert.Contains("<title>Ana en Conectando</title>", html);
    }

    [Fact]
    public void Build_TurnsRelativeUrlsIntoAbsolute()
    {
        // Un "/posts/123" solo no sirve: el crawler necesita una URL completa.
        var html = OpenGraphTags.Build(Preview(), Site);

        Assert.Contains($"content=\"{Site.Url}/posts/123\"", html);
    }

    [Fact]
    public void Build_KeepsAbsoluteUrlsUntouched()
    {
        Assert.Equal(
            "https://cdn.ejemplo.com/a.jpg",
            OpenGraphTags.AbsoluteUrl("https://cdn.ejemplo.com/a.jpg", Site.Url));
    }

    [Fact]
    public void Build_EscapesQuotesSoTheHtmlStaysValid()
    {
        var preview = new PagePreviewDto
        {
            Title = "Ana dijo \"hola\"",
            Description = "comillas \"dentro\"",
            CanonicalUrl = "/posts/1",
            Type = "article",
        };

        var html = OpenGraphTags.Build(preview, Site);

        // Las comillas van codificadas, si no romperían el atributo.
        Assert.Contains("&quot;hola&quot;", html);
        Assert.DoesNotContain("content=\"Ana dijo \"hola\"\"", html);
    }

    [Fact]
    public void Build_WithoutImage_UsesSummaryCard()
    {
        var preview = new PagePreviewDto
        {
            Title = "Sin imagen",
            Description = "Descripción",
            CanonicalUrl = "/posts/1",
            Type = "article",
            ImageUrl = null,
        };

        var html = OpenGraphTags.Build(preview, Site);

        Assert.Contains("name:twitter:card\" content=\"summary\"", html);
        Assert.DoesNotContain("og:image", html);
    }

    [Fact]
    public void Defaults_AreTheGenericSiteValues()
    {
        var html = OpenGraphTags.Build(OpenGraphTags.Defaults(Site), Site);

        Assert.Contains($"content=\"{Site.Name}\"", html);
    }
}