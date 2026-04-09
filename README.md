*[English](README.en.md) | **Portugues***

# RVM.NearBy

Rede social baseada em localizacao com feed por proximidade, posts geolocalizados, likes, comentarios e places.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-45%20passed-brightgreen)
![license](https://img.shields.io/badge/license-MIT-blue)
![dotnet](https://img.shields.io/badge/.NET-10.0-purple)

---

## Sobre

RVM.NearBy e uma rede social baseada em localizacao onde usuarios criam posts geolocalizados visiveis por proximidade. O feed principal exibe posts proximos usando a formula de Haversine para calculo de distancia. Suporta tres niveis de visibilidade (Public, NearbyOnly, Private), midia anexada (Image, Video, Audio), likes, comentarios, e Places (pontos de interesse) com busca por proximidade.

---

## Tecnologias

| Camada         | Tecnologia                          |
|----------------|-------------------------------------|
| Runtime        | .NET 10 / ASP.NET Core 10          |
| ORM            | Entity Framework Core 10            |
| Banco de dados | PostgreSQL + Npgsql 10.0.1          |
| Logging        | Serilog + RenderedCompactJson       |
| Testes         | xUnit 2.9 + Moq 4.20 + EF InMemory |
| Containers     | Docker Compose (dev + prod)         |

---

## Arquitetura

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

## Estrutura do Projeto

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

## Como Executar

### Pre-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/) (ou Docker)

### Banco de Dados (Docker)

```bash
docker compose -f docker-compose.dev.yml up -d
```

### Executar a API

```bash
cd src/RVM.NearBy.API
dotnet run
```

A API estara disponivel em `http://localhost:5013/nearby` (via Docker) ou `http://localhost:5000` (local).

### Executar Testes

```bash
dotnet test
```

---

## Endpoints da API

### Feed

| Metodo | Rota                  | Descricao                          | Auth |
|--------|-----------------------|------------------------------------|------|
| GET    | `/api/feed/nearby`    | Feed por proximidade (Haversine)   | Nao  |
| GET    | `/api/feed/recent`    | Feed recente (posts publicos)      | Nao  |

**Parametros do feed nearby:** `latitude`, `longitude`, `radiusKm` (default 5), `offset`, `limit` (default 20)

### Posts

| Metodo | Rota                                  | Descricao              | Auth |
|--------|---------------------------------------|------------------------|------|
| GET    | `/api/posts/{id}`                     | Buscar post por ID     | Nao  |
| GET    | `/api/posts/by-author/{authorId}`     | Posts por autor         | Nao  |
| POST   | `/api/posts`                          | Criar post geolocalizado | Sim |
| DELETE | `/api/posts/{id}`                     | Deletar post           | Sim  |

### Comentarios

| Metodo | Rota                                          | Descricao             | Auth |
|--------|-----------------------------------------------|-----------------------|------|
| GET    | `/api/posts/{postId}/comments`                | Listar comentarios    | Nao  |
| POST   | `/api/posts/{postId}/comments`                | Comentar post         | Sim  |
| DELETE | `/api/posts/{postId}/comments/{commentId}`    | Deletar comentario    | Sim  |

### Likes

| Metodo | Rota                          | Descricao       | Auth |
|--------|-------------------------------|-----------------|------|
| POST   | `/api/posts/{postId}/like`    | Curtir post     | Sim  |
| DELETE | `/api/posts/{postId}/like`    | Descurtir post  | Sim  |

### Places

| Metodo | Rota                    | Descricao              | Auth |
|--------|-------------------------|------------------------|------|
| GET    | `/api/places/{id}`      | Buscar place por ID    | Nao  |
| GET    | `/api/places/nearby`    | Places proximos        | Nao  |
| GET    | `/api/places/search`    | Buscar places por nome | Nao  |
| POST   | `/api/places`           | Criar place            | Sim  |
| PUT    | `/api/places/{id}`      | Atualizar place        | Sim  |

### Profiles

| Metodo | Rota                                 | Descricao                  | Auth |
|--------|--------------------------------------|----------------------------|------|
| GET    | `/api/profiles/{id}`                 | Buscar perfil por ID       | Nao  |
| GET    | `/api/profiles/by-username/{user}`   | Buscar perfil por username | Nao  |
| GET    | `/api/profiles`                      | Buscar perfis              | Nao  |
| POST   | `/api/profiles`                      | Criar perfil               | Sim  |
| PUT    | `/api/profiles/{id}`                 | Atualizar perfil           | Sim  |
| PUT    | `/api/profiles/{id}/location`        | Atualizar localizacao      | Sim  |

**Autenticacao:** Header `Authorization: ApiKey <chave>` (default: `dev-key`)

---

## Testes

45 testes automatizados cobrindo todas as camadas:

| Suite                          | Testes | Descricao                                    |
|--------------------------------|--------|----------------------------------------------|
| EntityTests                    | 10     | Defaults e propriedades de todas as entidades |
| PostRepositoryTests            | 9      | CRUD, nearby (Haversine), visibilidade        |
| CommentAndLikeRepositoryTests  | 8      | CRUD de comentarios e likes, paginacao        |
| UserProfileRepositoryTests     | 8      | CRUD de perfis, busca, paginacao              |
| PlaceRepositoryTests           | 5      | CRUD de places, busca por proximidade         |
| FeedServiceTests               | 5      | Criacao de posts, midia, visibilidade, mapper |

```bash
dotnet test --verbosity normal
```

---

## Funcionalidades

- **Feed por proximidade** -- algoritmo Haversine com bounding box como pre-filtro para performance
- **3 niveis de visibilidade** -- Public (feed global), NearbyOnly (so no feed nearby), Private (so o autor)
- **Posts com midia** -- suporte a Image, Video e Audio com ordenacao e legendas
- **Likes com contagem desnormalizada** -- LikeCount no Post para leitura rapida
- **Comentarios sincronizados** -- CommentCount desnormalizado com contagem real pos-operacao
- **Places** -- pontos de interesse com busca por proximidade e por nome/categoria
- **Perfis com localizacao** -- latitude/longitude do ultimo check-in
- **Autenticacao ApiKey** -- handler customizado ASP.NET Core
- **Health check** -- endpoint `/health` com verificacao do banco
- **Correlation ID** -- middleware para rastreamento de requisicoes
- **Structured logging** -- Serilog com formato JSON compacto

---

<p align="center">
  <strong>RVM.NearBy</strong> &mdash; Parte do ecossistema <a href="https://github.com/rvenegas5">RVM Tech</a>
</p>
