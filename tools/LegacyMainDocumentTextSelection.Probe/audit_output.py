"""Raw original object rows -> NULL05AFA0/C21 key -> pending kind2 TEXT.
No recognized labels or chosen strings are read by this audit.
"""
import importlib.util,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
s=importlib.util.spec_from_file_location('ops',root/'tools/LegacyRawInkRangeOperations.Probe/reference_controls.py');ops=importlib.util.module_from_spec(s);s.loader.exec_module(ops)
capture=json.loads(Path(sys.argv[1]).read_bytes());total=0
for sample in capture['samples']:
 counts=sample['pointCounts'];rows=sample['selectedInputs'];assert len(rows)==len(counts)
 ranges=[]
 for index,(row,count)in enumerate(zip(rows,counts)):
  assert row==[index,count,0,-1,0,0,True,True,False]
  first=(index<<32)|(0xff03 if count==1 else 3)
  last=(index<<32)|((count-1)<<16)|0xff05
  #053F90 no-overrides/ordinary whole selection; VA8 enters native07FAF0.
  ranges=ops.union(ranges,[[first,last]])
 assert sample['rootC0']==ranges
 pending=sample['pending'];prefix=sample['prefixCount'];assert len(pending)==prefix+1
 assert [p['name']for p in pending]==['CHAR']*(prefix-1)+['WORD','TEXT']
 for ordinal,command in enumerate(pending):
  assert command['id']==len(counts)+ordinal
  assert command['identity']==ordinal+1 and command['previous']==ordinal
  assert(command['code'],command['enabled'],command['category'])==(4,1,100)
 assert pending[-1]['ranges']==ranges and sample['textId']==pending[-1]['id']
 assert sample['nextId']==pending[-1]['id']+1
 total+=len(ranges)
assert len(capture['samples'])==3 and total==3
print(f'PASS independent3 actual root owners/{total} NULL-projected original-object ranges/native normalizedC21/pendingTEXT after exact CHAR+WORD prefix; no accepted text.')
