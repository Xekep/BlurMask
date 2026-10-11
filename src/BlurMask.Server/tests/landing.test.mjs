import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { runInNewContext } from 'node:vm';

test('download page offers desktop software and keeps shared board keys out of requests',async()=>{
  const html=await readFile(new URL('../wwwroot/index.html',import.meta.url),'utf8');
  const js=await readFile(new URL('../wwwroot/download.js',import.meta.url),'utf8');
  assert.ok(!/<canvas\b|data-tool=|app\.js/.test(html));
  for(const platform of ['windows','linux','arch','macos'])assert.ok(html.includes(`id="${platform}"`));
  assert.ok(!js.includes('/api/boards')&&!js.includes('localStorage'));
  assert.ok(js.includes("credentials:'omit'")&&js.includes("referrerPolicy:'no-referrer'"));
  assert.ok(html.includes('releases/latest'));
});

async function downloadPage(language,assets) {
  const source=await readFile(new URL('../wwwroot/download.js',import.meta.url),'utf8');
  const links=Object.fromEntries(['windows','linux','arch','macos','invitation','copy-invite'].map(id=>[id,{
    href:'https://github.com/Xekep/BlurMask/releases/latest',
    dataset:{text:id},classList:{toggle(){}},parentElement:{prepend(){}}
  }]));
  const heading={dataset:{text:'download'}};
  runInNewContext(source.replace(/^import .*;\r?\n/,''),{
    navigator:{language,platform:'Linux'},location:{hash:''},
    document:{documentElement:{},getElementById:id=>links[id],querySelectorAll:selector=>selector==='[data-text]'?[heading]:Object.values(links)},
    fetch:async()=>({ok:true,json:async()=>({assets})}),
    AbortSignal:{timeout:()=>null},addEventListener(){}
  });
  await new Promise(resolve=>setImmediate(resolve));
  return {links,heading};
}

test('Arch download resolves the current x86_64 package in every page language',async()=>{
  const url='https://github.com/Xekep/BlurMask/releases/download/v1.2.3/blurmask-1.2.3-1-x86_64.pkg.tar.zst';
  for(const language of ['en','ru','fr','de']) {
    const {links,heading}=await downloadPage(language,[
      {name:'blurmask-1.2.3-1-x86_64.pkg.tar.zst.sha256',browser_download_url:`${url}.sha256`},
      {name:'blurmask-1.2.3-1-x86_64.pkg.tar.zst',browser_download_url:url}
    ]);
    assert.equal(links.arch.href,url);
    assert.ok(heading.textContent.includes('BlurMask'));
  }
});

test('Arch download keeps the release fallback if metadata has no package or an unrelated URL',async()=>{
  for(const assets of [[],[{name:'blurmask-1.2.3-1-x86_64.pkg.tar.zst',browser_download_url:'https://example.com/download'}]]) {
    const {links}=await downloadPage('en',assets);
    assert.equal(links.arch.href,'https://github.com/Xekep/BlurMask/releases/latest');
  }
});
