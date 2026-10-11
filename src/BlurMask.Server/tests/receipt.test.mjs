import test from 'node:test';
import assert from 'node:assert/strict';

const base = new URL(process.env.PEN_TEST_URL || 'http://127.0.0.1:5079');
const media = 'application/vnd.blurmask.board-receipt+json';
const patch = extra => ({operationId:crypto.randomUUID(),add:[],update:[],remove:[],...extra});
const mark = () => ({id:crypto.randomUUID(),tool:'Pen',color:'#ff0000',width:4,points:[{x:20,y:20},{x:40,y:40}],text:'',fontSize:28,font:''});
const request = (path,key,body,revision) => fetch(new URL(path,base), {
  method:body === undefined ? 'GET' : 'POST',
  headers:{...(key ? {Authorization:'Bearer '+key} : {}),...(body === undefined ? {} : {'Content-Type':'application/json'}),
    ...(revision === undefined ? {} : {'X-BlurMask-Base-Revision':String(revision)})},
  body:body === undefined ? undefined : JSON.stringify(body)
});
const compact = response => response.headers.get('content-type')?.split(';')[0] === media;

test('compact write receipts: known revision, PNG omission, metadata, conflicts, lost replies and authorization',async()=>{
  assert.ok(['127.0.0.1','localhost','[::1]'].includes(base.hostname),'Run receipt writes on an isolated loopback test server only.');
  const created=await request('/api/boards',null,{title:'Compact receipt test',background:'white'});
  assert.equal(created.status,201); const access=await created.json(), path='/api/boards/'+access.board.id;
  const image='iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aR1cAAAAASUVORK5CYII=';
  const images=Array.from({length:32},()=>({...mark(),tool:'Image',image}));
  const initial=await request(path+'/changes',access.editKey,patch({add:images}));
  assert.equal(compact(initial),false); const baseline=await initial.json(); assert.equal(baseline.revision,1); assert.equal(baseline.marks.length,32);

  const operation=patch({add:[mark()],title:'  Canonical title  ',background:'black'});
  const saved=await request(path+'/changes',access.editKey,operation,1);
  assert.equal(saved.status,200); assert.equal(compact(saved),true);
  const bytes=await saved.text(), receipt=JSON.parse(bytes);
  assert.equal(receipt.boardId,access.board.id); assert.equal(receipt.operationId,operation.operationId);
  assert.equal(receipt.baseRevision,1); assert.equal(receipt.revision,2);
  assert.equal(receipt.title,'Canonical title'); assert.equal(receipt.patch.title,'Canonical title');
  assert.equal(receipt.background,'black'); assert.equal(receipt.width,baseline.width); assert.equal(receipt.height,baseline.height);
  assert.equal(receipt.patch.add.length,1); assert.equal(receipt.patch.add[0].id,operation.add[0].id);
  assert.ok(!bytes.includes(image)); assert.ok(!Object.hasOwn(receipt,'marks'));
  const full=await (await request(path,access.editKey)).text();
  assert.ok(bytes.length < full.length/4,'Unchanged image payload should not be repeated in the HTTP acknowledgement.');
  const retried=await request(path+'/changes',access.editKey,operation,1);
  assert.equal(compact(retried),false); assert.equal((await retried.json()).revision,2);
  const noChange=await request(path+'/changes',access.editKey,patch({remove:[crypto.randomUUID()]}),2);
  assert.equal(compact(noChange),true); const unchanged=await noChange.json(); assert.equal(unchanged.revision,2); assert.equal(unchanged.patch,null);

  const lost=patch({add:[mark()]});
  // Discard the successful response, then retry the exact operation with the old baseline.
  const discarded=await request(path+'/changes',access.editKey,lost,2); assert.equal(compact(discarded),true); await discarded.arrayBuffer();
  const recovered=await request(path+'/changes',access.editKey,lost,2); assert.equal(compact(recovered),false);
  const recoveredBoard=await recovered.json(); assert.equal(recoveredBoard.revision,3); assert.equal(recoveredBoard.marks.filter(m=>m.id===lost.add[0].id).length,1);

  const peers=[patch({add:[mark()]}),patch({add:[mark()]})];
  const responses=await Promise.all(peers.map(p=>request(path+'/changes',access.editKey,p,3)));
  assert.equal(responses.filter(compact).length,1,'Only the commit immediately following the known revision may acknowledge a delta.');
  const bodies=await Promise.all(responses.map(r=>r.json()));
  assert.deepEqual(bodies.map(b=>b.revision).sort(),[4,5]);
  const merged=await (await request(path,access.editKey)).json(); assert.equal(merged.revision,5);
  for(const p of peers) assert.equal(merged.marks.filter(m=>m.id===p.add[0].id).length,1);
  const oldRetry=await request(path+'/changes',access.editKey,lost,3); assert.equal(compact(oldRetry),false); assert.equal((await oldRetry.json()).revision,5);
  for(const revision of [-1,'invalid','1,2','9223372036854775807']) {
    const response=await request(path+'/changes',access.editKey,patch({}),revision); assert.equal(compact(response),false); assert.equal((await response.json()).revision,5);
  }
  assert.equal((await request(path+'/changes',access.viewKey,patch({add:[mark()]}),5)).status,403);
  assert.equal((await request(path+'/changes','x'.repeat(43),patch({add:[mark()]}),5)).status,404);
  const after=await (await request(path,access.editKey)).json(); assert.deepEqual(after,merged);

  // Remove two lower layers, then restore them ahead of surviving layers.
  // Only two UUID anchors are sent, rather than a complete document/order list.
  const originalOrder=after.marks.map(m=>m.id), restoredLayers=after.marks.slice(0,2);
  const removal=await request(path+'/changes',access.editKey,patch({remove:restoredLayers.map(m=>m.id)}),5);
  assert.equal(removal.status,200); assert.equal((await removal.json()).revision,6);
  const restore=patch({add:restoredLayers,insertBefore:[{id:restoredLayers[0].id,before:restoredLayers[1].id},{id:restoredLayers[1].id,before:after.marks[2].id}]});
  const restored=await request(path+'/changes',access.editKey,restore,6);
  assert.equal(restored.status,200); assert.equal(compact(restored),true);
  const restoredReceipt=await restored.json(); assert.deepEqual(restoredReceipt.patch.insertBefore,restore.insertBefore);
  const ordered=await (await request(path,access.editKey)).json(); assert.deepEqual(ordered.marks.map(m=>m.id),originalOrder);
  for(const insertBefore of [null,[{id:restoredLayers[0].id,before:restoredLayers[0].id}],[{id:after.marks[2].id,before:restoredLayers[0].id}]])
    assert.equal((await request(path+'/changes',access.editKey,patch({add:restoredLayers,insertBefore}))).status,400);
  assert.equal((await request(path+'/changes',access.viewKey,restore,7)).status,403);
  const again=await request(path+'/changes',access.editKey,restore,6); assert.equal(compact(again),false);
  assert.deepEqual((await again.json()).marks.map(m=>m.id),originalOrder);
});
