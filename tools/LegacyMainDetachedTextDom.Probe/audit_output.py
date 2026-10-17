"""Independent logical031880 tree from raw40/48/ref24/TEXT32/line56.
Fault checks replay pointer-free service prefixes and arena ownership; inputs
are post-inference metadata, never expected recognition labels.
"""
import base64,hashlib,importlib.util,json,math,struct,sys
from pathlib import Path
rp=Path(__file__).resolve().parents[1]/'LegacyMainSourceRangeUtf8.Probe/reference_controls.py'
spec=importlib.util.spec_from_file_location('range_reference',rp);ranges=importlib.util.module_from_spec(spec);spec.loader.exec_module(ranges)
b64=base64.b64decode;enc=lambda b:base64.b64encode(b).decode()
a=json.loads(Path(sys.argv[1]).read_bytes())
def lit(b):return b.split(b'\0',1)[0]
def value(b,tag=None):
 b=lit(b)if tag is None else b
 return dict(tag=(3 if not b else 2 if len(b)<16 else 1)if tag is None else tag,value=enc(b))
def attr(name,b,tag=None):return dict(kind=0,name=name,attributes=[],children=[],**value(b,tag))
def node(name,attributes=(),children=()):return dict(kind=3,name=name,tag=0,value='',attributes=list(attributes),children=list(children))
def number(name,n,fmt,tag):return attr(name,struct.pack(fmt,n),tag)
def range_bytes(s):
 rows=list(map(list,struct.iter_unpack('<QQ',b64(s['ranges16']))));v=b64(ranges.replay(rows)['utf8'])
 if s.get('flags',0)&0x400000:v=(b'{'if v[:1]==b'['else b'}')+v[1:]
 return v
