using Conectando.Api.DTOs.Posts;
using Conectando.Api.DTOs.Social;
using Conectando.Api.Models;

namespace Conectando.Api.Services;

public static class PostDtoMapping
{
    public static PostDto ToDto(Post post, AppUser author, List<PostMedia> media, int likesCount = 0, bool likedByMe = false, int commentsCount = 0, bool savedByMe = false)
    {
        return new PostDto
        {
            Id = post.Id,
            Author = new UserSummaryDto
            {
                Id = author.Id,
                UserName = author.UserName,
                DisplayName = author.DisplayName,
                ProfileImageUrl = author.ProfileImageUrl,
            },
            Content = post.Content,
            Privacy = post.Privacy,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            LikesCount = likesCount,
            LikedByMe = likedByMe,
            CommentsCount = commentsCount,
            SavedByMe = savedByMe,
            Media = media
                .Where(m => m.State == PostMediaState.Attached)
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new PostMediaDto
                {
                    Id = m.Id,
                    Url = m.Url,
                    DisplayOrder = m.DisplayOrder,
                })
                .ToList(),
        };
    }
}