import test from 'node:test';
import assert from 'node:assert/strict';
import { emptyBoard, diff, rebase, History, hit, constrain, validBoard } from '../wwwroot/ink.js';
const mark = () => ({id: crypto.randomUUID(), tool:'Pen', color:'#111111', width:4, fontSize:28, text:'', points:[{x:0,y:0},{x:100,y:100}]});
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
