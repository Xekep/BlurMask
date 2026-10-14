"""Independent audit of real rich-word to source720 fields and parent projection."""
import base64
import json
import math
import struct
import sys
from pathlib import Path

cases=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'))
assert len(cases)==12
def f32(value):return struct.pack('<f',value)
for case in cases:
    glyph=base64.b64decode(case['glyph']);source=base64.b64decode(case['source'])
    assert len(glyph)==40 and len(source)==720
    mapping=case['map'];parent=case['parent'];expected=[]
    for first,a,start,last,b,end in case['childRanges']:
        run=a
        for s in range(a,b+1):
            if s<b and mapping[s+1]==mapping[s]+1:continue
            expected.append([first,mapping[run],start if run==a else 0,last,mapping[s],end if s==b else len(parent[mapping[s]])-1])
            run=s+1
    assert expected==case['ranges']
    assert struct.unpack_from('<q',source,8)[0]==len(expected)
    assert struct.unpack_from('<i',source,16)[0]==-1
    assert source[40:42]==glyph[8:10]
    probability=struct.unpack_from('<f',glyph,12)[0]
    assert source[80:84]==f32(math.pow(probability,case['exponent']))
    assert source[84:88]==glyph[16:20] and source[240:244]==glyph[20:24]
    #No fabricated alternatives are introduced by the single-word glyph owner.
    assert all(struct.unpack_from('<H',source,40+2*i)[0]==65535 for i in range(1,20))
    assert all(struct.unpack_from('<i',source,240+4*i)[0]==0x4000000 for i in range(1,20))
    points=[]
    for first,a,start,last,b,end in expected:
        for physical in range(a,b+1):
            lo=start if physical==a else 0;hi=end+1 if physical==b else len(parent[physical])
            points.extend(parent[physical][lo:hi])
    bounds=[min(p[0]for p in points),max(p[0]for p in points),min(p[1]for p in points),max(p[1]for p in points)]
    assert source[24:40]==b''.join(f32(v)for v in bounds)
print('PASS 12 real NN/rich-word/source720 snapshots; independent parent map projection, cropped point bounds, actual249 .75 versus raw1, literal glyph and confidence/cost ownership, no fabricated alternatives')
