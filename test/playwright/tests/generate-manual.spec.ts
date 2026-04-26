/**
 * RVM.NearBy — Gerador de Manual Visual
 *
 * Playwright script que navega por todas as telas do sistema,
 * captura screenshots em diferentes estados e viewports, e gera as imagens
 * para o manual do usuario.
 *
 * Uso:
 *   cd test/playwright
 *   npx playwright test tests/generate-manual.spec.ts --reporter=list
 */
import { test, type Page } from '@playwright/test';
import path from 'path';
import fs from 'fs';

const BASE_URL = process.env.NEARBY_BASE_URL ?? 'https://nearby.lab.rvmtech.com.br';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../../../docs/screenshots');

// Garantir que o diretorio de screenshots existe
if (!fs.existsSync(SCREENSHOTS_DIR)) {
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });
}

/** Captura desktop (1280x800) + mobile (390x844) */
async function capture(page: Page, name: string, opts?: { fullPage?: boolean }) {
  const fullPage = opts?.fullPage ?? true;
  await page.screenshot({
    path: path.join(SCREENSHOTS_DIR, `${name}--desktop.png`),
    fullPage,
  });
  await page.setViewportSize({ width: 390, height: 844 });
  await page.screenshot({
    path: path.join(SCREENSHOTS_DIR, `${name}--mobile.png`),
    fullPage,
  });
  await page.setViewportSize({ width: 1280, height: 800 });
}

// ---------------------------------------------------------------------------
// Telas principais
// ---------------------------------------------------------------------------
test.describe('RVM.NearBy — Telas Principais', () => {
  test('1. Dashboard / Home', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    await page.waitForLoadState('networkidle');
    await capture(page, '01-dashboard');
  });

  test('2. Feed de Publicacoes', async ({ page }) => {
    await page.goto(`${BASE_URL}/feed`);
    await page.waitForLoadState('networkidle');
    await capture(page, '02-feed');
  });

  test('3. Lugares (Places)', async ({ page }) => {
    await page.goto(`${BASE_URL}/places`);
    await page.waitForLoadState('networkidle');
    await capture(page, '03-places');
  });

  test('4. Perfis de Usuarios', async ({ page }) => {
    await page.goto(`${BASE_URL}/profiles`);
    await page.waitForLoadState('networkidle');
    await capture(page, '04-profiles');
  });
});
