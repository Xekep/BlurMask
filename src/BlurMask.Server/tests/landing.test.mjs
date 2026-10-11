import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

test('download page offers desktop software and keeps shared board keys out of requests',async()=>{
  const html=await readFile(new URL('../wwwroot/index.html',import.meta.url),'utf8');
  const js=await readFile(new URL('../wwwroot/download.js',import.meta.url),'utf8');
  assert.ok(!/<canvas\b|data-tool=|app\.js/.test(html));
  for(const platform of ['windows','linux','macos'])assert.ok(html.includes(`id="${platform}"`));
  assert.ok(!js.includes('/api/boards')&&!js.includes('localStorage'));
  assert.ok(js.includes("credentials:'omit'")&&js.includes("referrerPolicy:'no-referrer'"));
  assert.ok(html.includes('releases/latest'));
});
