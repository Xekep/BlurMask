"""Independent actual source720?C1A/IRLS/main guide check; no supplied glyph labels."""
import json,base64,importlib.util,sys,struct,math
from pathlib import Path
import numpy as np

def load(name,folder):
    spec=importlib.util.spec_from_file_location(name,Path(__file__).parents[1]/folder/'reference_controls.py');value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value
transfer=load('guide_transfer','LegacyFirstResultLine.Probe');fit=load('guide_fit','LegacyCharacterLineFit.Probe');f=fit.f
root=json.loads(Path(sys.argv[1]).read_bytes());published=[base64.b64decode(v) for v in root['published']];p=root['fitProfile'];count=0
for descbytes in root['descriptors']:
    desc=base64.b64decode(descbytes)
    if struct.unpack_from('<H',desc)[0]&15==3:continue
    line=struct.unpack_from('<i',desc,16)[0];first,end=struct.unpack_from('<ii',desc,52);a,z=struct.unpack_from('<ii',desc,36)
    assert line==count and first<end
    items=[]
    for row in published[first:end]:
        b=transfer.transfer(row,root['markers']);n=struct.unpack_from('<I',b,4)[0];candidates=[]
        for i in range(n):
            weight,coefficient=struct.unpack_from('<ff',b,24+i*8);kind=struct.unpack_from('<i',b,184+i*4)[0]
            top,atop=struct.unpack_from('<ff',b,264+i*8);bottom,abottom=struct.unpack_from('<ff',b,424+i*8)
            candidates.append([weight,coefficient,kind,top,atop,bottom,abottom])
        items.append(dict(known=bool(b[0]),clip=bool(b[1]),marker=bool(b[2]),bounds=list(struct.unpack_from('<4f',b,8)),candidates=candidates))
    expected=fit.run(dict(items=items,profile=p,force=False,incoming=float(np.finfo(np.float64).max)))['output']
    points=[point for st in root['parentPoints'][a:z+1] for point in st];miny=min(q[1] for q in points);maxy=max(q[1] for q in points)
    fraction=f(f(maxy-miny)*f(.05))
    if fraction>expected[1]:expected=[f(f(maxy+miny)*f(.5)),max(fraction,p[4]),0]
    guide=base64.b64decode(root['guides'][line]);baseline,height=struct.unpack_from('<ff',guide,12);slope=struct.unpack_from('<f',guide,8)[0]
    assert np.allclose([baseline,height,slope],expected,rtol=2e-5,atol=2e-4),([baseline,height,slope],expected)
    assert struct.unpack_from('<ii',guide,32)==(-1,-2147483648) and struct.unpack_from('<f',guide,24)[0]==0
    #Native16F170 current guide0/4 versus preserved originalC/10.
    cosine=f(1/math.sqrt(float(f(slope*slope))+1));sine=f(math.sqrt(float(f(1-f(cosine*cosine)))))
    if slope>0:sine=-sine
    minx=min(q[0] for q in points);maxx=max(q[0] for q in points);cx=f(f(maxx+minx)*f(.5));cy=f(f(cx*slope)+baseline)
    tx=f(f(f(cy*sine)-f(cx*cosine))+cx);ty=f(f(f(-sine*cx)-f(cy*cosine))+cy)
    b=f(f(f(baseline*cosine)+f(sine*0))+ty);h=f(f(f(f(f(baseline+height)*cosine)+f(sine*0))+ty)-b)
    current=list(struct.unpack_from('<ff',guide,0));assert np.allclose(current,[b,max(h,p[4])],atol=2e-4)
    for original,now in zip(points,[q for st in root['currentPoints'][a:z+1] for q in st]):
        expectedpoint=[f(f(f(cosine*original[0])-f(sine*original[1]))+tx),f(f(f(sine*original[0])+f(cosine*original[1]))+ty)]
        assert np.allclose(now,expectedpoint,atol=2e-4)
    count+=1
print(f'PASS {count} independently solved actual NN/source720 C1A/IRLS guides +published/currentgeometry separation +perdescriptordeskew')
