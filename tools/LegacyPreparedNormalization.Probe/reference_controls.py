"""Own float32 geometry/frame fixtures, independent of C# and SDK bytes."""
import json, random, struct, sys
from pathlib import Path

def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
r=random.Random(0x1b4e40);cases=[]
for case in range(768):
    strokes=[[[f(r.uniform(-200,200)),f(r.uniform(-100,100))] for _ in range(r.randrange(2,12))] for _ in range(r.randrange(1,5))]
    frame=[f(min(p[0] for stroke in strokes for p in stroke)-r.uniform(0,10)),f(r.uniform(50,150)),f(r.uniform(20,100)),r.randrange(1,301)]
    if case%31==0:frame[0]=f(-1e6) # upper clamp case.
    ranges=[];expected=[]
    # Preserve caller order, including a deliberate reverse physical order.
    owners=list(range(len(strokes)))
    if case%2:owners.reverse()
    for owner in owners:
        start=r.randrange(len(strokes[owner]));end=r.randrange(start+1,len(strokes[owner])+1)
        ranges.append([owner,start,end]);coordinates=[]
        for p in strokes[owner][start:end]:
            x=f(f(f(p[0]-frame[0])*frame[3])/frame[2]);y=f(f(f(p[1]-frame[1])*frame[3])/frame[2])
            coordinates.append([int(min(32767,x)),int(min(32767,y))])
        expected.append(coordinates)
    cases.append(dict(strokes=strokes,ranges=ranges,frame=frame,expected=expected))
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(cases=cases),separators=(',',':')),encoding='utf-8')
print('Generated',len(cases),'own float32 normalization controls:',out)
