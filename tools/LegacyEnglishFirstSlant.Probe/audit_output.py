"""Independent positive24C scalar, actual raw geometry and chronology audit.
Reuses prior independent raw-wire/dense geometry definitions, never SDK code.
"""
import sys
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('models-directory capture.json')
prior=Path(__file__).resolve().parents[1]/'LegacyEnglishLiteFirstPreprocessing.Probe/audit_output.py'
#The prefix contains raw-resource checks and independent function definitions;
#its old short-only capture loop is deliberately not executed on the full path.
exec(compile(prior.read_text(encoding='utf8').split('accepted=rejected=pointcount=0')[0],str(prior),'exec'))

def slant(points,minimum,maximum):
 points=[[[f(v)for v in p]for p in s]for s in points];count=direction_count(points)
 length=weighted=angle=0.
 if count>=20:
  for s in points:
   for a,b in zip(s,s[1:]):
    dx=f(b[0]-a[0]);dy=f(b[1]-a[1])
    if dy<0:dx=-dx;dy=-dy
    arg=math.atan2(dx,dy)
    if minimum<=arg<=maximum:
     weight=math.sqrt(f(f(dy*dy)+f(dx*dx)));length+=weight;weighted+=weight*arg
  angle=weighted/length if length else 0.
  c=f(math.cos(angle));s=f(math.sin(angle));flat=[p for stroke in points for p in stroke]
  cx=f(f(min(p[0]for p in flat)+max(p[0]for p in flat))*.5);cy=f(f(min(p[1]for p in flat)+max(p[1]for p in flat))*.5)
  tx=f(f(f(cy*s)-f(cx*c))+cx);ty=f(cy-f(cy*c))
  points=[[[f(f(f(p[1]*-s)+f(p[0]*c))+tx),f(f(f(p[1]*c)+f(p[0]*0.))+ty)]for p in stroke]for stroke in points]
 return count,length,weighted,angle,points

def trace(case,points,minimum,maximum,output):
 count,length,weighted,angle,result=slant(points,minimum,maximum)
 assert count==case['DirectionCount']
 for name,expected in(('AcceptedLength',length),('WeightedAngleSum',weighted),('Angle',angle)):near(case[name],expected,2e-13)
 coordinates(result,output)
 return count,result

positive=zero_weight=nonzero_angle=parity=pointcount=0
for case in capture['Cases']:
 raw=[[[f(v)for v in p]for p in s]for s in case['Raw']];original,rawkeys,metrics=source(raw)
 estimate=automatic.estimate(original,gp);rotated,line=deskew(original,estimate)
 assert original==[[[f(v)for v in p]for p in s]for s in case['Original']];coordinates(rotated,case['Deskewed'])
 for key,expected in(('Baseline',estimate['baseline']),('UpperGuideline',estimate['upper']),('Slope',estimate['slope'])):near(case['Estimate'][key],expected,2e-7)
 for key,expected in zip(('Baseline','Height'),line):near(case['Line'][key],expected)
 assert [f(case['SourceMetrics'][k])for k in('OrthogonalFraction','CornerFraction')]==metrics
 count,_=trace(case,case['Deskewed'],angles[0],angles[1],case['Slanted'])
 positive+=count>=20;parity+=count<20
 #The source guide remains the pre24C24A guide. Do not refit after shearing.
 output,keys,_,weight=smoothing['keys']([[[f(v)for v in p]for p in s]for s in case['Slanted']],sp,f(case['Line']['Height']),metrics)
 coordinates(output,case['Prepared']);assert keys==case['Keys200'];assert case['Counts']==list(map(len,output));near(weight,case['SmoothingWeight'],1e-6)
 pointcount+=len(keys);ticks=iter(keys)
 for stroke in case['Identities']:
  for identity in stroke:
   whole,remainder=divmod(next(ticks),200);index=whole+bool(remainder);k=identity['Key']
   assert[k['First'],k['Second'],k['Third']]==rawkeys[index]
   assert identity['MetadataPointIndex']==identity['KeyRecordIndex']==index
   assert identity['RemainderByte']==(200-remainder if remainder else 0)
for case in capture['PreparedControls']:
 trace(case,case['Strokes'],f(case['Minimum']),f(case['Maximum']),case['Output'])
 if case['DirectionCount']>=20:
  zero_weight+=case['AcceptedLength']==0
  nonzero_angle+=abs(case['Angle'])>1e-6
assert{19,20,21}<=set(c['DirectionCount']for c in capture['PreparedControls'])
assert zero_weight>=10 and nonzero_angle>=10
assert(positive,parity,pointcount)==(capture['Positive'],capture['OldParity'],capture['Points'])==(28,93,4204)
assert any(c['Minimum']==c['Maximum']==0 and c['DirectionCount']>=20 and c['AcceptedLength']>0 for c in capture['PreparedControls'])
assert capture['Guards']>=134 and len(capture['PreparedControls'])==198
print(f'PASS121 actual rawEN rows/{positive} positive24C/{pointcount} prepared points/93 oldparity;198 scalarcontrols/{zero_weight} zero-weight/{nonzero_angle} nonzero-angle;raw-wire+geometry+keys.')
