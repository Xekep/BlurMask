"""Generate bounded PREPARED numeric controls; no NN or runtime imports."""
import argparse,base64,hashlib,json,random,struct,sys
from pathlib import Path
sys.dont_write_bytecode=True;sys.path.insert(0,str(Path(__file__).resolve().parent))
from raw_profile import read_profile
from reference_rows import f,produce,overwrite_flat
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
profile=read_profile(a.models/'en-US/en_US-ak-cur.res');cases=[]
def bits(x):return struct.unpack('<i',struct.pack('<f',f(x)))[0]
def encoded(rows):
 out=bytearray()
 for label,prob,score,cost in rows:
  row=bytearray(40);struct.pack_into('<H',row,8,label);struct.pack_into('<ffi',row,12,f(prob),f(score),cost);out.extend(row)
 return base64.b64encode(out).decode()
def add(name,points,mapping,ranges,records,selected=0,**extra):
 case=dict(Name=name,Bits=[[[bits(x),bits(y)] for x,y in s] for s in points],Mapping=mapping,Ranges=ranges,Records=records,Selected=selected,**extra)
 if 'ExceptionType' not in extra:
  expected,projection,count,visits=produce(profile,case['Bits'],mapping,ranges,records,selected)
  case.update(Expected=base64.b64encode(expected).decode(),Projected=projection,LabelCount=count,PointVisits=visits)
 cases.append(case)
for k in range(256):
 add('pow75-k255-'+str(k),[[(-1,2)]],[0],[[0,0,0,0,0,0]],encoded([(10,k/255,17/16,100+k),(10,.5,-2,-10),(14,.25,3,777)]))
add('pow-negative-zero',[[(-0.,-0.),(0.,0.)]],[0],[[0,0,0,0,0,1]],encoded([(10,-0.,1,0)]))
add('zero-order-reversed',[[ (0.,0.),(-0.,-0.)]],[0],[[0,0,0,0,0,1]],encoded([(10,1,1,0)]))
add('selected-last-duplicate-label',[[(-4,-3),(7,11)]],[0],[[0,0,0,0,0,1]],encoded([(10,.25,-1,5),(14,.5,2,6),(10,1,3,7)]),2)
add('twenty-first-distinct',[[(-4,-3),(7,11)]],[0],[[0,0,0,0,0,1]],encoded([(i,1/16,i/8,i*8192) for i in range(40)]),39)
add('flat-overwrite40-generic',[[(-4,-3),(7,11)]],[0],[[0,0,0,0,0,1]],overwrite_flat(profile))
add('cropped-discontinuous',[[(-4,-3),(0,0)],[(20,30)],[(7,11),(10,14)]],[0,2],[[0,0,1,0,1,0]],encoded([(10,.25,3,7)]))
add('repeated-overlapping-membership',[[(-4,-3),(7,11),(0,0)]],[0],[[0,0,0,0,0,2],[0,0,1,0,0,2]],encoded([(14,.5,4,8)]))
maximum=struct.unpack('<f',bytes.fromhex('ffff7f7f'))[0]
add('finite-coordinate-extrema',[[(-maximum,maximum),(maximum,-maximum)]],[0],[[0,0,0,0,0,1]],encoded([(10,1,1,0)]))
rng=random.Random(156950)
for i in range(64):
 n=rng.randrange(1,12);points=[[(f(rng.randrange(-400,401)/8),f(rng.randrange(-400,401)/8)) for _ in range(rng.randrange(1,10))] for _ in range(n)]
 mapping=sorted(rng.sample(range(n),rng.randrange(1,n+1)));ranges=[]
 for _ in range(rng.randrange(1,6)):
  first=rng.randrange(len(mapping));last=rng.randrange(first,len(mapping));start=rng.randrange(len(points[mapping[first]]));end=rng.randrange(start if first==last else 0,len(points[mapping[last]]));ranges.append([0,first,start,0,last,end])
 rows=[(rng.randrange(220),rng.randrange(256)/255,rng.randrange(-100,101)/8,rng.randrange(-20000,20001)) for _ in range(rng.randrange(1,48))]
 add('membership-'+str(i),points,mapping,ranges,encoded(rows),rng.randrange(len(rows)))
for name,selected,extra in [('negative-signed-selection',-1,{}),('high-selection',32767,{}),('canceled',0,{'Cancel':True}),('null-parent',0,{'NullParent':True})]:
 add(name,[[(0,0)]],[0],[[0,0,0,0,0,0]],encoded([(10,1,1,0)]),selected,ExceptionType='OperationCanceledException' if name=='canceled' else 'ArgumentNullException' if name=='null-parent' else 'ArgumentException',**extra)
report=dict(PreparedOnly=True,NNEvaluated=False,TargetsRead=False,ModelSHA256=profile['ResourceSHA256'],Cases=cases,Scope='Generic numeric inputs, not actual typedB8 owners/words; no transcriptions')
a.output.write_text(json.dumps(report,separators=(',',':'))+'\n');print(json.dumps(dict(Status='GENERATED_PREPARED_ONLY',Cases=len(cases),SHA256=hashlib.sha256(a.output.read_bytes()).hexdigest())))
