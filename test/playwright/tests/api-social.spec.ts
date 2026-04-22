import { expect, test } from '@playwright/test';

const defaultBaseUrl = process.env.NEARBY_BASE_URL ?? 'https://nearby.lab.rvmtech.com.br';

test.describe('NearBy API', () => {
  test.skip(
    process.env.NEARBY_RUN_SMOKE !== '1',
    'Defina NEARBY_RUN_SMOKE=1 para rodar o smoke contra um ambiente real.',
  );

  test('GET /api/feed retorna feed ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/feed`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/places retorna lista de lugares ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/places`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/posts retorna posts ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/posts`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/profiles retorna perfis ou exige autenticacao', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/profiles`);
    expect([200, 401]).toContain(response.status());
  });

  test('POST /api/posts sem corpo retorna 400 ou 401', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.post(`${currentBaseUrl}/api/posts`);
    expect([400, 401]).toContain(response.status());
  });
});
