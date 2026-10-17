"""Independent raw-wire + dense geometry/key oracle; no research/runtime imports.
NumPy solves the Gaussian/spline systems independently of the port.
"""
import ast,hashlib,importlib.util,json,math,struct,sys
from pathlib import Path
import numpy as np
if len(sys.argv)!=3:raise SystemExit('models-directory capture.json')
repo=Path(__file__).resolve().parents[2];models=Path(sys.argv[1]);capture=json.loads(Path(sys.argv[2]).read_bytes())

def definitions(path):
 tree=ast.parse(path.read_text(encoding='utf8'))
 allowed=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))or isinstance(n,ast.Assign)and isinstance(n.value,ast.Lambda)]
 env={'__file__':str(path)};exec(compile(ast.Module(body=allowed,type_ignores=[]),str(path),'exec'),env);return env
wire=definitions(repo/'tools/LegacySource247Wire.Probe/audit_output.py')
smoothing=definitions(repo/'tools/LegacyOrientationSmoothing.Probe/reference_controls.py')
keys_ast=ast.parse((repo/'tools/LegacySmoothingPointKeys.Probe/reference_controls.py').read_text(encoding='utf8'))
exec(compile(ast.Module(body=[n for n in keys_ast.body if isinstance(n,ast.FunctionDef)and n.name=='keys'],type_ignores=[]),'independent-keys','exec'),smoothing)
spec=importlib.util.spec_from_file_location('automatic_oracle',repo/'tools/LegacyAutomaticLineEstimator.Probe/reference_controls.py')
automatic=importlib.util.module_from_spec(spec);spec.loader.exec_module(automatic)
f=lambda x:float(np.float32(x))

data=(models/capture['Path']).read_bytes();manifest={v['path']:v['sha256']for v in json.loads((models/'manifest.json').read_bytes())['resources']}
assert hashlib.sha256(data).hexdigest()==manifest[capture['Path']]
parent,_=wire['source'](data[:struct.unpack_from('<I',data,4)[0]+8]);child=next(c for c in parent.children if c['Kind']==0x249)
start=child['Offset']+16;end=child['End']-4;r,profile=wire['source'](data[start:end])
assert(start,end)==(capture['SourceOffset'],capture['SourceEnd'])and profile['SourceKind']==0x135
assert hashlib.sha256(data[start:end]).hexdigest()==capture['Fingerprint'].lower()
processors=[c for c in r.children if c['destination'].startswith('parametersC0.58[')]
assert [c['Kind']for c in processors]==[0x24a,0x24c,0x265]
for c,name in zip(processors,('Geometry','Slant','Smoothing')):
 assert(c['Offset']+start,c['End']+start)==(capture[name]['Offset'],capture[name]['End'])
gp=struct.unpack_from('<ifBffBBB',data,start+processors[0]['Offset']+16)
sp=struct.unpack_from('<8f',data,start+processors[2]['Offset']+16)
angles=struct.unpack_from('<2f',data,start+processors[1]['Offset']+16)
assert angles==tuple(f(capture['Slant'][v])for v in('MinimumSlantRadians','MaximumSlantRadians'))
for name,expected in zip(('NativeFirstField','NativeSecondField','IncludeEndpoints','MinimumSlope','MaximumSlope','SparseThreshold','DenseThreshold','ReservedByte'),gp):
 actual=capture['Geometry']['Profile'][name]
 assert (f(actual)if isinstance(actual,float)else actual)==expected
for name,expected in zip(('Minimum','Maximum','Extra','OrthogonalLow','OrthogonalHigh','CornerLow','CornerHigh','FlatnessSquaredFactor'),sp):
 assert f(capture['Smoothing']['Profile'][name])==expected
for dest in('20','21','legacyThirdPresence','22'):
 field=next(v for v in r.fields if v['owner']=='parametersC0'and v['destination']==dest)
 assert data[start+field['offset']]==0

def direction_count(strokes):
 #Run decomposition, independent of the sign-state implementation.
 count=0
 for points in strokes:
  signs=[1 if b[1]>a[1]else-1 for a,b in zip(points,points[1:])if a[1]!=b[1]]
  if signs:count+=2+sum(a!=b for a,b in zip(signs,signs[1:]))
 return count

def source(raw):
 out=[];rawkeys=[]
 def changed(a,b):return abs(a)+abs(b)<abs(f(a-b))*8388608.
 for s,points in enumerate(raw):
  retained=[]
  for i,p in enumerate(points):
   if i==0 or changed(p[0],points[i-1][0])or changed(p[1],points[i-1][1]):retained.append(p);rawkeys.append([0,s,i])
  out.append(retained)
 edges=ortho=corners=0
 for points in out:
  edges+=len(points)-1
  for i in range(1,len(points)):
   v=points[i][0]==points[i-1][0];h=points[i][1]==points[i-1][1];ortho+=int(v or h)
   if i+1<len(points):corners+=int(v and points[i][1]==points[i+1][1]or h and points[i][0]==points[i+1][0])
 return out,rawkeys,[f(ortho/edges)if edges else 0.,f(corners/(edges-len(out)))if edges>len(out)else 0.]

