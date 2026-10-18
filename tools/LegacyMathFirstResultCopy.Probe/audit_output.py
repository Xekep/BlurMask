"""Independent source positions, C21 ranges, raw AL and first-copy oracle.
Only tracked numerical helpers are used; vendor code is never loaded.
"""
from pathlib import Path
import ast,base64,importlib.util,json,runpy,struct,sys
import numpy as np
if len(sys.argv) not in (4,5):raise SystemExit('capture.json rawMath2.res source-output.json [same-platform-other.json]')
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'tools/LegacyMathSourcePositions.Probe'))
from reference_positions import f,compose,gather
from reference_features import group_extrema_scale,resample_v1
def load(name,path):
 spec=importlib.util.spec_from_file_location(name,path);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
position=load('position',root/'tools/LegacyInkPosition.Probe/reference_controls.py')
union=load('range_union',root/'tools/LegacyRawInkRangeOperations.Probe/reference_controls.py').union
def functions(path,names,namespace):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));body=[n for n in tree.body if isinstance(n,ast.FunctionDef)and n.name in names]
 assert len(body)==len(names);exec(compile(ast.Module(body=body,type_ignores=[]),str(path),'exec'),namespace)
ns={'struct':struct}
functions(root/'tools/LegacyMathMeanXOrder.Probe/audit_output.py',{'native_order'},ns)
ns.update(dict(f=f,np=np,compose=compose,gather=gather,position=position,group_extrema_scale=group_extrema_scale,resample_v1=resample_v1))
functions(root/'tools/LegacyMathSourcePositions.Probe/audit_output.py',{'bits','expected'},ns)
capture=json.loads(Path(sys.argv[1]).read_bytes());raw=Path(sys.argv[2]).read_bytes();source_output=json.loads(Path(sys.argv[3]).read_bytes())
reader=runpy.run_path(str(root/'tools/export-ink-math-model.py'))
r=reader['Reader'](raw,106,reader['header'](raw,90)[3]);assert r.read('H',8)==[65535]*7+[299]
n=r.read('H')
for increment in (32,64,1):
 table=r.read('H',n);n=next(x+increment for x in reversed(table)if x!=65535)
spans=r.read('H',n+1);entries=[r.read('II')for _ in range(spans[-1]+1)];blob=r.bytes(entries[-1][0])
intervals={label&65535:blob[offset:entries[i+1][0]]for i,(offset,label)in enumerate(entries[:-1])}
def ranges(row,e):
 result=[[]for _ in row['Units']];previous=0
 for run in row['Runs']:
  end=run['EndSegmentExclusive'];index=run['PrimaryIndex']
  additional=position.select_ranges(e['positions'],e['ends'],e['originals'],previous,end)
  if index>=0:result[index]=union(result[index],additional)
  previous=end
 return [[dict(First=a,Last=b)for a,b in row]for row in result]
def prefix_ranges(stats):
 return [[dict(First=r['First']['Value'],Last=r['Last']['Value'])for r in row]for row in stats['RangePrefix']]
