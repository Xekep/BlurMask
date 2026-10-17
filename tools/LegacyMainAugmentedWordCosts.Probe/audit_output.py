"""Independent219580 signed int32 delta and sequential SSE float32 cost oracle."""
import base64,collections,json,struct,sys
from pathlib import Path
f32=lambda v:struct.unpack('<f',struct.pack('<f',v))[0]
i32=lambda v:(v+2**31)%2**32-2**31
stats=collections.Counter();cases=json.loads(Path(sys.argv[1]).read_bytes())
assert len(cases)==16384
for c in cases:
    data=base64.b64decode(c['input']);assert len(data)==160
    values=list(struct.unpack_from('<ii',data,60))
    if c['parent']!=-1:
        previous=struct.unpack_from('<ii',data,c['parent']*40+20)
        delta=[a-b for a,b in zip(values,previous)]
        stats['wrapped']+=any(i32(v)!=v for v in delta)
        values=[i32(v)for v in delta]
    else:stats['root']+=1
    denominator=f32(i32((c['last']+1-c['first'])<<13))
    cost=f32(f32(values[0])/denominator)
    score=f32((-5.0 if c['kind']==0x135 else -1.0)*cost)
    secondary=f32(f32(values[1])*f32(1/8192))
    expected=struct.pack('<fff',score,cost,secondary)
    assert base64.b64decode(c['output'])==expected,c
    stats['source135']+=c['kind']==0x135;stats['negativePrimary']+=values[0]<0
assert all(stats[k]>0 for k in ('wrapped','root','source135','negativePrimary'))
print('PASS independent16384 exact12B cost triples:',dict(stats),'; prepared numeric coverage only')
