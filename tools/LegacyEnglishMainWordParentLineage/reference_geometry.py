"""Independent raw-point group193860 geometry; no target labels or native execution."""
import argparse,json,hashlib,struct,ast
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--acoustic',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
def defs(path,name):
 tree=ast.parse(path.read_text());nodes=[x for x in tree.body if isinstance(x,ast.FunctionDef)and x.name==name];assert len(nodes)==1;scope={'struct':struct,'f32':f};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),scope);return scope[name]
rnd=defs(a.repo/'tools/LegacyCursiveSeparation.Probe/reference_controls.py','native_round_float')
table=[f(x)for x in json.loads((a.models/'ru-RU/legacy-separation-profile.json').read_bytes())['values']];assert hashlib.sha256(struct.pack('<256f',*table)).hexdigest()=='b23ed52a3a10808eab597a8f371a7bbb66bde2fab5fe2666431b1b192abc1baf'
raw=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();assert hashlib.sha256(raw).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
# Raw sampler fields are independently located by the previous frozen reference.
categoryBytes=(Path(__file__).parent/'actual-category-reference.json').read_bytes();assert hashlib.sha256(categoryBytes).hexdigest()=='82ca0232e4005fc352f92c830b13bef5765b212619448ff49634ca14aa78502d';scalars=json.loads(categoryBytes)['scalars']
gap=struct.unpack('<f',struct.pack('<I',scalars['68']))[0];slope=struct.unpack('<f',struct.pack('<I',scalars['108']))[0];cap=scalars['118'];assert (gap,slope,cap)==(f(.2),f(3),67108864)
data=a.acoustic.read_bytes();h=hashlib.sha256(data).hexdigest();actual=json.loads(data);assert actual['TargetsRead'] is False
rows=[];counts={'singleton':0,'optimized':0,'general':0};maximumRanges=0
for row in actual['Rows']:
 groups=[];height=f(row['Height']);assert height>0
 for g in row['Groups']:
  chunks=[row['CutInk'][s][b:e]for s,b,e in g['Stored']];assert chunks and all(chunks);total=0;maximumRanges=max(maximumRanges,len(chunks));counts['singleton'if len(chunks)==1 else'optimized'if len(chunks)<9 else'general']+=1
  for b in range(1,len(chunks)):
   left=[f(p[0])for c in chunks[:b]for p in c];right=[f(p[0])for c in chunks[b:]for p in c];distance=max(f(min(right)-max(left)),f(min(left)-max(right)));x=f(f(f(distance/height)-gap)*f(-slope));v=0 if x>4 else table[int(f(x+4)*255+.5)>>3]if x>=-4 else x;scaled=f(-v*8192);cost=int(rnd(scaled))if len(chunks)<9 else int(f(scaled+(.5 if scaled>0 else-.5)));total=total+cost if len(chunks)>=9 or f(f(total)+f(cost))<0x4000000 else 0x4000000;assert 0<=total<=2147483647
  groups.append([g['Group'],min(total,cap)])
 rows.append({'Name':row['Name'],'Groups':groups})
a.output.write_text(json.dumps({'scope':'Independent actual cut-point/guide group geometry only; no scheduler request or native execution','acousticSha256':h,'counts':counts,'maximumRanges':maximumRanges,'prototypeCount':24,'rows':rows},separators=(',',':'))+'\n');print(counts,maximumRanges,len(rows))
