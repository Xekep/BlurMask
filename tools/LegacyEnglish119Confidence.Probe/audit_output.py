"""Independent owned119 vector10[0] audit, composed with an audited cache capture.
No vendor/research/runtime imports; only Python standard library.
"""
from pathlib import Path
import ast,hashlib,json,math,struct,sys
sys.dont_write_bytecode=True
if len(sys.argv)!=4:raise SystemExit('models-directory audited-cache.json confidence.json')
repo=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);cache=json.loads(Path(sys.argv[2]).read_bytes());capture=json.loads(Path(sys.argv[3]).read_bytes())
def functions(path,names=None):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,ast.FunctionDef)and(names is None or n.name in names)]
 g={'struct':struct,'math':math};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.pack('<f',x)
d=(models/capture['Path']).read_bytes();manifest={x['path']:x['sha256']for x in json.loads((models/'manifest.json').read_bytes())['resources']}
assert hashlib.sha256(d).hexdigest()==manifest[capture['Path']]and capture['Path']==cache['Path']
assert capture['Fingerprint'].lower()==cache['Fingerprint'].lower()==hashlib.sha256(d[8843:182187]).hexdigest()
header,labels=functions(repo/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',{'labels'})['labels'](d[8843:182187])
assert header==[181,178,65535,180,184,182,183,217]
mapping={}
network=functions(repo/'tools/LegacyPreparedNetwork107.Probe/audit_output.py',{'node','first_networks'})
at=network['first_networks'](d)[-1][2]+1+5*11
def take(fmt,n=1):
 global at
 v=struct.unpack_from('<'+fmt*n,d,at);at+=struct.calcsize('<'+fmt)*n;return v
take('B');take('f');take('B');take('f',2)
for i in range(take('H')[0]):take('H',take('H')[0])
take('f',5);take('B');take('f',2);group_count=take('H')[0];ends=take('H',group_count+1);members=take('H',ends[-1]);weights=take('f',group_count)
assert group_count==8 and len(members)==17
for i,alpha in enumerate(weights):
 group=members[ends[i]:ends[i+1]]
 if alpha<=0 or not any(s>=182 for s in group):continue
 target=next((s for s in group if s<182),65535)
 if target!=65535:
  for s in group:
   if s>=182:mapping.setdefault(s,target)
assert not mapping #Actual group smoothing is not a new alias within NN width.
for text in('-','\uff0d','_','\uff3f','\u00ad','\u30fc','\u4e00','\U000f0002','\U000f0003'):
 if text in labels and not(text=='\u30fc'and labels[text]<182):mapping.setdefault(labels[text],header[1])
mapping[218]=labels["'"];mapping[219]=labels['"'];assert capture['Mappings']==[list(x)for x in sorted(mapping.items())]
assert(0x21703&~struct.unpack_from('<I',d,182001+13*4)[0])&0x800==0
enclosure=functions(repo/'tools/LegacySourceEnclosure.Probe/reference_controls.py')['evaluate']
def calibrated(p):return f(1/(1+math.exp(-f(p*f(1/32768))*.575)))
assert len(capture['Curve'])==32769
float_slope_differences=0
for p,actual in enumerate(capture['Curve']):
 expected=calibrated(p);assert bits(actual)==bits(expected),('calibration',p)
 float_slope_differences+=bits(expected)!=bits(f(1/(1+math.exp(-f(p*f(1/32768))*f(.575)))))
assert float_slope_differences>0
assert len(capture['Rows'])==len(cache['Rows'])==124
groups=values=closed=partial=partial_positive=remapped=0
for ri,(row,reference)in enumerate(zip(capture['Rows'],cache['Rows'])):
 assert row['Raw']==reference['Raw'];original=[]
 for stroke in row['Raw']:
  original.append([p for i,p in enumerate(stroke)if i==0 or any(abs(p[j])+abs(stroke[i-1][j])<abs(f(p[j]-stroke[i-1][j]))*2**23 for j in(0,1))])
 assert len(row['Groups'])==len(reference['Groups'])
 for g,ref in zip(row['Groups'],reference['Groups']):
  for key in('Id','Ranges','Whole','Trajectory','Raster','Candidates'):assert g[key]==ref[key],(ri,g['Id'],key)
  physical=g['Ranges'][0]['StrokeIndex'];probability=enclosure(original,physical)
  assert abs(g['SourceProbability']-probability)<3e-6;partial+=not g['Whole'];partial_positive+=not g['Whole']and g['SourceProbability']>0
  assert g['Missing']==[s for s in range(220)if all(x['Symbol']!=s for x in g['Candidates'])]
  assert len(g['Values'])==len(g['Candidates'])
  for rank,(actual,candidate)in enumerate(zip(g['Values'],g['Candidates'])):
   symbol=candidate['Symbol'];assert actual['Symbol']==symbol
   if rank>=20:expected=0.;output=65535;source=False
   elif symbol==labels['\U000f0001']:
    expected=g['SourceProbability'];output=65535;source=True;closed+=1
   else:
    output=mapping.get(symbol,symbol);assert output<182;source=False;remapped+=output!=symbol
    a,b=[calibrated(g[key][output])for key in('Trajectory','Raster')];expected=min(1.,f(f(b+a)/2))
   assert(actual['OutputLabel'],actual['SourceProbability'])==(output,source)
   assert bits(actual['Value'])==bits(expected),('vector',ri,g['Id'],symbol,actual['Value'],expected);values+=1
  groups+=1
assert groups==capture['Groups']==3241 and values==32152 and closed==1 and partial==capture['Partial']==3076 and partial_positive==1 and remapped>0 and capture['Guards']==622
print(f'PASS raw217AL/3A0 +same audited3241cache groups/{values} vector values/{remapped} aliases/{closed} rawsource211;3076actual partial-source inspections/1positive;32769 scalar controls/{float_slope_differences} DOUBLE-vs-FLOAT discriminators. No selected word/extraNN.')
