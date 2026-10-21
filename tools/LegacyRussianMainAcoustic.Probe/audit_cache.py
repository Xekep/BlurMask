"""Original MAIN final195 classcache against raw-wire profile/scalar reference.
Model/geometry inputs, no word targets, no vendor execution.
"""
import ast,hashlib,json,math,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
def defs(path):
 t=ast.parse(path.read_text(encoding='utf-8-sig'));t.body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];g={'__file__':str(path)};exec(compile(t,str(path),'exec'),g);return g
stream=defs(Path(__file__).with_name('audit_output.py'))['samples'];ref=defs(ROOT/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py');ref['IMP']=0x4000000;f=ref['f']
#Use the existing independent special-symbol/scalar assignments, then a
#literal bounded maxheap instead of its old unique-boundary sorted-set shortcut.
tree=ast.parse((ROOT/'tools/LegacyFreshMainAcoustic.Probe/audit_output.py').read_text(encoding='utf8'))
node=next(n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name=='cache_reference')
cut=next(i for i,n in enumerate(node.body)if isinstance(n,ast.Assign)and any(isinstance(v,ast.Name)and v.id=='eligible'for v in n.targets))
node.name='precache';node.body=node.body[:cut]+[ast.Return(value=ast.Name(id='costs',ctx=ast.Load()))];ast.fix_missing_locations(node)
exec(compile(ast.Module(body=[node],type_ignores=[]),'independent-precache','exec'),ref);precache=ref['precache']
def prune(costs,minimum,capacity,threshold):
 def fill(upper):
  h=[]
  for symbol,cost in enumerate(costs):
   if symbol==192 or cost>=upper:continue
   item=(cost,symbol)
   if len(h)==capacity:
    if h[0][0]<=cost:continue
    at=0
    while at*2<len(h)-1:
     child=at*2+1
     if child<len(h)-1 and h[child][0]<=h[child+1][0]:child+=1
     if h[child][0]<=cost:break
     h[at]=h[child];at=child
   else:
    at=len(h);h.append(None)
    while at:
     parent=(at-1)//2
     if cost<=h[parent][0]:break
     h[at]=h[parent];at=parent
   h[at]=item
  return h
 h=fill(min(threshold,85173))
 if len(h)<minimum:h=fill(170346)
 if len(h)<minimum:h=fill(0x4000000)
 if len(h)<minimum:return costs
 result=[0x4000000]*195;result[192]=costs[192];result[178]=costs[178]
 for cost,symbol in h:result[symbol]=cost
 return result
data=(Path(sys.argv[1])/'ru-RU/ru_RU-ak-cur.lite.res').read_bytes();assert hashlib.sha256(data).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
class Reader:
 def __init__(self,pos):self.pos=pos
 def take(self,fmt,n=1):
  value=struct.unpack_from('<'+fmt*n,data,self.pos);self.pos+=struct.calcsize('<'+fmt)*n;return value[0]if n==1 else list(value)
 def skip(self,n):assert n>=0 and self.pos+n<=len(data);self.pos+=n
 def array(self,copies):self.skip(self.take('H')*2*copies)
 def network(self):
  assert struct.unpack_from('<HH',data,self.pos)==(0xfea0,0x106);self.skip(8+struct.unpack_from('<I',data,self.pos+4)[0])
r=Reader(99);header=r.take('H',8);n=r.take('H')
for step in [32,64,1]:
 values=r.take('H',n);n=next(v for v in reversed(values)if v!=65535)+step
spans=r.take('H',n+1);entries=[r.take('I',2)for _ in range(spans[-1]+1)];strings=r.pos
labels={data[strings+a:strings+b].decode('utf8'):label&65535 for (a,label),(b,_)in zip(entries,entries[1:])};assert header[7]==192 and all(0<=i<192 for i in labels.values())
r=Reader(189734+20);assert r.take('B')==1;r.network();r.skip(3*4+2+2+4+1);contextSelector=r.take('B');r.skip(2);assert r.take('B')==1;r.skip(2+8*4+4);assert r.take('B')==1 and r.take('B')==0;r.skip(2+4+5*2+7*4)
for copies in [1,2,2]:r.array(copies)
r.skip(2+2+4+4+2+2);assert r.take('B')==0
if r.take('B'):r.array(2)
for family in range(7):
 if r.take('B'):r.array(2)
 first,second,mode=r.take('B',3)
 if mode==0:r.take('f')
 for i in range(3):
  if r.take('B'):r.network()
prefixEnd=r.pos;r.skip(1);fallback=r.take('f');r.skip(1+8)
for _ in range(r.take('H')):r.array(1)
r.skip(4);profile=[fallback]+r.take('f',5);assert r.take('B')==0;r.skip(8);assert r.take('B')==0 and r.take('h')==0
profile +=[r.take('i'),r.take('H'),r.take('H')];assert header[:4]==[179,140,65535,178]
count=0;channels=0
for sample in stream(Path(sys.argv[2])):
 for group in sample['mainGroups']:
  assert len(group['normalized'])==1 and group['sourceProbability']==0
  costs=precache(group['rawCosts'],[p for s in group['normalized']for p in s],sample['mask'],labels,profile,group['whole'],group['zeroEndpoints'])
  costs=prune(costs,int(profile[7]),int(profile[8]),int(profile[6]))
  if '\u00ad' in labels and '-' in labels:costs[labels['\u00ad']]=costs[labels['-']]
  assert costs==group['finalCosts'],('FINAL_CACHE',sample['id'],group['id'],[(i,a,b)for i,(a,b)in enumerate(zip(costs,group['finalCosts']))if a!=b])
  count+=1;channels+=195
 if count%2000<150:print('cache',sample['id'],count,flush=True)
assert count==27866
report=dict(groups=count,finalClassCosts=channels,prefixEnd=prefixEnd,profile=profile,contextSelector=contextSelector,expectedLabelsRead=False,scope='Raw-wire profile and original final classcost195 scalar comparison. Literal bounded maxheap models equal-cost cutoff identities; candidate publication ordering is separate.')
if len(sys.argv)>3:Path(sys.argv[3]).write_text(json.dumps(report,indent=2)+'\n')
print('PASS',json.dumps(report))
