// Headless Chromium capture via Playwright
// Usage: node capture.mjs --photo
import { chromium } from 'playwright';
import { createServer } from 'http';
import { readFileSync, existsSync } from 'fs';
import { resolve, extname } from 'path';

const MIME = { '.html':'text/html','.js':'application/javascript','.mjs':'application/javascript','.css':'text/css','.png':'image/png','.jpg':'image/jpeg','.svg':'image/svg+xml' };
const DIR = resolve(import.meta.dirname || '.');

// Simple static file server
function startServer() {
  return new Promise(res => {
    const srv = createServer((req, rsp) => {
      const p = resolve(DIR, (req.url === '/' ? '/scene.html' : req.url).slice(1));
      if (!existsSync(p)) { rsp.writeHead(404); rsp.end(); return; }
      rsp.writeHead(200, { 'Content-Type': MIME[extname(p)] || 'application/octet-stream' });
      rsp.end(readFileSync(p));
    });
    srv.listen(0, '127.0.0.1', () => {
      const port = srv.address().port;
      console.log(`Serving assets on http://127.0.0.1:${port}`);
      res({ srv, port });
    });
  });
}

async function main() {
  console.log('Starting local asset server...');
  const { srv, port } = await startServer();

  console.log('Launching headless Chromium...');
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

  await page.goto(`http://127.0.0.1:${port}/scene.html`, { waitUntil: 'networkidle', timeout: 60000 });
  // Let the scene render and settle
  await page.waitForTimeout(3000);

  // Position the hero camera if the function exists
  await page.evaluate(() => {
    if (typeof window.stepHero === 'function') window.stepHero();
    if (typeof window.captureRender === 'function') window.captureRender();
  });
  await page.waitForTimeout(1000);

  console.log('=== Screenshot pass ===');
  const outPath = resolve(DIR, 'latest-scene.png');
  await page.screenshot({ path: outPath, type: 'png', fullPage: false, timeout: 90000 });
  console.log(`Screenshot saved: ${outPath}`);

  await browser.close();
  srv.close();
  console.log('\nDone (photo only).');
}

main().catch(e => { console.error(e); process.exit(1); });
