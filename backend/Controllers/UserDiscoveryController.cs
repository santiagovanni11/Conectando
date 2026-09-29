using Conectando.Api.DTOs.Users;
using Conectando.Api.Extensions;
using Conectando.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conectando.Api.Controllers;

/// <summary>
/// Búsqueda de personas, sugerencias y listas de conexiones.
/// Se separó de UsersController para que ninguno crezca sin límite.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UserDiscoveryController(
    IUserDiscoveryService discovery,
    IUserConnectionsService connections) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<List<UserCardDto>>> Search(
        [FromQuery] string query,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var results = await discovery.SearchAsync(User.GetUserId(), query, limit, cancellationToken);
        return Ok(results);
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<List<UserCardDto>>> Suggestions(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var results = await discovery.GetSuggestionsAsync(User.GetUserId(), limit, cancellationToken);
        return Ok(results);
    }

    [HttpGet("{id:guid}/friends")]
    public async Task<ActionResult<List<UserCardDto>>> Friends(Guid id, CancellationToken cancellationToken)
    {
        var result = await connections.GetFriendsAsync(id, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/followers")]
    public async Task<ActionResult<List<UserCardDto>>> Followers(Guid id, CancellationToken cancellationToken)
    {
        var result = await connections.GetFollowersAsync(id, User.GetUserId(), cancellationToken);
        return Ok(result);
    }
}