def deskew(strokes,estimate):
 slope=f(estimate['slope']);baseline=f(estimate['baseline']);height=f(estimate['baseline']-estimate['upper'])
 #Line.Height is the double difference cast once, before float rotation.
 cosine=f(1./f(math.sqrt(float(f(slope*slope))+1.)));sine=f(math.sqrt(float(f(1.-f(cosine*cosine)))))
 if slope>0:sine=-sine
 xs=[p[0]for s in strokes for p in s];cx=f(f(max(xs)+min(xs))*.5);cy=f(f(cx*slope)+baseline)
 ty=f(f(f(-sine*cx)-f(cy*cosine))+cy);tx=f(f(f(cy*sine)-f(cx*cosine))+cx)
 transformed_baseline=f(f(f(baseline*cosine)+f(sine*0.))+ty)
 transformed_height=f(f(f(f(f(baseline+height)*cosine)+f(sine*0.))+ty)-transformed_baseline)
 if abs(transformed_height)<=max(f(abs(transformed_height)*2.3841858e-7),1.1920929e-7):
  fallback=f(f(max(xs)-min(xs))*.15);transformed_height=max(fallback,f(.1));transformed_baseline=f(transformed_baseline-f(fallback*.5))
 points=[[[f(f(f(p[1]*-sine)+f(p[0]*cosine))+tx),f(f(f(p[1]*cosine)+f(p[0]*sine))+ty)]for p in s]for s in strokes]
 return points,[transformed_baseline,transformed_height]

def near(a,b,tolerance=2e-6):
 assert abs(a-b)<=tolerance*max(1.,abs(a),abs(b)),(a,b)
def coordinates(a,b):
 assert len(a)==len(b)
 for x,y in zip(a,b):
  assert len(x)==len(y),(len(x),len(y))
  for p,q in zip(x,y):
   for v,w in zip(p,q):near(v,w)

accepted=rejected=pointcount=0
for case in capture['Cases']:
 raw=[[[f(v)for v in p]for p in s]for s in case['Raw']];original,rawkeys,metrics=source(raw)
 estimate=automatic.estimate(original,gp);rotated,line=deskew(original,estimate);count=direction_count(rotated)
 assert case['RejectedPositive24C']==(count>=20)
 if count>=20:rejected+=1;continue
 accepted+=1;assert case['SlantDirectionCount']==count
 assert original==[[[f(v)for v in p]for p in s]for s in case['Original']];coordinates(rotated,case['Deskewed'])
 for key,expected in(('Baseline',estimate['baseline']),('UpperGuideline',estimate['upper']),('Slope',estimate['slope'])):near(case['Estimate'][key],expected,2e-7)
 assert case['Estimate']['VerticalRunCount']==estimate['runs']and case['Estimate']['UsedDenseFit']==estimate['dense']and case['Estimate']['BlendedSparseStatistics']==estimate['blended']
 for actual,expected in zip((case['Line']['Baseline'],case['Line']['Height']),line):near(actual,expected)
 assert [f(case['SourceMetrics'][v])for v in('OrthogonalFraction','CornerFraction')]==metrics
 #Use the independently checked float boundary as the input to the separate
 #dense-spline oracle: IRLS solver rounding is not an invented native bit claim.
 output,keys,derived,weight=smoothing['keys']([[[f(v)for v in p]for p in s]for s in case['Deskewed']],sp,f(case['Line']['Height']),metrics)
 coordinates(output,case['Prepared']);assert keys==case['Keys200'];near(weight,case['SmoothingWeight'],1e-6)
 assert case['Counts']==list(map(len,output));pointcount+=sum(case['Counts'])
 ticks=iter(keys)
 for stroke in case['Identities']:
  for identity in stroke:
   tick=next(ticks);whole,remainder=divmod(tick,200);ordinal=whole+bool(remainder)
   key=identity['Key'];assert [key['First'],key['Second'],key['Third']]==rawkeys[ordinal]
   assert identity['MetadataPointIndex']==identity['KeyRecordIndex']==ordinal
   assert identity['RemainderByte']==(200-remainder if remainder else 0)
for item in capture['CounterControls']:assert item['Count']==direction_count(item['Strokes'])
assert {19,20,21}<=set(v['Count']for v in capture['CounterControls'])
assert(accepted,rejected,pointcount)==(capture['Accepted'],capture['Rejected'],capture['Points'])
assert accepted>=30 and rejected>=3 and capture['Guards']>=194
print(f'PASS raw ownedEN wire + independent24A/deskew/24C/265/keys/chronology: {accepted} actual inputs/{pointcount} points, {rejected} positive24C refusals, {len(capture["CounterControls"])} prepared counters.')
