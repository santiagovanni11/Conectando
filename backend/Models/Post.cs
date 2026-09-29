using Conectando.Api.Models;

namespace Conectando.Api.Models;

public class Post
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string? Content { get; set; }
    public PostPrivacy Privacy { get; set; } = PostPrivacy.Public;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AppUser Author { get; set; } = null!;
    public List<PostMedia> Media { get; set; } = [];
}