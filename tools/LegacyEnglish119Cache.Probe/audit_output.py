"""Raw-wire119/mask/ensemble/final-cache oracle; standard library only.
The separately audited candidate producer supplies the two network outputs.
This audit independently checks every subsequent integer channel and ordered write.
"""
from pathlib import Path
import ast,base64,hashlib,json,math,struct,sys
sys.dont_write_bytecode=True
if len(sys.argv)!=3:raise SystemExit('models-directory capture.json')
repo=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes())
def functions(path,names=None):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef))and(names is None or n.name in names)]
 g={'struct':struct,'base64':base64,'math':math};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def smooth(costs,mask,groups,weights):
 out=costs[:];writes=[]
 def probability(v):return 0. if v>=0x4000000 else f(math.exp(f(-f(v)*f(1/8192))))
 for gi,(group,alpha)in enumerate(zip(groups,weights)):
  if alpha<=0:continue
  maximum=max([probability(out[s])for s in group if mask[s]]or[0.])
  if maximum==0:continue
  for ei,s in enumerate(group):
   if not mask[s]:continue
   before=out[s];mix=f(f(f(1-alpha)*probability(before))+f(alpha*maximum));v=-math.log(mix)*8192
   out[s]=int(v+(.5 if v>0 else -.5));writes.append(dict(Group=gi,Entry=ei,Symbol=s,Before=before,After=out[s]))
 return out,writes

d=(models/capture['Path']).read_bytes();manifest={x['path']:x['sha256']for x in json.loads((models/'manifest.json').read_bytes())['resources']}
assert hashlib.sha256(d).hexdigest()==manifest[capture['Path']]
wire=functions(repo/'tools/LegacyFirstAcoustic119Wire.Probe/audit_output.py');_,_,_,children=wire['source'](d,0)
processor=next(x for x in children if x[3]==0x249);start=processor[1]+16;end=processor[2]-4
assert(start,end)==(8843,182187)and hashlib.sha256(d[start:end]).hexdigest()==capture['Fingerprint'].lower()
_,kind,count,children=wire['source'](d,start);assert(kind,count)==(0x135,217)
ak=next(x for x in children if x[0]=='140');assert ak[1:]==(18695,181996,0x21d,0x119)
net=functions(repo/'tools/LegacyPreparedNetwork107.Probe/audit_output.py');networks=net['first_networks'](d);assert len(networks)==2
q=networks[-1][2]+1+5*11;r=wire['Acoustic'](d,q,ak[2])
r.u('78');r.f('7C');r.u('80');r.f('88');r.f('84')
for i in range(r.u('90',2)):r.lists(f'98[{i}].count',f'98[{i}]',1,'21D')
for name in('A4','A8','AC','B0','B4'):r.f(name)
r.u('B8');r.f('BC');r.f('C0');ng=r.u('C8',2)
ends=struct.unpack('<'+'H'*(ng+1),r.take('21D','D0',2,ng+1));ids=struct.unpack('<'+'H'*ends[-1],r.take('21D','D8',2,ends[-1]));weights=[r.f(f'E0[{i}]')for i in range(ng)]
r.u('E8',4);r.u('EC',2);r.u('EE',2)
for name in('F0','F4','F8'):r.u(name,4)
r.u('FC');r.f('100');r.u('108',4);r.u('128',4);r.u('148/150.present');r.f('120');r.f('124');assert r.p==ak[2]
fields={x['Destination']:base64.b64decode(x['Raw'])for x in r.fields}
u=lambda name:int.from_bytes(fields[name],'little');ff=lambda name:struct.unpack('<f',fields[name])[0]
assert(ng,u('78'),u('80'),u('B8'),ff('100'),u('108'),u('128'),u('148/150.present'))==(8,0,0,0,0,0,0,0)
groups=[list(ids[a:b])for a,b in zip(ends,ends[1:])];assert len(ids)==17 and all(w==f(.99)for w in weights)
profile=[ff(x)for x in('7C','A4','A8','AC','B0','B4')]+[u(x)for x in('E8','EC','EE')]

#Read the AL reverse table independently: missing reverse entries are virtual.
at=start+99
def take(fmt,n=1):
 global at
 v=struct.unpack_from('<'+fmt*n,d,at);at+=struct.calcsize('<'+fmt)*n;return v
header=take('H',8);assert header==(181,178,65535,180,184,182,183,217);n=take('H')[0]
for step in(32,64,1):n=next(x for x in reversed(take('H',n))if x!=65535)+step
spans=take('H',n+1);entries=[take('I',2)for _ in range(spans[-1]+1)];blob=d[at:at+entries[-1][0]];at+=len(blob)
labels={blob[a:b].decode('utf8'):s&65535 for(a,s),(b,_)in zip(entries,entries[1:])};reverse=take('H',217)
assert len(d[start+99:start+7907])==7808 and hashlib.sha256(d[start+99:start+7907]).hexdigest()=='5554e287937d2d8682c46eb7d85a701b15f1b86afb45f55228dbc8c1ebf7a1ff'
mask=[int(x!=65535)for x in reverse];assert[s for s,x in enumerate(mask)if not x]==[178,180,181]
mask[header[1]]=int(any(mask[labels[t]]for t in('-','_','\U000f0002','\U000f0003')if t in labels))
for i in(3,0,4,5,6):
 if header[i]!=65535:mask[header[i]]=1
