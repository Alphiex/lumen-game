import { chromium } from 'playwright';
import { createServer } from 'http';
import { readFileSync, existsSync } from 'fs';
import { resolve, extname } from 'path';

const MIME = { '.html':'text/html','.js':'application/javascript','.mjs':'application/javascript','.css':'text/css','.png':'image/png','.jpg':'image/jpeg','.svg':'image/svg+xml' };
const DIR = resolve(import.meta.dirname || '.');

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
console.log(`Serving on :${port}`);

const browser = await chromium.launch({
  headless: true,
  args: [
    '--use-angle=swiftshader',
    '--enable-unsafe-swiftshader',
    '--use-gl=angle',
    '--enable-webgl',
    '--ignore-gpu-blocklist',
    '--disable-gpu-sandbox',
  ],
});
const page = await browser.newPage({ viewport: { width: 1920, height: 1080 } });
const errors = [];
page.on('console', m => { if (m.type() === 'error') errors.push(`[console.error] ${m.text().substring(0, 600)}`); });
page.on('pageerror', e => errors.push(`[pageerror] ${e.message.substring(0, 600)}`));

await page.goto(`http://127.0.0.1:${port}/scene.html`, { waitUntil: 'networkidle', timeout: 60000 });
await page.waitForTimeout(3500);
await page.evaluate(() => { if (typeof window.stepHero === 'function') window.stepHero(); if (typeof window.captureRender === 'function') window.captureRender(); });
await page.waitForTimeout(1500);

const outPath = resolve(DIR, 'latest-scene.png');
await page.screenshot({ path: outPath, type: 'png', fullPage: false, timeout: 90000 });
console.log(`Saved: ${outPath}`);
if (errors.length) {
  console.log('---ERRORS---');
  for (const e of errors) console.log(e);
}
await browser.close();
srv.close();
