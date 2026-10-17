"""Independent finite column-matching oracle, no vendor execution."""
import json, struct, sys
from pathlib import Path
fmax=struct.unpack('<f',bytes.fromhex('ffff7f7f'))[0]
def f32(x):return struct.unpack('<f',struct.pack('<f',x))[0]
capture=json.loads(Path(sys.argv[1]).read_bytes())
assert len(capture['Rows'])==4100 and capture['Guards']==11
multi=ties=0
for case in capture['Rows']:
 n,m=case['N'],case['M'];a=[list(map(f32,case['Input'][i*m:(i+1)*m])) for i in range(n)]
 matching=[];rounds=0
 if n and m:
  while len(matching)<min(n,m):
   if matching:
    usedrows={r for r,c in matching};usedcols={c for r,c in matching}
    a=[[(-fmax if r in usedrows or c in usedcols else a[r][c]) for c in range(m)] for r in range(n)]
    for r,c in matching:a[r][c]=fmax
   # Stable max per column, then group in first-seen row order. Independent
   # grouping instead of production's destructive duplicate suppression.
   grouped={}
   for c in range(m):
    score=max(a[r][c] for r in range(n))
    if score==-fmax:continue
    r=next(r for r in range(n) if a[r][c]==score)
    if sum(a[k][c]==score for k in range(n))>1:ties+=1
    grouped.setdefault(r,[]).append((score,c))
   nextmatching=[(r,max(options,key=lambda p:p[0])[1]) for r,options in grouped.items()]
   assert len(nextmatching)>len(matching)
   matching=nextmatching;rounds+=1
 assert [p[0] for p in matching]==case['SelectedRows']
 assert [p[1] for p in matching]==case['SelectedColumns']
 assert rounds==case['Rounds']
 assert [struct.pack('<f',x) for row in a for x in row]==[struct.pack('<f',x) for x in case['Adjusted']]
 assert len({p[0] for p in matching})==len(matching)==len({p[1] for p in matching})
 multi+=rounds>1
assert multi>1000 and ties>1000
print(f'PASS independent040270/0403E0/040670:4100 matrices/{multi} repeated/{ties} tied columns/11 guards; prepared only')
