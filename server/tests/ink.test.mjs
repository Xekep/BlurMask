import test from 'node:test';
import assert from 'node:assert/strict';
import { emptyBoard, diff, rebase, History, hit, constrain, validBoard } from '../wwwroot/ink.js';
const mark = () => ({id: crypto.randomUUID(), tool:'Pen', color:'#111111', width:4, fontSize:28, text:'', points:[{x:0,y:0},{x:100,y:100}]});
test('moving an object keeps its layer and peer ink; undo preserves concurrent peer moves', () => {
  const a=mark(), peer=mark(), moved={...a,points:[{x:40,y:30},{x:140,y:130}]};
  const before={...emptyBoard(),marks:[a]}, after={...before,marks:[moved]};
  assert.deepEqual(diff(before,after).update,[moved]);
  assert.deepEqual(rebase({...before,marks:[a,peer]},before,after).marks,[moved,peer]);
  assert.deepEqual(rebase(emptyBoard(),before,after).marks,[]);
  const history=new History();history.push(before,after);
  assert.deepEqual(history.undo({...after,marks:[moved,peer]}).marks,[a,peer]);
  assert.deepEqual(history.redo({...before,marks:[a,peer]}).marks,[moved,peer]);
  const other={...moved,points:[{x:500,y:500}]};
  assert.deepEqual(history.undo({...after,marks:[other,peer]}).marks,[other,peer]);
});
test('PNG raster payload and dimensions are validated before rendering', () => {
  const png='iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aR1cAAAAASUVORK5CYII=';
  const image={...mark(),tool:'Image',image:png};
  assert.equal(validBoard({...emptyBoard(),marks:[image]}),true);
  assert.equal(hit(image,{x:50,y:50},0),true);
  for(const bad of [{...image,image:'invalid'},{...image,image:png.repeat(40000)},{...image,points:[{x:0,y:0}]},{...image,tool:'Pen'}])
    assert.equal(validBoard({...emptyBoard(),marks:[bad]}),false);
});
test('concurrent additions and edits made while a request is pending survive merge', () => {
  const a=mark(), b=mark(), c=mark(), baseline=emptyBoard();
  const submitted={...baseline,marks:[a]}, current={...baseline,marks:[a,c]}, remote={...baseline,marks:[a,b],revision:2};
  assert.deepEqual(rebase(remote,submitted,current).marks,[a,b,c]);
});
test('clear removes only strokes seen by the author', () => {
  const a=mark(), b=mark(), before={...emptyBoard(),marks:[a]};
  assert.deepEqual(diff(before,emptyBoard()).remove,[a.id]);
  assert.deepEqual(rebase({...before,marks:[a,b]},before,emptyBoard()).marks,[b]);
});
test('undo/redo preserve peer strokes and original insertion order', () => {
  const a=mark(), b=mark(), peer=mark(), before=emptyBoard(), after={...before,marks:[a,b]};
  const history=new History(); history.push(before,after);
  const undone=history.undo({...after,marks:[a,b,peer]});
  assert.deepEqual(undone.marks,[peer]);
  assert.deepEqual(history.redo(undone).marks,[peer,a,b]);
});
test('eraser hits reversed rectangle outline but not its interior', () => {
  const rectangle={...mark(),tool:'Rectangle',points:[{x:200,y:200},{x:100,y:100}]};
  assert.equal(hit(rectangle,{x:100,y:150},2),true);
  assert.equal(hit(rectangle,{x:150,y:150},2),false);
  assert.deepEqual(constrain({x:200,y:200},{x:100,y:150},'Rectangle'),{x:100,y:100});
});
test('import rejects malformed or duplicate strokes before rendering', () => {
  const m=mark();assert.equal(validBoard({...emptyBoard(),marks:[m]}),true);
  for (const bad of [{...m,color:'invalid'},{...m,points:null},{...m,width:NaN},{...m,text:null},{...m,fontSize:0}])
    assert.equal(validBoard({...emptyBoard(),marks:[bad]}),false);
  assert.equal(validBoard({...emptyBoard(),marks:[m,m]}),false);
});
