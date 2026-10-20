"""Independent EN main wire/2E3 NN/physical maps/child chronology/full720.
Vendor binaries are not loaded; only tracked raw model bytes and numeric oracles.
"""
import ast,base64,hashlib,importlib.util,inspect,json,math,struct,sys
from pathlib import Path
import numpy as np
repo=Path(__file__).resolve().parents[2]
if len(sys.argv)!=3:raise SystemExit('models capture.json')
models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes())
def load(folder):
 spec=importlib.util.spec_from_file_location('enparent_'+folder.replace('.','_'),repo/'tools'/folder/'reference_controls.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
reorder=load('LegacyFreshMainPhysicalReordering.Probe');ref=reorder.ref
networks=[ref.proposal_ref.network.load(models/'line-geometry'/name)for name in ['line-2e3-30.res','line-2e3-50.res']]
def definitions(path,names=None):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))and(names is None or isinstance(n,(ast.FunctionDef,ast.ClassDef))and n.name in names)]
 scope={'__file__':str(path),'struct':struct,'math':math};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),scope);return scope
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
numeric=lambda strokes:[[[f(x)for x in p]for p in s]for s in strokes]
wire=(models/'en-US/en_US-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(wire).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
profile=struct.unpack_from('<ffff',wire,7959);assert profile==tuple(map(f,[.96,.85,3,10]))
assert hashlib.sha256(wire[7983:8118]).hexdigest()=='319b7934037964b875a6f76739f4b336de0d36e2f7f449b991c8088fb3c3e38a'
assert hashlib.sha256(wire[8118:8811]).hexdigest()=='1bf9e83966098a4e611ae2ed6a53a43c30e270b8ced586260ec6c1e0775fd7bd'
header,labels=definitions(repo/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',{'labels'})['labels'](wire[8843:182187]);assert header[-1]==217
sampler=definitions(repo/'tools/LegacySource247Wire.Probe/audit_output.py')['sampler'];sc,error=sampler(wire,dict(Offset=16952,End=18340,Version=0x108),217);assert error is None
a=sc['categoryOffset'];assert a==17017 and struct.unpack_from('<HH',wire,a)==(0x201,217);a+=4
def ids():
 global a
 n=wire[a];a=(a+4)&~3;v=struct.unpack_from('<'+'i'*n,wire,a);a+=4*n;return v
first=ids();a+=220;geometry=ids();start=struct.unpack_from('<220b',wire,a);a+=220;end=struct.unpack_from('<220b',wire,a);a+=220
protos=[]
for _ in geometry:protos.append(struct.unpack_from('<fffB',wire,a));a+=13
seed_scope=definitions(repo/'tools/LegacyEnglish119Candidates.Probe/audit_output.py',{'seed_order'});seed_scope['f']=f;seed_order=seed_scope['seed_order']
def decode(r):
 signed=lambda x,n:(x^(1<<(n-1)))-(1<<(n-1))
 return signed(r&0xffffff,24),signed(r>>24&65535,16),signed(r>>40&65535,16)
def child_resolver(case):
 raw=numeric(case['Raw']);keys=[];ends={};clean=[]
 for s,stroke in enumerate(raw):
  ids=[i for i,p in enumerate(stroke)if i==0 or any(abs(p[j])+abs(stroke[i-1][j])<abs(f(p[j]-stroke[i-1][j]))*2**23 for j in(0,1))]
  clean.append([stroke[i]for i in ids])
  for j,i in enumerate(ids):key=(0,s,i);keys.append(key);ends[key]=(0,s,ids[j+1]-1 if j+1<len(ids)else len(stroke)-1)
 assert clean==numeric(case['Deduplicated'])
 cutids=[]
 for originalpoint in case['CutSourceIndices']:
  index,remainder=divmod(case['Map200'][originalpoint],200);index+=bool(remainder)
  cutids.append((keys[index],200-remainder if remainder else 0,index))
 starts=[0]
 for count in case['CutCounts']:starts.append(starts[-1]+count)
 seeds=[case['Atoms'][i]for i in seed_order(numeric(case['Cut']),case['Atoms'],f(case['Height']))]
 assert len(cutids)==sum(case['CutCounts'])==sum(map(len,case['Cut']))and len(seeds)==len(case['Nodes'])
 def resolve(node,arc):
  lo,hi,span=case['Nodes'][node][arc];assert lo==(node-span)*128 and hi==node*128+3
  rs=[]
  for s,begin,finish in seeds[lo>>7:(hi>>7)+1]:
   firstid=cutids[starts[s]+begin];laststart=cutids[starts[s]+finish-1];last=(ends[laststart[0]],laststart[1],laststart[2]);rs.append([firstid,laststart,last,finish==case['CutCounts'][s]])
  rs.sort(key=lambda r:(r[0][0],-r[0][1],r[2][0],-r[2][1]))
  for j in range(len(rs)-1,0,-1):
   current,previous=rs[j],rs[j-1];nextindex=previous[1][2]+1
   equal=current[0][:2]==previous[1][:2];adjacent=previous[3]and nextindex<len(keys)and current[0][0]==keys[nextindex]
   if equal or adjacent:rs[j-1]=[previous[0],*current[1:]];rs.pop(j)
  return[[list(r[0][0]),list((r[2]if r[3]else r[1])[0])]for r in rs]
 return resolve

def output(ranges,points,alternatives,selected):
 chosen=struct.unpack_from('<H',alternatives,selected*40+8)[0];seen=[chosen];ordered=[alternatives[selected*40:(selected+1)*40]]
 for off in range(0,len(alternatives),40):
  row=alternatives[off:off+40];label=struct.unpack_from('<H',row,8)[0]
  if label not in seen and len(ordered)<20:seen.append(label);ordered.append(row)
 b=bytearray(720)
 def put(o,fmt,v):struct.pack_into('<'+fmt,b,o,v)
 put(8,'q',len(ranges));put(16,'i',-1)
 for off,value in zip((24,28,32,36),(min(p[0]for p in points),max(p[0]for p in points),min(p[1]for p in points),max(p[1]for p in points))):put(off,'f',value)
 for i,row in enumerate(ordered):
  label=seen[i];put(40+i*2,'H',label)
  if i==0:put(80,'f',math.pow(struct.unpack_from('<f',row,12)[0],.75))
  else:b[80+i*8:84+i*8]=row[12:16]
  b[84+i*8:88+i*8]=row[16:20];b[240+i*4:244+i*4]=row[20:24]
  if start[label]>=0:
   proto=protos[start[label]];lower=f(-1-f(proto[0]*2));upper=f(f(proto[1]*2)+lower)
   for channel in range(1 if end[label]<0 else 2):put(560+i*8+channel*4,'f',lower);put(400+i*8+channel*4,'f',upper)
   put(320+i*4,'i',1 if end[label]<0 else 2)
 for i in range(len(ordered),20):put(40+i*2,'H',65535);put(240+i*4,'i',0x4000000)
 return b,len(ordered)

rows=children=multiline=flat=filtered=cropped=permuted=0
for ci,case in enumerate(capture['Actual']):
 raw=numeric(case['Raw']);clean=[ref.source_ref.clean(s)for s in raw]
 assert clean==numeric(case['Retained'])
 samples=[v for stroke in clean for v in ref.source_ref.sample.extract(stroke,False)]
 assert list(map(f,case['Samples']))==list(map(f,samples))
 primary,secondary=ref.source_ref.geometry.scales(samples,0,case['Resolution'],profile)
 assert case['SourcePrimary']==primary and case['SourceSecondary']==secondary
 #The frozen historical reference explicitly used actual96. Generalize ONLY
 #that numeric input for this new EN adapter, keeping all NN kernels unchanged.
 code=inspect.getsource(ref.whole).replace('def whole(raw,half,quarter,q,models):','def whole_actual(raw,half,quarter,q,models):').replace('0,96,','0,actual_resolution,').replace('f(96)/f(25.4)','f(actual_resolution)/f(25.4)')
 scope=dict(ref.__dict__);scope['actual_resolution']=case['Resolution'];exec(code,scope);whole=scope['whole_actual']
 expected=whole(raw,[False]*len(raw),True,np.float32(.8),networks)
 assert f(case['InitialNeuralScale'])==expected['neuralScale'] and [sorted(g)for g in case['Groups']]==expected['groups'],(case['Name'],case['InitialNeuralScale'],expected['neuralScale'],case['Groups'],expected['groups'])
 prepared=dict(strokes=raw,clean=clean,primary=primary,expected=expected)
 split=reorder.split.prepare(prepared);assert case['SplitOrder']==split['order']
 #Reorder's old receiver uses the same input resolution96; the new owner
 #uses actual resolution and carries only NN scale, preserving parentB0.
 def receiver(points,carry,models):
  old=ref.source_ref.geometry.scales;ref.source_ref.geometry.scales=lambda *args:(carry,carry)
  try:return whole(points,[False]*len(points),True,np.float32(.8),models)
  finally:ref.source_ref.geometry.scales=old
 oldreceiver=reorder.receiver;reorder.receiver=receiver
 try:ordered=reorder.prepare(prepared,networks)
 finally:reorder.receiver=oldreceiver
 assert case['Order']==ordered['original']and [x.lower()for x in case['DescriptorsBefore']]==ordered['descriptors']
 calls=[[v['DescriptorOrdinal'],v['StartingNeuralScale'],v['EndingNeuralScale'],v['LocalPhysicalCount'],v['ProviderGroupCount'],v['VerticalStripCount']]for v in case['Calls']]
 assert [[v[0],f(v[1]),f(v[2]),*v[3:]]for v in calls]==ordered['calls'],(case['Name'],calls,ordered['calls'])
 current=[clean[i]for i in case['Order']];assert current==numeric(case['Current'])and [clean[i]for i in case['SplitOrder']]==numeric(case['Split'])
 permuted+=case['Order']!=list(range(len(raw)))
 retainedids=[];keyoffset=[];offset=0
 for s,stroke in enumerate(raw):
  ids=[i for i,p in enumerate(stroke)if i==0 or any(abs(p[j])+abs(stroke[i-1][j])<abs(f(p[j]-stroke[i-1][j]))*2**23 for j in(0,1))];retainedids.append(ids);keyoffset.append(offset);offset+=len(ids)
 for si,old in enumerate(case['Order']):
  for pi,m in enumerate(case['Metadata'][si]):
   ids=retainedids[old];assert m==dict(Key=[0,old,ids[pi]],End=[0,old,ids[pi+1]-1 if pi+1<len(ids)else len(raw[old])-1],MetadataPointIndex=keyoffset[old]+pi,KeyRecordIndex=keyoffset[old]+pi,RemainderByte=0)
 descriptors=[bytearray.fromhex(x)for x in case['DescriptorsBefore']];cursor=0;trace=['enter']
 for child in case['Child']:
  children+=1;d=child['DescriptorOrdinal'];trace+=['child-begin:'+str(d),'interval-begin:'+str(d)];first,last=struct.unpack_from('<ii',descriptors[d],36);mapping=[]
  def isflat(stroke):
   width=f(max(p[0]for p in stroke)-min(p[0]for p in stroke));height=f(max(p[1]for p in stroke)-min(p[1]for p in stroke))
   return float(width)>3*primary and float(f(height/width))<.02
  for i in range(first,last+1):
   if isflat(current[i])and(mapping or i!=last):continue
   mapping.append(i)
  sole=first==last and isflat(current[first]);flat+=sole;filtered+=len(mapping)!=last-first+1
  assert child['Map']==mapping and child['SoleFlat']==sole and numeric(child['Raw'])==[current[i]for i in mapping]
  resolve=child_resolver(child);owners={o['Key']:base64.b64decode(o['Records'])for o in child['Owners']}
  refs=[(wi,ri,r)for wi,word in enumerate(child['Words'])for ri,r in enumerate(word)if decode(r)[1]>=0]
  assert refs
  begin=cursor
  for index,(wi,ri,r)in enumerate(refs):
   row=case['Rows'][cursor];trace.append('row:'+str(cursor));cursor+=1;rows+=1;node,arc,selected=decode(r);cr=resolve(node,arc)
   assert(row['Descriptor'],row['Word'],row['Ordinal'],row['Reference'])==(d,wi,ri,r)and row['ChildRanges']==cr
   projected=[];points=[]
   for a,b in cr:
    run=a[1]
    for s in range(a[1],b[1]+1):
     lo=a[2]if s==a[1]else 0;hi=b[2]+1 if s==b[1]else len(current[mapping[s]]);points+=current[mapping[s]][lo:hi];cropped+=lo!=0 or hi!=len(current[mapping[s]])
     if s==b[1]or mapping[s+1]!=mapping[s]+1:
      projected.append([[a[0],mapping[run],a[2]if run==a[1]else 0],[b[0],mapping[s],b[2]if s==b[1]else len(current[mapping[s]])-1]]);run=s+1
   override=sole and index==len(refs)-1;assert row['Flat']==override
   alternatives=owners[r&0x10000ffffffffff]
   if override:
    scratch=bytearray(40);struct.pack_into('<H',scratch,8,labels['_']);struct.pack_into('<ff',scratch,12,1,1);alternatives=scratch;selected=0
   expected720,labelcount=output(projected,points,alternatives,selected)
   assert row['Ranges']==projected and row['Labels']==labelcount and base64.b64decode(row['Storage'])==expected720,(case['Name'],d,index,row['Ranges'],projected,[(i,a,b)for i,(a,b)in enumerate(zip(base64.b64decode(row['Storage']),expected720))if a!=b][:20])
  struct.pack_into('<ii',descriptors[d],52,begin,cursor);trace.append('child-close:'+str(d))
 assert cursor==len(case['Rows'])and[bytes(d).hex()for d in descriptors]==[d.lower()for d in case['Descriptors']]
 assert case['Trace']==trace+['complete']
 multiline+=len(case['Child'])>1
assert len(capture['Actual'])==137 and not capture['Refused']and rows>300 and children>137 and multiline>=2 and flat>=1 and filtered>=2 and cropped>0 and permuted>0
assert capture['PreparedMidPreflightCancellation']==1
assert capture['PreparedRawCap']==4096 and capture['PreparedZeroChildRefusal']==1
assert capture['PreparedRangeCap']==65536 and capture['PreparedPointVisitCap']==1048576
for failure in capture['PreparedFailures']:
 assert failure['Trace'][-1]=='fault'and failure['Rows']==(0 if failure['Point']=='enter'else 1)
 before=[bytearray.fromhex(d)for d in capture['Actual'][0]['DescriptorsBefore']]
 expected=['enter']
 if failure['Point']!='enter':
  expected+=['child-begin:0','interval-begin:0','row:0'];struct.pack_into('<ii',before[0],52,0,0)
  if failure['Point']=='child-close:0':expected+=['child-close:0'];struct.pack_into('<ii',before[0],52,0,1)
 assert failure['Trace']==expected+['fault']and[d.lower()for d in failure['Descriptors']]==[d.hex()for d in before]
assert len(capture['PreparedFailures'])==3 and capture['Guards']==426
print(f'PASS137 actual EN parents/{children} child119 NN submissions/{rows} full720 rows; independent raw wire/actual2E3 NN/maps/chronology/geometry; multiline{multiline}, filtered{filtered}, soleflat{flat}, permuted{permuted}, cropped{cropped}; prepared limits+3 entered-cancel prefixes, no24B+/main/DOM claim')