assert all(mask)
metadata=end-186;flags=0x21703&~struct.unpack_from('<I',d,metadata+13*4)[0];assert flags==0x21700 and d[metadata+128]>13

#Reuse the independently written scalar heap/primitive oracle, adding only the
#new ordered119 stage at its native position. No runtime code is imported.
path=repo/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py';tree=ast.parse(path.read_text(encoding='utf8'));node=next(n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name=='cache')
text=ast.get_source_segment(path.read_text(encoding='utf8'),node)
text=text.replace('whole,zero):','whole,zero,probability,groups,weights):')
old="  #All source2AD probabilities for this actual nonenclosing corpus are0.\n  assign('\\U000f0001',IMP)";assert old in text
text=text.replace(old,"  assign('\\U000f0001',IMP if probability<=0 else rounded(-math.log(probability)*closed*8192))")
assert ' if "\'" in label:'in text
text=text.replace(' if "\'" in label:', ' result,_=smooth(result,mask,groups,weights)\n if "\'" in label:')
old=' candidates=sorted([i,result[i]] for i,_ in heap if i<width)';assert old in text
text=text.replace(old,' candidates=sort([[i,result[i]] for i,_ in heap if i<width],lambda x:x[1])')
sort=functions(repo/'tools/LegacyEnglishNoInitialGlyph.Probe/audit_output.py',{'quicksort'})['quicksort']
scope={'f':f,'math':math,'smooth':smooth,'sort':sort};exec(compile(text,'independent119cache','exec'),scope);cache=scope['cache']
cost=functions(repo/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['cost']
enclosure=functions(repo/'tools/LegacySourceEnclosure.Probe/reference_controls.py')['evaluate'];enclosure.__globals__.update(f32=f,f=f)
real=closed=positive=dot=changed=0
for ri,row in enumerate(capture['Rows']):
 assert row['Flags']==flags and list(base64.b64decode(row['Primary']))==mask and list(base64.b64decode(row['Secondary']))==mask
 original=[]
 for stroke in row['Raw']:
  original.append([p for i,p in enumerate(stroke)if i==0 or any(abs(p[j])+abs(stroke[i-1][j])<abs(f(p[j]-stroke[i-1][j]))*2**23 for j in(0,1))])
 for g in row['Groups']:
  a,b=g['Trajectory'],g['Raster'];assert len(a)==len(b)==182
  neural=[(cost(x)+cost(y))>>1 for x,y in zip(a,b)];neural[header[0]]=0x4000000;assert neural==g['Neural'],('ensemble',ri,g['Id'])
  rs=[[x[k]for k in('StrokeIndex','StartPoint','EndPointExclusive')]for x in g['Ranges']];s,begin,_=rs[0];last,_,stop=rs[-1]
  whole=len(g['Normalized'])==1 and begin==0 and stop==row['CutCounts'][last];zero=whole and len(row['Raw'][s])==1
  assert(whole,zero)==(g['Whole'],g['Zero']);dot+=zero;closed+=whole
  probability=enclosure(original,s)if whole else None
  if whole:assert abs(probability-g['Probability'])<3e-6;positive+=probability>.5
  else:assert g['Probability']is None
  expected,candidates=cache(neural,g['Normalized'],mask,header,labels,profile,whole,zero,probability,groups,weights)
  assert expected==g['Costs'],('cache',ri,g['Id'],[(i,x,y)for i,(x,y)in enumerate(zip(expected,g['Costs']))if x!=y][:12])
  assert candidates==[[x['Symbol'],x['Cost']]for x in g['Candidates']]and g['Fallback']==expected[217];real+=1
  without,_=cache(neural,g['Normalized'],mask,header,labels,profile,whole,zero,probability,[],[]);changed+=without!=expected
ordered=0
for i,row in enumerate(capture['Prepared']):
 mask=list(base64.b64decode(row['Mask']));out,writes=smooth(row['Costs'],mask,row['Groups'],row['Weights'])
 assert(out,writes)==(row['Output'],row['Writes']),('prepared',i)
 ordered+=sum(w['Before']!=row['Costs'][w['Symbol']]for w in writes)
assert len(capture['Rows'])==124 and real==capture['Groups']==3241 and len(capture['Prepared'])==1024 and ordered>500 and positive>0 and dot>0 and changed>0 and capture['Guards']==503
print(f'PASS raw119 tail/AL masks/flags and {real} real ensembles/full220 caches/ordered shortlists;{changed} changed by actual C8; {closed} whole/{positive} positive-source/{dot} dot scopes;1024 independent ordered controls/{ordered} rereads. No confidence/winner.')
