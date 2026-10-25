"""PURE native-derived FIRST geometry composition, no managed/vendor/model execution."""
from pathlib import Path
import importlib.util,json,struct,math,hashlib
D=Path(__file__).resolve().parent;R=Path(globals().get('REPO_ROOT',Path(__file__).resolve().parents[2]))
p=R/'tools/LegacyEnglishMainAcoustic.Probe/reference_features.py';spec=importlib.util.spec_from_file_location('first_scalar_parts',p);parts=importlib.util.module_from_spec(spec);spec.loader.exec_module(parts)
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
def i32(x):return (x+2**31)%2**32-2**31
def div(a,b):return (-1 if (a<0)!=(b<0) else 1)*(abs(a)//abs(b))
def sat(x):return max(-32768,min(32767,x))
def ratio(v,d):return i32(div(i32(v*2048),d)-1024)
raw=(R/'models/math/math-ak.res').read_bytes();assert hashlib.sha256(raw).hexdigest()=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
lo=struct.unpack_from('<276h',raw,7593);hi=struct.unpack_from('<276h',raw,8145);assert struct.unpack_from('<H',raw,8733)[0]==10

def normalize(rows):
 points=[[f(c) for c in p] for row in rows for p in row];low=[min(p[a]for p in points) for a in (0,1)];high=[max(p[a]for p in points)for a in(0,1)]
 width=f(high[0]-low[0]);height=f(high[1]-low[1]);scale=f(1)
 if width!=0 and height!=0:
  scale=f(8192/width);ys=f(8192/height)
  if ys<=scale:scale=ys
 out=[]
 for row in rows:
  q=[]
  for xy in row:
   value=[]
   for a in(0,1):
    v=f(f(f(f(xy[a])-low[a])*scale)+0.0);z=f(v-.5) if v<=0 else f(v+.5)
    if not math.isfinite(z) or z < -32768 or z>=32768:raise ValueError('nonwrapped SOURCE subset')
    value.append(math.trunc(z))
   q.append(value)
  out.append(q)
 return out,scale

def compose(global_,windows):
 assert len(global_)==17 and len(windows)==16 and all(len(v)==16 for v in windows)
 values=[1024]
 for i,v in enumerate(global_):
  x=i32(div(i32(i32(v*2048)-2048),5)-1024) if i==2 else ratio(v,3072) if i==3 else i32(v*2) if i==8 else ratio(v,3) if 9<=i<=15 else ratio(v,1024)
  values.append(sat(x))
 for window in windows:
  for i,v in enumerate(window):
   x=i32(i32(v*2048)-1024) if i in (4,5,9,10,11,12,13,14) else i32(v*2) if i==8 else ratio(v,128) if i==15 else ratio(v,1024)
   values.append(sat(x))
 assert len(values)==274
 for i in range(1,274):
  span=hi[i]-lo[i]
  if span:values[i]=sat(div((values[i]-lo[i])*2048,span)-1024)
 return values

def calculate(rows,first_start=0,last_end=None,last_length=None):
 norm,scale=normalize(rows);coords=parts.coord['norm'](norm);samples=parts.coord['resample'](coords,100)
 points=[p for row in samples['strokes']for p in row];assert 1<=len(points)<=255
 samples['width']=max(1,max(p[0]for p in points));samples['height']=max(1,max(p[1]for p in points))
 flags=parts.land(samples['strokes'])['flags'];case=dict(strokes=samples['strokes'],width=samples['width'],height=samples['height'],step=samples['step'],gridScale=10)
 inter,loops=parts.cross(case);case.update(arc=samples['cumulative'][-1],flags=flags,intersections=inter,intersectionCount=loops,cumulative=samples['cumulative'],shift=coords['shift'],unit=False)
 g=parts.globalstat(case);n=len(points);lengths=samples['cumulative'];windows=[[]for _ in range(16)]
 if n<=16:
  for i in range(n):windows[i]=[i]
 else:
  step=(lengths[-1]<<10)//16;boundary=step+512;start=k=0
  for i in range(1,n):
   if lengths[i] > boundary>>10:windows[k]=list(range(start,i));k+=1;start=i;boundary+=step
  windows[k]=list(range(start,n))
 assert sorted(v for w in windows for v in w)==list(range(n))
 local=[parts.temporalstat(dict(case,members=w)) for w in windows];values=compose(g,local)
 if last_end is None:last_end=len(rows[-1])-1
 if last_length is None:last_length=len(rows[-1])
 values += [1024 if first_start==0 else -1024,1024 if last_end+1==last_length else -1024]
 return {'normalized':norm,'preparedPointCount':n,'scaleBits':struct.unpack('<I',struct.pack('<f',scale))[0],'values':values,'valuesSHA256':hashlib.sha256(struct.pack('<276h',*values)).hexdigest()}

if __name__=='__main__':
 fixtures=[('point',[[[4,9]]]),('repeated',[[[4,9],[4,9],[4,9]]]),('horizontal',[[[0,0],[8,0],[16,0]]]),('vertical',[[[0,0],[0,8],[0,16]]]),('corner',[[[0,0],[2,6],[8,0]]]),('two-physical-strokes',[[[0,0],[4,8]],[[10,4],[14,0]]])]
 out=[]
 for name,rows in fixtures:
  value=calculate(rows);assert len(value['values'])==276 and value['values'][0]==1024 and value['values'][-2:]==[1024,1024];out.append({'name':name,**value})
 for start,end,length,want in [(0,2,3,[1024,1024]),(1,2,3,[-1024,1024]),(0,1,3,[1024,-1024]),(1,1,3,[-1024,-1024])]:
  assert calculate([[[0,0],[8,0],[16,0]]],start,end,length)['values'][-2:]==want
 assert out[0]['preparedPointCount']==1 and out[1]['preparedPointCount']==1
 (D/'pure-feature-reference.json').write_bytes((json.dumps({'scope':'Prepared numerical DATA only, not authority/CSharp/runtime parity or learned evaluation','fixtures':out,'boundaryControls':4,'modelsExecuted':0},indent=2)+'\n').encode())
 print('PASS 6 prepared FIRST276 numerical projections, 4 SAME-source endpoint policy controls; no CSharp/source/model/NN')
