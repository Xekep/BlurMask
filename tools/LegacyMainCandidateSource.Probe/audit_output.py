"""Independent05AFA0 ordinary projection for the actual single-object raw root.
The root's duplicated raw point is retained: six caller points, not sampled IDs.
"""
import base64, json, math, struct, sys
from pathlib import Path
d = json.loads(Path(sys.argv[1]).read_text())
f32 = lambda x: struct.unpack('<f',struct.pack('<f',x))[0]

def pack(coordinate,mode,count):
    assert 0<=coordinate<=count-1
    integral = math.floor(coordinate); remainder = f32(coordinate-f32(integral))
    if remainder==0 and integral==count-1: fraction = 255
    else:
        fraction = int(f32(f32(remainder*200)+f32(.5)))
        if fraction==200:
            integral+=1; fraction=255 if integral==count-1 else 0
    return (integral<<16)|(fraction<<8)|mode

assert [s['configured'] for s in d['sources']]==[False,True,True]
count=0
for source in d['sources']:
    for glyph in source['glyphs']:
        refs=base64.b64decode(glyph['references']);residuals=base64.b64decode(glyph['residuals'])
        assert glyph['pointCount']==6 and len(refs)%24==0 and len(residuals)==len(refs)//12
        expected=[]
        for i in range(len(refs)//24):
            group,obj,point,last_group,last_obj,last_point=struct.unpack_from('<6i',refs,i*24)
            assert (group,obj,last_group,last_obj)==(0,0,0,0)
            first=pack(f32(f32(point)-f32(f32(residuals[i*2])/200)),3,6)
            last=pack(f32(f32(last_point)-f32(f32(residuals[i*2+1])/200)),5,6)
            same=(first>>16)==(last>>16) and (((first>>8)&255)==((last>>8)&255) or (first>>8)&255==255 or (last>>8)&255==255)
            closed=last_point==5 and residuals[i*2+1]==0 or (last>>8)&255==255 or (last>>16)==0 and (last>>8)&255==0 or same
            expected.append([first,(last&~255)|(5 if closed else 1)])
        assert glyph['positions']==expected
        count+=len(expected)
assert count==d['projected']==3 and d['guards']==21
print('PASS three actual selected-candidate metadata exports -> original six-point raw-document packed positions, independent residual/float32/terminalFF;21 ownership guards.')
