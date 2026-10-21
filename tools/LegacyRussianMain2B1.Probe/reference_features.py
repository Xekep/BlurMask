"""Independent configured1111/raw88=1 producer, including ALL three flag paths.
Shared scalar references supply only normalization/resampling points/statistics;
whole dimensions are derived literally here, not inherited from their default.
"""
import importlib.util
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]

def prepare(profile,include_whole_unit=False,include_legacy_landmarks=False):
 spec=importlib.util.spec_from_file_location('independent_main_features',ROOT/'tools/LegacyEnglishMainAcoustic.Probe/reference_features.py');f=importlib.util.module_from_spec(spec);spec.loader.exec_module(f)
 if include_legacy_landmarks:f.land=f.defs('LegacyDirectional.Probe')['reference']
 else:
  ls=importlib.util.spec_from_file_location('actual_bc280_landmarks',ROOT/'tools/LegacyDirectional2B1.Probe/reference_landmarks.py');land=importlib.util.module_from_spec(ls);ls.loader.exec_module(land);f.land=land.reference
 def assembly(global_,windows,baseline,spatial):
  def ratio(v,d):return f.div(f.i32(v*2048),d)-1024
  result=[1024]
  for i,v in enumerate(global_):result.append(f.sat(f.div(f.i32(v*2048-2048),5)-1024 if i==2 else f.i32(v*2)if i==8 else ratio(v,3072 if i==3 else 3 if 9<=i<=15 else 1024)))
  for window in windows:
   for i,v in enumerate(window):result.append(f.sat(ratio(v,256)if spatial and i in(0,1,2,15)else f.i32(v*2048-1024)if i in(4,5,9,10,11,12,13,14)else f.i32(v*2)if i==8 else ratio(v,128 if i==15 else 1024)))
  assert len(result)==274;cal=profile['spatial'if spatial else'temporal']
  for i in range(1,274):
   low,high=cal['minima'][i],cal['maxima'][i]
   if high!=low:result[i]=f.sat(f.div(f.i32((result[i]-low)*2048),high-low)-1024)
  result += [f.sat(ratio(f.i32(v+(4096 if i==3 else 0)),8192 if i in(2,3)else 1024 if i>=6 else 4096))for i,v in enumerate(baseline)];assert len(result)==283
  return result
 def directional(strokes):
  normalized=f.coord['norm'](strokes);samples=f.coord['resample'](normalized,100)
  if not 1<=sum(map(len,samples['strokes']))<=255:raise ValueError('Explicit actual2B1 facade prepared-point cap')
  maximum=[max(p[axis]for stroke in samples['strokes']for p in stroke)for axis in(0,1)];unit=1 if normalized['shift']<0 else 1<<normalized['shift']
  # B4AF0 raw88=1 branch AFTER point resampling; keep point/cumulative/step data.
  samples['width'],samples['height']=[v+unit if include_whole_unit else max(1,v)for v in maximum]
  flags=f.land(samples['strokes'])['flags'];case=dict(strokes=samples['strokes'],width=samples['width'],height=samples['height'],step=samples['step'],gridScale=10);intersections,loops=f.cross(case);case.update(arc=samples['cumulative'][-1],flags=flags,intersections=intersections,intersectionCount=loops,cumulative=samples['cumulative'],shift=normalized['shift'],unit=False)
  global_=f.globalstat(case);shift=normalized['shift'];scale=lambda v:v<<shift if shift>=0 else v>>-shift
  if samples['step']==0:by,bh=0,100
  else:by,bh=scale(-normalized['minY']),max(1,scale(100))
  base=f.basestat(dict(case,baselineY=by,baselineHeight=bh));n=len(flags);windows=[[]for _ in range(16)];lengths=samples['cumulative']
  if n<=16:
   for i in range(n):windows[i]=[i]
  else:
   step=(lengths[-1]<<10)//16;boundary=step+512;start=k=0
   for i in range(1,n):
    if lengths[i]>boundary>>10:windows[k]=list(range(start,i));k+=1;start=i;boundary+=step
   windows[k]=list(range(start,n))
  temporal=[f.temporalstat(dict(case,members=indices))for indices in windows];spatial=[[]for _ in range(16)]
  for i,(x,y)in enumerate([p for s in samples['strokes']for p in s]):
   sx=(samples['width']<<10)//4;sy=(samples['height']<<10)//4;ix=x*1024//sx;iy=y*1024//sy;ix=ix if 0<=ix<4 else 3;iy=iy if 0<=iy<4 else 3;spatial[iy*4+ix].append(i)
  return assembly(global_,temporal,base,False),assembly(global_,[f.spatialstat(dict(case,members=indices))for indices in spatial],base,True)
 return directional
