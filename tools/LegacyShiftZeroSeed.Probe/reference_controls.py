import json,random,sys
from pathlib import Path
r=random.Random(0x1a1ef0);cases=[]
for i in range(512):
 n=r.randrange(2,11);marks=[(r.randrange(n-1),r.randrange(7,11)) for _ in range(r.randrange(6))]
 # With these disjoint ordinary boxes, nearest native interval is known
 # geometrically without reproducing its distance-comparison implementation.
 order=[]
 for slot in range(n):
  tied=[(j,delta) for j,(s,delta) in enumerate(marks) if s==slot]
  order.extend(n+j for j,delta in sorted(tied,key=lambda v:(v[1],-v[0])))
  order.append(slot)
 cases.append(dict(count=n,marks=marks,order=order))
target=Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True);target.write_text(json.dumps(cases))
