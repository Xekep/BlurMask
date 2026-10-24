"""Independent BDC10 literal branch/reference geometry; no model weights."""
from pathlib import Path
import importlib.util,struct
ROOT=Path(__file__).resolve().parents[2]
def load(name,path):
 spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
parts=load('upper_scalar_parts',ROOT/'tools/LegacyEnglishMainAcoustic.Probe/reference_features.py')
geometry=load('upper_math_geometry',ROOT/'tools/LegacyMathAkTemporal.Probe/reference_temporal.py')

def vote(dx,dy):
 result=[0]*8;x,y=abs(dx),abs(dy)
 horizontal=0 if dx>=0 else 4;vertical=2 if dy>0 else 6
 diagonal=(1 if dx>=0 else 3)if dy>0 else(7 if dx>=0 else 5)
 result[horizontal if x>=y else vertical]=abs(x-y)
 result[diagonal]+=(min(x,y)*1448)>>10
 return result

def literal(dx,dy):
 r=[0]*8
 if dy<1:
  v=-dy
  if dx<0:
   x=-dx
   if v<x:r[4]+=x+dy;r[5]+=(-dy*1448)>>10
   else:r[6]+=v-x;r[5]+=(x*1448)>>10
  elif dx<v:r[6]+=v-dx;r[7]+=(dx*1448)>>10
  else:r[0]+=dx+dy;r[7]+=(-dy*1448)>>10
 elif dx<1:
  x=-dx
  if x<dy:r[2]+=dy-x;r[3]+=(x*1448)>>10
  else:r[4]+=x-dy;r[3]+=(dy*1448)>>10
 elif dy<dx:r[0]+=dx-dy;r[1]+=(dy*1448)>>10
 else:r[2]+=dy-dx;r[1]+=(dx*1448)>>10
 return r

def calculate(original,resource,uniform=False):
 lo=struct.unpack_from('<223h',resource,1628214);hi=struct.unpack_from('<223h',resource,1628660)
 normalized=geometry.normalize(original);coord=parts.coord['norm']([normalized]);samples=parts.coord['resample'](coord,100)
 points=[p for row in samples['strokes']for p in row];n=len(points);assert 1<=n<=255
 samples['width']=max(1,max(x for x,y in points));samples['height']=max(1,max(y for x,y in points))
 flags=parts.land(samples['strokes'])['flags'];case=dict(strokes=samples['strokes'],width=samples['width'],height=samples['height'],step=samples['step'],gridScale=10)
 intersections,loops=parts.cross(case);case.update(arc=samples['cumulative'][-1],flags=flags,intersections=intersections,intersectionCount=loops,cumulative=samples['cumulative'],shift=coord['shift'],unit=False)
 global_=parts.globalstat(case);ordered=[sorted(p[a]for p in points)for a in (0,1)]
 def locate(v,a):
  if uniform:
   step=(samples['width'if a==0 else 'height']<<10)//5;q=(v<<10)//step
   return q if 0<=q<5 else 4
  return next((k for k in range(5)if ordered[a][k*n//5]<=v<(ordered[a][(k+1)*n//5]if k<4 else ordered[a][-1])),4)
 groups=[[]for _ in range(25)]
 for i,(x,y)in enumerate(points):groups[5*locate(y,1)+locate(x,0)].append(i)
 assert sorted(i for g in groups for i in g)==list(range(n))
 local=[]
 for g in groups:
  hist=[0]*8
  for i in g:
   if i>=n-1 or flags[i]>>6 in (2,3):continue
   raw=literal(points[i+1][0]-points[i][0],points[i][1]-points[i+1][1])
   hist=[x+y for x,y in zip(hist,raw)]
  local.append(hist)
 denominator=samples['cumulative'][-1]//20
 #F0 active prefix: same independently restored global transform, then upper8.
 values=[1024]
 for i,v in enumerate(global_):values.append(parts.sat(parts.div(parts.i32(v*2048-2048),5)-1024 if i==2 else parts.i32(v*2)if i==8 else parts.div(parts.i32(v*2048),3072 if i==3 else 3 if 9<=i<=15 else 1024)-1024))
 for cell in local:
  values += [0 if denominator==0 else parts.sat(parts.div(parts.i32(v<<11),denominator)-1024)for v in cell]
 assert len(values)==218
 uncalibrated=values.copy()
 for i in range(1,218):
  if hi[i]!=lo[i]:values[i]=parts.sat(parts.div(parts.i32((values[i]-lo[i])*2048),hi[i]-lo[i])-1024)
 values += [-1024]*3+[1024]*2
 assert len(values)==223
 return dict(normalized=normalized,preparedCount=n,arc=samples['cumulative'][-1],denominator=denominator,members=groups,rawCells=local,uncalibrated=uncalibrated,values=values)
