"""Independent selected3F279 geometry/statistics join; no model evaluation.
Scalar helper references are independently pinned predecessor ports; calibration
comes directly from the same actual AK, not captured channels/default tables.
"""
from pathlib import Path
import importlib.util,struct
ROOT=Path(__file__).resolve().parents[2]
def load(name,path):
 spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
parts=load('math_directional_scalar_parts',ROOT/'tools/LegacyEnglishMainAcoustic.Probe/reference_features.py')
math_geometry=load('math_directional_geometry',ROOT/'tools/LegacyMathAkTemporal.Probe/reference_temporal.py')
def calculate(original,resource):
 minimum=struct.unpack_from('<279h',resource,1288054);maximum=struct.unpack_from('<279h',resource,1288612)
 normalized=math_geometry.normalize(original);coords=parts.coord['norm']([normalized]);samples=parts.coord['resample'](coords,100)
 points=[p for stroke in samples['strokes']for p in stroke];assert 1<=len(points)<=255
 samples['width']=max(1,max(p[0]for p in points));samples['height']=max(1,max(p[1]for p in points))
 flags=parts.land(samples['strokes'])['flags'];case=dict(strokes=samples['strokes'],width=samples['width'],height=samples['height'],step=samples['step'],gridScale=10)
 intersections,loops=parts.cross(case);case.update(arc=samples['cumulative'][-1],flags=flags,intersections=intersections,intersectionCount=loops,cumulative=samples['cumulative'],shift=coords['shift'],unit=False)
 global_=parts.globalstat(case);n=len(flags);windows=[[]for _ in range(16)];lengths=samples['cumulative']
 if n<=16:
  for i in range(n):windows[i]=[i]
 else:
  step=(lengths[-1]<<10)//16;boundary=step+512;start=k=0
  for i in range(1,n):
   if lengths[i]>boundary>>10:windows[k]=list(range(start,i));k+=1;start=i;boundary+=step
  windows[k]=list(range(start,n))
 assert sorted(i for window in windows for i in window)==list(range(n))
 local=[parts.temporalstat(dict(case,members=indices))for indices in windows]
 full=parts.assembly(global_,local,[0]*9,False,minimum[:274],maximum[:274])
 values=full[:277]+[1024,1024];assert len(values)==279
 default=parts.assembly(global_,local,[0]*9,False)[:277]+[1024,1024]
 return dict(normalized=normalized,preparedCount=n,values=values,defaultDifferences=sum(a!=b for a,b in zip(values,default)))
