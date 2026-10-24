"""DATA-only connected classifier audit. No model forward or vendor execution.
CLI REPOSITORY ACTUAL PRIOR_1F PRIOR_2F PRIOR_3F PRIOR_4F PRIOR_5F
Prior captures are independently audited producer receipts, not prediction authority.
"""
import pathlib,sys,json,struct,hashlib,math,copy,tempfile
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent))
import reference_shape as s
import reference_finalization as final
INF=0x4000000
AK='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
PRIOR_SHA=['ecb2273f1dfac51383da9c18b4ace804469bd78d0e23883123905da25e09c7d5','c91b7732e2a0e9d2c1333264e21490dcd00764df02cf93850ebf442e88af1950','c440262c1caa71e3ad619cb942030f2b3af0782d1b3925478c869db789ecc258','49d06002057a99d1e9c0e5da83368c758ff5b2a5264cd3c00f8f115c63c5211e','aad766ada3d55502c09a435d51dd36433e81ba5c99c6bb37c0d371d0ec5359bc']
PE_SHA='c633012619b4db77a73de117913d4d1eac6f5c340156bfe054f246cfe3701afd'
class PE:
 def __init__(self,b):
  self.b=b;o=struct.unpack_from('<I',b,60)[0];n=struct.unpack_from('<H',b,o+6)[0];z=struct.unpack_from('<H',b,o+20)[0];self.sections=[]
  for i in range(n):
   at=o+24+z+40*i;vs,va,sz,off=struct.unpack_from('<IIII',b,at+8);self.sections.append((va,max(vs,sz),off))
 def read(self,r,n):
  for va,z,o in self.sections:
   if va<=r and r+n<=va+z:return self.b[o+r-va:o+r-va+n]
  raise ValueError(hex(r))
def alphabet(b):
 p=34
 def rd(t,n=1):
  nonlocal p
  v=struct.unpack_from('<'+t*n,b,p);p+=struct.calcsize('<'+t*n);return v[0]if len(v)==1 else v
 fields=rd('H',8);n=rd('H')
 for k in(32,64,1):
  table=rd('H',n);n=next(v+k for v in reversed(table)if v!=65535)
 spans=rd('H',n+1);es=[rd('II')for _ in range(spans[-1]+1)];strings=b[p:p+es[-1][0]];p+=len(strings)
 labels={label&65535:strings[a:es[i+1][0]].decode('utf8')for i,(a,label)in enumerate(es[:-1])};reverse=rd('H',fields[-1])
 assert fields==(256,65535,65535,65535,65535,65535,65535,259)and len(labels)==258 and reverse[256]==65535
 return labels
def ints(got,want):
 assert type(got)is list and len(got)==len(want)and all(type(x)is int and x==y for x,y in zip(got,want))
def prior_shape(j,start):
 assert type(j['xy'])is list and len(j['xy'])==3
 for got,want in zip(j['xy'],[[0,0],[2,1],[4,0]]):
  ints(got,want)
 assert type(j['ModelStart'])is int and j['ModelStart']==start

def expected(root,prior):
 assert len(prior)==5
 raw=(root/'models/math/math-ak.res').read_bytes();assert hashlib.sha256(raw).hexdigest()==AK
 peBytes=(root/'research/myscript/sdk/build/x64/MyScript2D.dll').read_bytes();assert hashlib.sha256(peBytes).hexdigest()==PE_SHA
 pe=PE(peBytes);labels=alphabet(raw);lookup={v:k for k,v in labels.items()};aliases={}
 specs=[(0x8b740,1),(0x8b744,1),(0x8b748,1),(0x8b74c,1),(0x8b750,3),(0x8b754,3),(0x8b758,4),(0x8b760,4),(0x8b768,4),(0x8b770,4),(0x8b778,1),(0x8b77c,1),(0x8b780,1),(0x8b784,3)]
 for i,(r,n)in enumerate(specs):aliases[0x140+2*i]=lookup.get(pe.read(r,n).decode('utf8'),65535)
 assert aliases=={0x140:107,0x142:65535,0x144:202,0x146:257,0x148:65535,0x14a:65535,0x14c:65535,0x14e:65535,0x150:65535,0x152:65535,0x154:70,0x156:24,0x158:0,0x15a:65535}
 logs=struct.unpack('<2049i',pe.read(0x74290,2049*4))
 def cost(p):
  assert type(p)is int and 0<=p<=32768
  p=max(1,p);q,r=divmod(p,16);return -(logs[q+(r>8)]if q else logs[r]-0x58b9)
 families=[];pins=[]
 for path,start,sha in zip(prior,[1007739,1151097,1289175,1459255,1629111],PRIOR_SHA):
  b=path.read_bytes();assert hashlib.sha256(b).hexdigest()==sha
  j=json.loads(b.decode('utf-8-sig'));prior_shape(j,start)
  assert len(j['probability'])==257
  c=[cost(p)for p in j['probability']];ints(j['costs'],c);families.append(c);pins.append(dict(path=str(path),sha256=hashlib.sha256(b).hexdigest()))
 c=[INF]*262
 for label in range(257):
  accum=0;weight=s.f(0)
  for family in families:accum+=math.trunc(s.f(family[label]));weight=s.add(weight,1)
  c[label]=min(INF,math.trunc(s.div(s.f(accum),weight)))
 c[256]=INF;c[260]=c[aliases[0x140]];assert c[70]!=INF
 best=min(range(259),key=lambda i:c[i]);w,ps=s.raw_prototypes(raw);entries=ps[best]['entries'];selected=None
 if entries:
  # Match native lazy geometry even for one alternative.
  feature=s.features([[[0,0],[2,1],[4,0]]],w)[0];selected=0
  if len(entries)>1:
   ds=s.distances(feature,entries);selected=min(range(len(ds)),key=lambda i:ds[i])
  entry=entries[selected];factor=entry['factor']
  if factor is not None:
   for t in entry['targets']:
    if t==65535:continue
    v=s.add(s.mul(c[best],factor),s.mul(s.sub(1,factor),c[t]))if factor<1 else s.div(c[best],factor)
    c[t]=math.trunc(s.add(v,-.5 if v<=0 else .5))
 return dict(families=families,best=best,selected=selected,prior=pins,**final.finalize(c))
