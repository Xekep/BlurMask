"""Independent full prepared-range normalization/merge/feature/107 audit."""
import ast,hashlib,importlib.util,json,math,struct,sys
from pathlib import Path
sys.dont_write_bytecode=True
if len(sys.argv)!=3:raise SystemExit('models-directory capture.json')
repo=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes())
def definitions(path):
 tree=ast.parse(path.read_text(encoding='utf8'));nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))]
 g={'__file__':str(path)};exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
wire=definitions(repo/'tools/LegacyFirstAcoustic119Wire.Probe/audit_output.py')
network=definitions(repo/'tools/LegacyPreparedNetwork107.Probe/audit_output.py')
network['exp_table']=[int(25*math.exp(-4+22*i/351))for i in range(352)]
spec=importlib.util.spec_from_file_location('feature_scalar',Path(__file__).with_name('feature_oracle.py'));features=importlib.util.module_from_spec(spec);spec.loader.exec_module(features)
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
data=(models/capture['Path']).read_bytes();manifest={x['path']:x['sha256']for x in json.loads((models/'manifest.json').read_bytes())['resources']}
assert hashlib.sha256(data).hexdigest()==manifest[capture['Path']]
_,_,_,children=wire['source'](data,0);chosen=next(c for c in children if c[3]==0x249);start=chosen[1]+16;stop=chosen[2]-4
assert(start,stop)==(capture['SourceOffset'],capture['SourceEnd'])and hashlib.sha256(data[start:stop]).hexdigest()==capture['Fingerprint'].lower()
assert capture['HiddenTable']==[int(1024*math.tanh(3*i/47))for i in range(48)]
assert hashlib.sha256(struct.pack('<48i',*capture['HiddenTable'])).hexdigest()=='db88702e24d4ac761fb778dca068606627c3a8b25e1ff614410c963fa627e26f'
assert capture['OutputTable']==[int(1024/(1+math.exp(-8*i/127)))for i in range(127)]+[1024]
assert hashlib.sha256(struct.pack('<128i',*capture['OutputTable'])).hexdigest()=='eae762e8cc8c234d983fdd93e20be8450b6fc91bee828dbb8a247bc17b656cd3'
networks={slot:network['load'](data,p,end)[0]for slot,p,end in network['first_networks'](data)}
def quicksort(items,key,reverse=False):
 items=items[:]
 def sort(a,b):
  l,r=a,b;p=key(items[(a+b)//2])
  while l<=r:
   while (key(items[l])>p if reverse else key(items[l])<p):l+=1
   while (key(items[r])<p if reverse else key(items[r])>p):r-=1
   if l>r:break
   items[l],items[r]=items[r],items[l];l+=1;r-=1
  if a<r:sort(a,r)
  if l<b:sort(l,b)
 if len(items)>1:sort(0,len(items)-1)
 return items
def evaluate(vector,observed,matrices):
 signed=network['signed'];hidden=[1024];dots=[]
 for row,scale in zip(*matrices[0]):
  dot=signed(signed(sum(a*b for a,b in zip(vector,row)))*scale);dots.append(dot);i=abs(dot)>>16;v=capture['HiddenTable'][i]if i<48 else 1018;hidden.append(v if dot>0 else-v)
 raw=[signed(signed(sum(a*b for a,b in zip(hidden,row)))*scale)for row,scale in zip(*matrices[1])]
 assert observed['HiddenDots']==dots and observed['HiddenActivations']==hidden and observed['RawDots']==raw
 probabilities,_=network['exp_prob'](raw[:180]+raw[181:]);argument=f(f(f(raw[180])*-2)/1048576);special=0 if argument>75 else int(32768/(math.exp(argument)+1)+.5)
 probabilities=probabilities[:180]+[special]+probabilities[180:];assert observed['Probability32768']==probabilities

degenerated=merged=reordered=0;vectors=0
for case in capture['Cases']:
 ps=[[[f(v)for v in p]for p in s]for s in case['Prepared']];ranges=case['Ranges'];keys=case['StartKeys']
 keytuples=[tuple(k[x]for x in('First','Second','Third'))for k in keys];order=quicksort(list(range(len(ranges))),lambda i:keytuples[i]);assert order==case['Order'];reordered+=order!=list(range(len(order)))
 minimum=min(ps[s][p][0]for s,a,b in ranges for p in range(a,b));baseline=f(case['Line']['Baseline']);height=f(case['Line']['Height'])
 normalized=[]
 for i in order:
  s,a,b=ranges[i];normalized.append([[int(min(32767,f(f(f(x-minimum)*100)/height))),int(min(32767,f(f(f(y-baseline)*100)/height)))]for x,y in ps[s][a:b]])
 assert normalized==case['Normalized']
 boundary=[1024 if ranges[0][1]==0 else-1024,1024 if ranges[-1][2]==len(ps[ranges[-1][0]])else-1024]
 assert boundary==[case['Boundary']['Start'],case['Boundary']['End']]
 retained=list(range(len(normalized)-1))
 if len(normalized)>10:
  squares=[sum((a-b)**2 for a,b in zip(s[-1],t[0]))for s,t in zip(normalized,normalized[1:])]
  retained=sorted(quicksort(list(range(len(squares))),lambda i:squares[i],True)[:9])
 assert retained==case['Retained'];reduced=[];joined=[]
 for i,s in enumerate(normalized):
  joined+=s
  if i==len(normalized)-1 or i in retained:reduced.append(joined);joined=[]
 assert reduced==case['Merged'];merged+=len(reduced)<len(normalized)
 t,r=features.extract(reduced);assert t+boundary==case['Trajectory']and r+boundary==case['Raster']
 degenerate=len({tuple(p)for s in reduced for p in s})==1;assert degenerate==case['Degenerate'];degenerated+=degenerate
 for observed,vector in zip(case['Networks'],(case['Trajectory'],case['Raster'])):evaluate(vector,observed,networks[observed['Slot']]);vectors+=1
assert len(capture['Cases'])==124 and capture['Guards']>=255
assert any(c['Boundary']==dict(Start=-1024,End=-1024)and c['Ranges'][c['Order'][0]][1]==0 and c['Ranges'][c['Order'][-1]][2]==len(c['Prepared'][c['Ranges'][c['Order'][-1]][0]])for c in capture['Cases'])
assert(degenerated,merged,reordered)==(capture['Degenerate'],capture['Merged'],capture['Reordered'])==(5,1,2)
print(f'PASS independent rawAK/table/normalization/penup/tieorder/storedboundary/ALL217+199 features/{vectors} actual107 diagnostics;124 prepared-range controls/5degenerate/1merge/2reordered.')
