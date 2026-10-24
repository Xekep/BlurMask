"""Independent selected4F279 geometry/statistics join; no model evaluation.
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
 minimum=struct.unpack_from('<279h',resource,1458134);maximum=struct.unpack_from('<279h',resource,1458692)
 normalized=math_geometry.normalize(original);coords=parts.coord['norm']([normalized]);samples=parts.coord['resample'](coords,100)
 points=[p for stroke in samples['strokes']for p in stroke];assert 1<=len(points)<=255
 samples['width']=max(1,max(p[0]for p in points));samples['height']=max(1,max(p[1]for p in points))
 flags=parts.land(samples['strokes'])['flags'];case=dict(strokes=samples['strokes'],width=samples['width'],height=samples['height'],step=samples['step'],gridScale=10)
 intersections,loops=parts.cross(case);case.update(arc=samples['cumulative'][-1],flags=flags,intersections=intersections,intersectionCount=loops,cumulative=samples['cumulative'],shift=coords['shift'],unit=False)
 global_=parts.globalstat(case)
 def groups(quantiles):
  result=[[]for _ in range(16)]
  ordered=[sorted(p[axis]for p in points)for axis in (0,1)]
  def locate(value,axis):
   if quantiles:
    intervals=[(ordered[axis][k*len(points)//4],ordered[axis][(k+1)*len(points)//4]if k<3 else ordered[axis][-1])for k in range(4)]
    return next((k for k,(low,high)in enumerate(intervals)if low<=value<high),3)
   step=(samples['width'if axis==0 else 'height']<<10)//4
   quotient=value*1024//step
   return quotient if 0<=quotient<4 else 3
  for i,(x,y)in enumerate(points):result[locate(y,1)*4+locate(x,0)].append(i)
  return result
 membership=groups(True);local=[parts.spatialstat(dict(case,members=indices))for indices in membership]
 full=parts.assembly(global_,local,[0]*9,True,minimum[:274],maximum[:274])
 values=full[:277]+[1024,1024];assert len(values)==279
 uniform=parts.assembly(global_,[parts.spatialstat(dict(case,members=indices))for indices in groups(False)],[0]*9,True,minimum[:274],maximum[:274])[:277]+[1024,1024]
 default=parts.assembly(global_,local,[0]*9,True)[:277]+[1024,1024]
 return dict(normalized=normalized,preparedCount=len(points),values=values,defaultDifferences=sum(a!=b for a,b in zip(values,default)),uniformDifferences=sum(a!=b for a,b in zip(values,uniform)),members=membership,uniform=uniform)
