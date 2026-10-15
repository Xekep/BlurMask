"""Independent16-byte source-reference wire oracle, no vendor execution."""
import json,random,sys
from pathlib import Path
rng=random.Random(0x19a1d0);tests=[]
for i in range(2048):
    node=([0,0xffffff,0x1000000,0x7fffffff,0x80000000,0xffffffff][i] if i<6 else rng.getrandbits(32))
    arc=([0,1,32767,32768,65534,65535][i] if i<6 else rng.getrandbits(16))
    slots=i%5;before=rng.randbytes(slots*16);after=bytearray(before)
    if slots:
        #Independent byte lanes, avoiding the implementation's shift/or form.
        after[0:3]=node.to_bytes(4,'little')[0:3]
        after[3:5]=arc.to_bytes(2,'little');after[5:9]=bytes(4)
    tests.append(dict(node=node,arc=arc,before=before.hex(),after=after.hex()))
for i in range(512):
    count=i%21;glyph=[0,1,19,20,255][i%5]
    frame=bytearray(rng.randbytes(944));frame[0x25c:0x260]=count.to_bytes(4,'little')
    expected=bytearray(frame)
    flag_bits=[bool(frame[0x268+item*16]&1) for item in range(max(count-1,0))]
    unflagged=flag_bits.count(False);flagged=flag_bits.count(True)
    for offset,value in [(0x3a0,unflagged),(0x3a4,glyph-unflagged),(0x3a8,flagged)]:
        expected[offset:offset+4]=value.to_bytes(4,'little',signed=True)
    tests.append(dict(frame=frame.hex(),glyph=glyph,after=expected.hex()))
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
p.write_bytes((json.dumps(tests,separators=(',',':'))+'\n').encode('ascii'))
print('Generated2048 source-coordinate controls +512 independent whole944 counter frames')
