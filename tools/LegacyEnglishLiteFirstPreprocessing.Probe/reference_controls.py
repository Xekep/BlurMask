"""Raw geometry only; no expected text/feature or output injected into inference."""
import json,math,random,sys
from pathlib import Path
random.seed(176265)
cases=[]
for n in range(1,23):
 for tilt in (0.,.35):
  points=[[float(i*7),float((i%2)*40+tilt*i*7)] for i in range(n)]
  cases.append([points])
for trial in range(48):
 strokes=[]
 for s in range(1+trial%4):
  #Gentle curves, exact horizontal/vertical corners and repeated raw points.
  p=[[float(s*70+i*6),float(math.sin(i*.8+trial)*12+s*5)] for i in range(1+trial%7)]
  if trial%5==0:p=[[float(s*70),0.],[float(s*70),30.],[float(s*70+20),30.]]
  if trial%3==0:p=[v for x in p for v in(x,x)]
  strokes.append(p)
 cases.append(strokes)
cases.extend([[[[0.,0.]]],[[[0.,0.],[100.,0.]]],[[[1.,2.],[1.,2.]]],
              [[[0.,0.],[30.,1e-8],[60.,0.],[90.,0.]]]])
counters=[]
for n in range(0,35):
 p=[[float(i),float(i%2)] for i in range(n+1)]
 counters.extend([[p],[[[float(i),3.] for i in range(n+1)]],[p,p],[[v for x in p for v in(x,x)]]])
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(cases=cases,counterControls=counters),separators=(',',':')))
print(f'{len(cases)} raw sources, {len(counters)} explicit prepared24C counter controls -> {out}')
