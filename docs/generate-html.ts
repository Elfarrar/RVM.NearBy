/**
 * RVM.NearBy — Gerador de Manual HTML
 *
 * Le os screenshots gerados pelo Playwright e produz um manual HTML standalone
 * com descritivos de cada funcionalidade.
 *
 * Uso:
 *   npx tsx docs/generate-html.ts
 *
 * Saida:
 *   docs/manual-usuario.html
 *   docs/manual-usuario.md
 */
import fs from 'fs';
import path from 'path';

const SCREENSHOTS_DIR = path.resolve(__dirname, 'screenshots');
const OUTPUT_HTML = path.resolve(__dirname, 'manual-usuario.html');
const OUTPUT_MD = path.resolve(__dirname, 'manual-usuario.md');

interface Section {
  id: string;
  title: string;
  description: string;
  screenshot: string;
  features: string[];
  tips?: string[];
}

const sections: Section[] = [
  {
    id: 'dashboard',
    title: '1. Dashboard / Home',
    description:
      'Pagina inicial do RVM.NearBy. Apresenta um resumo da atividade proxima ao usuario: ' +
      'posts recentes, lugares populares e perfis sugeridos na regiao.',
    screenshot: '01-dashboard',
    features: [
      'Resumo de atividade local recente',
      'Posts e eventos proximos em destaque',
      'Lugares mais populares na area',
      'Sugestoes de perfis para seguir',
      'Indicadores de distancia em tempo real',
    ],
    tips: [
      'A localizacao e usada para personalizar o conteudo exibido.',
      'Atualize a pagina para ver o conteudo mais recente da sua regiao.',
    ],
  },
  {
    id: 'feed',
    title: '2. Feed de Publicacoes',
    description:
      'Feed cronologico de publicacoes dos usuarios e lugares proximos. ' +
      'Veja fotos, textos, check-ins e eventos compartilhados na sua regiao.',
    screenshot: '02-feed',
    features: [
      'Posts de usuarios proximos em ordem cronologica',
      'Fotos, textos e check-ins de lugares',
      'Curtir e comentar publicacoes',
      'Filtrar por tipo de conteudo (fotos, eventos, check-ins)',
      'Distancia do autor em relacao a sua posicao',
      'Carregar mais publicacoes ao rolar a pagina',
    ],
    tips: [
      'Use os filtros para ver apenas o tipo de conteudo que interessa.',
      'Publicacoes mais proximas aparecem primeiro no feed.',
    ],
  },
  {
    id: 'places',
    title: '3. Lugares (Places)',
    description:
      'Descubra e explore lugares cadastrados na plataforma: restaurantes, cafes, ' +
      'parques, comercios e pontos turisticos. Veja avaliacao, fotos e check-ins de cada lugar.',
    screenshot: '03-places',
    features: [
      'Listagem de lugares proximos com distancia',
      'Categorias: Restaurantes, Cafes, Comercio, Lazer, Cultura',
      'Avaliacao media e numero de visitas',
      'Fotos enviadas por usuarios',
      'Botao para fazer check-in',
      'Mapa com localizacao dos lugares',
      'Busca por nome ou categoria',
    ],
    tips: [
      'Faca check-in em lugares para compartilhar sua visita com seguidores.',
      'Avalie os lugares que voce visita para ajudar outros usuarios.',
    ],
  },
  {
    id: 'profiles',
    title: '4. Perfis de Usuarios',
    description:
      'Explore e conecte-se com usuarios proximos. Veja o perfil publico, ' +
      'publicacoes, lugares visitados e interesses em comum.',
    screenshot: '04-profiles',
    features: [
      'Listagem de usuarios ativos na sua area',
      'Foto de perfil, nome e distancia aproximada',
      'Publicacoes e check-ins publicos do usuario',
      'Seguir/deixar de seguir usuarios',
      'Interesses e categorias preferidas',
      'Busca por nome de usuario',
    ],
    tips: [
      'Siga usuarios com interesses em comum para personalizar seu feed.',
      'A distancia mostrada e aproximada e preserva a privacidade do usuario.',
    ],
  },
];

// ---------------------------------------------------------------------------
// Gerar HTML
// ---------------------------------------------------------------------------
function imageToBase64(filePath: string): string | null {
  if (!fs.existsSync(filePath)) return null;
  const buffer = fs.readFileSync(filePath);
  return `data:image/png;base64,${buffer.toString('base64')}`;
}

