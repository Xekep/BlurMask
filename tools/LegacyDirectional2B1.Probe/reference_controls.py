import json,random,sys
from pathlib import Path
from reference_landmarks import reference
rng=random.Random(0x2b1);controls=[]
for iteration in range(640):
 strokes=[]
 for _ in range(1+iteration%15):
  n=1+rng.randrange(40);strokes.append([[rng.choice([-32768,-100,0,0,1,100,32767]),rng.choice([-32768,-100,0,0,1,100,32767])]for _ in range(n)])
 controls.append(reference(strokes))
controls.append(reference([[[i-2048,0]for i in range(4096)]]))
Path(sys.argv[1]).write_text(json.dumps(controls,separators=(',',':'))+'\n');print('Generated',len(controls),'literalBC280 controls')
