"""Own unique-gap fixtures. Reference selects descending distances by sorting."""
import random,json,sys
from pathlib import Path
r=random.Random(0x1b4bd0);cases=[]
for case in range(512):
 n=r.randrange(1,65);gaps=r.sample(range(1,257),n-1);strokes=[];x=0
 for i in range(n):
  strokes.append([[x,0],[x+2,0]]);x+=2+(gaps[i] if i<len(gaps) else 0)
 threshold=r.randrange(1,n+2)
 retained=sorted(sorted(range(n-1),key=lambda i:gaps[i],reverse=True)[:threshold-1])
 cases.append(dict(strokes=strokes,threshold=threshold,retained=retained))
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(cases=cases),separators=(',',':')),encoding='utf-8')
print('Generated',len(cases),'own unique-gap controls:',out)
