"""Independent raw positions -> mapped-run C21 ranges; no expected labels or vendor execution."""
import ast
import base64
import hashlib
import importlib.util
import json
from pathlib import Path
import struct
import sys
import numpy as np
sys.path.insert(0,str(Path(__file__).resolve().parent))
from reference_positions import f, compose, gather
from reference_features import group_extrema_scale,resample_v1,features26

if len(sys.argv) not in (3,4,5):raise SystemExit('capture.json rawMath2.res [same-platform-other.json] [old-structured.json]')
root=Path(__file__).resolve().parents[2]

def load(name,path):
 spec=importlib.util.spec_from_file_location(name,path);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
position=load('position',root/'tools/LegacyInkPosition.Probe/reference_controls.py')
union=load('range_union',root/'tools/LegacyRawInkRangeOperations.Probe/reference_controls.py').union
# Reuse the separately verified independent native16B sorter, not the C# kernel.
tree=ast.parse((root/'tools/LegacyMathMeanXOrder.Probe/audit_output.py').read_text())
namespace={'struct':struct};exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name=='native_order'],type_ignores=[]),'independent_source_sort','exec'),namespace)
native_order=namespace['native_order']
def bits(x):return struct.unpack('<I',struct.pack('<f',x))[0]
def raw_ink(case):return [[[p['X'],p['Y']]for p in s['Points']]for s in case['Ink']]
def expected(ink):
 means=[]
 for stroke in ink:
  total=f(0)
  for x,y in stroke:total=f(total+f(x))
  means.append(f(total/f(len(stroke))))
 raw=b''.join(struct.pack('<IIQ',i,bits(mean),0)for i,mean in enumerate(means));sorted_bytes=native_order(raw)
 order=[struct.unpack_from('<I',sorted_bytes,i)[0]for i in range(0,len(raw),16)]
 ordered=[np.asarray(ink[i],dtype=np.float32)for i in order]
 scale,_,_=group_extrema_scale(ordered)
 center=[f(0),f(0)];n=sum(map(len,ordered))
 for stroke in ordered:
  for point in stroke:
   for k in range(2):center[k]=f(center[k]+f(point[k]))
 center=[f(c/f(n))for c in center]
 normalized=[np.array([[f(f(f(p[k])-center[k])/f(scale))for k in range(2)]for p in stroke],dtype=np.float32)for stroke in ordered]
 sampled,indices,_=resample_v1(normalized)
 counts=list(map(len,ink));initial,separators=position.make_snapshot([[i,c,0,-1,0.,0.,True,True,False]for i,c in enumerate(counts)])
 prior,prior_ends=gather(initial,separators,order);coordinates=[]
 for source,values in zip(order,indices):
  coords=[]
  for value in values:
   packed=position.pack(0,f(value),5,counts[source]);coords.append([(packed>>16)&65535,(packed>>8)&255])
  # Literal017060 tail overwrite is a different plane from the last XY sample.
  coords[-1]=[counts[source]-1,255];coordinates.append(coords)
 projected,ends=compose(prior,prior_ends,coordinates)
 # A singleton position itself is FF3; its original enclosing range still
 # ends at Pack(..., mode5). Do not substitute the sampled terminal's mode.
 originals=[[position.pack(0,0,3,counts[0]),position.pack(len(counts)-1,counts[-1]-1,5,counts[-1])]]
 return dict(order=order,scale=bits(scale),sampled=sampled,positions=projected,ends=ends,
             originals=originals,coordinates=coordinates,counts=counts)

data=json.loads(Path(sys.argv[1]).read_bytes());raw=Path(sys.argv[2]).read_bytes()
assert data['Resource']==hashlib.sha256(raw).hexdigest()=='2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
assert len(data['Scalar'])==4097
from reference_positions import compose_one
for a,b,q,result in data['Scalar']:assert result==compose_one(a,b,q)
feature_error=0.;fractional=terminal_override=intervals=0;prepared=[]
for case in data['Features']:
 e=expected(raw_ink(case));prepared.append(e)
 assert case['Order']==e['order']and case['ScaleBits']==e['scale']
 assert case['Positions']==e['positions']and case['Separators']==e['ends']and case['StrokeEnds']==e['ends']
 assert case['Frames']==len(e['positions'])
 want=features26(e['sampled'],297).ravel();actual=np.asarray(case['Values'],dtype=np.uint32).view(np.float32)
 error=float(np.max(np.abs(actual-want)));feature_error=max(feature_error,error);assert np.allclose(actual,want,rtol=1e-5,atol=1e-6),error
 assert case['PayloadReservation']==sum(e['counts'])*8+len(e['positions'])*8+len(e['order'])*128+32
 fractional+=sum(0<((p>>8)&255)<255 for p in e['positions'])
 assert len(case['Queries'])==64
 for query in case['Queries']:
  assert query['Ranges']==position.select_ranges(e['positions'],e['ends'],e['originals'],query['First'],query['Last'])
  intervals+=1
