using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.NearBy.API.Dtos;
using RVM.NearBy.Domain.Entities;
using RVM.NearBy.Domain.Interfaces;

namespace RVM.NearBy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlacesController(IPlaceRepository placeRepo) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var place = await placeRepo.GetByIdAsync(id, ct);
        if (place is null) return NotFound();
        return Ok(MapToResponse(place));
    }

    [HttpGet("nearby")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNearby([FromQuery] double latitude, [FromQuery] double longitude,
        [FromQuery] double radiusKm = 5, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var places = await placeRepo.GetNearbyAsync(latitude, longitude, radiusKm, limit, ct);
        return Ok(places.Select(MapToResponse));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var places = await placeRepo.SearchAsync(query, limit, ct);
        return Ok(places.Select(MapToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlaceRequest request, CancellationToken ct)
    {
        var place = new Place
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Address = request.Address
        };

        await placeRepo.AddAsync(place, ct);
        return CreatedAtAction(nameof(GetById), new { id = place.Id }, MapToResponse(place));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlaceRequest request, CancellationToken ct)
    {
        var place = await placeRepo.GetByIdAsync(id, ct);
        if (place is null) return NotFound();

        if (request.Name is not null) place.Name = request.Name;
        if (request.Description is not null) place.Description = request.Description;
        if (request.Category is not null) place.Category = request.Category;
        if (request.Address is not null) place.Address = request.Address;

        await placeRepo.UpdateAsync(place, ct);
        return Ok(MapToResponse(place));
    }

    private static PlaceResponse MapToResponse(Place p)
        => new(p.Id, p.Name, p.Description, p.Category, p.Latitude, p.Longitude, p.Address, p.PostCount, p.CreatedAt);
}
