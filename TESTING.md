# Testes — RVM.NearBy

## Testes Unitarios
- **Framework:** xUnit + Moq
- **Localizacao:** `test/RVM.NearBy.Test/`
- **Total:** 89 testes
- **Foco:** FeedService (raio, ordenacao por distancia), logica de follow graph, queries PostGIS mockadas, controladores

```bash
dotnet test test/RVM.NearBy.Test/
```

## Testes E2E (Playwright)
- **Localizacao:** `test/playwright/`
- **Cobertura:** feed de proximidade, mapa de posts, perfil de usuario, publicacao de post, busca por localizacao

```bash
cd test/playwright
npm install
npx playwright install --with-deps
npx playwright test
```

Variaveis de ambiente necessarias:
```
NEARBY_BASE_URL=http://localhost:5000
NEARBY_API_KEY=<api-key-dev>
```

## CI
- **Arquivo:** `.github/workflows/ci.yml`
- Pipeline: build → testes unitarios → Playwright
- Testes de integracao PostGIS requerem PostgreSQL com extensao PostGIS — usar TestContainers com imagem `postgis/postgis`
