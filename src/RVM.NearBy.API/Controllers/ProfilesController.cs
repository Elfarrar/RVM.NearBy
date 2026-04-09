using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.NearBy.API.Dtos;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Interfaces;

namespace RVM.NearBy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfilesController(IUserProfileRepository profileRepo, IPostRepository postRepo) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var profile = await profileRepo.GetByIdAsync(id, ct);
        if (profile is null) return NotFound();

        var postCount = await postRepo.CountByAuthorAsync(id, ct);
        return Ok(MapToResponse(profile, postCount));
    }

    [HttpGet("by-username/{username}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByUsername(string username, CancellationToken ct)
    {
        var profile = await profileRepo.GetByUsernameAsync(username, ct);
        if (profile is null) return NotFound();

        var postCount = await postRepo.CountByAuthorAsync(profile.Id, ct);
        return Ok(MapToResponse(profile, postCount));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] int offset = 0,
        [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var profiles = await profileRepo.SearchAsync(query, offset, limit, ct);
        var responses = new List<ProfileResponse>();
        foreach (var p in profiles)
        {
            var count = await postRepo.CountByAuthorAsync(p.Id, ct);
            responses.Add(MapToResponse(p, count));
        }
        return Ok(responses);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProfileRequest request, CancellationToken ct)
    {
        var existing = await profileRepo.GetByUsernameAsync(request.Username, ct);
        if (existing is not null) return Conflict("Username already taken");

        var profile = new UserProfile
        {
            Username = request.Username,
            DisplayName = request.DisplayName,
            Bio = request.Bio,
            AvatarUrl = request.AvatarUrl
        };

        await profileRepo.AddAsync(profile, ct);
        return CreatedAtAction(nameof(GetById), new { id = profile.Id }, MapToResponse(profile, 0));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var profile = await profileRepo.GetByIdAsync(id, ct);
        if (profile is null) return NotFound();

        if (request.DisplayName is not null) profile.DisplayName = request.DisplayName;
        if (request.Bio is not null) profile.Bio = request.Bio;
        if (request.AvatarUrl is not null) profile.AvatarUrl = request.AvatarUrl;

        await profileRepo.UpdateAsync(profile, ct);
        var postCount = await postRepo.CountByAuthorAsync(id, ct);
        return Ok(MapToResponse(profile, postCount));
    }

    [HttpPut("{id:guid}/location")]
    public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateLocationRequest request, CancellationToken ct)
    {
        var profile = await profileRepo.GetByIdAsync(id, ct);
        if (profile is null) return NotFound();

        profile.LastLatitude = request.Latitude;
        profile.LastLongitude = request.Longitude;
        profile.LastLocationUpdate = DateTime.UtcNow;

        await profileRepo.UpdateAsync(profile, ct);
        return NoContent();
    }

    private static ProfileResponse MapToResponse(UserProfile p, int postCount)
        => new(p.Id, p.Username, p.DisplayName, p.Bio, p.AvatarUrl, p.IsActive, postCount, p.CreatedAt);
}
