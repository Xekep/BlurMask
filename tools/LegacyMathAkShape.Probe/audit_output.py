"""Independent original-points299 features and raw13A6 prototype audit; no NN."""
import pathlib,json,sys,struct,hashlib,copy
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent))
import reference_shape as ref
NAMES=['point','horizontal','vertical','triangle','reverse-triangle','corner','signed','repeated','two-strokes','short-last-stroke','point-strokes','all-identical','asymmetric']
XY=[[[[3,4]]],[[[0,0],[10,0]]],[[[0,0],[0,10]]],[[[0,0],[2,1],[4,0]]],[[[4,0],[2,1],[0,0]]],[[[0,0],[0,1],[1,1]]],[[[-4,-2],[-1,3],[5,-1]]],[[[0,0],[0,0],[1,2],[1,2],[3,0]]],[[[0,0],[2,2]],[[4,0],[4,3]]],[[[0,0],[100,0]],[[0,1],[0,2]]],[[[0,0]],[[3,0]],[[3,4]]],[[[2,2],[2,2]],[[2,2]]],[[[0,0],[1,7],[2,4],[8,1],[10,10]]]]
TAIL=['feature-copy-owned','equal-geometry-foreign','equal-profile-foreign','same-shared-profile','same-prototype-owner','payload-accounting','prototype-copy-owned','targets-copy-owned','distance-copy-owned','negative-label','label259','missing-alternative','negative-alternative','precancel-feature-token','precancel-distance-token','parallel-private-inspection','foreign-source-resource','cleared-caller-resource']
GUARDS=['owned-'+n for n in NAMES]+TAIL

def ints(actual,expected):
 assert type(actual)is list and len(actual)==len(expected)
 assert all(type(a)is int and -32768<=a<=32767 and a==b for a,b in zip(actual,expected))
def fp(a,b):
 assert type(a)in(int,float)and type(a)is not bool
 assert struct.pack('<f',a)==struct.pack('<f',b),(a,b)
def validate(j,raw):
 assert j['scope']=='actual299 same-source geometry and immutable13A6 data only'
 assert type(j['modelExecutions'])is int and j['modelExecutions']==0 and j['publicClassifierAuthority']is False
 assert j['guards']==GUARDS and len(set(j['guards']))==31
 weights,prototypes=ref.raw_prototypes(raw)
 assert j['prototypes']==prototypes
 for got,want in zip(j['prototypes'],prototypes):
  assert type(got['label'])is int
  for a,b in zip(got['entries'],want['entries']):
   assert type(a['ordinal'])is int;ints(a['values'],b['values']);assert all(type(x)is int for x in a['targets'])
 assert len(j['cases'])==13
 channels=distances=0;samples=[]
 for got,name,xy in zip(j['cases'],NAMES,XY):
  assert got['name']==name and got['xy']==xy
  assert all(type(c) in (int,float) and type(c) is not bool for row in got['xy'] for point in row for c in point)
  values,count,groups,_=ref.features(xy,weights);out=got['result']
  ints(out['values'],values);assert type(out['ResampledPointCount'])is int and out['ResampledPointCount']==count
  assert type(out['ResampledStrokeCount'])is int and out['ResampledStrokeCount']==groups
  expected=[dict(label=x['label'],values=ref.distances(values,x['entries']))for x in prototypes if x['entries']]
  assert len(out['distances'])==len(expected)
  for a,b in zip(out['distances'],expected):
   assert type(a['label'])is int and a['label']==b['label']and len(a['values'])==len(b['values'])
   for v,w in zip(a['values'],b['values']):fp(v,w);distances+=1
  channels+=210;samples.append(count)
 return dict(cases=13,channels=channels,prototypeValues=39*210,prototypeTargets=66,distances=distances,guards=31,resampledCounts=samples,modelExecutions=0)

if __name__=='__main__':
 if len(sys.argv)!=3:raise SystemExit('REPOSITORY CAPTURE')
 raw=(pathlib.Path(sys.argv[1])/'models/math/math-ak.res').read_bytes()
 assert hashlib.sha256(raw).hexdigest()=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
 j=json.loads(pathlib.Path(sys.argv[2]).read_text(encoding='utf-8-sig'));r=validate(j,raw)
 controls=[]
 def corrupt(name,fn):
  q=copy.deepcopy(j);fn(q)
  try:validate(q,raw)
  except (AssertionError,ValueError,KeyError,TypeError,struct.error):controls.append(name)
  else:raise AssertionError('accepted '+name)
 corrupt('feature-channel',lambda q:q['cases'][3]['result']['values'].__setitem__(20,q['cases'][3]['result']['values'][20]^1))
 corrupt('feature-bool',lambda q:q['cases'][0]['result']['values'].__setitem__(0,True))
 corrupt('feature-row-deleted',lambda q:q['cases'][1]['result']['values'].pop())
 corrupt('original-point',lambda q:q['cases'][3]['xy'][0][1].__setitem__(0,3))
 corrupt('original-bool',lambda q:q['cases'][0]['xy'][0][0].__setitem__(0,True))
 corrupt('sample-count',lambda q:q['cases'][2]['result'].__setitem__('ResampledPointCount',29))
 corrupt('group-count',lambda q:q['cases'][8]['result'].__setitem__('ResampledStrokeCount',1))
 corrupt('distance',lambda q:q['cases'][3]['result']['distances'][0]['values'].__setitem__(0,-1))
 corrupt('distance-label',lambda q:q['cases'][3]['result']['distances'][0].__setitem__('label',1))
 corrupt('prototype-row',lambda q:q['prototypes'][0]['entries'][0]['values'].__setitem__(0,0))
 corrupt('prototype-target',lambda q:q['prototypes'][0]['entries'][0]['targets'].__setitem__(0,1))
 corrupt('prototype-factor',lambda q:q['prototypes'][0]['entries'][0].__setitem__('factor',1))
 corrupt('prototype-label-bool',lambda q:q['prototypes'][0].__setitem__('label',False))
 corrupt('guard-removed',lambda q:q['guards'].pop())
 corrupt('guard-order',lambda q:q['guards'].reverse())
 corrupt('classifier-authority',lambda q:q.__setitem__('publicClassifierAuthority',True))
 corrupt('inference-claim',lambda q:q.__setitem__('modelExecutions',1))
 r['tamperControls']=controls
 print(json.dumps(r,sort_keys=True))
