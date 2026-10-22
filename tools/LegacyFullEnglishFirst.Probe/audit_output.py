"""Raw-wire119/mask/ensemble/final-cache oracle; NumPy plus scalar references.
Owned cut coordinates/ranges are the ingress; all four network outputs are recomputed.
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

d=(models/'en-US/en_US-ak-cur.res').read_bytes();manifest={x['path']:x['sha256']for x in json.loads((models/'manifest.json').read_bytes())['resources']}
assert hashlib.sha256(d).hexdigest()==manifest['en-US/en_US-ak-cur.res']
wire=functions(repo/'tools/LegacyFirstAcoustic119Wire.Probe/audit_output.py');_,_,_,children=wire['source'](d,0)
processor=next(x for x in children if x[3]==0x249);start=processor[1]+16;end=processor[2]-4
assert(start,end)==(8843,373957) and hashlib.sha256(d).hexdigest()=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
_,kind,count,children=wire['source'](d,start);assert(kind,count)==(0x135,217)
ak=next(x for x in children if x[0]=='140');assert ak[1:]==(18695,373766,0x21d,0x119)
net=functions(repo/'tools/LegacyPreparedNetwork107.Probe/audit_output.py');networks=net['first_networks'](d);assert len(networks)==4
q=networks[-1][2]+1+3*11;r=wire['Acoustic'](d,q,ak[2])
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
metadata=end-186;flags=0x21703&~struct.unpack_from('<I',d,metadata+13*4)[0];assert flags==0x21400 and d[metadata+128]>13

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

import importlib.util,numpy as np
module_path=Path(__file__).with_name('reference_features.py');spec=importlib.util.spec_from_file_location('full_first_features',module_path);feature=importlib.util.module_from_spec(spec);spec.loader.exec_module(feature)
ensemble=functions(repo/'tools/LegacyCostEnsemble.Probe/reference_controls.py')['reference'];ensemble.__globals__['f32']=f
nets=[]
for slot,at,stop in networks:
 matrix,columns,hidden,outputs=net['load'](d,at,stop);nets.append([(np.array(w,dtype=np.int64),np.array(list(scales),dtype=np.int64))for w,scales in matrix])
assert [len(m[0][0][0])for m in nets]==[219,201,276,276]
lut=[int(math.floor(1024*math.tanh(3*i/47)))for i in range(48)]
assert hashlib.sha256(struct.pack('<48i',*lut)).hexdigest()=='db88702e24d4ac761fb778dca068606627c3a8b25e1ff614410c963fa627e26f'
exp_prob=net['exp_prob'];exp_prob.__globals__['exp_table']=[int(25*math.exp(-4+22*i/351))for i in range(352)]
def signed(x):return(int(x)+2**31)%2**32-2**31

def evaluate(layers,vector):
 w,s=layers[0];dots=[signed(signed(v)*scale)for v,scale in zip(w@np.array(vector,dtype=np.int64),s)];hidden=[1024]+[(1 if x>0 else -1)*(lut[abs(x)>>16]if abs(x)>>16<48 else 1018)for x in dots]
 w,s=layers[1];raw=[signed(signed(v)*scale)for v,scale in zip(w@np.array(hidden,dtype=np.int64),s)]
 ordinary,offset=exp_prob(raw[:180]+raw[181:]);arg=f(f(f(raw[180])*-2)/1048576);special=0 if arg>75 else int(32768/(math.exp(arg)+1)+.5);probabilities=ordinary[:180]+[special]+ordinary[180:]
 return dots,hidden,raw,probabilities,offset

real=nn=closed=positive=dot=changed=0
assert capture['TargetsRead']is False
for record in capture['Actual']:
 if record['Status']=='Refused':continue
 assert record['ProductionDiagnosticParity'] is True
 for row in record['Children']:
  row['Name']=record['Name'];assert row['KnowledgeFlags']==flags and row['Primary']==mask
  original=[]
  for stroke in row['Raw']:original.append([p for i,p in enumerate(stroke)if i==0 or any(abs(p[j])+abs(stroke[i-1][j])<abs(f(p[j]-stroke[i-1][j]))*2**23 for j in(0,1))])
  for g in row['Groups']:
   feature.audit(row,g)
   for layers,vector,out in zip(nets,g['Inputs'],g['Networks']):
    dots,hidden,raw,prob,offset=evaluate(layers,vector)
    assert(dots,hidden,raw,prob,offset)==(out['HiddenDots'],out['Hidden'],out['Raw'],out['Probabilities'],out['StabilizationOffset']),('NN',record['Name'],g['Group'])
    nn+=1
   neural=ensemble([{'probabilities':n['Probabilities'],'weight':1}for n in g['Networks']],1,mask[:182]);neural[header[0]]=0x4000000
   assert neural==g['Neural'],('ensemble',record['Name'],g['Group'])
   rs=g['Stored'];s,begin,_=rs[0];last,_,stop=rs[-1];whole=len(g['Merged'])==1 and begin==0 and stop==row['CutCounts'][last];zero=whole and len(row['Raw'][s])==1
   assert(whole,zero)==(g['Whole'],g['Zero']);closed+=whole;dot+=zero
   probability=enclosure(original,s)if whole else None
   if whole:assert abs(probability-g['Probability'])<3e-6;positive+=probability>.5
   else:assert g['Probability']is None
   expected,candidates=cache(neural,g['Merged'],mask,header,labels,profile,whole,zero,probability,groups,weights)
   assert expected==g['Final'],('cache',record['Name'],g['Group'],[(i,x,y)for i,(x,y)in enumerate(zip(expected,g['Final']))if x!=y][:12])
   assert candidates==g['Candidates']and g['Fallback']==expected[217]
   without,_=cache(neural,g['Merged'],mask,header,labels,profile,whole,zero,probability,[],[]);changed+=without!=expected;real+=1
two_stage_discriminators=0
for control in capture.get('PreparedAssembly',[]):
 for spatial,key in [(False,'Temporal'),(True,'Spatial')]:
  expected=feature.assembly(control['Global'],control['Windows'],spatial,control['Minimum'],control['Maximum'])
  assert expected==control[key] and len(expected)==274 and expected[0]==1024
  #Negative control: do not carry transformed int32 values through the first
  #short write. Count witnesses where omitting native first SAT changes output.
  div=feature.div;i32=feature.i32;sat=feature.sat
  ratio=lambda v,d:div(i32(v*2048),d)-1024
  raw=[1024]+[div(i32(v*2048-2048),5)-1024 if i==2 else i32(v*2)if i==8 else ratio(v,3072 if i==3 else 3 if 9<=i<=15 else 1024)for i,v in enumerate(control['Global'])]
  for window in control['Windows']:
   raw += [ratio(v,256)if spatial and i in(0,1,2,15)else i32(v*2048-1024)if i in(4,5,9,10,11,12,13,14)else i32(v*2)if i==8 else ratio(v,128 if i==15 else 1024)for i,v in enumerate(window)]
  for i in range(1,274):
   span=control['Maximum'][i]-control['Minimum'][i]
   raw[i]=sat(div(i32((raw[i]-control['Minimum'][i])*2048),span)-1024)if span else sat(raw[i])
  two_stage_discriminators+=raw!=expected
assert two_stage_discriminators>100

assert len(capture.get('PreparedAssembly',[]))==128 and capture.get('Guards',0)>=40 and capture.get('Cancellations')==10
assert real==capture['Groups'] and nn==4*real
assert capture['Accepted']+capture['Refused']==len(capture['Actual'])
print(f'PASS actual full-FIRST {capture["Accepted"]}/{len(capture["Actual"])} admitted;{real} complete feature/4NN/220cache groups/{nn} NNs;{closed} whole/{positive} positive-source/{dot} dots/{changed} group-smoothing changes; no targets/winner.')
