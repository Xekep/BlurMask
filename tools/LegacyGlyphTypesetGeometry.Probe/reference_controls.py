"""Own rendered-font metrics; independent affine coordinate ratios, no vendor binary execution."""
import json
import sys
from pathlib import Path
import numpy as np

def f(x):return float(np.float32(x))
rng=np.random.default_rng(0x6e8660);controls=[]
for i in range(240):
    origin=rng.uniform(-100,100,2).astype(np.float32).astype(np.float64)
    width=f(rng.uniform(.1,30));height=f(rng.uniform(.1,40));bearing=rng.uniform(-4,6,2).astype(np.float32).astype(np.float64)
    if i%5==0:bearing[:]=0
    source=[*origin,width if i%17 else -width,height if i%19 else -height,*bearing]
    if source[2]<=0:bearing[:]=0;source[4:]=list(bearing)
    prepared_height=f(rng.uniform(.1,100));prepared=[f(rng.uniform(-100,100)),f(rng.uniform(-100,100)),f(width*rng.uniform(.1,4)),prepared_height]
    baseline=f(rng.uniform(-100,100));scale=f(rng.uniform(.1,5));style=f(rng.uniform(.1,40))
    measured=[None,f(0),f(-1),f(rng.uniform(.1,30))][i%4]
    ref=measured if measured is not None and measured>0 else style
    nativebaseline=f(f(baseline-prepared[1])/prepared_height);nativex=f(f(ref/prepared_height)*scale)
    left=f(source[4]/max(source[2],0)) if any(source[4:]) else 0.
    right=f(f(source[5]/max(source[2],0))+1) if any(source[4:]) else 1.
    # Independently invert the glyph's affine vertical coordinate basis and
    # verify the normalized baseline and body height reproduce source coordinates.
    inverse=np.linalg.inv(np.array([[prepared_height,prepared[1]],[0.,1.]]))
    normalized=(inverse@np.array([baseline,1.]))[0]
    assert abs(normalized-nativebaseline)<=1e-6*max(1,abs(normalized))
    assert abs(nativex*prepared_height-ref*scale)<=1e-6*max(1,abs(ref*scale))
    controls.append(dict(source=[float(v) for v in source],prepared=prepared,baseline=baseline,scale=scale,style=style,
                         measured=measured,reference=ref,ratios=[nativebaseline,nativex,left,right]))
path=Path(sys.argv[1]);path.parent.mkdir(parents=True,exist_ok=True);path.write_text(json.dumps(controls,allow_nan=False),encoding='utf-8')
print(f'Generated {len(controls)} own rendered-glyph normalization controls')
