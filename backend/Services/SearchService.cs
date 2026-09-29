using Conectando.Api.Data;
using Conectando.Api.DTOs.Social;
using Conectando.Api.Exceptions;
using Conectando.Api.Interfaces;
using Conectando.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectando.Api.Services;

public class SearchService(ConectandoDbContext dbContext)
{
    private readonly ConectandoDbContext _dbContext = dbContext;

    public async Task<List<UserSummaryDto>> SearchUsersAsync(string query, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var searchTerm = query.Trim().ToLowerInvariant();

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(u =>
                EF.Functions.Like(u.UserName.ToLowerInvariant(), $"%{searchTerm}%") ||
                EF.Functions.Like(u.DisplayName.ToLowerInvariant(), $"%{searchTerm}%"))
            .Where(u => u.Id != currentUserId)
            .Take(20)
            .ToListAsync(cancellationToken);

        return users.Select(u => new UserSummaryDto
        {
            Id = u.Id,
            UserName = u.UserName,
            DisplayName = u.DisplayName,
            ProfileImageUrl = u.ProfileImageUrl,
        }).ToList();
    }
}