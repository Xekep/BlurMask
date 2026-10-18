"""Independent041EC0 block composition, retaining unknown native padding."""
from pathlib import Path
import json,base64,struct,sys
c=json.loads(Path(sys.argv[1]).read_bytes());assert len(c['Rows'])==2048 and c['Guards']==7
unknown=inserted=images=0
for case in c['Rows']:
 mask=base64.b64decode(case['Mask']);width=case['Width'];cursor=0;expected=[]
 for unit,flag in zip(case['Units'],mask):
  if not flag:expected.extend(case['Initial'][cursor:cursor+width]);cursor+=width
  elif width:
   expected.append(dict(Symbol=unit&65535,Padding=0,ScoreBits=0));inserted+=1
   expected.extend(dict(Symbol=0,Padding=None,ScoreBits=0xff7fffff) for i in range(width-1))
 assert cursor==len(case['Initial']) and expected==case['Output']
 full=all(row['Padding'] is not None for row in expected);assert full==case['CompleteImage']
 image=base64.b64decode(case['Image'])
 if full:
  assert image==b''.join(struct.pack('<HHI',r['Symbol'],r['Padding'],r['ScoreBits']) for r in expected);images+=1
 else:assert image==b''
 unknown+=sum(r['Padding'] is None for r in expected)
assert unknown>1000 and inserted>1000 and images>100
print(f'PASS independent2048 auxiliary blocks/{inserted} insertions/{unknown} explicitly unknown padding fields/{images} fully known images/7 guards')
