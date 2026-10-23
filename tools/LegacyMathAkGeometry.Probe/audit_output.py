"""Independent00FB90 actual-mode1/1 whole-range geometry check. No model execution."""
from pathlib import Path
import argparse,hashlib,json,math,struct,copy
p=argparse.ArgumentParser();p.add_argument('capture',type=Path);p.add_argument('output',type=Path);a=p.parse_args();r=json.loads(a.capture.read_text())
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
GUARDS=['count-read-once-before-allocation','caller-input-detached','returned-output-detached','ends-detached','resource-mutation','resource-truncated','empty-source','empty-stroke','nan-source','point-over-cap','stroke-over-cap','line-short-overflow','cancel-exact-token','point-cap-positive']
def check(r):
 assert r['resourceSHA256']=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
 assert r['guards']==GUARDS and r['modelExecutions']==0 and r['vendorExecution']is False
 assert [c['name']for c in r['cases']]==['point','horizontal-half','vertical','nonsquare','fractional','two-strokes','duplicates-retained']
 for c in r['cases']:
  rows=[[[f(v)for v in p]for p in row]for row in c['original']];flat=[p for row in rows for p in row]
  lo=[min(p[k]for p in flat)for k in (0,1)];hi=[max(p[k]for p in flat)for k in (0,1)];w,h=[f(hi[k]-lo[k])for k in (0,1)]
  scale=f(1)
  if w!=0 and h!=0:scale=min(f(f(8192)/w),f(f(8192)/h))
  want=[]
  for row in rows:
   dest=[]
   for p in row:
    v=[f(f(f(p[k]-lo[k])*scale)+f(0))for k in (0,1)]
    dest.append([math.trunc(f(x-f(.5)))if x<=0 else math.trunc(f(x+f(.5)))for x in v])
   want.append(dest)
  ends=[];at=0
  for row in rows:at+=len(row);ends.append(at)
  assert c['normalized']==want and f(c['scale'])==scale and c['ends']==ends
  assert c['min']==lo and c['max']==hi and c['PointCount']==len(flat)and c['StrokeCount']==len(rows)
  assert c['NormalizationTarget']==8192 and c['HasCandidateAuthority']is False and c['HasClassifierOutput']is False
check(r);controls=[]
for name,change in [('normalized-coordinate',lambda z:z['cases'][3]['normalized'][0][1].__setitem__(0,1)),('degenerate-scale',lambda z:z['cases'][1].__setitem__('scale',8192)),('source-boundary',lambda z:z['cases'][5]['ends'].__setitem__(0,1)),('removed-duplicate',lambda z:z['cases'][6]['normalized'][0].pop()),('authority-promotion',lambda z:z['cases'][0].__setitem__('HasCandidateAuthority',True)),('guard-omission',lambda z:z['guards'].pop())]:
 z=copy.deepcopy(r);change(z)
 try:check(z)
 except AssertionError:controls.append(name)
 else:raise AssertionError(name)
out=dict(captureSHA256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),geometryCases=7,guards=14,corruptions=controls,modelExecutions=0,scope='Independent native F32 normalization only; no feature vectors, classifier outputs or tree authority')
a.output.write_text(json.dumps(out,indent=2)+'\n');print('PASS 7 cases / 14 guards / 6 corruptions / 0 model executions')