assert fractional>0 and len(prepared)==20

def ranges_for(rows,e,stop=None):
 output=[[[]for _ in row['Units']]for row in rows];runs=0
 for at,row in enumerate(rows):
  previous=0
  for run in row['Runs']:
   end=run['EndSegmentExclusive'];index=run['PrimaryIndex'];additional=position.select_ranges(e['positions'],e['ends'],e['originals'],previous,end)
   if index>=0:output[at][index]=union(output[at][index],additional)
   previous=end;runs+=1
   if stop is not None and at==stop:return output,runs
 return output,runs
range_count=0
for case in data['Cases']:
 e=prepared[1 if case['Reversed']else 0]
 assert case['Order']==e['order']and case['Positions']==e['positions']and case['Separators']==e['ends']
 stats=case['Statistics'];assert stats['Complete']and not stats['Failed']
 expected_ranges,run_count=ranges_for(case['Rows'],e)
 assert run_count==stats['Runs']
 for row,want in zip(case['Rows'],expected_ranges):assert row['Ranges']==want;range_count+=sum(map(len,want))
 projected=[[[[p['First']['Value'],p['Last']['Value']]for p in ranges]for ranges in record]for record in stats['RangePrefix']]
 assert projected==expected_ranges
 assert case['SourceReservation']==data['Features'][1 if case['Reversed']else 0]['PayloadReservation']
 counts=[len(row['Runs'])*len(e['order'])for row in case['Rows']]
 assert stats['PayloadReservation']==case['OutputReservation']+case['SourceReservation']+len(case['Rows'])*8+sum(counts)*16+max(counts)*32
 assert case['RawSteps']==len(case['RawHashes'])>0
assert range_count==40
for failure in data['Failures']:
 stats=failure['Statistics'];assert stats['Failed']and not stats['Complete']
 base=next(c for c in data['Cases']if c['Width']==3)
 rows=base['Rows'];e=prepared[0]
 if failure['Stage']=='Claimed':want=[[[]for _ in row['Units']]for row in rows];runs=0
 else:want,runs=ranges_for(rows,e,stop=1 if failure['Stage']=='AfterRun'else None)
 got=[[[[p['First']['Value'],p['Last']['Value']]for p in r]for r in record]for record in stats['RangePrefix']]
 assert got==want and stats['Runs']==runs,(failure['Stage'],got,want)
assert len(data['Failures'])==6 and data['Guards']==83
if len(sys.argv)>=4:assert Path(sys.argv[1]).read_bytes()==Path(sys.argv[3]).read_bytes()
if len(sys.argv)==5:
 old=json.loads(Path(sys.argv[4]).read_bytes())
 for case in data['Cases']:
  previous=next(c for c in old['Cases']if c['Width']==case['Width'])
  assert case['RawHashes']==previous['RawHashes']and case['RawSteps']==previous['RawSteps']
  for a,b in zip(case['Rows'],previous['Rows']):
   for name in ['Units','Map','Runs']:assert a[name]==b[name]
   assert a['ScoreBits']==b['ScoreBits']%(2**32)
   for x,y in zip(a['UnitRows'],b['UnitRows']):
    for name in ['RawUnit','ByteCount','ScalarBits','Flags','Metadata','Utf8']:assert x[name]==y[name]
print(f'PASS20 actual source-bound records/40 range collections;20 independent raw feature/map inputs/{fractional} fractional processed positions/{intervals} processed intervals;4097 prepared V18 scalars;6 whole-prefix faults/83guards; max feature error{feature_error}; no DOM/acceptance')