def validate(a,e):
 assert a['scope']=='fixed-five fresh unmasked guide0 numerical primitive'
 assert type(a['NetworkExecutions'])is int and a['NetworkExecutions']==5 and a['Confidence']is None and a['HasGrammarCandidateAuthority']is False
 assert a['xy']==[[0,0],[2,1],[4,0]]and all(type(c)is int for p in a['xy']for c in p)
 assert len(a['families'])==5
 for f,w in zip(a['families'],e['families']):ints(f,w)
 ints(a['costs'],e['costs']);ints(a['rank'],e['rank']);assert len(a['symbols'])==len(e['output'])
 for got,want in zip(a['symbols'],e['output']):
  assert set(got)=={'Label','Cost8192'}and type(got['Label'])is int and type(got['Cost8192'])is int and [got['Label'],got['Cost8192']]==want
if __name__=='__main__':
 if len(sys.argv)!=8:raise SystemExit('REPOSITORY ACTUAL PRIOR_1F PRIOR_2F PRIOR_3F PRIOR_4F PRIOR_5F')
 root=pathlib.Path(sys.argv[1]);actual=pathlib.Path(sys.argv[2]);e=expected(root,list(map(pathlib.Path,sys.argv[3:])));a=json.loads(actual.read_text(encoding='utf-8-sig'));validate(a,e);negative=[]
 changes=[('cost',lambda a:a['costs'].__setitem__(0,a['costs'][0]+1)),('rank',lambda a:a['rank'].__setitem__(0,-1)),('family',lambda a:a['families'][0].__setitem__(0,-1)),('label-bool',lambda a:a['symbols'][0].__setitem__('Label',False)),('score',lambda a:a['symbols'][0].__setitem__('Cost8192',0)),('NN-count',lambda a:a.__setitem__('NetworkExecutions',4)),('confidence',lambda a:a.__setitem__('Confidence',1)),('authority',lambda a:a.__setitem__('HasGrammarCandidateAuthority',True))]
 for name,change in changes:
  bad=copy.deepcopy(a);change(bad)
  try:validate(bad,e)
  except(AssertionError,KeyError,TypeError,ValueError):negative.append(name)
  else:raise AssertionError('accepted '+name)
 prior_negative=[]
 def refuse_prior(name,fn):
  try:fn()
  except(AssertionError,KeyError,TypeError,ValueError):prior_negative.append(name)
  else:raise AssertionError('accepted prior '+name)
 prior=list(map(pathlib.Path,sys.argv[3:]))
 pe=PE((root/'research/myscript/sdk/build/x64/MyScript2D.dll').read_bytes());logs=struct.unpack('<2049i',pe.read(0x74290,2049*4))
 with tempfile.TemporaryDirectory(prefix='math-fixed-prior-negative-')as temp:
  original=json.loads(prior[0].read_text(encoding='utf-8-sig'));changed=copy.deepcopy(original)
  changed['probability'][0]=1 if changed['probability'][0]!=1 else 2
  v=changed['probability'][0];q,r=divmod(max(1,v),16);changed['costs'][0]=-(logs[q+(r>8)]if q else logs[r]-0x58b9)
  assert changed['ModelStart']==original['ModelStart']
  fake=pathlib.Path(temp)/'consistent-prob-cost.json';fake.write_text(json.dumps(changed),encoding='utf8')
  refuse_prior('consistent-prob-cost-receipt-forgery',lambda:expected(root,[fake,*prior[1:]]))
  refuse_prior('reversed-receipts',lambda:expected(root,list(reversed(prior))))
  changed=copy.deepcopy(original);changed['xy'][0][0]=False
  fake_bool=pathlib.Path(temp)/'prior-xy-bool.json';fake_bool.write_text(json.dumps(changed),encoding='utf8')
  refuse_prior('prior-xy-bool',lambda:expected(root,[fake_bool,*prior[1:]]))
  # Additionally demonstrate the strict decoder itself refuses bool even
  # independently of the authoritative whole-receipt hash failure above.
  try:prior_shape(changed,1007739)
  except AssertionError:pass
  else:raise AssertionError('strict prior bool decoder')
 print(json.dumps(dict(scope='DATA coupling to independently audited prior5; no new NN/quality/native-bit claim',captureSha=hashlib.sha256(actual.read_bytes()).hexdigest(),familyCosts=1285,finalCosts=262,ranks=len(a['rank']),symbols=len(a['symbols']),best=e['best'],shapeProduced=e['selected']is not None,negative=negative,priorNegative=prior_negative,prior=e['prior'],ownModelExecutions=0),sort_keys=True))