assert len(capture['Actual'])==4 and capture['ActualAliasPairs']==1 and capture['Guards']==84
for index,actual in enumerate(capture['Actual']):
 before=actual['Before'];assert before==actual['After'];rows=before['Rows'];pool={}
 assert len(rows)==len(before['Units'])
 for word,row in zip(before['Units'],rows,strict=True):
  value=intervals[word&65535]
  if value not in pool:pool[value]=len(pool)+1
  assert row==dict(Word=word,Identity=pool[value],Length=len(value),ScalarBits=0,Flags=0,Metadata=4294967295,Bytes=base64.b64encode(value).decode())
 evidence=actual['Source'];ink=[[[p['X'],p['Y']]for p in s['Points']]for s in evidence['Ink']];e=ns['expected'](ink)
 assert(evidence['Positions'],evidence['Separators'],evidence['Order'])==(e['positions'],e['ends'],e['order'])
 assert before['Ranges']==ranges(before,e)
 stats=actual['Statistics'];assert stats['Complete']and not(stats['Failed']or stats['Released'])
 assert stats['StringsInterned']==len(pool)and stats['RowsWritten']==len(rows)and stats['ReleaseCount']==0
 assert stats['ScoreBits']==before['ScoreBits']and stats['Frontier']==before['Frontier']and stats['Parent']==before['Parent']
 assert[stats[k]for k in('PrimaryCount','SecondaryCount','UnitCount','RunCount')]==[len(before[k])for k in('Primary','Secondary','Units','Runs')]
 assert prefix_ranges(stats)==before['Ranges']and stats['RangeRowsWritten']==len(rows)
 count=sum(map(len,before['Ranges']));assert stats['RangesWritten']==count
 if index<3:
  source=source_output['Cases'][index];baseline=source['Rows'][0]
  assert actual['Width']==source['Width']and before['Units']==baseline['Units']and before['Runs']==baseline['Runs']and before['ScoreBits']%(2**32)==baseline['ScoreBits']
  assert before['Ranges']==[[dict(First=a,Last=b)for a,b in row]for row in baseline['Ranges']]
  assert evidence['AttachmentReservation']==source['Statistics']['PayloadReservation']
 strings=sum(5*len(x)+1 for x in pool);peak=max((5*len(x)for x in pool),default=0)
 reservation=evidence['AttachmentReservation']+96+4*len(before['Primary'])+68*len(rows)+8*len(before['Runs'])+16*count+strings+peak
 assert stats['PayloadReservation']==reservation
stages=['Claimed','AfterPool','AfterHeader','AfterPrimary','AfterSecondary','AfterFrontier','AfterUnits','AfterRuns','AfterIntern','AfterAlternative','AfterRow','BeforeCompletion']
assert len(capture['Failures'])==24
reference=capture['Actual'][1]['Before']
for ordinal,stage in enumerate(stages):
 for cancel,fault in zip((False,True),capture['Failures'][2*ordinal:2*ordinal+2],strict=True):
  assert fault['Stage']==stage and fault['Canceled']==cancel
  p=fault['Prefix'];assert p['Phase']==stage and not(p['Complete']or p['Failed']or p['Released'])
  assert p['PayloadReservation']==capture['Actual'][1]['Statistics']['PayloadReservation']and p['ReleaseCount']==0
  assert p['ScoreBits']==(reference['ScoreBits']if ordinal>=2 else 0)
  assert p['Frontier']==(reference['Frontier']if ordinal>=5 else 0)
  assert p['Parent']==(reference['Parent']if ordinal>=2 else -1)
  for key,start,name in [('PrimaryCount',3,'Primary'),('SecondaryCount',4,'Secondary'),('UnitCount',6,'Units'),('RunCount',7,'Runs')]:
   assert p[key]==(len(reference[name])if ordinal>=start else 0)
  assert p['StringsInterned']==(len(reference['Rows'])if ordinal==11 else 1 if ordinal>=8 else 0)
  assert p['RowsWritten']==(len(reference['Rows'])if ordinal==11 else 1 if ordinal>=9 else 0)
  completed=len(reference['Rows'])if ordinal==11 else 1 if ordinal>=10 else 0
  want=reference['Ranges'][:completed]+[[]for _ in reference['Ranges'][completed:]]if ordinal>=8 else []
  assert prefix_ranges(p)==want and p['RangeRowsWritten']==completed
  assert p['RangesWritten']==sum(map(len,reference['Ranges'][:completed]))
assert capture['Gc']==dict(DetachedAllowsSourceCollection=True,SourceBackedRetainsSource=True,Row=reference)
if len(sys.argv)==5:assert Path(sys.argv[1]).read_bytes()==Path(sys.argv[4]).read_bytes()
print('PASS4 actual first copies/raw AL/C79 intervals/C21 unions/new-pool aliases/budget/24 prefixes/84 guards/GC; no outer DOM acceptance')
