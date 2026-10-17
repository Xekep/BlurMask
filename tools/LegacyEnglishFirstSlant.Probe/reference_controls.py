"""Actual raw rows and separately prepared angle-boundary arithmetic controls."""
import json,math,struct,sys
from pathlib import Path
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
def adjacent(v,step):return struct.unpack('<f',struct.pack('<I',struct.unpack('<I',struct.pack('<f',v))[0]+step))[0]
base=json.loads(Path(sys.argv[1]).read_bytes());cases=base['cases'][:]
for trial in range(24):
 p=[]
 for i in range(19+trial%9):
  y=float((i%2)*80+50);p.append([f(100+i*2+y*(.12+trial*.01)),y])
 cases.append([p])
cases.append([[[float(100+s*30),50.],[float(120+s*30),130.]]for s in range(10)])
prepared=[]
for n in(18,19,20,21,24,31):
 for flip in(-1,1):
  for lean in(0.,.1,.25,1.):
   p=[[f(125+i*3+lean*flip*(i%2)*80),f(45+flip*(i%2)*80)]for i in range(n)]
   for minimum,maximum in((-.5235988,.5235988),(0.,0.),(.4,.5),(-2.,2.)):
    prepared.append(dict(strokes=[p],minimum=f(minimum),maximum=f(maximum)))
#Counter across physical strokes, nonzero center, exactzero-angle bound,
#horizontal edges, and two adjacentfloatbounds aroundatan2(1,4).
angle=f(math.atan2(1.,4.));low=adjacent(angle,-1);high=adjacent(angle,1)
for bounds in((low,high),(high,f(.5)),(f(-.5),low),(0.,0.)):
 strokes=[[[f(100+s*2),50.],[f(101+s*2),54.],[f(100+s*2),50.]]for s in range(7)]
 prepared.append(dict(strokes=strokes,minimum=bounds[0],maximum=bounds[1]))
prepared.append(dict(strokes=[[[float(i),3.]for i in range(50)]],minimum=-.5,maximum=.5))
prepared.append(dict(strokes=[[[123.,float(50+(i%2)*80)]for i in range(21)]],minimum=0.,maximum=0.))
out=Path(sys.argv[2]);out.write_text(json.dumps(dict(cases=cases,preparedControls=prepared),separators=(',',':')))
print(f'{len(cases)} actual raw rows/{len(prepared)} explicitly prepared angle controls -> {out}')