function generateHTML(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let sectionsHtml = '';
  for (const s of sections) {
    const desktopPath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`);
    const mobilePath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--mobile.png`);
    const desktopImg = imageToBase64(desktopPath);
    const mobileImg = imageToBase64(mobilePath);

    const featuresHtml = s.features.map((f) => `<li>${f}</li>`).join('\n            ');
    const tipsHtml = s.tips
      ? `<div class="tips">
          <strong>Dicas:</strong>
          <ul>${s.tips.map((t) => `<li>${t}</li>`).join('\n            ')}</ul>
        </div>`
      : '';

    const screenshotsHtml = desktopImg
      ? `<div class="screenshots">
          <div class="screenshot-group">
            <span class="badge">Desktop</span>
            <img src="${desktopImg}" alt="${s.title} - Desktop" />
          </div>
          ${
            mobileImg
              ? `<div class="screenshot-group mobile">
              <span class="badge">Mobile</span>
              <img src="${mobileImg}" alt="${s.title} - Mobile" />
            </div>`
              : ''
          }
        </div>`
      : '<p class="no-screenshot"><em>Screenshot nao disponivel. Execute o script Playwright para gerar.</em></p>';

    sectionsHtml += `
    <section id="${s.id}">
      <h2>${s.title}</h2>
      <p class="description">${s.description}</p>
      <div class="features">
        <strong>Funcionalidades:</strong>
        <ul>
            ${featuresHtml}
        </ul>
      </div>
      ${tipsHtml}
      ${screenshotsHtml}
    </section>`;
  }

  return `<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>RVM.NearBy - Manual do Usuario</title>
  <style>
    :root {
      --primary: #10b981;
      --surface: #ffffff;
      --bg: #f4f6fa;
      --text: #1e293b;
      --text-muted: #64748b;
      --border: #e2e8f0;
      --sidebar-bg: #064e3b;
      --accent: #10b981;
    }
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      background: var(--bg);
      color: var(--text);
      line-height: 1.6;
    }
    .container { max-width: 1100px; margin: 0 auto; padding: 2rem 1.5rem; }
    header {
      background: var(--sidebar-bg);
      color: white;
      padding: 3rem 1.5rem;
      text-align: center;
    }
    header h1 { font-size: 2rem; margin-bottom: 0.5rem; }
    header p { color: #6ee7b7; font-size: 1rem; }
    header .version { color: #34d399; font-size: 0.85rem; margin-top: 0.5rem; }
    nav {
      background: var(--surface);
      border-bottom: 1px solid var(--border);
      padding: 1rem 1.5rem;
      position: sticky;
      top: 0;
      z-index: 100;
    }
    nav .container { padding: 0; }
    nav ul { list-style: none; display: flex; flex-wrap: wrap; gap: 0.5rem; }
    nav a {
      display: inline-block;
      padding: 0.35rem 0.75rem;
      border-radius: 0.5rem;
      font-size: 0.85rem;
      color: var(--text);
      text-decoration: none;
      background: var(--bg);
      transition: background 0.2s;
    }
    nav a:hover { background: var(--primary); color: white; }
    section {
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: 1rem;
      padding: 2rem;
      margin-bottom: 2rem;
    }
    section h2 {
      font-size: 1.5rem;
      color: var(--primary);
      margin-bottom: 1rem;
      padding-bottom: 0.5rem;
      border-bottom: 2px solid var(--border);
    }
    .description { font-size: 1.05rem; margin-bottom: 1.25rem; color: var(--text); }
    .features, .tips {
      background: var(--bg);
      border-radius: 0.75rem;
      padding: 1rem 1.25rem;
      margin-bottom: 1.25rem;
    }
    .features ul, .tips ul { margin-top: 0.5rem; padding-left: 1.25rem; }
    .features li, .tips li { margin-bottom: 0.35rem; }
    .tips { background: #ecfdf5; border-left: 4px solid var(--accent); }
    .tips strong { color: var(--accent); }
    .screenshots {
      display: flex;
      gap: 1.5rem;
      margin-top: 1rem;
      align-items: flex-start;
    }
    .screenshot-group {
      position: relative;
      flex: 1;
      border: 1px solid var(--border);
      border-radius: 0.75rem;
      overflow: hidden;
    }
    .screenshot-group.mobile { flex: 0 0 200px; max-width: 200px; }
    .screenshot-group img { width: 100%; display: block; }
    .badge {
      position: absolute;
      top: 0.5rem;
      right: 0.5rem;
      background: var(--sidebar-bg);
      color: white;
      font-size: 0.7rem;
      padding: 0.2rem 0.5rem;
      border-radius: 0.35rem;
      font-weight: 600;
      text-transform: uppercase;
    }
    .no-screenshot {
      background: var(--bg);
      padding: 2rem;
      border-radius: 0.75rem;
      text-align: center;
      color: var(--text-muted);
    }
    footer {
      text-align: center;
      padding: 2rem 1rem;
      color: var(--text-muted);
      font-size: 0.85rem;
    }
    @media (max-width: 768px) {
      .screenshots { flex-direction: column; }
      .screenshot-group.mobile { max-width: 100%; flex: 1; }
      section { padding: 1.25rem; }
    }
    @media print {
      nav { display: none; }
      section { break-inside: avoid; page-break-inside: avoid; }
      .screenshots { flex-direction: column; }
      .screenshot-group.mobile { max-width: 250px; }
    }
  </style>
</head>
<body>
  <header>
    <h1>RVM.NearBy - Manual do Usuario</h1>
    <p>Rede Social por Geolocalizacao — Guia Completo de Funcionalidades</p>
    <div class="version">Gerado em ${now} | RVM Tech</div>
  </header>

  <nav>
    <div class="container">
      <ul>
        ${sections.map((s) => `<li><a href="#${s.id}">${s.title}</a></li>`).join('\n        ')}
      </ul>
    </div>
  </nav>

  <div class="container">
    <section id="visao-geral">
      <h2>Visao Geral</h2>
      <p class="description">
        O <strong>RVM.NearBy</strong> e uma rede social baseada em geolocalizacao.
        Descubra o que esta acontecendo perto de voce: posts, check-ins, eventos e lugares
        compartilhados por pessoas na sua regiao.
      </p>
      <div class="features">
        <strong>Recursos principais:</strong>
        <ul>
          <li><strong>Feed local</strong> — publicacoes e eventos proximos em tempo real</li>
          <li><strong>Mapa interativo</strong> — visualize posts e lugares no mapa</li>
          <li><strong>Places</strong> — descubra e avalie lugares na sua cidade</li>
          <li><strong>Check-in</strong> — compartilhe sua visita a lugares</li>
          <li><strong>Perfis</strong> — conecte-se com pessoas da sua regiao</li>
          <li><strong>PostGIS</strong> — busca geografica precisa com raio configuravel</li>
        </ul>
      </div>
    </section>

    ${sectionsHtml}
  </div>

  <footer>
    <p>RVM Tech &mdash; Rede Social por Geolocalizacao</p>
    <p>Documento gerado automaticamente com Playwright + TypeScript</p>
  </footer>
</body>
</html>`;
}

