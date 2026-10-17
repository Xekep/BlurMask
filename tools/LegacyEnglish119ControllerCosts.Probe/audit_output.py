"""Independent raw-wire + scalar controller-cost audit; stdlib only.
The paired cache capture was independently audited by its own full NN/cache
oracle. Prepared requests below do not establish native scheduler reachability.
"""
import ast,base64,hashlib,json,math,struct,sys
from pathlib import Path
if len(sys.argv)!=5:raise SystemExit('models-directory audited-candidates.json audited-cache.json controller.json')
root=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);sources=json.loads(Path(sys.argv[2]).read_bytes());cache=json.loads(Path(sys.argv[3]).read_bytes());out=json.loads(Path(sys.argv[4]).read_bytes())
def definitions(path,names=None):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))and(names is None or isinstance(n,(ast.Import,ast.ImportFrom))or n.name in names)]
 g={'__file__':str(path)};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.pack('<f',x)
def weights(costs,scale):
 if len(costs)==1:return [1.]
 values=[];total=0.
 for cost in costs[:20]:
  value=f(math.exp(f(-f(cost)/f(scale*8192))));values.append(value);total=f(total+value)
 return [f(v/total)for v in values]+[0.] * max(0,len(costs)-20)
d=(models/out['Path']).read_bytes();manifest={x['path']:x['sha256']for x in json.loads((models/'manifest.json').read_bytes())['resources']}
assert out['Path']==cache['Path']=='en-US/en_US-ak-cur.lite.res'and hashlib.sha256(d).hexdigest()==manifest[out['Path']]
assert out['Fingerprint'].lower()==cache['Fingerprint'].lower()==sources['Fingerprint'].lower()==hashlib.sha256(d[8843:182187]).hexdigest()
header,labels=definitions(root/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',{'labels'})['labels'](d[8843:182187])
assert header==[181,178,65535,180,184,182,183,217]
wire=definitions(root/'tools/LegacySource247Wire.Probe/audit_output.py');sampler,error=wire['sampler'](d,dict(Offset=16952,End=18340,Version=0x108),217);assert error is None
fields={v['destination']:base64.b64decode(v['raw'])for v in sampler['fields']}
gap=struct.unpack('<f',fields['68'])[0];slope=struct.unpack('<f',fields['108'])[0];cap=struct.unpack('<i',fields['118'])[0]
assert(gap,slope,cap)==(f(.2),3.,0x4000000)and struct.unpack('<f',fields['114'])[0]==1
at=sampler['categoryOffset']+4;firstCount=d[at];at=(at+4)&~3;first=list(struct.unpack_from('<'+'i'*firstCount,d,at));at+=firstCount*4
firstMap=list(struct.unpack_from('<220b',d,at));at+=220;prototypeCount=d[at];at=(at+4)&~3;at+=prototypeCount*4
startMap=list(struct.unpack_from('<220b',d,at));at+=220;endMap=list(struct.unpack_from('<220b',d,at))
assert(first,prototypeCount)==([0,2,3,1,4,6,5],24)and(firstMap[217],startMap[217],endMap[217])==(5,7,-1)
assert sampler['categoryCompositeCount']==42 and first[firstMap[217]]==6 and firstMap[218:]==[6,6]
composites=[];starts=[]
for a,b,c in zip(firstMap,startMap,endMap):
 mapped=[]
 for firstIndex,geometryIndex in((a,b),(a,c),(first.index(0),b),(first.index(0),c)):
  if firstIndex==-1 or geometryIndex==-1:mapped.append(-1);continue
  pair=(first[firstIndex],geometryIndex)
  if pair not in composites:composites.append(pair)
  mapped.append(composites.index(pair))
 starts.append(mapped[0])
assert starts[217]==40 and composites[40]==(6,7)and starts[218:]==[41,41]and composites[41]==(5,7)
assert struct.unpack_from('<f',d,181975)==(0.,) #Actual119 power100 -> group scale1.
table=[f(x)for x in json.loads((models/'ru-RU/legacy-separation-profile.json').read_bytes())['values']]
assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
roundFloat=definitions(root/'tools/LegacyCursiveSeparation.Probe/reference_controls.py',{'native_round_float'})['native_round_float']
def geometry(points,ranges,height):
 chunks=[points[r['StrokeIndex']][r['StartPoint']:r['EndPointExclusive']]for r in ranges];total=0
 for b in range(1,len(chunks)):
  left=[f(p[0])for s in chunks[:b]for p in s];right=[f(p[0])for s in chunks[b:]for p in s]
  distance=max(f(min(right)-max(left)),f(min(left)-max(right)))
  x=f(f(f(distance/height)-gap)*f(-slope));v=0 if x>4 else table[int(f(x+4)*255+.5)>>3]if x>=-4 else x
  scaled=f(-v*8192);cost=int(roundFloat(scaled))if len(chunks)<9 else int(f(scaled+(.5 if scaled>0 else -.5)))
  total=total+cost if len(chunks)>=9 or f(f(total)+f(cost))<0x4000000 else 0x4000000
 return min(total,cap)
bypass={labels[t]for t in('\U000f0001','\U000f0002','\U000f0003','\U000f0004','\U000f0008')if t in labels}|set(header[4:7]);bypass.discard(65535)
assert bypass=={179,182,183,184,189,190,211}
mapping={labels[t]:178 for t in('-','_','\u00ad','\U000f0002','\U000f0003')};mapping[218]=74;mapping[219]=69
enclosure=definitions(root/'tools/LegacySourceEnclosure.Probe/reference_controls.py')['evaluate']
def calibrated(p):return f(1/(1+math.exp(-f(p*f(1/32768))*.575)))
groups=queries=positive=distinct=0
assert len(out['Rows'])==len(cache['Rows'])==len(sources['Cases'])==124
for ri,(row,reference,source)in enumerate(zip(out['Rows'],cache['Rows'],sources['Cases'])):
 assert row['Raw']==reference['Raw']and(f(row['Gap']),f(row['Slope']),row['Cap'])==(gap,slope,cap)
 assert row['Raw']==source['Raw']and f(row['Height'])==f(source['Line']['Height'])
 records=[(bits(p[0]),bits(p[1]))for s in row['Cut']for p in s]
 assert records==[(bits(p['X']),bits(p['Y']))for p in source['Records']]and list(map(len,row['Cut']))==source['Counts']
 original=[[p for i,p in enumerate(s)if i==0 or any(abs(p[j])+abs(s[i-1][j])<abs(f(p[j]-s[i-1][j]))*2**23 for j in(0,1))]for s in row['Raw']]
 assert len(row['Groups'])==len(reference['Groups'])
 for g,ref in zip(row['Groups'],reference['Groups']):
  assert g['Id']==ref['Id']and g['Ranges']==ref['Ranges']and g['FinalCosts']==ref['Costs']and g['Candidates']==ref['Candidates']and g['Fallback']==ref['Fallback']
  expectedGeometry=geometry(row['Cut'],g['Ranges'],f(row['Height']));assert expectedGeometry==g['GeometryCost'],('geometry',ri,g['Id'],expectedGeometry,g['GeometryCost'])
  positive+=expectedGeometry>0
  entries=g['Candidates'];w=weights([e['Cost']for e in entries],1);bySymbol={e['Symbol']:i for i,e in enumerate(entries)}
  assert [(x['Symbol'],x['Cost'])for x in g['Entries']]==[(x['Symbol'],x['Cost'])for x in entries]
  assert all(bits(x['NormalizedLikelihood'])==bits(v)for x,v in zip(g['Entries'],w))
  def request(symbol,ordinal,restrict=False,primitive=False):
   if primitive:cost=40000
   elif ordinal==65535 and symbol==217:cost=g['Fallback']
   else:
    ordinal=bySymbol.get(symbol,len(entries))if ordinal==65535 else ordinal
    cost=0x4000000 if ordinal>=len(entries)or restrict and ordinal>=20 else entries[ordinal]['Cost']
   return cost if cost>=0x4000000 or symbol in bypass else cost+expectedGeometry
  for symbol in range(220):
   for key,restricted,primitive in [('Lookup',False,False),('Top',True,False),('Primitive',False,True)]:assert g[key][symbol]==request(symbol,65535,restricted,primitive),(ri,g['Id'],key,symbol)
   weight=w[bySymbol[symbol]]if symbol in bySymbol else 0.;assert bits(g['Weights'][symbol])==bits(weight)
   assert base64.b64decode(g['Bytes'])[symbol]==max(1,min(255,int(f(weight*255))))
   confidence=0.
   if symbol in bySymbol and bySymbol[symbol]<20:
    if symbol==211:confidence=enclosure(original,g['Ranges'][0]['StrokeIndex'])
    else:
     output=mapping.get(symbol,symbol);assert output<182
     confidence=f(f(calibrated(ref['Raster'][output])+calibrated(ref['Trajectory'][output]))/2)
   distinct+=bits(weight)!=bits(confidence);queries+=3
  for q in g['Queries']:assert q['Cost']==request(q['Symbol'],q['Ordinal'],q['Restrict'],q['Primitive']);queries+=1
  groups+=1
for row in out['Prepared']:
 values=weights(row['Costs'],row['Scale']);assert all(bits(e['NormalizedLikelihood'])==bits(v)for e,v in zip(row['Entries'],values))
 assert list(base64.b64decode(row['Bytes']))==[max(1,min(255,int(f(v*255))))for v in values]
assert len(out['GeometryBoundaries'])==233
for row in out['GeometryBoundaries']:
 x=f(row['Offset']);native=int(f(x+4)*255+.5)>>3;wrong=int(f(f(f(x+4)*255)+.5))>>3
 assert native!=wrong and row['Cost']==int(roundFloat(f(-table[native]*8192)))and row['Cost']!=int(roundFloat(f(-table[wrong]*8192)))
assert len(out['Prepared'])==512 and groups==out['Groups']==3241 and positive==out['PositiveGeometry']==154 and distinct==out['DistinctChannels']==32152 and out['Guards']==23434
print(f'PASS raw217AL/182NN/24prototype/42composite/blank217 firstmap5->firstvalue6; {groups} owned groups/{positive}positive193860/{queries} actual-backed explicit requests;512prepared numerical groups/233 DOUBLE bucket discriminators/{distinct}weight-vs-confidence discriminators/{out["Guards"]}guards. No scheduler/winner claim.')
