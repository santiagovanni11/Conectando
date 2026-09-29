namespace Conectando.Api.Services;

public static class SocialGuidPair
{
    public static (Guid Low, Guid High) Normalize(Guid a, Guid b) =>
        a.CompareTo(b) <= 0 ? (a, b) : (b, a);
}