"""Independent041240 oracle using stable unit tokens instead of index shifts."""
from pathlib import Path
import json,struct,sys,base64
def f32(x):return struct.unpack('<f',struct.pack('<f',x))[0]
c=json.loads(Path(sys.argv[1]).read_bytes());assert len(c['Rows'])==4096 and c['Guards']==14
inserted=paircases=globalcases=holes=0
for case in c['Rows']:
 src=case['Existing'];s=len(case['Assignments']);n=len(case['Map'])
 # token = (identity, full unit bytes, inserted flag)
 out=[(i,v,0) for i,v in enumerate(case['Initial'])]
 a=[None if v==-1 else out[v][0] for v in case['Assignments']];nextid=len(out)
 for current in range(s):
  if a[current] is not None:continue
  at=0 if current==0 else next(i for i,t in enumerate(out) if t[0]==a[current-1])+1
  if src and src[current]>>24==0:
   assert current+1<s and src[current+1]>>24==0 and a[current+1] is None
   value=src[current];a[current]=nextid;a[current+1]=nextid;paircases+=1
  else:
   value=0x01000000|case['Replacement']
   if src and n:
    scores=[]
    for r in range(n):
     score=0.
     for col in range(s):
      if src[col]>>24:score=f32(score+f32(case['Matrix'][r*s+col]))
     scores.append(score)
    selected=max(range(n),key=lambda r:scores[r]);slot=case['Map'][selected]
    value=out[slot][1]
    holes+=slot!=selected
   a=[nextid if v is None else v for v in a];globalcases+=1
  out.insert(at,(nextid,value,1));nextid+=1;inserted+=1
 positions={t[0]:i for i,t in enumerate(out)}
 assert [t[1] for t in out]==case['Output']
 assert [positions[v] for v in a]==case['Mapped']
 assert bytes(t[2] for t in out)==base64.b64decode(case['Mask'])
assert inserted>1000 and paircases>100 and globalcases>100 and holes>100
print(f'PASS independent4096 token-based insertion controls/{inserted} inserted/{paircases} paired/{globalcases} all-absent branches/{holes} row-map distinctions/14 guards')
