"""Independent raw AK/B8 FIRST terminal replay; no SDK/research/runtime imports."""
from pathlib import Path
import ast,base64,functools,hashlib,json,math,struct,sys
if len(sys.argv)!=4:raise SystemExit('models-directory audited-context.json terminal.json')
models=Path(sys.argv[1]);prior=json.loads(Path(sys.argv[2]).read_bytes());capture=json.loads(Path(sys.argv[3]).read_bytes())
root=Path(__file__).resolve().parents[2]
#Reuse only bounded raw-wire definitions, never its capture expectations.
p=root/'tools/LegacySource247Wire.Probe/audit_output.py';tree=ast.parse(p.read_text(encoding='utf-8-sig'));env={}
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(p),'exec'),env)
name='en-US/en_US-ak-cur.lite.res';data=(models/name).read_bytes();manifest={x['path']:x['sha256']for x in json.loads((models/'manifest.json').read_bytes())['resources']}
assert hashlib.sha256(data).hexdigest()==manifest[name]
source=data[8843:182187];wire,raw=env['source'](source)
assert raw['Version']==0x110 and raw['SourceKind']==0x135 and raw['alphabetCount']==217
assert capture['Path']==prior['Path']==name and capture['Fingerprint'].lower()==prior['Fingerprint'].lower()==hashlib.sha256(source).hexdigest()
f=lambda n:next(base64.b64decode(x['raw'])for x in wire.fields if x['owner']=='247'and x['destination']==n)
width=1;height=int.from_bytes(f('BA'),'little');assert height==5 and f('B9')==b'\0'
#Actual serialized AL strings; no presumed English/Russian glyph mapping.
at=next(x['offset']for x in wire.fields if x['owner']=='2B'and x['destination']=='payload')
count=struct.unpack_from('<8H',source,at)[-1];at+=16;n=struct.unpack_from('<H',source,at)[0];at+=2
for step in(32,64,1):
 values=struct.unpack_from('<'+'H'*n,source,at);at+=2*n;n=next(x for x in reversed(values)if x!=65535)+step
spans=struct.unpack_from('<'+'H'*(n+1),source,at);at+=2*(n+1)
entries=[struct.unpack_from('<II',source,at+i*8)for i in range(spans[-1]+1)];at+=8*len(entries)
labels={label&65535:source[at+a:at+b]for(a,label),(b,_)in zip(entries,entries[1:])}
shadda=next((k for k,v in labels.items()if v==b'\xd9\x91'),65535);assert shadda==65535 and labels[211]=='\U000f0001'.encode()
INF=0x4000000
def field(b,p,fmt):return struct.unpack_from('<'+fmt,b,p)[0]
def f32(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.unpack('<i',struct.pack('<f',x))[0]
def wrap(x):return(x+2**31)%2**32-2**31
def sequences(nodes):
 lookup={n['Identity']:n for n in nodes};assert len(lookup)==len(nodes)
 @functools.lru_cache(None)
 def seq(i):
  if i==0:return()
  node=lookup[i];return seq(node['ParentLink'])+(node['Symbol'],)
 for i in lookup:seq(i)
 return seq,{seq(i)for i in lookup}
assert len(capture['Actual'])==len(prior['Actual'])==124
terminals=cells=blank=closed=boundaries=empty=fullBytes=0
for index,(actual,old)in enumerate(zip(capture['Actual'],prior['Actual'])):
 assert actual['Raw']==old['Raw'] and actual['ScoredBefore']==actual['ScoredAfter']
 seq,created=sequences(actual['Contexts']);oldseq,oldcreated=sequences(old['Contexts']);assert created==oldcreated
 nodes=[]
 assert len(actual['B8'])==len(old['B8'])==len(actual['Arcs'])
 for rows,oldrows in zip(actual['B8'],old['B8']):
  assert len(rows)==len(oldrows)==height;node=[]
  for a,b in zip(rows,oldrows):
   aa=base64.b64decode(a['Bytes'],validate=True);bb=base64.b64decode(b['Bytes'],validate=True)
   assert len(aa)==len(bb)==40*width and a['Count']==b['Count']
   assert aa[:24]+aa[32:]==bb[:24]+bb[32:]
   if a['Count']:assert seq(field(aa,24,'Q'))==oldseq(field(bb,24,'Q'))
   node.append(aa);fullBytes+=len(aa)
  nodes.append(node)
 def get(n,r,c):
  assert 0<=n<len(nodes)and 0<=r<height and 0<=c<width
  return nodes[n][r][c*40:(c+1)*40]
 best=None
 for row in range(height):
  for col in range(width):
   record=get(len(nodes)-1,row,col);cost=field(record,0,'i')
   if cost>=INF:break
   tn,tr,tc=len(nodes)-1,row,col;trait=record
   while field(trait,12,'H')==shadda:
    previous=tn-((field(trait,14,'H')>>1)&7)-1
    if previous==-1:break
    assert previous>=0;tr,tc,tn=trait[9],trait[10],previous;trait=get(tn,tr,tc)
   if trait[32]&0x13==1:cost=wrap(cost+0xa000)
   if best is None or cost<best[0]:best=(cost,row,col,field(record,4,'i'),field(record,24,'Q'),[tn,tr,tc])
 expected=[]
 if best is not None:
  total,row,col,acoustic,identity,trait=best
  log=f32(f32(-5.0*f32(0))*f32(.00012207031))
  score=[0,bits(f32(f32(0)/f32(len(nodes)<<13))),bits(log),bits(f32(math.exp(log))),bits(f32(f32(acoustic)*f32(.00012207031)))]
  n,r,c=len(nodes)-1,row,col;reverse=[]
  while n>=0:
   record=get(n,r,c);assert field(record,0,'i')<INF
   packed=field(record,14,'H');span=(packed>>1)&7;pn=n-span-1;assert pn>=-1
   arc=field(record,16,'H');symbol=field(record,12,'H');pa=field(get(pn,record[9],record[10]),16,'H')if pn>=0 else 65535
   assert arc<len(actual['Arcs'][n])and actual['Arcs'][n][arc][3]==span
   reverse.append(dict(Coordinates=[n,r,c,arc,symbol,span,packed&1,pn,pa],Bytes=base64.b64encode(record).decode()))
   r,c,n=record[9],record[10],pn
  reverse.reverse();expected=[dict(Row=row,Column=col,Total=total,Acoustic=acoustic,Identity=identity,Trait=trait,ScoreBits=score,Cells=reverse)]
  assert actual['Reference78']==total
 else:assert actual['Reference78']==0
 assert actual['Alternatives']==expected,('terminal/path/full40B',index)
 terminals+=len(expected);empty+=not expected
 for a in expected:
  cells+=len(a['Cells'])
  for c in a['Cells']:
   co=c['Coordinates'];blank+=co[4]==217;closed+=co[4]==211;boundaries+=co[6]!=0
assert(terminals,cells,blank,closed,boundaries,empty)==(capture['Terminals'],capture['Cells'],capture['Blank'],capture['Closed'],capture['Boundaries'],capture['Empty'])==(124,306,0,1,152,0)
assert capture['Guards']==373
print(f'PASS raw217AL/BA5/119 source ownership +124 actual B8 winners/{cells} full40B path cells/{fullBytes} B8bytes/closed{closed}/373guards; no extra NN. Rich273/WORD/text remain unjoined.')
