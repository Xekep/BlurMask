"""Independent031880 raw-tree + BE2 recursive-copy/root mutation oracle."""
import base64,copy,hashlib,json,struct,sys
from pathlib import Path
from reference_tree import expected,clean,b64,enc

a=json.loads(Path(sys.argv[1]).read_bytes())
class Cancelled(Exception):pass
class Arena:
 def __init__(self,before,fail=-1,cancel=-1):
  self.state=copy.deepcopy(before);self.fail=fail;self.cancel=cancel;self.base=before['attempts']
 def allow(self,op):
  n=self.state['attempts']-self.base;self.state['attempts']+=1;self.state['trace'].append('try:'+op)
  if n==self.cancel:raise Cancelled()
  if n==self.fail:self.state['trace'].append('fail:'+op);return False
  return True
 def node(self,kind,name=''):
  names=self.state['names']
  if name and name not in names:names.append(name);self.state['trace'].append(f'name:{len(names)}:{name}')
  if not self.allow('node'+str(kind)):return None
  ident=len(self.state['nodes'])+1;n=dict(id=ident,kind=kind,name=name,nameId=names.index(name)+1 if name else 0,
   tag=3 if kind<2 else 0,value='',parent=0,released=False,attributes=[],children=[])
  self.state['nodes'].append(n);self.state['trace'].append(f'new:{ident}:{kind}:{name}');return n
 def get(self,i):return self.state['nodes'][i-1]
 def value(self,n,tag,value):
  if tag==1 and not self.allow('value-bytes'):return False
  n['tag']=tag;n['value']=value;self.state['trace'].append(f'value:{n["id"]}:{tag}:{value}');return True
 def append(self,p,c,attr=False):
  c['parent']=p['id'];p['attributes'if attr else'children'].append(c['id'])
  self.state['trace'].append(f'{"attr"if attr else"append"}:{p["id"]}:{c["id"]}')
 def clear(self,n):
  self.state['trace'].append('clear:'+str(n['id']))
  for i in list(n['children']):self.get(i)['parent']=0;self.release(self.get(i))
  n['children']=[]
 def release(self,n,bare=False):
  assert not n['released']
  if not bare:
   for i in list(n['attributes']):self.get(i)['parent']=0;self.release(self.get(i))
   n['attributes']=[];self.clear(n)
  n['released']=True;self.state['trace'].append(('release-bare:'if bare else'release:')+str(n['id']))
 def clone(self,raw):
  #EBE40: element->ordered attrs->ordered recursive children. Attribute
  #copy failure frees bare attr then the element; no generic outer cleanup.
  n=self.node(raw['kind'],raw['name'])
  if n is None:return None
  if raw['kind']==1:
   if not self.value(n,raw['tag'],raw['value']):self.release(n,True);return None
   return n
  assert raw['kind']==3
  for attr in raw['attributes']:
   c=self.node(0,attr['name'])
   if c is None:self.release(n);return None
   if not self.value(c,attr['tag'],attr['value']):self.release(c,True);self.release(n);return None
   self.append(n,c,True)
  for child in raw['children']:
   c=self.clone(child)
   if c is None:self.release(n);return None
   self.append(n,c)
  return n
 def remove(self):
  root=self.get(1);assert len(root['children'])==1;child=self.get(root['children'].pop());child['parent']=0
  self.state['trace'].append(f'remove:1:{child["id"]}');self.release(child)
 def snapshot(self,n):
  return{k:n[k]for k in('id','kind','name','nameId','tag','value')}|dict(attributes=[self.snapshot(self.get(i))for i in n['attributes']],children=[self.snapshot(self.get(i))for i in n['children']])

def source_arena(result,names):return dict(attempts=result['attempts'],names=names,trace=result['trace'],nodes=result['arena'])
def fresh(names):
 return dict(attempts=1,names=copy.deepcopy(names),trace=['try:node4','new:1:4:'],nodes=[dict(id=1,kind=4,name='',nameId=0,tag=0,value='',parent=0,released=False,attributes=[],children=[])])
def row(op,tree):return dict(scalar40=enc(struct.pack('<I',op)+bytes(36)),path=[],payload=tree)
def export(before,tree,repeat=False,fail=-1,cancel=-1):
 sim=Arena(before,fail,cancel);rows=[]
 if not sim.allow('export-header')or not sim.allow('export-array'):return sim,False,rows
 for op in([4,2]if repeat else[2]):
  node=sim.clone(tree)
  if node is None:return sim,False,rows
  rows.append(row(op,sim.snapshot(node)))
 return sim,True,rows