def scratch_format(raw,before):
 rows=list(map(list,struct.iter_unpack('<QQ',b64(raw))));r=ranges.replay(rows);out=b64(r['utf8']);capacity=before;allocations=[]
 if not r['canonical']:allocations.append('range-copy')
 def reserve(required):
  nonlocal capacity
  if capacity<required:capacity=max(required,capacity*3//2);allocations.append('range-bytes')
 reserve(len(r['rows'])*18+1);written=0
 for i,part in enumerate(out.split(b'U')if out else[]):
  if i:reserve(written+2);written+=1
  reserve(written+len(part)+1);written+=len(part)
 return out,capacity,allocations
def clean(n):return {k:([clean(x)for x in v]if k in('attributes','children')else v)for k,v in n.items()if k not in('id','nameId')}
def expected(s):
 chars=[];byid={x['id']:x for x in s['characters']}
 for c in s['characters']:
  ats=[attr('range',range_bytes(c))]
  if c['tag']!=-1:ats+=[number('tag',c['tag'],'<q',4)]
  if c['line']!=-1:ats+=[number('line',c['line'],'<i',7)]
  assert c['force']==-1 and c['freeze']==0
  children=[]
  for r in c['records40']:
   raw=b64(r['raw']);assert len(raw)==40 and struct.unpack_from('<I',raw,8)[0]&0xc00==0
   children.append(node('charCandidate',children=[dict(kind=1,name='',attributes=[],children=[],**value(b64(r['text'])))]))
  chars.append(node('charSegment',ats,children))
 word=s['word'];wordats=[attr('range',range_bytes(word))]
 if word['tag']!=-1:wordats+=[number('tag',word['tag'],'<q',4)]
 candidates=[]
 for row in word['rows48']:
  raw=b64(row['raw']);assert len(raw)==48 and struct.unpack_from('<I',raw,24)[0]==len(row['refs24'])
  children=[]
  for link in row['refs24']:
   raw=b64(link['raw']);assert len(raw)==24 and raw[20]==0
   children.append(node('char',[attr('ref',range_bytes(byid[link['charId']])),number('selected',struct.unpack_from('<i',raw,16)[0],'<i',7)]))
  candidates.append(node('wordCandidate',children=children))
 text=s['text'];textats=[number('tag',text['tag'],'<q',4),attr('range',range_bytes(text)),number('selected',text['selected'],'<i',7)];alternatives=[]
 assert text['selected']==0 and text['freeze']==0 and len(text['rows32'])==2
 for row in text['rows32']:
  raw=b64(row['raw']);ref=b64(row['ref24']);assert len(raw)==32 and len(ref)==24 and ref[20]==0 and struct.unpack_from('<I',raw,24)[0]==1
  alternatives.append(node('textCandidate',children=[node('word',[attr('ref',range_bytes(word)),number('selected',struct.unpack_from('<i',ref,16)[0],'<i',7)])]))
 lines=[];raw=b64(s['lines56']);assert len(raw)==56
 for offset in range(0,len(raw),56):
  x,y,angle,h,tag=struct.unpack_from('<ddddq',raw,offset+8);ats=[]
  if abs(x)>=2**-23 and abs(angle)>=2**-23:ats.append(number('x',x,'<d',5))
  ats +=[number('y',y,'<d',5),number('a',angle,'<d',5),number('h',h,'<d',5)]
  if tag>=0:ats.append(number('tagId',tag,'<q',4))
  lines.append(node('line',ats))
 return node('textResult',[attr('gen',b64(s['uuid36']))],[node('charSegmentList',children=chars),node('wordSegmentList',children=[node('wordSegment',wordats,candidates)]),node('text',textats,alternatives),node('lines',children=lines)])
def replay_events(result,verify=True):
 nodes={};names={};attempts=0;failed=False
 for line in result['trace']:
  p=line.split(':');op=p[0]
  if op=='name':
   ident=int(p[1]);assert ident==len(names)+1 and p[2]not in names;names[p[2]]=ident
  elif op=='try':attempts+=1
  elif op=='fail':assert not failed;failed=True
  elif op=='new':
   assert not failed;ident,kind=int(p[1]),int(p[2]);name=p[3];assert ident==len(nodes)+1
   nodes[ident]=dict(id=ident,kind=kind,name=name,nameId=names.get(name,0),tag=3 if kind<2 else 0,value='',parent=0,released=False,attributes=[],children=[])
  elif op=='value':
   assert not failed;n=nodes[int(p[1])];n['tag']=int(p[2]);n['value']=p[3]
  elif op in('append','attr'):
   assert not failed;parent,child=int(p[1]),int(p[2]);assert not nodes[child]['parent'];nodes[child]['parent']=parent;nodes[parent]['children'if op=='append'else'attributes'].append(child)
  elif op=='clear':
   n=nodes[int(p[1])]
   for child in n['children']:nodes[child]['parent']=0
   n['children']=[]
  elif op in('release','release-bare'):
   n=nodes[int(p[1])];assert not n['released'];n['released']=True;n['parent']=0
   for parent in nodes.values():
    if n['id']in parent['attributes']:parent['attributes'].remove(n['id'])
   n['attributes']=[]
  elif op=='range':
   #All real fixtures have already canonical source keys; scratch growth
   #is checked separately from tree range attributes and failure prefixes.
   assert len(p)==5
  else:raise AssertionError(line)
 if verify:
  assert attempts==result['attempts']
  assert list(nodes.values())==result['arena']
 return nodes

def failure_cleanup(success,cut,fault):
 #Independent helper ownership rules:0309C0 CHAR,030CC0 WORD,
 #031360 TEXT-reference,031590 line, then031880 outer VD0/V90.
 #The completed logical tree has already been checked against raw metadata.
 #Use its ancestry only to identify the native active helper at this service;
 #derive every cleanup and surviving orphan from the PRE-failure arena.
 current=replay_events(dict(trace=success['trace'][:cut+1]),False)
 complete={n['id']:n for n in success['arena']}
 upcoming=next(x for x in success['trace'][cut+1:]if x.startswith(('new:','value:')))
 target=int(upcoming.split(':')[1]);kind=complete[target]['kind'];path={}
 node=complete[target]
 while True:
  if node['kind']==3:path[node['name']]=node['id']
  if not node['parent']:break
  node=complete[node['parent']]
 cleanup=[]
 def release(ident):
  if ident not in current:return
  n=current[ident];assert not n['released']
  for child in list(n['attributes']):release(child)
  n['attributes']=[];clear(ident)
  n['released']=True;n['parent']=0;cleanup.append('release:'+str(ident))
  for parent in current.values():
   if ident in parent['attributes']:parent['attributes'].remove(ident)
 def clear(ident):
  n=current[ident];cleanup.append('clear:'+str(ident))
  children=list(n['children']);n['children']=[]
  for child in children:current[child]['parent']=0;release(child)
 #A newly created attribute whose value copy failed is freed by EBAF0.
 #V80's failed text copy instead uses bare F44D0, without EA480 recursion.
 if success['trace'][cut]=='try:value-bytes':
  if kind==0:release(target)
  else:
   assert kind==1;current[target]['released']=True
   cleanup.append('release-bare:'+str(target))
 if 'charSegment'in path:
  if kind==1 and 'charCandidate'in path:release(path['charCandidate'])
  release(path['charSegment'])
 elif 'wordSegment'in path:
  if 'wordCandidate'in path:release(path['wordCandidate'])
  release(path['wordSegment'])
 elif 'textCandidate'in path:
  if complete[target]['name']=='textCandidate'and target not in current:
   assert fault['status']=='unsupported'
   assert fault['result']['trace'][cut+2:]==[]
   assert fault['result']['arena']==list(current.values());return
  if 'word'in path:release(path['word'])
  release(path['textCandidate'])
 elif 'line'in path:release(path['line'])
 if 1 in current:clear(1);release(1)
 actual=[x for x in fault['result']['trace'][cut+2:]if x.startswith(('clear:','release:','release-bare:'))]
 assert actual==cleanup,(fault['fail'],actual,cleanup)
 assert fault['result']['arena']==list(current.values())
total=faults=orphans=0;stable=json.loads(json.dumps(a))
for s in a['actual']:
 assert s['registryBefore']==s['registryAfter']and s['pendingBefore']==s['pendingAfter']
 r=s['result'];assert r['succeeded']and clean(r['tree'])==expected(s)
 nodes=replay_events(r);assert all(not n['released']for n in nodes.values());total+=len(nodes)
 bykey={('CHAR',str(c['id'])):c for c in s['characters']};bykey['WORD',str(s['word']['id'])]=s['word'];bykey['TEXT','0']=s['text']
 expected_order=[('CHAR',str(c['id']))for c in s['characters']]+[('WORD',str(s['word']['id']))]+[('CHAR',str(link['charId']))for row in s['word']['rows48']for link in row['refs24']]+[('TEXT','0')]+[('WORD',str(s['word']['id']))for _ in s['text']['rows32']]
 events=[x.split(':')for x in r['trace']if x.startswith('range:')];assert [(p[1],p[2])for p in events]==expected_order
 capacity=0;allocations=[]
 for p in events:
  raw,capacity,alloc=scratch_format(bykey[p[1],p[2]]['ranges16'],capacity);allocations+=alloc
  assert int(p[3])==capacity and b64(p[4])==raw
 assert capacity==r['capacity']and allocations==[x[4:]for x in r['trace']if x.startswith('try:range-')]
 assert b64(r['scratch'])==range_bytes(s['word'])
 if 'faults'in s:
  success=r['trace'];tries=[i for i,x in enumerate(success)if x.startswith('try:')]
  for fault in s['faults']:
   f=fault['result'];assert not f['succeeded']and f['tree']is None
   cut=tries[fault['fail']];assert f['trace'][:cut+1]==success[:cut+1]
   assert f['trace'][cut+1]=='fail:'+success[cut][4:]
   failure_cleanup(r,cut,fault)
   nodes=replay_events(f)
   if fault['status']=='result':assert not nodes or nodes[1]['released']
   else:assert fault['status']=='unsupported'and 'new:'not in ''.join(f['trace'][cut+2:])
   orphans+=sum(not n['released']for n in nodes.values());faults+=1
  assert len(s['faults'])==r['attempts']
  cancelled=s['cancelled'];cut=tries[5]
  assert cancelled['trace']==success[:cut+1]and not cancelled['succeeded']
  assert not replay_events(cancelled)[1]['released']
  for control in s['literal']:
   raw=lit(b64(control['raw']));assert b64(control['text'])==raw and control['tag']==value(raw)['tag']
   if len(raw)>=16:assert control['failureTrace']==['try:node1','new:1:1:','try:value-bytes','fail:value-bytes','release-bare:1']
  assert [c['tag']for c in s['numeric']]==[4,5,6,7,8]
  for control in s['numeric']:
   width={4:8,5:8,6:1,7:4,8:4}[control['tag']]
   assert b64(control['raw'])==b64(control['value'])==bytes([255])*width
  assert len(s['scratchControls'])==4
  for control in s['scratchControls']:
   raw,capacity,alloc=scratch_format(control['ranges16'],control['before'])
   assert b64(control['utf8'])==raw and control['after']==capacity and control['allocations']==alloc
  assert s['scratchControls'][2]['allocations']==['range-copy']
 #UUIDentropy belongs to the environment, not inference. Normalize only
 #this retained36-bytegen value for Managed/Native structural parity.
oldids=[x['uuid36']for x in a['actual']]
def normalize(x):
 if isinstance(x,dict):return{k:normalize(v)for k,v in x.items()}
 if isinstance(x,list):return[normalize(v)for v in x]
 if isinstance(x,str):
  for ident in oldids:x=x.replace(ident,'LIBRARY-GEN36')
 return x
projection=json.dumps(normalize(a),sort_keys=True,separators=(',',':')).encode();sha=hashlib.sha256(projection).hexdigest()
Path(str(sys.argv[1])+'.stable.sha256').write_bytes((sha+'\n').encode())
assert len(a['actual'])==4
print(f'PASS independent4 raw-metadata trees/{total} full arena nodes/{faults} service prefixes/{orphans} retained fault-node observations;stableSHA={sha}')
