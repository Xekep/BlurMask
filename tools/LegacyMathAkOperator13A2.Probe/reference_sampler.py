"""Independent literal Math13A2 numeric composition, never C#/NN/vendor execution.
CLI: DEPENDENCY_ROOT MODELS ORIGINAL14_FIXTURES FRESH_REPORT
"""
from pathlib import Path
import sys,json,hashlib,struct,math,heapq,random,runpy,importlib.util
D=Path(__file__).resolve().parent

def load(path,name):
 spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m

def definitions(root):
 p=root/'tools/LegacyCursiveProducer.Probe/reference_controls.py';text=p.read_text(encoding='utf-8-sig');part=text[:text.index('def make(rng):')];env={'__file__':str(p)}
 exec(compile(part,str(p),'exec'),env);return env

def limiter(rows,counts,original,height,cvt):
 f,merge,cvtt,i32,divide=[cvt[n]for n in ['f','merge','cvtt','i32','divide']];visible=lambda t:t!=0 and not 11<=t<=14
 tags=[r[5]for r in rows];start=0;traces=[]
 for stroke,count in enumerate(counts):
  end=start+count;indices=[i for i in range(start,end)if visible(tags[i])];trace=dict(markerCount=len(indices),attempts=0,removed=[])
  if tags[start]==1 and len(indices)>6:
   raw=original[stroke];minimum=maximum=raw[0][0]
   for p in raw[1:]:
    if maximum<=p[0]:maximum=p[0]
    if not minimum<=p[0]:minimum=p[0]
   target=cvtt(divide(f(f(maximum-minimum)*4),height));trace['target']=target
   if target<len(indices):
    weights=[rows[start][4]];at=0
    for i in range(start+1,end):
     if visible(tags[i]):weights.append(rows[i][4]);at+=1
     weights[at]=f(weights[at]+rows[i][4])
    assert len(weights)==len(indices)
    attempts=i32(len(indices)-target);trace['attempts']=attempts
    removed=linked_limit(indices,weights,tags,attempts,merge,f)
    for i in removed:tags[i]=0
    trace['removed']=removed
  traces.append(trace);start=end
 return tags,traces

def linked_limit(indices,weights,tags,attempts,merge,f):
 # Exact first-minimum tie order: surviving entries retain increasing source indices.
 # Only neighbours' costs change after the literal left-weight replacement.
 n=len(indices);previous=[i-1 for i in range(n)];following=[i+1 for i in range(n)];alive=[True]*n;versions=[0]*n;heap=[];removed=[]
 maximum=f(3.4028234663852886e38)
 def push(i):
  if not 0<=i<n:return
  versions[i]+=1;p=previous[i]
  if not alive[i]or p<0 or not alive[p]or tags[indices[i]]in [7,4]:return
  cost=merge(weights[p],weights[i])
  if cost<maximum:heapq.heappush(heap,(cost,indices[i],i,versions[i]))
 for i in range(1,n):push(i)
 for attempt in range(max(attempts,0)):
  while heap and(not alive[heap[0][2]]or versions[heap[0][2]]!=heap[0][3]):heapq.heappop(heap)
  if not heap:break # All remaining native fixed attempts have no selection and no writes.
  cost,_,i,_=heapq.heappop(heap);p=previous[i];q=following[i]
  removed.append(indices[i]);alive[i]=False;following[p]=q
  if q<n:previous[q]=p
  weights[p]=cost;push(p);push(q)
 return removed

def heap_controls(cvt):
 rng=random.Random(0x13a2074a70)
 for case in range(256):
  n=rng.randrange(2,32);tags=[1]+[rng.choice([2,4,7,8])for _ in range(n-1)];weights=[cvt['f'](rng.choice([0.,.125,1.,2.,100.,3.4028234663852886e38]))for _ in range(n)];target=rng.randrange(0,n)
  expected=cvt['limit'](tags,weights,target)['removed'];actual=linked_limit(list(range(n)),weights.copy(),tags,n-target,cvt['merge'],cvt['f']);assert actual==expected,(case,actual,expected)
 return 256