def apply(before,tree,repeat=False,fail=-1,cancel=-1):
 sim=Arena(before,fail,cancel)
 if repeat:sim.remove()
 try:
  node=sim.clone(tree)
  if node is None:return sim,False
  sim.append(sim.get(1),node);return sim,True
 except Cancelled:return sim,False

faults=0;copies=0
for sample in a['actual']:
 tree=expected(sample);assert clean(sample['result']['tree'])==tree
 names=sample['writerAfterExport']['names'];first=source_arena(sample['result'],names)
 empty=fresh(names);sim=Arena(empty);assert sample['targetBefore']==sim.snapshot(sim.get(1))
 exported,ok,rows=export(first,tree)
 assert ok and sample['export']==dict(succeeded=True,epoch=0,rows=rows)
 assert sample['writerAfterExport']==exported.state
 applied,ok=apply(empty,tree);assert ok and sample['targetArenaAfter']==applied.state
 assert sample['targetAfter']==applied.snapshot(applied.get(1))
 assert sample['targetAfter']['children'][0]['id']!=sample['export']['rows'][0]['payload']['id']
 #Queue4 copy is produced BEFORE the next031880 source tree, then queue2.
 repeat=sample['repeatBuild'];assert clean(repeat['tree'])==tree
 old=Arena(dict(attempts=0,names=copy.deepcopy(names),trace=[],nodes=[]));assert old.allow('queue4');queued=old.clone(tree);assert queued is not None
 assert repeat['trace'][:len(old.state['trace'])]==old.state['trace']
 assert repeat['arena'][:len(old.state['nodes'])]==old.state['nodes']
 assert repeat['tree']['id']==len(old.state['nodes'])+1 and repeat['trace'][-1]=='try:queue2'
 re=source_arena(repeat,names);exported2,ok,rows2=export(re,tree,True)
 assert ok and sample['repeatExport']==dict(succeeded=True,epoch=1,rows=rows2)and sample['repeatWriter']==exported2.state
 replaced,ok=apply(applied.state,tree,True);assert ok and sample['replacedArena']==replaced.state
 assert sample['replacedTarget']==replaced.snapshot(replaced.get(1));copies+=6
 assert sample['registryBefore']==sample['registryAfter']and sample['pendingBefore']==sample['pendingAfter']
 if 'exportFaults'in sample:
  for q in sample['queueFaults']:
   f=q['result'];assert not f['succeeded']and f['tree']is None
   if q['mode']==0:
    assert f['arena']==sample['result']['arena']and f['trace']==sample['result']['trace']+['fail:queue2']
   else:
    initial=dict(attempts=0,names=copy.deepcopy(names),trace=[],nodes=[]);sim=Arena(initial,q['mode']-1)
    if sim.allow('queue4'):assert sim.clone(tree)is None
    assert f['arena']==sim.state['nodes']and f['trace']==sim.state['trace']
   faults+=1
  for f in sample['exportFaults']:
   assert f['before']==first
   predicted,ok,rows=export(f['before'],tree,fail=f['fault']);assert not ok
   assert f['after']==predicted.state and f['edit']==dict(succeeded=False,epoch=0,rows=rows);faults+=1
  for f in sample['applyFaults']:
   assert f['before']==empty;predicted,ok=apply(f['before'],tree,fail=f['fault']);assert not ok and f['after']==predicted.state;faults+=1
  for f in sample['replaceFaults']:
   assert f['before']==applied.state;predicted,ok=apply(f['before'],tree,True,fail=f['fault']);assert not ok and f['after']==predicted.state;faults+=1
  predicted,ok=apply(empty,tree,cancel=2);assert not ok and sample['cancelledArena']==predicted.state
  sim=Arena(first,cancel=3)
  try:
   assert sim.allow('export-header')and sim.allow('export-array');sim.clone(tree);raise AssertionError('Expected export cancellation')
  except Cancelled:pass
  assert sample['cancelledExportArena']==sim.state

ids=[s['uuid36']for s in a['actual']]
def stable(value):
 if isinstance(value,dict):return{k:stable(v)for k,v in value.items()}
 if isinstance(value,list):return[stable(v)for v in value]
 if isinstance(value,str):
  for ident in ids:value=value.replace(ident,'LIBRARY-GEN36')
 return value
sha=hashlib.sha256(json.dumps(stable(a),sort_keys=True,separators=(',',':')).encode()).hexdigest()
Path(str(sys.argv[1])+'.stable.sha256').write_bytes((sha+'\n').encode())
print(f'PASS independent4 raw trees/8 exported edits/{copies} recursive copy stages/{faults} full fault prefixes;stableSHA={sha}')
