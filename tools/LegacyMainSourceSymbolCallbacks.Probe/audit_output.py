"""Independent prepared-request oracle over actual owned graph arc snapshots."""
import json,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
p=json.loads(Path(sys.argv[1]).read_bytes());total=0;fallbacks=0;emitted=0;flag8=0
assert len(p['owners'])==2 and [o['name'] for o in p['owners']]==['case','direct']
for owner in p['owners']:
    assert owner['nodes']==4 and len(owner['fallbacks'])==10
    flags={}
    for node,arc,symbol,arcFlags,replacement,normalized,external in owner['fallbacks']:
        assert 0<=node<owner['nodes'] and symbol==65535 and (replacement,normalized,external)==(189,189,0)
        assert (node,arc) not in flags;flags[node,arc]=arcFlags;fallbacks+=1
    assert len(owner['delimiterRequests'])==1152
    for previous,previousArc,current,currentArc,cursor,kind,remaining,c,d,symbols in owner['delimiterRequests']:
        assert previous==-1 or (previous,previousArc) in flags
        assert current==owner['nodes'] or (current,currentArc) in flags
        assert 0<=cursor<=256 and c==d==0
        expected=[]
        if cursor>0 and previous!=-1 and current!=owner['nodes']:
            if kind==0:expected=[65532]
            else:
                assert kind==2
                if flags[current,currentArc]&8:expected=[65531];flag8+=1
        assert symbols==expected and remaining==cursor-len(expected)
        emitted+=len(expected);total+=1
assert (p['calls'],p['fallbacks'],p['guards'])==(total,fallbacks,28)
a,b=p['owners'];assert a['fallbacks']==b['fallbacks'] and a['delimiterRequests']==b['delimiterRequests']
print(f'PASS independent {total} prepared graph delimiter requests/{emitted} actualcallback tokens; {fallbacks} type0 FFFD lookups; flag8observed={flag8}. No winning-path/handwriting claim.')