def execute(spec,policy,ref,env,cvt,metric):
 upstream=ref.execute(spec,policy);original=[[list(p)for p in row]for row in upstream['points']];height=upstream['scale'];f=env['f'];bits=env['bits'];Stage=env['Stage'];state=Stage(original,[int(i==0)for row in original for i in range(len(row))],0);shortcuts=[]
 for stroke in range(len(original)):
  state.stroke=stroke;state.start=sum(state.counts[:stroke]);direction=0
  for i in range(state.start,state.end-1):
   r=state.rows[i]
   if r[3]<0:
    if direction==-1:r[5]=7
    direction=1
   elif r[3]>0:
    if direction==1 and i>0 and state.rows[i-1][2]>0:r[5]=8
    direction=-1
  # Strictly monotone X proves disjoint X intervals for every nonadjacent
  # segment pair. This skips no possible intersection and avoids quadratic
  # Python work on unchanged8192/16384-point controls; no class/output fitting.
  row=state.rows[state.start:state.end];dx=[b[0]-a[0]for a,b in zip(row,row[1:])]
  monotone=bool(dx)and(all(x>0 for x in dx)or all(x<0 for x in dx))
  if monotone:shortcuts.append(stroke)
  else:env['loops'](state,height)
  state.run(height,False,True);state.run(height,True,True)
  env['filter_stage'](state,env['pre']);env['baseline_stage'](state,height,None);env['filter_stage'](state,env['post'],height,None)
  # Original-header bbox is scanned by073F00, then unknown FLT_MAX returns.
  assert all(math.isfinite(v)for p in original[stroke]for v in p)
 tags=env['prune'](state.rows,height)
 for r,t in zip(state.rows,tags):r[5]=t
 tags,traces=limiter(state.rows,state.counts,original,height,cvt)
 for r,t in zip(state.rows,tags):r[5]=t
 starts=[0]
 for row in original:starts.append(starts[-1]+len(row))
 marker=[];ranges=[];ends=[];points=[];maps=[];at=0
 for stroke,count in enumerate(state.counts):
  for local in range(count):
   r=state.rows[at];assert r[6]==stroke;old=r[7]-starts[stroke];assert 0<=old<len(upstream['originalIndices'][stroke]);points.extend([bits(r[0]),bits(r[1])]);maps.append(upstream['originalIndices'][stroke][old])
   if r[5]!=0 and not 11<=r[5]<=14:marker.append([r[5],stroke,local])
   at+=1
  ends.append(at)
 for i,(_,stroke,start)in enumerate(marker):
  end=marker[i+1][2]+1 if i+1<len(marker)and marker[i+1][1]==stroke else state.counts[stroke];ranges.append([stroke,start,end])
 return dict(name=spec['name'],HeightBits=bits(height),BeforePointBits=[bits(v)for row in original for p in row for v in p],BeforeOriginalIndices=[v for row in upstream['originalIndices']for v in row],BeforeEnds=starts[1:],PointBits=points,OriginalIndices=maps,Ends=ends,MarkerFields=[v for row in marker for v in row],RangeFields=[v for row in ranges for v in row],MetricBits=[bits(v)for row in original for v in metric(row)],upstreamModes=upstream['modes'],monotoneIntersectionShortcuts=shortcuts,limiterTrace=traces)

def main(root,models,fixture):
 for p in json.loads((D/'reference-dependencies.json').read_text())['files']:assert hashlib.sha256((root/p['relative']).read_bytes().replace(b'\r\n',b'\n')).hexdigest()==p['lfSha256']
 old=sys.argv[:];sys.argv=[str(root/'tools/LegacyMathAkOperator13A0.Probe/reference_13a0.py'),str(root)]
 try:ref=load(root/'tools/LegacyMathAkOperator13A0.Probe/reference_13a0.py','reference13a0')
 finally:sys.argv=old
 env=definitions(root);cvt=runpy.run_path(str(D/'reference_limiter.py'));metric=runpy.run_path(str(D/'reference_metrics.py'))['metrics'];raw=(models/'math/math-ak.res').read_bytes();assert hashlib.sha256(raw).hexdigest()=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
 policy=struct.unpack_from('<8f',raw,7317);assert struct.unpack_from('<4f',raw,7381)==tuple(env['f'](x)for x in [.5,.35,.2,1.8]);assert struct.unpack_from('<6f',raw,7400)==tuple(env['f'](x)for x in [.35,.2,.25,.5,.5,.1]);assert raw[7429:7432]==bytes(3);assert raw[7397:7400]==b'\1\1\1'and raw[7424]==6 and struct.unpack_from('<f',raw,7425)[0]==4
 specs=json.loads(fixture.read_text(encoding='utf-8-sig'))['cases'];assert len(specs)==14
 return dict(resourceSHA=hashlib.sha256(raw).hexdigest(),fixtureSHA=hashlib.sha256(fixture.read_bytes()).hexdigest(),heapEquivalentControls=heap_controls(cvt),limiterExceptionalControls=cvt['controls']()['totalControls'],cases=[execute(c,policy,ref,env,cvt,metric)for c in specs],networkForwards=0,sourceExecution=False,scope='Independent scalar expectations; inherited13A0 small dense algebra/transcendental coordinate tolerance remains separate from exact discrete maps/counts')
if __name__=='__main__':
 report=Path(sys.argv[4]);assert not report.exists();result=main(Path(sys.argv[1]),Path(sys.argv[2]),Path(sys.argv[3]));report.write_text(json.dumps(result,indent=2)+'\n',encoding='utf8');print(json.dumps(dict(cases=len(result['cases']),heapControls=result['heapEquivalentControls'],exceptionalControls=result['limiterExceptionalControls'],networkForwards=0)))
