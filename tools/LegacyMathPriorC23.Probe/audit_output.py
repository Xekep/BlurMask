"""Independent C23 intersection, source positions, raw AL and copied ranges.
Read-only diagnostic observations are never used to construct an owner.
"""
from pathlib import Path
import ast,base64,importlib.util,json,runpy,struct,sys
import numpy as np
root=Path(__file__).resolve().parents[2];capture=json.loads(Path(sys.argv[1]).read_bytes());raw=Path(sys.argv[2]).read_bytes()
if len(sys.argv)>3:
 assert Path(sys.argv[1]).read_bytes()==Path(sys.argv[3]).read_bytes(), 'Managed/native capture differs'
sys.path.insert(0,str(root/'tools/LegacyMathSourcePositions.Probe'))
from reference_positions import f,compose,gather
from reference_features import group_extrema_scale,resample_v1
def load(name,path):
 spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
position=load('position',root/'tools/LegacyInkPosition.Probe/reference_controls.py')
algebra=load('algebra',root/'tools/LegacyRawInkRangeOperations.Probe/reference_controls.py')
def functions(path,names,namespace):
 body=[n for n in ast.parse(path.read_text(encoding='utf-8-sig')).body if isinstance(n,ast.FunctionDef)and n.name in names]
 assert len(body)==len(names);exec(compile(ast.Module(body=body,type_ignores=[]),str(path),'exec'),namespace)
ns=dict(struct=struct)
functions(root/'tools/LegacyMathMeanXOrder.Probe/audit_output.py',{'native_order'},ns)
ns.update(dict(f=f,np=np,compose=compose,gather=gather,position=position,group_extrema_scale=group_extrema_scale,resample_v1=resample_v1))
functions(root/'tools/LegacyMathSourcePositions.Probe/audit_output.py',{'bits','expected'},ns)
reader=runpy.run_path(str(root/'tools/export-ink-math-model.py'))
r=reader['Reader'](raw,106,reader['header'](raw,90)[3]);assert r.read('H',8)==[65535]*7+[299]
n=r.read('H')
for increment in(32,64,1):
 table=r.read('H',n);n=next(x+increment for x in reversed(table)if x!=65535)
spans=r.read('H',n+1);entries=[r.read('II')for _ in range(spans[-1]+1)];blob=r.bytes(entries[-1][0])
intervals={label&65535:blob[offset:entries[i+1][0]]for i,(offset,label)in enumerate(entries[:-1])}
def values(rows):return[[r['First']['Value'],r['Last']['Value']]for r in rows]
assert len(capture['Actual'])==6 and capture['Controls']==278
for case in capture['Actual']:
 e=ns['expected']([[[p['X'],p['Y']]for p in s['Points']]for s in case['Ink']]);prior=case['Prior'];prepared=case['Prepared'];done=case['Finished'];nn=case['NewNn']
 nodes=prior['SelectedField'];field=next(n for n in nodes if n['Parent']==0 and n['Kind']==3 and n['Name']=='mathField')
 source=next(n for n in nodes if n['Parent']==field['Id'] and n['Kind']==3 and n['Name']=='input')
 tag=next(n for n in nodes if n['Parent']==source['Id'] and n['Kind']==0 and n['Name']=='tag')
 assert tag['ValueTag']==4 and tag['Number']==prepared['InputId']
 registered=next(r for r in prior['Registry'] if r['Id']==prepared['InputId'])
 assert registered['Name']=='INPUT' and registered['Category']==102
 c23=algebra.intersect(e['originals'],values(registered['Ranges']))
 assert values(prepared['C23Ranges'])==values(prepared['WorkRanges'])==c23==e['originals']
 assert prepared['Complete'] and not(prepared['Failed']or prepared['Released']or prepared['InferenceClaimed']or prepared['SourcePrepared']or prepared['WorkCompleted'])
 assert prepared['WorkCount']==1 and prepared['InitialComputeFlag']==1 and prepared['TransformCount']==prepared['FreezeCount']==0
 assert done==dict(prepared,InferenceClaimed=True,SourcePrepared=True,WorkCompleted=True)
 assert nn['Complete'] and not(nn['Failed']or nn['Released']or nn['DecoderReleased'])
 assert nn['Width']==case['Width'] and nn['Frames']==len(e['positions'])
 assert(nn['Positions'],nn['Separators'],nn['Order'])==(e['positions'],e['ends'],e['order'])
 assert nn['DecoderCalls']==len(nn['RawHashes'])>0
 row=nn['FirstCopy'];assert row is not None # Completed parent is produced by NN, not the initial F0=-1 header.
 pool={}
 for unit,text in zip(row['Units'],row['Text'],strict=True):
  expected=intervals[unit&65535]
  if expected not in pool:pool[expected]=len(pool)+1
  assert text==dict(Unit=unit,Identity=pool[expected],ByteCount=len(expected),Bytes=base64.b64encode(expected).decode())
 ranges=[[]for _ in row['Units']];previous=0
 for run in row['Runs']:
  end=run['EndSegmentExclusive'];at=run['PrimaryIndex'];additional=position.select_ranges(e['positions'],e['ends'],e['originals'],previous,end)
  if at>=0:ranges[at]=algebra.union(ranges[at],additional)
  previous=end
 assert[values(r)for r in row['Ranges']]==ranges
print(f"PASS6 actual new current-C23 NN cases;{capture['Controls']} ownership/claim/prefix controls; independent intersection/source positions/raw-AL/first-copy ranges. No DOM update.")
