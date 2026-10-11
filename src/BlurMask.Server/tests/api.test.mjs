import test from 'node:test';
import assert from 'node:assert/strict';
import { parseInvite } from '../wwwroot/invite.js';
const base=process.env.PEN_TEST_URL || 'http://127.0.0.1:5079';
const request=(path,key,body)=>fetch(base+path,{method:body?'POST':'GET',headers:{...(key?{Authorization:'Bearer '+key}:{}),...(body?{'Content-Type':'application/json'}:{})},body:body?JSON.stringify(body):undefined});
const mark=()=>({id:crypto.randomUUID(),tool:'Pen',color:'#ff001180',width:4,points:[{x:50,y:50},{x:150,y:150}],text:'',fontSize:28});

test('conditional reads validate credentials, role and current revision before returning 304',async()=>{
  const access=await (await request('/api/boards',null,{title:'ETag integration',background:'white'})).json();
  const path='/api/boards/'+access.board.id;
  const first=await request(path,access.editKey), tag=first.headers.get('etag');
  assert.equal(first.status,200); assert.ok(tag); await first.json();
  const read=(key,etag)=>fetch(base+path,{headers:{Authorization:'Bearer '+key,'If-None-Match':etag}});
  const unchanged=await read(access.editKey,tag);
  assert.equal(unchanged.status,304); assert.equal(await unchanged.text(),'');
  const weakTag=tag.startsWith('W/')?tag:'W/'+tag;
  assert.equal((await read(access.editKey,weakTag)).status,304);
  assert.equal((await read('x'.repeat(43),weakTag)).status,404);
  const denied=await read('x'.repeat(43),tag);
  assert.equal(denied.status,404); assert.equal(denied.headers.get('etag'),null);
  const view=await read(access.viewKey,tag);
  assert.equal(view.status,200); assert.equal((await view.json()).readOnly,true);
  const viewTag=view.headers.get('etag'); assert.notEqual(viewTag,tag);
  assert.equal((await read(access.viewKey,viewTag)).status,304);
  assert.equal((await read(access.editKey,viewTag)).status,200);
  for(const invalid of ['"bad"', '"'+crypto.randomUUID()+'-0-e"', '"'+access.board.id.replaceAll('-','')+'--e"'])
    assert.equal((await read(access.editKey,invalid)).status,200);
  await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[mark()],remove:[]});
  const changed=await read(access.editKey,tag);
  assert.equal(changed.status,200); assert.equal((await changed.json()).marks.length,1);
  assert.notEqual(changed.headers.get('etag'),tag);
});

test('object transforms and embedded font choices persist through shared updates',async()=>{
  const access=await (await request('/api/boards',null,{title:'Object transforms',background:'white'})).json();
  const path='/api/boards/'+access.board.id;
  const item={...mark(),tool:'Text',text:'English русский 123 + ∑',font:'Inter',transform:{m11:0,m12:1.5,m21:-2,m22:0,offsetX:800,offsetY:200}};
  const patch={operationId:crypto.randomUUID(),add:[item],remove:[]};
  assert.equal((await request(path+'/changes',access.editKey,patch)).status,200);
  const saved=await (await request(path,access.viewKey)).json();
  assert.deepEqual(saved.marks[0].transform,item.transform); assert.equal(saved.marks[0].font,'Inter');
  const resized={...item,font:'Manrope',transform:{...item.transform,m12:2}};
  assert.equal((await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[],update:[resized],remove:[]})).status,200);
  const read=await (await request(path,access.editKey)).json(); assert.deepEqual(read.marks[0].transform,resized.transform);
  for(const bad of [
    {...item,font:'untrusted-font'},
    {...item,transform:{m11:0,m12:0,m21:0,m22:0,offsetX:0,offsetY:0}},
    {...item,transform:{...item.transform,offsetX:1e100}},
    {...item,transform:{...item.transform,m12:1001}},
  ]) assert.equal((await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[],update:[bad],remove:[]})).status,400);
});
test('Postgres API: access keys, idempotent retries, concurrency, validation, targeted erase',async()=>{
  const created=await request('/api/boards',null,{title:'Integration test',background:'white'});
  assert.equal(created.status,201); const access=await created.json(), path='/api/boards/'+access.board.id;
  assert.deepEqual(parseInvite(access.editUrl),{id:access.board.id,key:access.editKey});
  assert.deepEqual(parseInvite(access.viewUrl),{id:access.board.id,key:access.viewKey});
  assert.equal(new URL(access.editUrl).hash.includes('='),false);
  assert.equal((await request(path)).status,404);
  assert.equal((await request(path,'x'.repeat(43))).status,404);
  const a=mark(), patch={operationId:crypto.randomUUID(),add:[a],remove:[]};
  const written=await request(path+'/changes',access.editKey,patch); assert.equal(written.status,200);
  const saved=await written.json(), retried=await (await request(path+'/changes',access.editKey,patch)).json();
  assert.equal(retried.revision,saved.revision); assert.equal(retried.marks.length,1);
  const view=await (await request(path,access.viewKey)).json(); assert.equal(view.readOnly,true);
  assert.equal((await request(path+'/changes',access.viewKey,{...patch,operationId:crypto.randomUUID()})).status,403);
  const b=mark(), c=mark();
  const results=await Promise.all([b,c].map(m=>request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[m],remove:[]})));
  assert.equal(results.every(r=>r.status===200),true);
  const merged=await (await request(path,access.editKey)).json(); assert.equal(merged.marks.length,3); assert.equal(merged.revision,3);
  const moved={...b,points:[{x:250,y:250},{x:350,y:350}]}, movePatch={operationId:crypto.randomUUID(),add:[],remove:[],update:[moved]};
  const movedBoard=await (await request(path+'/changes',access.editKey,movePatch)).json();
  assert.deepEqual(movedBoard.marks.map(m=>m.id),merged.marks.map(m=>m.id));
  assert.deepEqual(movedBoard.marks.find(m=>m.id===b.id).points,moved.points);
  assert.equal((await (await request(path+'/changes',access.editKey,movePatch)).json()).revision,movedBoard.revision);
  assert.equal((await request(path+'/changes',access.viewKey,{...movePatch,operationId:crypto.randomUUID()})).status,403);
  assert.equal((await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[{...mark(),tool:'Unknown'}],remove:[]})).status,400);
  assert.equal((await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[{...mark(),points:null}],remove:[]})).status,400);
  const erased=await (await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[],remove:[a.id]})).json();
  assert.deepEqual(new Set(erased.marks.map(m=>m.id)),new Set([b.id,c.id]));
  const oldRetry=await (await request(path+'/changes',access.editKey,patch)).json();
  assert.equal(oldRetry.marks.length,2); assert.equal(oldRetry.revision,erased.revision);
  assert.equal((await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[],remove:[],title:'   '})).status,400);
  const offline=Array.from({length:501},mark);
  const uploaded=await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:offline,remove:[]});
  assert.equal(uploaded.status,200); assert.equal((await uploaded.json()).marks.length,503);
  const image={...mark(),tool:'Image',image:'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aR1cAAAAASUVORK5CYII='};
  const raster=await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[image],remove:[]});
  assert.equal(raster.status,200); assert.equal((await raster.json()).marks.at(-1).image,image.image);
  assert.equal((await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[],remove:[],update:[{...image,image:'invalid'}]})).status,400);
  assert.equal((await request(path+'/changes',access.editKey,{operationId:crypto.randomUUID(),add:[image],remove:[],update:[image]})).status,400);
});
