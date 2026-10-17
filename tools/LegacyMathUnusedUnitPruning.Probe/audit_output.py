"""Independent run/stroke-boundary041660 oracle, actual fingerprinted EF3 gate."""
import base64
import hashlib
from itertools import groupby
import json
from pathlib import Path
import sys
if len(sys.argv)!=3:raise SystemExit('rawMath2.res capture.json')
raw=Path(sys.argv[1]).read_bytes();d=json.loads(Path(sys.argv[2]).read_bytes())
assert d['Resource']==hashlib.sha256(raw).hexdigest()=='2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
# The independent source alphabet/gate derivation is pinned separately by
# audit_math2_observable_token_gate; captures cannot supply their own truth set.
assert d['Gate']==[2,190,261,268,276,65535,242,296]
observable=set(range(2,262))|{268,276,296}
assert len(d['Rows'])==4096 and d['Guards']==13
suppressed_total=deletions_total=preserved_nonobs=flagged_deleted=0
seen_flags=set();begins_and_ends=0;previous_suppressed_then_repeat=0
for row in d['Rows']:
 units,assignments,ends=row['Input'],row['Assignments'],set(row['Ends'])
 n=len(assignments)
 runs=[list(indices)for _,indices in groupby(range(n),key=assignments.__getitem__)]
 suppressed=[indices[0]for indices in runs if indices[0]>0 and indices[0]not in ends and
             (indices[0]+1==n or indices[0]+1 in ends)]
 counts=[sum(assignments[s]==i for s in set(range(n))-set(suppressed))for i in range(len(units))]
 deleted=[counts[i]==0 and ((unit>>16)&255!=0 or unit&65535 in observable)for i,unit in enumerate(units)]
 kept=[i for i in range(len(units))if not deleted[i]]
 mapping={old:new for new,old in enumerate(kept)}
 assert row['Suppressed']==suppressed and row['Counts']==counts
 assert list(base64.b64decode(row['Deletions'],validate=True))==list(map(int,deleted))
 assert row['Output']==[units[i]for i in kept]
 assert row['Map']==[mapping.get(i,-1)for i in range(len(units))]
 assert row['Mapped']==[-1 if s in suppressed else mapping.get(original,-1)for s,original in enumerate(assignments)]
 suppressed_total+=len(suppressed);deletions_total+=sum(deleted)
 preserved_nonobs+=sum(counts[i]==0 and units[i]&65535 not in observable and (units[i]>>16)&255==0 for i in kept)
 flagged_deleted+=sum(deleted[i]and units[i]&65535 not in observable and (units[i]>>16)&255!=0 for i in range(len(units)))
 seen_flags.update((unit>>16)&255 for unit in units)
 begins_and_ends+=sum((s==0 or s in ends)and(s+1==n or s+1 in ends)for s in range(n))
 previous_suppressed_then_repeat+=sum(s+1<n and assignments[s+1]==assignments[s]for s in suppressed)
assert suppressed_total>1000 and deletions_total>10000 and preserved_nonobs>1000 and flagged_deleted>1000
assert seen_flags=={0,1,2,255} and begins_and_ends>1000 and previous_suppressed_then_repeat>100
print(f'PASS4096 independent run/boundary masks; {suppressed_total} suppressed segments/'
      f'{deletions_total} removed units/{preserved_nonobs} unused nonobservables retained/'
      f'{flagged_deleted} flagged nonobservables deleted;13 guards')
