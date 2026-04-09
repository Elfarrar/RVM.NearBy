***English** | [Portugues](README.md)*

# RVM.NearBy

Location-based social network with proximity feed, geolocated posts, likes, comments, and places.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-45%20passed-brightgreen)
![license](https://img.shields.io/badge/license-MIT-blue)
![dotnet](https://img.shields.io/badge/.NET-10.0-purple)

---

## About

RVM.NearBy is a location-based social network where users create geolocated posts visible by proximity. The main feed displays nearby posts using the Haversine formula for distance calculation. It supports three visibility levels (Public, NearbyOnly, Private), attached media (Image, Video, Audio), likes, comments, and Places (points of interest) with proximity search.

---

## Technologies

| Layer          | Technology                          |
|----------------|-------------------------------------|
| Runtime        | .NET 10 / ASP.NET Core 10          |
| ORM            | Entity Framework Core 10            |
| Database       | PostgreSQL + Npgsql 10.0.1          |
| Logging        | Serilog + RenderedCompactJson       |
| Testing        | xUnit 2.9 + Moq 4.20 + EF InMemory |
| Containers     | Docker Compose (dev + prod)         |

---

## Architecture

```
┌──────────────────────────────────────────────────┐
│                    Clients                       │
│              (Mobile / Web / cURL)               │
└────────────────────┬─────────────────────────────┘
                     │ HTTP
┌────────────────────▼─────────────────────────────┐
│              RVM.NearBy.API                      │
│  Controllers ─► FeedService ─► DTOs / Responses  │
│  Auth (ApiKey) │ Middleware │ HealthCheck         │
└────────────────────┬─────────────────────────────┘
                     │
┌────────────────────▼─────────────────────────────┐
│            RVM.NearBy.Domain                     │
│  Entities: Post, Comment, Like, Place,           │
│            UserProfile, PostMedia                │
│  Enums: PostVisibility, MediaType                │
│  Interfaces: IPostRepository, ICommentRepository │
│              ILikeRepository, IPlaceRepository   │
│              IUserProfileRepository              │
└────────────────────┬─────────────────────────────┘
                     │
┌────────────────────▼─────────────────────────────┐
│         RVM.NearBy.Infrastructure                │
│  NearByDbContext (EF Core)                       │
│  Repositories (Haversine + Bounding Box)         │
│  Configurations (Fluent API)                     │
│  DependencyInjection                             │
└────────────────────┬─────────────────────────────┘
                     │
                ┌────▼────┐
                │PostgreSQL│
                └─────────┘
```

---

## Project Structure

```
RVM.NearBy/
├── src/
│   ├── RVM.NearBy.API/
│   │   ├── Auth/
│   │   │   ├── ApiKeyAuthHandler.cs
│   │   │   └── ApiKeyAuthOptions.cs
│   │   ├── Controllers/
│   │   │   ├── FeedController.cs
│   │   │   ├── PlacesController.cs
│   │   │   ├── PostsController.cs
│   │   │   └── ProfilesController.cs
│   │   ├── Dtos/
│   │   │   ├── CommentDtos.cs
│   │   │   ├── PlaceDtos.cs
│   │   │   ├── PostDtos.cs
│   │   │   └── ProfileDtos.cs
│   │   ├── Health/
│   │   │   └── DatabaseHealthCheck.cs
│   │   ├── Middleware/
│   │   │   └── CorrelationIdMiddleware.cs
│   │   ├── Services/
│   │   │   └── FeedService.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── RVM.NearBy.Domain/
│   │   ├── Entities/
│   │   │   ├── Comment.cs
│   │   │   ├── Like.cs
│   │   │   ├── Place.cs
│   │   │   ├── Post.cs
│   │   │   ├── PostMedia.cs
│   │   │   └── UserProfile.cs
│   │   ├── Enums/
│   │   │   ├── MediaType.cs
│   │   │   └── PostVisibility.cs
│   │   └── Interfaces/
│   │       ├── ICommentRepository.cs
│   │       ├── ILikeRepository.cs
│   │       ├── IPlaceRepository.cs
│   │       ├── IPostRepository.cs
│   │       └── IUserProfileRepository.cs
│   └── RVM.NearBy.Infrastructure/
│       ├── Data/
│       │   ├── Configurations/
│       │   │   ├── CommentConfiguration.cs
│       │   │   ├── LikeConfiguration.cs
│       │   │   ├── PlaceConfiguration.cs
│       │   │   ├── PostConfiguration.cs
│       │   │   ├── PostMediaConfiguration.cs
│       │   │   └── UserProfileConfiguration.cs
│       │   └── NearByDbContext.cs
│       ├── Repositories/
│       │   ├── CommentRepository.cs
│       │   ├── LikeRepository.cs
│       │   ├── PlaceRepository.cs
│       │   ├── PostRepository.cs
│       │   └── UserProfileRepository.cs
│       └── DependencyInjection.cs
├── test/
│   └── RVM.NearBy.Test/
│       ├── Domain/
│       │   └── EntityTests.cs
│       ├── Infrastructure/
│       │   ├── CommentAndLikeRepositoryTests.cs
│       │   ├── PlaceRepositoryTests.cs
│       │   ├── PostRepositoryTests.cs
│       │   └── UserProfileRepositoryTests.cs
│       └── Services/
│           └── FeedServiceTests.cs
├── docker-compose.dev.yml
├── docker-compose.prod.yml
├── global.json
└── RVM.NearBy.slnx
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/) (or Docker)

### Database (Docker)

```bash
docker compose -f docker-compose.dev.yml up -d
```

### Run the API

```bash
cd src/RVM.NearBy.API
dotnet run
```

The API will be available at `http://localhost:5013/nearby` (via Docker) or `http://localhost:5000` (local).

### Run Tests

```bash
dotnet test
```

---

## API Endpoints

### Feed

| Method | Route                 | Description                        | Auth |
|--------|-----------------------|------------------------------------|------|
| GET    | `/api/feed/nearby`    | Proximity feed (Haversine)         | No   |
| GET    | `/api/feed/recent`    | Recent feed (public posts)         | No   |

**Nearby feed parameters:** `latitude`, `longitude`, `radiusKm` (default 5), `offset`, `limit` (default 20)

### Posts

| Method | Route                                 | Description              | Auth |
|--------|---------------------------------------|--------------------------|------|
| GET    | `/api/posts/{id}`                     | Get post by ID           | No   |
| GET    | `/api/posts/by-author/{authorId}`     | Get posts by author      | No   |
| POST   | `/api/posts`                          | Create geolocated post   | Yes  |
| DELETE | `/api/posts/{id}`                     | Delete post              | Yes  |

### Comments

| Method | Route                                         | Description          | Auth |
|--------|-----------------------------------------------|----------------------|------|
| GET    | `/api/posts/{postId}/comments`                | List comments        | No   |
| POST   | `/api/posts/{postId}/comments`                | Add comment          | Yes  |
| DELETE | `/api/posts/{postId}/comments/{commentId}`    | Delete comment       | Yes  |

### Likes

| Method | Route                         | Description    | Auth |
|--------|-------------------------------|----------------|------|
| POST   | `/api/posts/{postId}/like`    | Like post      | Yes  |
| DELETE | `/api/posts/{postId}/like`    | Unlike post    | Yes  |

### Places

| Method | Route                   | Description              | Auth |
|--------|-------------------------|--------------------------|------|
| GET    | `/api/places/{id}`      | Get place by ID          | No   |
| GET    | `/api/places/nearby`    | Nearby places            | No   |
| GET    | `/api/places/search`    | Search places by name    | No   |
| POST   | `/api/places`           | Create place             | Yes  |
| PUT    | `/api/places/{id}`      | Update place             | Yes  |

### Profiles

| Method | Route                                | Description              | Auth |
|--------|--------------------------------------|--------------------------|------|
| GET    | `/api/profiles/{id}`                 | Get profile by ID        | No   |
| GET    | `/api/profiles/by-username/{user}`   | Get profile by username  | No   |
| GET    | `/api/profiles`                      | Search profiles          | No   |
| POST   | `/api/profiles`                      | Create profile           | Yes  |
| PUT    | `/api/profiles/{id}`                 | Update profile           | Yes  |
| PUT    | `/api/profiles/{id}/location`        | Update location          | Yes  |

**Authentication:** Header `Authorization: ApiKey <key>` (default: `dev-key`)

---

## Tests

45 automated tests covering all layers:

| Suite                          | Tests | Description                                   |
|--------------------------------|-------|-----------------------------------------------|
| EntityTests                    | 10    | Defaults and properties for all entities       |
| PostRepositoryTests            | 9     | CRUD, nearby (Haversine), visibility filtering |
| CommentAndLikeRepositoryTests  | 8     | Comment and like CRUD, pagination              |
| UserProfileRepositoryTests     | 8     | Profile CRUD, search, pagination               |
| PlaceRepositoryTests           | 5     | Place CRUD, proximity search                   |
| FeedServiceTests               | 5     | Post creation, media, visibility, mapper       |

```bash
dotnet test --verbosity normal
```

---

## Features

- **Proximity feed** -- Haversine algorithm with bounding box pre-filter for performance
- **3 visibility levels** -- Public (global feed), NearbyOnly (nearby feed only), Private (author only)
- **Posts with media** -- Image, Video, and Audio support with sort order and captions
- **Likes with denormalized count** -- LikeCount on Post for fast reads
- **Synchronized comments** -- Denormalized CommentCount with real count after each operation
- **Places** -- points of interest with proximity and name/category search
- **Profiles with location** -- latitude/longitude from last check-in
- **ApiKey authentication** -- custom ASP.NET Core auth handler
- **Health check** -- `/health` endpoint with database verification
- **Correlation ID** -- middleware for request tracing
- **Structured logging** -- Serilog with compact JSON format

---

<p align="center">
  <strong>RVM.NearBy</strong> &mdash; Part of the <a href="https://github.com/rvenegas5">RVM Tech</a> ecosystem
</p>
