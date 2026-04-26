# RVM.NearBy

## Visao Geral
Rede social baseada em geolocalizacao. Usuarios publicam posts georeferenciados e recebem um feed personalizado com conteudo de pessoas proximas, calculado via queries espaciais PostGIS. Interface Blazor Server com mapa, feed de proximidade e perfis de usuario.

Projeto portfolio demonstrando uso de PostGIS para queries espaciais, calculo de distancia geografica, feed social com filtro por raio, e autenticacao via API Key simples.

## Stack
- .NET 10, ASP.NET Core, Blazor Server
- PostGIS + NetTopologySuite (queries espaciais, tipos Geography/Geometry)
- Entity Framework Core + PostgreSQL com extensao PostGIS
- `FeedService` (servico principal de feed por proximidade)
- Autenticacao via API Key (chave unica em `Auth:ApiKey`)
- Serilog + Seq, RVM.Common.Security
- xUnit 89 testes, Playwright E2E

## Estrutura do Projeto
```
src/
  RVM.NearBy.API/
    Auth/                     # ApiKeyAuthHandler (chave unica por config)
    Components/               # Blazor pages (feed, mapa, perfil, posts, descobrir)
    Controllers/              # REST: posts, usuarios, feed, geolocalizacao
    Health/                   # DatabaseHealthCheck
    Middleware/               # CorrelationIdMiddleware
    Services/
      FeedService             # Feed por proximidade (query PostGIS por raio)
  RVM.NearBy.Domain/          # Entidades (User, Post, Location, Follow)
  RVM.NearBy.Infrastructure/
    Data/                     # NearByDbContext (com configuracao PostGIS)
    Repositories/             # IUserRepository, IPostRepository
test/
  RVM.NearBy.Test/            # xUnit (89 testes)
  playwright/                 # Testes E2E
```

## Convencoes
- Coordenadas armazenadas como `Point` (NetTopologySuite) com SRID 4326 (WGS84)
- `FeedService.GetFeedAsync(userId, radiusKm)` retorna posts ordenados por distancia
- Autenticacao por API Key unica (sem multi-tenant) — simplificado para portfolio
- PathBase configuravel via `PathBase` na config (sem prefixo `App:`)
- `EnsureCreated` para dev; sem rate limiting (portfolio)

## Como Rodar
### Dev
```bash
# PostgreSQL com extensao PostGIS
docker compose -f docker-compose.dev.yml up -d

# API + Blazor
cd src/RVM.NearBy.API
dotnet run
```

### Testes
```bash
dotnet test test/RVM.NearBy.Test/
```

## Decisoes Arquiteturais
- **PostGIS em vez de calculo em C#**: distancias geograficas corretas requerem projecao esferica — PostGIS faz isso eficientemente no banco sem trazer todos os registros para memoria
- **NetTopologySuite com EF Core**: integracao nativa via `UseNetTopologySuite()` — tipos `Point`/`Polygon` mapeados diretamente para colunas Geography do PostgreSQL
- **FeedService isolado**: logica de feed e complexa (raio, follow graph, ranking) — servico dedicado facilita testes e evolucao independente
- **API Key unica simples**: portfolio nao precisa multi-tenant; simplifica demo sem comprometer a demonstracao de autenticacao
