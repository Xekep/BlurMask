"""Independent actual191430 inputs-to-tags audit, absent optional model branch."""
import json, math, struct, sys
from pathlib import Path
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def visible(tag):return tag!=0 and not 11<=tag<=14
p=json.loads(Path(sys.argv[1]).read_text());t=p['limitTrace']
assert t['closedCap'] is None and t['gate']==6 and t['widthScale']==4
tags=t['tags'].copy();lengths=list(map(f,t['weights']));start=0;active=closed=changed=0
height=f(p['height'])
for raw,count in zip(t['raw'],t['counts']):
    end=start+count
    dx=f(f(raw[0][0])-f(raw[-1][0]));dy=f(f(raw[0][1])-f(raw[-1][1]))
    if math.sqrt(f(f(dy*dy)+f(dx*dx)))<f(height*f(.8)):closed+=1
    markers=sum(map(visible,tags[start:end]))
    if tags[start]==1 and markers>6:
        width=f(max(f(q[0]) for q in raw)-min(f(q[0]) for q in raw))
        target=int(f(f(width*4)/height))
        if target<markers:
            active+=1;indices=[start];weights=[lengths[start]]
            for i in range(start+1,end):
                if visible(tags[i]):indices.append(i);weights.append(lengths[i])
                weights[-1]=f(weights[-1]+lengths[i])
            for _ in range(markers-target):
                best=f(3.4028234663852886e38);chosen=-1
                for k in range(1,len(indices)):
                    a,b=weights[k-1],weights[k]
                    cost=f(min(a,b)+max(a,b)*.1)
                    if cost<best and tags[indices[k]] not in (4,7):best,chosen=cost,k
                if chosen>=0:
                    tags[indices[chosen]]=0;del indices[chosen];del weights[chosen];weights[chosen-1]=best;changed+=1
    start=end
assert start==len(tags)==len(lengths)
assert tags==t['finalTags'],[(i,a,b)for i,(a,b)in enumerate(zip(tags,t['finalTags']))if a!=b][:16]
assert closed>0,'No closed physical reached actual final limiter input.'
if '--require-active' in sys.argv:assert active>0 and changed>0,'No active marker reduction in actual fixture.'
print(f'PASS actual191430: {len(t["counts"])} physical inputs/{closed} closed/{active} active pruning/{changed} removed markers; no invented optional cap.')
