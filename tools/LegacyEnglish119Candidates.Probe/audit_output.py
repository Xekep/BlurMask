"""Independent raw108/cut arithmetic, shift0 candidate membership, pruning,
chronology and all217/199/107 scalar values. No vendor/research imports."""
from pathlib import Path
import ast,base64,hashlib,importlib.util,json,math,struct,sys
sys.dont_write_bytecode=True
if len(sys.argv)!=3:raise SystemExit('models-directory capture.json')
repo=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes())
def definitions(path):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))]
 g={'__file__':str(path)};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
wire=definitions(repo/'tools/LegacySource247Wire.Probe/audit_output.py')
network=definitions(repo/'tools/LegacyPreparedNetwork107.Probe/audit_output.py');network['exp_table']=[int(25*math.exp(-4+22*i/351))for i in range(352)]
featureaudit=definitions(repo/'tools/LegacyEnglishNoInitialGlyph.Probe/audit_output.py');featureaudit.update(capture=capture,network=network)
sort=featureaudit['quicksort'];evaluate=featureaudit['evaluate']
spec=importlib.util.spec_from_file_location('feature_scalar',repo/'tools/LegacyEnglishNoInitialGlyph.Probe/feature_oracle.py');features=importlib.util.module_from_spec(spec);spec.loader.exec_module(features)
source=repo/'tools/LegacyCursiveProducer.Probe/reference_controls.py';text=source.read_text(encoding='utf8');cuts={'__file__':str(source)}
exec(compile(text[:text.index('rng=random.Random')],str(source),'exec'),cuts)
body=text[text.index(' for stroke,raw in enumerate(strokes):'):text.index('\nrng=random.Random')]
exec(compile('def compute(strokes,height,baseline,confidence):\n tags=[1 if i==0 else 0 for s in strokes for i in range(len(s))]\n state=Stage(strokes,tags,0);whole=False;cap=None;previous=f(-3.4028234663852886e38)\n'+body,'independent108cuts','exec'),cuts)
f=cuts['f'];bits=cuts['bits']
featureaudit['f']=f
enclosure=definitions(repo/'tools/LegacySourceEnclosure.Probe/reference_controls.py')['evaluate']
data=(models/capture['Path']).read_bytes();manifest={x['path']:x['sha256']for x in json.loads((models/'manifest.json').read_bytes())['resources']}
assert hashlib.sha256(data).hexdigest()==manifest[capture['Path']]
assert hashlib.sha256(data[capture['SourceOffset']:capture['SourceEnd']]).hexdigest()==capture['Fingerprint'].lower()
def alphabet_tokens(data,at):
 def u(fmt,n=1):
  nonlocal at
  v=struct.unpack_from('<'+fmt*n,data,at);at+=struct.calcsize('<'+fmt)*n;return v
 header=u('H',8);assert header[-1]==217;n=u('H')[0]
 for step in(32,64,1):n=next(x for x in reversed(u('H',n))if x!=65535)+step
 spans=u('H',n+1);pairs=[u('I',2)for _ in range(spans[-1]+1)];assert pairs[-1][0]<=16*1024*1024
 return{data[at+a:at+b]for(a,_),(b,_)in zip(pairs,pairs[1:])}
assert '\U000f0009'.encode()not in alphabet_tokens(data,capture['SourceOffset']+99)
sampler,refusal=wire['sampler'](data,dict(Offset=16952,End=18340,Version=0x108),217);assert refusal is None
fields={x['destination']:base64.b64decode(x['raw'])for x in sampler['fields']};assert fields['134.present']==fields['1D0.present']==b'\0'
assert data[181996]==0 and data[18356:18358]==b'\7\1'and data[18646:18649]==b'\1\0\0'
gap=struct.unpack('<f',fields['68'])[0];slope=struct.unpack('<f',fields['114'])[0];threshold=struct.unpack('<i',fields['11C'])[0]
networks={slot:network['load'](data,p,end)[0]for slot,p,end in network['first_networks'](data)}
table=[f(x)for x in capture['Table']];assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
assert hashlib.sha256(struct.pack('<48i',*capture['HiddenTable'])).hexdigest()=='db88702e24d4ac761fb778dca068606627c3a8b25e1ff614410c963fa627e26f'
assert hashlib.sha256(struct.pack('<128i',*capture['OutputTable'])).hexdigest()=='eae762e8cc8c234d983fdd93e20be8450b6fc91bee828dbb8a247bc17b656cd3'
def compose(rs):
 result=[]
 for s,a,b in sorted(rs):
  if result and result[-1][0]==s and result[-1][2]==a+1:result[-1][2]=b
  else:result.append([s,a,b])
 return result
