"""Independent full-byte rich first pool/WORD replay from paired audited inputs.
Only stdlib/raw resource bytes; no research, vendor or managed runtime imports.
The primary/context/cache/confidence inputs have their own independent audits.
"""
import ast,base64,hashlib,json,struct,sys
from pathlib import Path
if len(sys.argv)!=8:raise SystemExit('models-directory audited-candidates.json audited-controller.json audited-confidence.json audited-primary.json audited-terminal.json plain-text.json')
repo=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);candidates,costs,confidence,primary,terminals,out=[json.loads(Path(p).read_bytes())for p in sys.argv[2:]]
def definitions(path,names=None):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))and(names is None or isinstance(n,(ast.Import,ast.ImportFrom))or n.name in names)]
 g={'__file__':str(path)};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def u(b,a):return struct.unpack_from('<H',b,a)[0]
def i(b,a):return struct.unpack_from('<i',b,a)[0]
def q(b,a):return struct.unpack_from('<Q',b,a)[0]
def put(b,a,fmt,x):struct.pack_into('<'+fmt,b,a,x)
def sign(x,n):return (x^(1<<(n-1)))-(1<<(n-1))
def key(r):return r&0x10000ffffffffff
def decode(r):return sign(r&0xffffff,24),sign((r>>24)&65535,16)
def canonical(node,arc,bit=0):return (node&0xffffff)|((arc&65535)<<24)|(bit<<56)
def raw(b):return base64.b64decode(b)
d=(models/out['Path']).read_bytes();manifest={r['path']:r['sha256']for r in json.loads((models/'manifest.json').read_bytes())['resources']}
assert out['Path']==costs['Path']==confidence['Path']==primary['Path']==terminals['Path']==candidates['Path']=='en-US/en_US-ak-cur.lite.res'
assert hashlib.sha256(d).hexdigest()==manifest[out['Path']]
assert all(r['Fingerprint'].lower()==hashlib.sha256(d[8843:182187]).hexdigest()for r in(out,costs,confidence,primary,terminals,candidates))
header,labels=definitions(repo/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',{'labels'})['labels'](d[8843:182187]);assert header==[181,178,65535,180,184,182,183,217]
wire=definitions(repo/'tools/LegacySource247Wire.Probe/audit_output.py');sampler,error=wire['sampler'](d,dict(Offset=16952,End=18340,Version=0x108),217);assert error is None
a=sampler['categoryOffset']+4;n=d[a];a=(a+4)&~3;first=struct.unpack_from('<'+'i'*n,d,a);a+=4*n
firstmap=struct.unpack_from('<220b',d,a);a+=220;n=d[a];a=(a+4)&~3;a+=4*n
start=struct.unpack_from('<220b',d,a);a+=220;end=struct.unpack_from('<220b',d,a)
composites=[];mapped=[]
for x,y,z in zip(firstmap,start,end):
 row=[]
 for one,two in((x,y),(x,z),(first.index(0),y),(first.index(0),z)):
  if one<0 or two<0:row.append(-1);continue
  pair=first[one],two
  if pair not in composites:composites.append(pair)
  row.append(composites.index(pair))
 mapped.append(row[0])
assert first==(0,2,3,1,4,6,5)and n==24 and len(composites)==42
policies=[0,1,4,2,4,1,0,2,2,2,2,2,0,0,1,1,2,1,2,0,0,3,1,2,2,2,0,0,1,1,2,1,2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0]
assert len(out['Actual'])==len(costs['Rows'])==len(confidence['Rows'])==len(primary['Actual'])==len(terminals['Actual'])==124
counts=dict(words=0,delimiter=0,hidden=0,literal=0,multiple=0,records=0,bytes=0,preserved=0)
for ri,(sample,cost,conf,pri,term,cut)in enumerate(zip(out['Actual'],costs['Rows'],confidence['Rows'],primary['Actual'],terminals['Actual'],candidates['Cases'])):
 assert sample['Index']==ri and sample['ScoredBefore']==sample['ScoredAfter']==term['ScoredBefore']==term['ScoredAfter']
 trace=term['Alternatives'][0]['Cells'];assert sample['Trace']==trace and len(term['Alternatives'])==1
 assert cost['Raw']==conf['Raw']==pri['Raw']==term['Raw']==cut['Raw']and f(sample['Height'])==f(cost['Height'])
 groups={g['Id']:g for g in cost['Groups']};vectors={g['Id']:g for g in conf['Groups']};arcs=term['Arcs']
 assert [[ [a['GroupId'],a['Start'],a['End'],a['Span']]for a in node]for node in cut['Nodes']]==arcs
 for gid,g in groups.items():assert [[r['StrokeIndex'],r['StartPoint'],r['EndPointExclusive']]for r in g['Ranges']]==cut['AllocatedGroups'][gid]
 def firstlast(c):return arcs[c[0]][c[3]][1:3]
 def group(c):return groups[arcs[c[0]][c[3]][0]]
 def bbox(c):
  points=[p for r in group(c)['Ranges']for p in cost['Cut'][r['StrokeIndex']][r['StartPoint']:r['EndPointExclusive']]]
  return min(p[0]for p in points),max(p[0]for p in points)
 def delimiter(previous,current):
  prior,symbol=previous[4],current[4];pid=first[firstmap[prior]];cid=first[firstmap[symbol]];policy=policies[cid+pid*7]
  dash=labels.get('-',65535);slash=labels.get('/',65535)
  if symbol in(labels.get('#',65535),labels.get('@',65535),dash,slash)or prior in(dash,slash):policy=5
  elif policy==4:policy=2
  if prior==dash and firstlast(previous)[0]==0:return 1
  if policy==3:policy=2 if firstlast(previous)[0]!=0 else 1
  left,right=bbox(previous),bbox(current);gap=f(left[0]-right[1])if right[0]<=left[0]else f(right[0]-left[1]);height=f(sample['Height'])
  ordinary=f(height*f(2.95));punct=f(height*f(.29));threshold=f(f(punct+ordinary)*f(.5))
  emit=ordinary<gap if policy==2 else punct<=gap if policy==1 else threshold<gap if policy==5 else True
  return int(emit or cut['Nodes'][current[0]][current[3]]['Flags']&8)
 for variant in sample['Variants']:
  wordseed=raw(variant['WordAllocation']);glyphseed=raw(variant['GlyphAllocation']);assert len(wordseed)==120 and len(glyphseed)==40
  assert hashlib.sha256(wordseed+glyphseed).hexdigest()==variant['AllocationFingerprint'].lower()
  if variant['Residue']==0:assert not any(wordseed+glyphseed)
  else:
   want=bytearray((j*37+11)&255 for j in range(120))
   for off,size in((32,8),(88,8),(104,8),(96,4),(112,4)):want[off:off+size]=bytes(size)
   assert wordseed==want and glyphseed==bytes((j*59+23)&255 for j in range(40))
  nodes={};duplicates=set();ownerScores={};allocated=[];detached=[];published=[];spanHeads={}
  def record(ref):return nodes[key(ref)][(ref>>40)&65535]
  def append(k,descriptor,force):
   rows=nodes.setdefault(k,[])
   if not force and k not in duplicates:
    for ordinal,b in enumerate(rows):
     if i(b,0)==descriptor:return k|(ordinal<<40)
   if force:duplicates.add(k)
   ref=k|(len(rows)<<40);b=bytearray(glyphseed);put(b,0,'i',descriptor);put(b,4,'H',u(b,4)&0xfff0);put(b,6,'I',0xffff0000);put(b,12,'f',1.);put(b,16,'f',-1.);put(b,20,'i',0);b[24:32]=bytes(8);b[32:40]=b'\xff'*8;rows.append(b);return ref
  def empty():
   b=bytearray(wordseed)
   for off,fmt,v in((8,'Q',2**64-1),(16,'f',1.),(20,'f',1.),(24,'i',0),(28,'i',0),(40,'i',0),(48,'i',65534),(53,'H',0),(60,'i',-1),(84,'i',-1),(96,'i',0),(112,'i',0)):put(b,off,fmt,v)
   b[55]=0;return b,[]
  word,chain=empty()
  def add(k,descriptor,force):
   ref=append(k,descriptor,force)
   if not chain:put(word,8,'Q',ref)
   chain.append(ref);return ref
  def addmacro(k,count,force):
   ref=add(k,-2,force);b=record(ref);put(b,4,'H',((u(b,4)&0xfff8)|1)&0xfff7);put(b,6,'H',u(b,6)|64);put(b,8,'H',6 if count else 1);put(b,12,'f',1.);put(b,16,'f',1.);ownerScores[k]=f(1);return ref
  def publish(isDelimiter,total,acoustic,span,lastOverride):
   if not chain:return
   b=bytearray(word);head=chain[0];lastnon=next((r for r in reversed(chain)if decode(r)[1]>=0),head)
   ln,la=decode(lastnon);rn,ra=decode(head)
   fst=arcs[ln][la][1]if la>=0 else ln+la;lst=arcs[rn][ra][2]if ra>=0 else rn
   if la<0 and key(lastnon)==key(head):fst,lst=lst,fst
   if lastOverride!=-1:lst=lastOverride
   heads=spanHeads.setdefault((fst,lst),[]);ordinal=heads.index(head)if head in heads else len(heads)
   if head not in heads:heads.append(head)
   capacity=0
   for length in range(1,len(chain)+1):
    if length>capacity:capacity=max(length,int(capacity*1.5))
   put(b,32,'Q',0);put(b,40,'i',len(chain));put(b,44,'i',capacity)
   flags=u(b,50)
   for ref in chain:flags|=u(record(ref),6)
   put(b,50,'H',flags&0xfbff)
   published.insert(0,dict(Delimiter=isDelimiter,Total=total,Acoustic=acoustic,NodeSpan=span,First=fst,Last=lst,Ordinal=ordinal,Storage=bytes(b),References=list(reversed(chain))))
  last=trace[-1]['Coordinates'];fst,lst=firstlast(last);detached.append(addmacro(canonical(lst,fst-lst),0,True));word,chain=empty()
  endtotal=term['Alternatives'][0]['Total'];endacoustic=term['Alternatives'][0]['Acoustic'];endnode=len(arcs)-1;wordEnd=-1;previous=None
  for ti in reversed(range(len(trace))):
   c=trace[ti]['Coordinates'];node,row,column,arc,symbol=c[:5]
   if wordEnd==-1:wordEnd=firstlast(c)[1]
   if symbol in(211,217):continue
   p=raw(pri['Primary'][node]['Bytes'])[row*40:(row+1)*40];assert len(p)==40
   cat=mapped[symbol];prototype=0 if cat<0 else composites[cat][1];assert 0<=prototype<24
   ref=add(canonical(node,arc),row,False)
   if previous is not None:put(record(previous),32,'Q',ref)
   previous=ref;b=record(ref);put(b,4,'H',(u(b,4)&0xfff0)|(0 if symbol==header[3]or cat==((u(p,14)>>4)&63)else 8));put(b,8,'H',symbol);put(b,6,'H',u(b,6)|64|(8 if symbol==209 else 0));put(b,12,'f',f(p[8]/f(255)))
   g=group(c);vg=vectors[g['Id']];byLabel={v['Symbol']:v['Value']for v in vg['Values']};put(b,16,'f',byLabel.get(symbol,0.));ownerScores[key(ref)]=f(vg['Values'][0]['Value'])if vg['Values']else f(0)
   entries=g['Candidates'];position=next((j for j,v in enumerate(entries)if v['Symbol']==symbol),len(entries));costValue=entries[position]['Cost']if position<min(20,len(entries))else 0x4000000
   if costValue<0x4000000 and symbol not in{179,182,183,184,189,190,211}:costValue+=g['GeometryCost']
   put(b,20,'i',costValue);allocated.append(ref)
   if ti>0 and not c[6]:continue
   prior=trace[ti-1]if ti else None;priornode=prior['Coordinates'][0]if prior else -1;priorcost=i(raw(prior['Bytes']),0)if prior else 0;prioracoustic=i(raw(prior['Bytes']),4)if prior else 0
   delta=sign((endtotal-priorcost)&0xffffffff,32);acousticDelta=sign((endacoustic-prioracoustic)&0xffffffff,32);span=endnode-priornode
   for off,value in((16,f(f(-5*f(delta))*f(.00012207031))),(20,1.),(24,f(f(delta)/f(span*8192))),(28,f(f(acousticDelta)*f(.00012207031)))):put(word,off,'f',value)
   publish(False,delta,acousticDelta,span,wordEnd);word,chain=empty();previous=None
   if ti==0:
    fst,lst=firstlast(c);detached.append(addmacro(canonical(lst,fst-lst,1),0,False))
   else:
    pc=prior['Coordinates'];count=delimiter(pc,c);k=canonical(firstlast(c)[0],firstlast(pc)[1]-firstlast(c)[0]);ref=addmacro(k,count,True)
    if count:publish(True,0,0,0,-1)
    else:detached.append(ref)
    word,chain=empty();endtotal=priorcost;endacoustic=prioracoustic;endnode=priornode;wordEnd=-1
  actualWords=variant['Words'];assert len(actualWords)==len(published),(ri,variant['Residue'],'word count')
  for actual,expected in zip(actualWords,published):
   for field,value in expected.items():assert(raw(actual[field])if field=='Storage'else actual[field])==value,(ri,variant['Residue'],'word',field,actual,expected)
   counts['bytes']+=120;counts['preserved']+=sum(x!=0 for x in wordseed)
  assert variant['Allocated']==list(reversed(allocated))and variant['Detached']==detached
  assert variant['OwnerScores']==[[k,sign(struct.unpack('<I',struct.pack('<f',v))[0],32)]for k,v in ownerScores.items()]
  minimum=min([1.]+[ownerScores[key(r)]for word in published for r in word['References']]);assert f(variant['MinimumScore'])==f(minimum)
  assert len(nodes)==len(variant['Owners'])
  for actual,(k,rows)in zip(variant['Owners'],nodes.items()):
   assert(actual['Key'],actual['PackedCount'],raw(actual['Records']))==(k,(len(rows)<<2)|(0x8000 if k in duplicates else 0),b''.join(rows)),(ri,variant['Residue'],'pool',k)
   counts['records']+=len(rows);counts['bytes']+=len(rows)*40
  if variant['Residue']==0:
   counts['words']+=sum(not word['Delimiter']for word in published);counts['delimiter']+=sum(word['Delimiter']for word in published);counts['multiple']+=sum(not word['Delimiter']for word in published)>1
   counts['hidden']+=sum(c['Coordinates'][4]in(211,217)for c in trace);counts['literal']+=sum(c['Coordinates'][4]==190 for c in trace)
assert(counts['words'],counts['delimiter'],counts['hidden'],counts['literal'],out['Guards'])==(out['Words'],out['Delimiters'],out['Hidden'],out['LiteralF0003'],760)==(152,22,1,4,760)
assert counts['multiple']==out['Multiple'] and counts['preserved']>0
assert len(out['PreparedFailurePrefixes'])==3
for phase,failed in enumerate(out['PreparedFailurePrefixes']):
 assert failed['Phase']==phase
 success=out['Actual'][0]['Variants'][0]['Owners'];assert len(success)==3 and len(failed['Owners'])==2
 for at,(actual,complete)in enumerate(zip(failed['Owners'],success[:2])):
  want=bytearray(raw(complete['Records']))
  if at==1:put(want,12,'f',1.);put(want,16,'f',-1.);put(want,20,'i',0)
  assert actual['Key']==complete['Key']and actual['PackedCount']==complete['PackedCount']and raw(actual['Records'])==want
print('PASS raw217AL/24prototype/42composite + audited124same-source primary/B8/cache/confidence: two allocator patterns, complete pool40/WORD120 bytes, owner/header/link/descriptor/dedup/scalars/real1A0/deltas; '+str(counts)+'. No TOP18/UTF8/accepted text claim.')
