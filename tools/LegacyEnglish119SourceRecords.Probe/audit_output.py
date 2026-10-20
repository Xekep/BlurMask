"""Independent raw EN AL/category/source-key/pow/full720 replay.
Uses stdlib and tracked independent scalar definitions, no research or runtime.
"""
import ast,base64,hashlib,json,math,struct,sys
from pathlib import Path
if len(sys.argv)!=5:raise SystemExit('models-directory audited-controller.json audited-plain-text.json capture.json')
repo=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);controller,plain,out=[json.loads(Path(p).read_bytes())for p in sys.argv[2:]]
def definitions(path,names=None):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))and(names is None or isinstance(n,(ast.FunctionDef,ast.ClassDef))and n.name in names)]
 g={'__file__':str(path),'struct':struct,'math':math};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def put(b,a,fmt,x):struct.pack_into('<'+fmt,b,a,x)
def signed(x,bits):return (x^(1<<(bits-1)))-(1<<(bits-1))
def decode(r):return signed(r&0xffffff,24),signed((r>>24)&65535,16),signed((r>>40)&65535,16)
def norm(r):return r&0x10000ffffffffff
def bits(x):return struct.unpack('<I',struct.pack('<f',x))[0]
def increment(x):return struct.unpack('<f',struct.pack('<I',bits(x)+(1 if x>=0 else -1)))[0]if x!=0 else struct.unpack('<f',b'\x01\0\0\0')[0]
data=(models/out['Path']).read_bytes();assert out['Path']==plain['Path']==controller['Path']=='en-US/en_US-ak-cur.lite.res'
manifest={x['path']:x['sha256']for x in json.loads((models/'manifest.json').read_bytes())['resources']}
assert hashlib.sha256(data).hexdigest()==manifest[out['Path']]=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
assert out['Fingerprint'].lower()==plain['Fingerprint'].lower()==hashlib.sha256(data[8843:182187]).hexdigest()
assert out['Exponent']==struct.unpack_from('<f',data,182187)[0]==.75
header,labels=definitions(repo/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',{'labels'})['labels'](data[8843:182187]);assert header[-1]==217
wire=definitions(repo/'tools/LegacySource247Wire.Probe/audit_output.py');sampler,error=wire['sampler'](data,dict(Offset=16952,End=18340,Version=0x108),217);assert error is None
a=sampler['categoryOffset'];assert a==17017 and struct.unpack_from('<HH',data,a)==(0x201,217);a+=4;entries=220
def ids():
 global a
 n=data[a];a=(a+4)&~3;v=struct.unpack_from('<'+'i'*n,data,a);a+=4*n;return v
first=ids();firstmap=struct.unpack_from('<220b',data,a);a+=entries;geometry=ids();start=struct.unpack_from('<220b',data,a);a+=entries;end=struct.unpack_from('<220b',data,a);a+=entries
protos=[]
for _ in geometry:protos.append(struct.unpack_from('<fffB',data,a));a+=13
seed_oracle=definitions(repo/'tools/LegacyEnglish119Candidates.Probe/audit_output.py',{'seed_order'});seed_oracle['f']=f;seed_order=seed_oracle['seed_order']
actual_count=rows=multi=delimiters=changed=duplicates=near=visits=geometry_count=0
assert len(out['Actual'])==132 and len(controller['Rows'])==len(plain['Actual'])==124
for ci,case in enumerate(out['Actual']):
 numeric=lambda strokes:[[[f(x)for x in p]for p in s]for s in strokes]
 raw=numeric(case['Raw']);case['Cut']=numeric(case['Cut']);assert ci==case['Index']and case['ScoredBefore']==case['ScoredAfter']
 if ci<124:
  assert case['Name']==f'actual-{ci}'and raw==numeric(controller['Rows'][ci]['Raw']);actual_count+=1
  expected=plain['Actual'][ci]['Variants'][0]
  assert case['Words']==[w['References']for w in expected['Words']]and case['Owners']==expected['Owners']
 else:
  vi,source=divmod(ci-124,4);original=numeric(controller['Rows'][source]['Raw']);assert case['Name']==('duplicate-'if vi==0 else'near-duplicate-')+str(source)
  assert len(raw)==len(original)
  for p,q in zip(raw,original):assert p[:-1]==q and p[-1]==(q[-1]if vi==0 else[increment(q[-1][0]),q[-1][1]])
  duplicates+=vi==0;near+=vi==1
 original=[];keys=[];ends={};keyindices={}
 for s,stroke in enumerate(raw):
  retained=[i for i,p in enumerate(stroke)if i==0 or any(abs(p[j])+abs(stroke[i-1][j])<abs(f(p[j]-stroke[i-1][j]))*2**23 for j in(0,1))]
  original.append([stroke[i]for i in retained])
  for j,i in enumerate(retained):
   key=(0,s,i);keyindices[key]=len(keys);keys.append(key);ends[key]=(0,s,retained[j+1]-1 if j+1<len(retained)else len(stroke)-1)
 assert original==numeric(case['Deduplicated'])
 assert sum(map(len,raw))<=4096
 cutids=[]
 for originalpoint in case['CutSourceIndices']:
  tick=case['Map200'][originalpoint];index,remainder=divmod(tick,200);index+=bool(remainder)
  assert 0<=index<len(keys);cutids.append((keys[index],200-remainder if remainder else 0,index))
 assert len(cutids)==sum(case['CutCounts'])==sum(map(len,case['Cut']))
 starts=[0]
 for count in case['CutCounts']:starts.append(starts[-1]+count)
 order=seed_order(case['Cut'],case['Atoms'],f(case['Height']));seeds=[case['Atoms'][i]for i in order];assert len(seeds)==len(case['Nodes'])
 def source_ranges(node,arc):
  lo,hi,span=case['Nodes'][node][arc];assert lo==(node-span)*128 and hi==node*128+3
  rs=[]
  for s,begin,finish in seeds[lo>>7:(hi>>7)+1]:
   firstid=cutids[starts[s]+begin];laststart=cutids[starts[s]+finish-1];last=(ends[laststart[0]],laststart[1],laststart[2]);rs.append([firstid,laststart,last,finish==case['CutCounts'][s]])
  rs.sort(key=lambda r:(r[0][0],-r[0][1],r[2][0],-r[2][1]))
  for j in range(len(rs)-1,0,-1):
   current,previous=rs[j],rs[j-1];nextindex=previous[1][2]+1
   equal=current[0][:2]==previous[1][:2];adjacent=previous[3]and nextindex<len(keys)and current[0][0]==keys[nextindex]
   if equal or adjacent:rs[j-1]=[previous[0],*current[1:]];rs.pop(j)
  return [[list(r[0][0]),list((r[2]if r[3]else r[1])[0])]for r in rs]
 owners={n['Key']:base64.b64decode(n['Records'])for n in case['Owners']};expectedrefs=[]
 for word,refs in enumerate(case['Words']):
  for ordinal,ref in enumerate(refs):
   if decode(ref)[1]<0:delimiters+=1
   else:expectedrefs.append((word,ordinal,ref))
 multi+=sum(any(decode(r)[1]>=0 for r in refs)for refs in case['Words'])>1
 assert len(expectedrefs)==len(case['Rows'])
 for produced,(word,ordinal,ref)in zip(case['Rows'],expectedrefs):
  assert(produced['Word'],produced['Ordinal'],produced['Reference'])==(word,ordinal,ref)
  node,arc,selected=decode(ref);assert node>=0 and arc>=0 and selected>=0
  ranges=source_ranges(node,arc);assert ranges==produced['ChildRanges']==produced['Ranges']
  points=[]
  for firstkey,lastkey in ranges:
   assert firstkey[0]==lastkey[0]==0
   for s in range(firstkey[1],lastkey[1]+1):
    lo=firstkey[2]if s==firstkey[1]else 0;hi=lastkey[2]+1 if s==lastkey[1]else len(raw[s]);points.extend(raw[s][lo:hi]);visits+=hi-lo
  alternatives=owners[norm(ref)];assert len(alternatives)%40==0;selectedrow=alternatives[selected*40:(selected+1)*40];assert len(selectedrow)==40
  chosen=struct.unpack_from('<H',selectedrow,8)[0];assert chosen in set(labels.values())|set(range(header[-1],header[-1]+3))
  labels_seen=[chosen];ordered=[selectedrow]
  for off in range(0,len(alternatives),40):
   row=alternatives[off:off+40];label=struct.unpack_from('<H',row,8)[0]
   if label not in labels_seen and len(ordered)<20:labels_seen.append(label);ordered.append(row)
  def output(exponent):
   b=bytearray(720);put(b,8,'q',len(ranges));put(b,16,'i',-1)
   for off,value in zip((24,28,32,36),(min(p[0]for p in points),max(p[0]for p in points),min(p[1]for p in points),max(p[1]for p in points))):put(b,off,'f',value)
   for i,row in enumerate(ordered):
    label=labels_seen[i];put(b,40+i*2,'H',label)
    if i==0:put(b,80,'f',math.pow(struct.unpack_from('<f',row,12)[0],exponent))
    else:b[80+i*8:84+i*8]=row[12:16]
    b[84+i*8:88+i*8]=row[16:20];b[240+i*4:244+i*4]=row[20:24]
    if start[label]>=0:
     proto=protos[start[label]];lower=f(-1-f(proto[0]*2));upper=f(f(proto[1]*2)+lower)
     for channel in range(1 if end[label]<0 else 2):put(b,560+i*8+channel*4,'f',lower);put(b,400+i*8+channel*4,'f',upper)
     put(b,320+i*4,'i',1 if end[label]<0 else 2)
   for i in range(len(ordered),20):put(b,40+i*2,'H',65535);put(b,240+i*4,'i',0x4000000)
   return b
  expected=output(.75);control=output(1);observed=base64.b64decode(produced['Storage']);assert observed==expected,(ci,word,ordinal,[(i,a,b)for i,(a,b)in enumerate(zip(observed,expected))if a!=b][:12])
  assert base64.b64decode(produced['PreparedPureExponentOneControl'])==control and produced['Labels']==len(ordered)
  changed+=control!=expected;geometry_count+=sum(start[l]>=0 and end[l]>=0 for l in labels_seen);rows+=1
assert(actual_count,duplicates,near,rows,multi,delimiters)==(124,4,4,out['Rows'],out['MultiWord'],out['SkippedDelimiters'])
assert rows==313 and multi==16 and delimiters==22 and changed>0 and geometry_count>0
assert out['Sources']==132 and out['Guards']==717 and out['PreparedControls']==dict(ExactRawCap=4096,ExactRangeCap=65536,ExactPointVisitCap=1048576,PreparedEmptyRows=0,ForeignEqualModel=1,MidIngressCancellation=1,PreparedMidSourcePreflightCancellation=1)
print(f'PASS132 genuine raw EN paths/{rows} full720 rows/{rows*720} bytes/{visits} raw point visits/{changed} exponent .75-vs1 differences/{geometry_count} two-channel categories; raw AL/category/chronology/WORD/pool independently joined; no native parent claim')
