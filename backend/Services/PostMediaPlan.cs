using Conectando.Api.Models;

namespace Conectando.Api.Services;

public static class PostMediaPlan
{
    public static Dictionary<Guid, int> BuildOrderIndex(List<Guid> mediaIds)
    {
        var order = new Dictionary<Guid, int>(mediaIds.Count);
        for (var i = 0; i < mediaIds.Count; i++)
        {
            order[mediaIds[i]] = i;
        }

        return order;
    }

    public static (List<Guid> ToAdd, List<PostMedia> ToRemove) Plan(Post post, List<Guid> desiredIds)
    {
        var desired = desiredIds.ToHashSet();
        var current = post.Media.Select(m => m.Id).ToHashSet();
        var toAdd = desiredIds.Where(id => !current.Contains(id)).ToList();
        var toRemove = post.Media.Where(m => !desired.Contains(m.Id)).ToList();
        return (toAdd, toRemove);
    }

    public static void AttachPending(Post post, IEnumerable<PostMedia> pending, Dictionary<Guid, int> order)
    {
        foreach (var item in pending)
        {
            item.Post = post;
            item.State = PostMediaState.Attached;
            item.DisplayOrder = order[item.Id];
        }
    }
}