// ---------------------------------------------------------------------------
// Gerar Markdown
// ---------------------------------------------------------------------------
function generateMarkdown(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let md = `# RVM.NearBy - Manual do Usuario

> Rede Social por Geolocalizacao — Guia Completo de Funcionalidades
>
> Gerado em ${now} | RVM Tech

---

## Visao Geral

O **RVM.NearBy** e uma rede social baseada em geolocalizacao.
Descubra o que esta acontecendo perto de voce: posts, check-ins, eventos e lugares
compartilhados por pessoas na sua regiao.

**Recursos principais:**
- **Feed local** — publicacoes e eventos proximos em tempo real
- **Mapa interativo** — visualize posts e lugares no mapa
- **Places** — descubra e avalie lugares na sua cidade
- **Check-in** — compartilhe sua visita a lugares
- **Perfis** — conecte-se com pessoas da sua regiao
- **PostGIS** — busca geografica precisa com raio configuravel

---

`;

  for (const s of sections) {
    const desktopExists = fs.existsSync(
      path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`),
    );

    md += `## ${s.title}\n\n`;
    md += `${s.description}\n\n`;
    md += `**Funcionalidades:**\n`;
    for (const f of s.features) {
      md += `- ${f}\n`;
    }
    md += '\n';

    if (s.tips) {
      md += `> **Dicas:**\n`;
      for (const t of s.tips) {
        md += `> - ${t}\n`;
      }
      md += '\n';
    }

    if (desktopExists) {
      md += `| Desktop | Mobile |\n`;
      md += `|---------|--------|\n`;
      md += `| ![${s.title} - Desktop](screenshots/${s.screenshot}--desktop.png) | ![${s.title} - Mobile](screenshots/${s.screenshot}--mobile.png) |\n`;
    } else {
      md += `*Screenshot nao disponivel. Execute o script Playwright para gerar.*\n`;
    }
    md += '\n---\n\n';
  }

  md += `## Informacoes Tecnicas

| Item | Detalhe |
|------|---------|
| **Backend** | ASP.NET Core + Blazor Server |
| **Banco de dados** | PostgreSQL 16 + PostGIS + EF Core |
| **Geolocalizacao** | PostGIS com consultas por raio (ST_DWithin) |
| **Tempo real** | SignalR para feed ao vivo |
| **Deploy** | Docker Compose + Nginx |

---

*Documento gerado automaticamente com Playwright + TypeScript — RVM Tech*
`;

  return md;
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------
const html = generateHTML();
fs.writeFileSync(OUTPUT_HTML, html, 'utf-8');
console.log(`HTML gerado: ${OUTPUT_HTML}`);

const md = generateMarkdown();
fs.writeFileSync(OUTPUT_MD, md, 'utf-8');
console.log(`Markdown gerado: ${OUTPUT_MD}`);
