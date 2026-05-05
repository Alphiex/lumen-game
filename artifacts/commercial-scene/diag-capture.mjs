import { chromium } from 'playwright';
import { createServer } from 'http';
import { readFileSync, existsSync } from 'fs';
import { resolve, extname } from 'path';

const MIME = { '.html':'text/html','.js':'application/javascript','.mjs':'application/javascript','.css':'text/css','.png':'image/png','.jpg':'image/jpeg','.svg':'image/svg+xml' };
const DIR = '/Users/turbo/.openclaw/workspace/projects/game-design-prototype/artifacts/commercial-scene';

function startServer() {
  return new Promise(res => {
    const srv = createServer((req, rsp) => {
      const p = resolve(DIR, (req.url === '/' ? '/scene.html' : req.url).slice(1));
      if (!existsSync(p)) { rsp.writeHead(404); rsp.end(); return; }
      rsp.writeHead(200, { 'Content-Type': MIME[extname(p)] || 'application/octet-stream' });
      rsp.end(readFileSync(p));
    });
    srv.listen(0, '127.0.0.1', () => res({ srv, port: srv.address().port }));
  });
}

const { srv, port } = await startServer();
const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ viewport: { width: 1920, height: 1080 } });
const errors = [];
page.on('console', m => { if (m.type() === 'error' || m.type() === 'warning') errors.push(`[${m.type()}] ${m.text()}`); });
page.on('pageerror', e => errors.push(`[pageerror] ${e.message}`));
await page.goto(`http://127.0.0.1:${port}/scene.html`, { waitUntil: 'networkidle', timeout: 60000 });
await page.waitForTimeout(3000);
await page.evaluate(() => { if (typeof window.stepHero === 'function') window.stepHero(); if (typeof window.captureRender === 'function') window.captureRender(); });
await page.waitForTimeout(1000);
console.log('---ERRORS/WARNINGS---');
for (const e of errors) console.log(e.substring(0, 1200));
await browser.close();
srv.close();