def seed_order(strokes,atoms,height):
 boxes=[];normal=[];delayed=[];maximum=f(-3.4028234663852886e38)
 for i,(s,a,b)in enumerate(atoms):
  p=strokes[s][a:b];box=[min(p[0]for p in p),max(p[0]for p in p)];boxes.append(box)
  if max(p[0]for p in strokes[s])<f(maximum-f(height*f(.3))):delayed.append(i)
  else:normal.append(i);maximum=max(maximum,box[1])
 slots={};fraction=f(.7)
 def position(i):return f(f(f(1-fraction)*boxes[i][0])+f(fraction*boxes[i][1]))
 for i in delayed:
  x=position(i);intervals=[(-math.inf,boxes[normal[0]][0])]
  for l,r in zip(normal,normal[1:]):
   lo,hi=boxes[l][1],boxes[r][0];intervals.append((lo,hi)if lo<hi else(f(f(lo+hi)*.5),)*2)
  intervals.append((boxes[normal[-1]][1],math.inf));distances=[f(max(lo-x,x-hi,0))for lo,hi in intervals];slots[i]=distances.index(min(distances))
 result=normal[:]
 for i in sorted(delayed,key=lambda i:(-slots[i],-position(i),i)):result.insert(min(slots[i],len(normal)),i)
 return result
def cost(strokes,rs,height):
 boxes=[(min(p[0]for p in strokes[s][a:b]),max(p[0]for p in strokes[s][a:b]))for s,a,b in rs];total=0
 for i in range(1,len(boxes)):
  left,right=boxes[:i],boxes[i:];distance=max(f(min(b[0]for b in right)-max(b[1]for b in left)),f(min(b[0]for b in left)-max(b[1]for b in right)))
  x=f(f(f(distance/height)-gap)*-slope);v=0 if x>4 else table[int(f(f(f(x+4)*255)+.5))>>3]if x>=-4 else x
  scaled=f(f(-v)*8192);total+=int(scaled+.5)
 return min(total,0x4000000)
