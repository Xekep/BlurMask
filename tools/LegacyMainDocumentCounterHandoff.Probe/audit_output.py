"""Independent operation7 raw body/link replay; no text or SDK execution."""
import base64,json,struct,sys
from pathlib import Path
a=json.loads(Path(sys.argv[1]).read_bytes());raw=json.loads(Path(sys.argv[2]).read_bytes())['cases'];b64=base64.b64decode
def links(rows):
 for ordinal,row in enumerate(rows):
  assert row['id']==ordinal+1 and row['previous']==ordinal
  assert row['next']==(ordinal+2 if ordinal+1<len(rows)else 0)
appends=failed=skips=0
for output,source in zip(a['prepared'],raw):
 prefix=source['prefix'];old=source['old'];new=source['new'];needs=old!=new
 success=not needs or not source['allocationFailure'];rows=output['commands'];links(rows)
 assert output['succeeded']==success and output['failed']==(needs and not success)
 assert output['next']==1+prefix
 assert len(rows)==prefix+(needs and success)
 for ordinal,row in enumerate(rows[:prefix]):
  assert row['op']==4 and row['selectionId']==ordinal+1 and row['category']==100
 if needs and success:
  assert rows[-1]['op']==7 and b64(rows[-1]['body56'])==struct.pack('<qq',new,old)+bytes(40)
  appends+=1
 elif needs:failed+=1
 else:skips+=1
assert len(a['prepared'])==len(raw)==1024
for sample in a['actual']:
 old=sample['oldCounter'];new=sample['newCounter'];ids=sample['selectionIds'];rows=sample['commands'];links(rows)
 assert sample['inputNext']==old==1 and ids==list(range(old,new))
 assert sample['selectionsBefore']==sample['selectionsAfter']==len(ids)
 assert b64(sample['registryBefore'])==b64(sample['registryAfter'])==struct.pack('<qqB',1,0,0)
 assert len(rows)==len(ids)+1 and [r['op']for r in rows]==[4]*len(ids)+[7]
 assert [r['selectionId']for r in rows[:-1]]==ids
 assert [r['name']for r in rows[:-1]][-2:]==['WORD','TEXT']
 assert b64(rows[-1]['body56'])==struct.pack('<qq',new,old)+bytes(40)
assert len(a['actual'])==4
print(f'PASS independent1024 attempts/{appends} full56B append bodies/{failed} retained failures/{skips} equality skips/4 actual retained C4D tails; actualC51 unchanged, pending only.')
