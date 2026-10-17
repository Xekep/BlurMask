"""Independent13B1D0 linked reversal and native per-owner trailing pop."""
import base64,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
MASK=(1<<24)-1|65535<<24|1<<56
MAX=(1<<64)-1
cases=json.loads(Path(sys.argv[1]).read_text());removed=retained=checked=0
for case in cases:
    owners={o['key']:[bytearray(base64.b64decode(o['rows']))[i:i+40]for i in range(0,len(base64.b64decode(o['rows'])),40)]for o in case['before']}
    flags={o['key']:o['packed']&0xc003 for o in case['before']}
    def row(ref):return owners[ref&MASK][ref>>40&65535]
    #Follow the actual links; do not reconstruct the segment from insertion
    #identities or from the production helper's returned traversal array.
    current=case['last'];previous=MAX;visited=[]
    while True:
        actual=row(current);next_ref=struct.unpack_from('<Q',actual,32)[0]
        struct.pack_into('<Q',actual,32,previous);previous=current;visited.append(current)
        if current==case['first']:break
        current=next_ref
        assert len(visited)<=4096
    traversal=[];released=[];current=case['first']
    while current!=MAX:
        actual=row(current);next_ref=struct.unpack_from('<Q',actual,32)[0];traversal.append(current)
        key=current&MASK;ordinal=current>>40&65535
        if ordinal==len(owners[key])-1:owners[key].pop();released.append(current)
        else:retained+=1
        current=next_ref
    assert traversal==case['visited']and released==case['removed']
    assert [o['key']for o in case['before']]==[o['key']for o in case['after']]
    for after in case['after']:
        key=after['key'];expected=b''.join(owners[key]);assert base64.b64decode(after['rows'])==expected
        assert after['packed']==flags[key]|len(owners[key])<<2
        checked+=len(expected)
    removed+=len(released)
assert len(cases)==1024 and removed>2000 and retained>1000
print(f'PASS independent1024 pools/{removed} trailing rows removed/{retained} non-tail rows retained/{checked} remaining record bytes exact; owner flags and empty nodes retained')