records=groups=removed=inserted=0;nonidentity=0;positive_probability=0
for case in capture['Cases']:
 ps=[[[f(x)for x in p]for p in s]for s in case['Prepared']];height=f(case['Line']['Height']);baseline=f(case['Line']['Baseline'])
 raw=[[[f(x)for x in p]for p in s]for s in case['Raw']];original=[];rawkeys=[]
 for physical,stroke in enumerate(raw):
  indices=[i for i,p in enumerate(stroke)if i==0 or any(abs(p[j])+abs(stroke[i-1][j])<abs(f(p[j]-stroke[i-1][j]))*2**23 for j in(0,1))]
  original.append([stroke[i]for i in indices]);rawkeys.extend((0,physical,i)for i in indices)
 for i,observed in enumerate(case['Probabilities']):
  assert abs(enclosure(original,i)-observed)<0.000003;positive_probability+=observed>.5
 for tick,identity in zip(case['Map200'],case['InputIdentities']):
  index,remainder=divmod(tick,200);index+=bool(remainder)
  assert tuple(identity['Key'][k]for k in('First','Second','Third'))==rawkeys[index]
  assert identity['KeyRecordIndex']==identity['MetadataPointIndex']==index and identity['RemainderByte']==(200-remainder if remainder else 0)
 expected=cuts['compute'](ps,height,baseline,[f(v)for v in case['Probabilities']]);rows=expected['expected'];assert case['Counts']==expected['counts']
 keys=['X','Y','DeltaX','DeltaY','SegmentLength'];rkeys=['Tag','StrokeIndex','OriginalPointIndex']
 actual=[[bits(r[k])for k in keys]+[r[k]for k in rkeys]for r in case['Records']];assert rows==actual
 records+=len(rows);inserted+=len(rows)-sum(map(len,ps));markers=[[m['Kind'],m['StrokeIndex'],m['PointIndex']]for m in case['Markers']];assert markers==expected['markers']
 counts=case['Counts'];atoms=[]
 for i,(_,s,a)in enumerate(markers):atoms.append([s,a,markers[i+1][2]+1 if i+1<len(markers)and markers[i+1][1]==s else counts[s]])
 assert atoms==[[a['Range'][k]for k in('StrokeIndex','StartPoint','EndPointExclusive')]for a in case['Atoms']]
 for row,identity in zip(rows,case['OutputIdentities']):assert identity==case['InputIdentities'][row[7]]
 nonidentity+=sum(i!=key for i,key in enumerate(case['Map200']))
 cutstrokes=[];at=0
 for n in counts:cutstrokes.append([[f(r['X']),f(r['Y'])]for r in case['Records'][at:at+n]]);at+=n
 order=seed_order(cutstrokes,atoms,height);seeds=[atoms[i]for i in order];allocated=case['AllocatedGroups']
 assert allocated[:len(seeds)]==[[r]for r in seeds]
 expected_candidates={};allgroups={tuple(map(tuple,[r]))for r in seeds}
 for node in range(len(seeds)):
  for span in range(min(7,node+1)):
   rs=compose(seeds[node-span:node+1]);allgroups.add(tuple(map(tuple,rs)))
   if span==0 or cost(cutstrokes,rs,height)<=threshold:expected_candidates[node,span]=rs
   else:removed+=1
 assert {tuple(map(tuple,rs))for rs in allocated}==allgroups
 assert len(case['Nodes'])==len(seeds)
 live=set()
 for node,arcs in enumerate(case['Nodes']):
  assert{a['Span']for a in arcs}=={s for n,s in expected_candidates if n==node}
  for a in arcs:
   span=a['Span'];first=node-span;rs=expected_candidates[node,span];assert allocated[a['GroupId']]==rs;live.add(a['GroupId'])
   assert(a['Start'],a['End'])==(first*128,node*128+3)
   assert a['Previous']==([-1]if first==0 else list(range(len(case['Nodes'][first-1]))))
   nexts=[dict(NodeOffset=n-node,ArcIndex=i)for n in range(node+1,len(seeds))for i,t in enumerate(case['Nodes'][n])if n-t['Span']-1==node]
   assert a['Next']==nexts
 assert sorted(live)==case['LiveGroups']and{g['Id']for g in case['Groups']}==live
 for g in case['Groups']:
  rs=g['Ranges'];assert rs==allocated[g['Id']];keytuples=[tuple(k[n]for n in('First','Second','Third'))for k in g['StartKeys']]
  order=sort(list(range(len(rs))),lambda i:keytuples[i]);assert order==g['Order'];minimum=min(p[0]for s,a,b in rs for p in cutstrokes[s][a:b]);normalized=[]
  for i in order:
   s,a,b=rs[i];normalized.append([[int(min(32767,f(f(f(x-minimum)*100)/height))),int(min(32767,f(f(f(y-baseline)*100)/height)))]for x,y in cutstrokes[s][a:b]])
  assert normalized==g['Normalized'];boundary=[1024 if rs[0][1]==0 else-1024,1024 if rs[-1][2]==counts[rs[-1][0]]else-1024]
  assert boundary==[g['Boundary'][k]for k in('Start','End')]
  assert len(normalized)<=7 and normalized==g['Merged']and g['Retained']==list(range(len(normalized)-1))
  t,r=features.extract(normalized);assert t+boundary==g['Trajectory']and r+boundary==g['Raster']
  for observed,vector in zip(g['Networks'],(g['Trajectory'],g['Raster'])):evaluate(vector,observed,networks[observed['Slot']])
  groups+=1
assert groups==capture['Groups']and removed==capture['Rejected']and inserted==capture['Inserted']
assert len(capture['Cases'])==124 and capture['Guards']>=607 and removed>0 and nonidentity>0 and positive_probability>0
print(f'PASS independent raw108/full191950/{records} records/shift0 membership/prune{removed}/chronology/allfeatures+107 for{groups} livegroups;{inserted} genuine insertions (not a required positive).')
