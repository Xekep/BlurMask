"""Prepared adjustment DATA from raw13A6 and independently recomputed original geometry. No NN."""
import pathlib,sys,json,math,struct,hashlib,copy
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent))
import reference_shape as r
INF=0x4000000
XY=[[[0,0],[2,1],[4,0]]]
def initial(label):
 a=[5000]*262;a[label]=1000;return a
def blend(a,label,targets,factor):
 a=a.copy()
 if factor is None:return a
 for t in targets:
  if t==65535:continue
  v=r.add(r.mul(a[label],factor),r.mul(r.sub(1,factor),a[t])) if factor<1 else r.div(a[label],factor)
  a[t]=math.trunc(r.add(v,-.5 if v<=0 else .5))
 return a
def ints(got,want):
 assert type(got)is list and len(got)==len(want)
 assert all(type(a)is int and a==b for a,b in zip(got,want))
def validate(j,raw):
 assert j['scope']=='prepared numeric DATA only; genuine source/prototypes; no evaluated Result'
 assert type(j['modelExecutions'])is int and j['modelExecutions']==0 and j['HasGrammarCandidateAuthority']is False
 assert j['xy']==XY[0] and all(type(c)is int for p in j['xy'] for c in p)
 weights,proto=r.raw_prototypes(raw);values,*_=r.features(XY,weights)
 labels=[p['label']for p in proto if p['entries']]
 assert len(j['adjustments'])==len(labels)
 for got,label in zip(j['adjustments'],labels):
  es=proto[label]['entries'];ds=r.distances(values,es)if len(es)>1 else []
  selected=min(range(len(ds)),key=lambda i:ds[i])if ds else 0
  for key,want in [('label',label),('BestLabel',label),('Alternatives',len(es)),('SelectedAlternative',selected),('GeometryCalls',1)]:
   assert type(got[key])is int and got[key]==want
  assert len(got['Distances'])==len(ds)
  for a,b in zip(got['Distances'],ds):assert type(a)in(int,float)and struct.pack('<f',a)==struct.pack('<f',b)
  ints(got['Costs'],blend(initial(label),label,es[selected]['targets'],es[selected]['factor']))
 entries=[(p['label'],e)for p in proto for e in p['entries']]
 assert len(entries)==39 and len(j['blends'])==39
 for got,(label,e)in zip(j['blends'],entries):
  assert type(got['label'])is int and got['label']==label and type(got['ordinal'])is int and got['ordinal']==e['ordinal']
  ints(got['targets'],e['targets']);assert got['factor']==e['factor']
  ints(got['costs'],blend(initial(label),label,e['targets'],e['factor']))
 tail=['no-alternatives-no-geometry','no-best-no-geometry','closest-first-tie','live-best-self-alias','negative-midpoint-away','bad-width','bad-cost','nan-distance','nan-factor','bad-target','foreign-profile-before-features','precancel-exact-token','numeric-data-copy-only','cleared-resource-owner']
 assert j['guards']==['input-unchanged-'+str(i)for i in labels]+tail
 ints(j['alias'],blend(initial(0),0,[0,1],2))
 signed=initial(0);signed[0]=-3;ints(j['signed'],blend(signed,0,[1],2))
 return dict(labels=len(labels),prototypeEntries=39,emptyTargetEntries=sum(not e['targets']for _,e in entries),singleAlternativeLabels=sum(len(p['entries'])==1 for p in proto),guards=len(j['guards']),modelExecutions=0)
if __name__=='__main__':
 if len(sys.argv)!=3:raise SystemExit('REPOSITORY CAPTURE')
 raw=(pathlib.Path(sys.argv[1])/'models/math/math-ak.res').read_bytes()
 assert hashlib.sha256(raw).hexdigest()=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
 j=json.loads(pathlib.Path(sys.argv[2]).read_text(encoding='utf-8-sig'));report=validate(j,raw);negative=[]
 def reject(name,fn):
  v=copy.deepcopy(j);fn(v)
  try:validate(v,raw)
  except(AssertionError,TypeError,ValueError,KeyError,IndexError,struct.error):negative.append(name)
  else:raise AssertionError('accepted '+name)
 reject('cost',lambda v:v['adjustments'][0]['Costs'].__setitem__(0,-1))
 reject('best-bool',lambda v:v['adjustments'][0].__setitem__('BestLabel',False))
 reject('distance',lambda v:v['adjustments'][1]['Distances'].__setitem__(0,-1))
 reject('one-alt-geometry-skipped',lambda v:v['adjustments'][0].__setitem__('GeometryCalls',0))
 reject('entry-deleted',lambda v:v['blends'].pop())
 reject('target',lambda v:v['blends'][0]['targets'].__setitem__(0,258))
 reject('factor',lambda v:v['blends'][0].__setitem__('factor',1))
 reject('alias-cached-best',lambda v:v['alias'].__setitem__(1,500))
 reject('guard-omitted',lambda v:v['guards'].pop())
 reject('model-execution',lambda v:v.__setitem__('modelExecutions',1))
 print(json.dumps(dict(report=report,negative=negative),sort_keys=True))
