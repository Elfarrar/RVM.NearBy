using Microsoft.AspNetCore.Mvc;
using RVM.NearBy.API.Dtos;
using RVM.NearBy.API.Services;
using RVM.NearBy.Domain.Interfaces;

namespace RVM.NearBy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedController(IPostRepository postRepo, FeedService feedService) : ControllerBase
{
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby([FromQuery] double latitude, [FromQuery] double longitude,
        [FromQuery] double radiusKm = 5, [FromQuery] int offset = 0,
        [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var posts = await postRepo.GetNearbyAsync(latitude, longitude, radiusKm, offset, limit, ct);
        return Ok(posts.Select(feedService.MapToResponse));
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int offset = 0,
        [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var posts = await postRepo.GetRecentAsync(offset, limit, ct);
        return Ok(posts.Select(feedService.MapToResponse));
    }
}
