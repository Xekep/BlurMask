import test from 'node:test';
import assert from 'node:assert/strict';
import net from 'node:net';

const base = new URL(process.env.PEN_TEST_URL || 'http://127.0.0.1:5079');
const local = ['127.0.0.1', 'localhost', '[::1]'].includes(base.hostname);
const request = (path, key, body, headers={}) => fetch(new URL(path,base), {
  method:body === undefined ? 'GET' : 'POST',
  headers:{...(key ? {Authorization:`Bearer ${key}`} : {}), ...(body === undefined ? {} : {'Content-Type':'application/json'}),...headers},
  body:body === undefined ? undefined : JSON.stringify(body)
});
const patch = extra => ({operationId:crypto.randomUUID(),add:[],remove:[],...extra});
const mark = text => ({id:crypto.randomUUID(),tool:'Text',color:'#111111',width:4,fontSize:28,text,points:[{x:1,y:1}]});
function pendingRequest(path,key,length) {
  const socket=net.connect(Number(base.port)||80,base.hostname);
  const status=new Promise((resolve,reject)=>{
    let response='';const timer=setTimeout(()=>reject(new Error('HTTP handshake timed out')),3000);
    socket.on('error',error=>{clearTimeout(timer);reject(error);});
    socket.on('data',data=>{response+=data.toString();const match=response.match(/^HTTP\/1\.[01] (\d+)/);if(match){clearTimeout(timer);resolve(Number(match[1]));}});
    socket.once('connect',()=>socket.write(`POST ${path} HTTP/1.1\r\nHost: ${base.host}\r\nAuthorization: Bearer ${key}\r\nContent-Type: application/json\r\nContent-Length: ${length}\r\nExpect: 100-continue\r\n\r\n`));
  });
  return {socket,status};
}

test('security: authorization, validation, no-op writes and bounded resource use', async t => {
  assert.ok(local, 'Run security load/boundary tests against an isolated loopback server only.');
  const title = "Security audit '; SELECT 1; -- <svg onload=alert(1)>";
  const created = await request('/api/boards',null,{title,background:'white'});
  assert.equal(created.status,201);
  const access=await created.json(), path=`/api/boards/${access.board.id}`;
  await t.test('unknown/wrong/malformed keys cannot read or write; credentials checked before JSON binding',async()=>{
    for(const key of [null,'short','?'.repeat(43),'x'.repeat(43)]) {
      assert.equal((await request(path,key)).status,404);
      assert.equal((await request(path+'/changes',key,patch({title:'unauthorized'}))).status,404);
    }
    const invalidBody=await fetch(new URL(path+'/changes',base),{method:'POST',headers:{'Content-Type':'application/json'},body:'{invalid'});
    assert.equal(invalidBody.status,404);
    assert.equal((await request(`/api/boards/${crypto.randomUUID()}`,access.editKey)).status,404);
    assert.equal((await request(path+'/changes',access.viewKey,patch({title:'viewer write'}))).status,403);
    const saved=await (await request(path,access.editKey)).json();
    assert.equal(saved.title,title); assert.equal(saved.revision,0);
  });
  await t.test('no-op requests neither advance the revision nor consume retry ids',async()=>{
    const noop=patch({remove:[crypto.randomUUID()],title,background:'white'});
    assert.equal((await (await request(path+'/changes',access.editKey,noop)).json()).revision,0);
    const actual={...noop,add:[mark('saved')]};
    assert.equal((await (await request(path+'/changes',access.editKey,actual)).json()).revision,1);
    assert.equal((await (await request(path+'/changes',access.editKey,actual)).json()).revision,1);
    assert.equal((await (await request(path+'/changes',access.editKey,patch({add:actual.add}))).json()).revision,1);
  });
  await t.test('null members, duplicate ids and cumulative point excess are rejected',async()=>{
    const m=mark('test');
    const manyPoints=Array.from({length:11},()=>({...mark(''),points:Array.from({length:20000},()=>({x:0,y:0}))}));
    for(const bad of [patch({add:[null]}),patch({add:[m,m]}),patch({add:null}),patch({remove:null}),patch({update:null}),patch({update:[null]}),patch({update:[m,m]}),
      patch({add:manyPoints})])
      assert.equal((await request(path+'/changes',access.editKey,bad)).status,400);
  });
  await t.test('request size and cumulative document size are bounded; rejected edits preserve saved data',async()=>{
    const over=pendingRequest(path+'/changes',access.editKey,4*1024*1024+1);
    try { assert.equal(await over.status,413); } finally { over.socket.destroy(); }
    const content=Array.from({length:550},()=>mark('a'.repeat(4000)));
    const written=await request(path+'/changes',access.editKey,patch({add:content}));
    assert.equal(written.status,200);
    await written.arrayBuffer(); // Release the concurrency lease of this large response.
    const before=await (await request(path,access.editKey)).json();
    assert.equal((await request(path+'/changes',access.editKey,patch({add:content.map(m=>({...m,id:crypto.randomUUID()}))}))).status,413);
    const after=await (await request(path,access.editKey)).json();
    assert.equal(after.revision,before.revision); assert.equal(after.marks.length,before.marks.length);
  });
  await t.test('no secrets/configuration files or cross-origin cookie authorization exposed',async()=>{
    for(const file of ['/appsettings.json','/.env','/BlurMask.Server.dll','/../etc/passwd'])
      assert.equal((await request(file)).status,404);
    const response=await request(path,access.editKey,undefined,{Origin:'https://other.example'});
    assert.equal(response.headers.get('access-control-allow-origin'),null);
    assert.equal(response.headers.get('cache-control'),'no-store');
    assert.equal(response.headers.get('referrer-policy'),'no-referrer');
    assert.match(response.headers.get('content-security-policy'),/frame-ancestors 'none'/);
    await response.arrayBuffer();
  });
  await t.test('eight pending requests bound global concurrency; rejected requests recover after disconnect',async()=>{
    assert.equal(base.protocol,'http:');
    const sockets=[];
    try {
      for(let i=0;i<8;i++) {
        const pending=pendingRequest(path+'/changes',access.editKey,100);
        sockets.push(pending.socket);
        assert.equal(await pending.status,100);
        pending.socket.write('{');
      }
      await new Promise(resolve=>setTimeout(resolve,200));
      assert.equal((await request('/healthz')).status,429);
    } finally { for(const socket of sockets) socket.destroy(); }
    await new Promise(resolve=>setTimeout(resolve,200));
    assert.equal((await request('/healthz')).status,200);
  });
});
