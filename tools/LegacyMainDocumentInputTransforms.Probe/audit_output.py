"""Raw selected-owner/037E80-vector/status replay, without inferred labels."""
import base64,json,struct,sys
from pathlib import Path
a=json.loads(Path(sys.argv[1]).read_bytes());b64=base64.b64decode
raw=json.loads(Path(sys.argv[2]).read_bytes())['cases']if len(sys.argv)>2 else None
faults=0
for ordinal,case in enumerate(a['swaps']):
 #037F15/1C saves status;037F2D executes on either status. The previous
 #vector becomes local cleanup ownership, not the result on failed export.
 assert case['current']==case['exported'] and case['released']==case['old']
 assert case['followingStage']==case['status']
 if raw is not None:
  source=raw[ordinal]
  assert case['old']==source['old'] and case['exported']==source['exported']
  assert case['current']==source['expected'] and case['released']==source['released']
  assert case['status']==source['status'] and case['followingStage']==source['followingStage']
 for name in ['current','released']:
  v=case[name];assert len(b64(v['scalars']))==len(v['ranges'])*64
  for row in v['ranges']:assert all(len(r)==2 and all(0<=x<2**64 for x in r)for r in row)
 faults+=not case['status']
assert len(a['swaps'])==1024 and faults==512
matrices=0
for case in a['samples']:
 counts=case['pointCounts'];assert counts
 #Library NEWtextField INPUT has registry id0 and retains one complete C22
 #selection across all raw source objects. This differs from original IDs.
 assert case['tag']==0 and b64(case['seed20'])==struct.pack('<qiii',0,0,0,0)
 last=((len(counts)-1)<<32)|((counts[-1]-1)<<16)|0xff05
 assert case['inputRanges']==[[3,last]]
 assert case['documentAlternatives']==2 and case['sourceLines']==1
 assert case['swapped'] and case['followingStage'] and case['transforms']==0 and case['dafInputs']==0
 assert case['pendingBefore']==case['pendingAfter'] and case['nextBefore']==case['nextAfter']
 identity=struct.pack('<6d',1,0,0,0,1,0)
 assert b64(case['matrices'])==identity*len(counts)
 assert case['matches']==list(range(len(counts)))
 #081ED0 preserves full-object terminal sentinel -1 when fractionFF is
 #present. C49's view resolves that sentinel only for its coverage check.
 assert case['fragments']==[[i,n,0,-1,0,0,True,True]for i,n in enumerate(counts)]
 matrices+=len(counts)
assert len(a['samples'])==4
print(f'PASS independent1024 swaps/512 failed-export prefixes/4 actual selected INPUT owners/{matrices} identity matrices; D8=2 distinct from1 guide, no transform or DAF row fabricated.')
