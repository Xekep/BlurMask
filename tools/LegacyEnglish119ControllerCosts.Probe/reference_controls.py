"""Explicit prepared numerical controls, not owned native scheduler requests."""
import json,random,struct,sys
from pathlib import Path
r=random.Random(0x1191be530)
controls=[]
for i in range(512):
 n=i%32;scale=[.5,1,1.5,2,4][i%5]
 controls.append(dict(costs=sorted(r.randrange(-2000,100000)for _ in range(n)),scale=scale))
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
boundaries=set()
for j in range(1,256):
 bits=struct.unpack('<I',struct.pack('<f',f((8*j-.5)/255-4)))[0]
 for k in range(-8,9):
  x=struct.unpack('<f',struct.pack('<I',bits+k))[0]
  if int(f(x+4)*255+.5)>>3!=int(f(f(f(x+4)*255)+.5))>>3:boundaries.add(x)
assert len(boundaries)==233
Path(sys.argv[1]).write_text(json.dumps(dict(ranked=controls,geometry=sorted(boundaries)))+'\n',encoding='utf-8')
