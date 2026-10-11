"""Own explicit lines, native float construction, independently composed affine matrices."""
import json
import math
import sys
from pathlib import Path
import numpy as np


def f(x): return float(np.float32(x))


def deskew(strokes, baseline, height, slope):
    points=np.array([p for s in strokes for p in s],dtype=np.float64)
    low=float(min(points[:,0]));high=float(max(points[:,0]));mid=f(f(high+low)*.5)
    cosine=f(1/f(math.sqrt(f(slope*slope)+1)))
    sine=f(math.sqrt(f(1-f(cosine*cosine))))
    if slope>0:sine=-sine
    anchor=f(f(mid*slope)+baseline)
    tx=f(f(f(anchor*sine)-f(mid*cosine))+mid)
    ty=f(f(f(-sine*mid)-f(anchor*cosine))+anchor)
    newbase=f(f(baseline*cosine)+ty)
    newheight=f(f(f(f(baseline+height)*cosine)+ty)-newbase)
    tolerance=max(f(abs(newheight)*f(2**-22)),f(2**-23))
    if abs(newheight)<=tolerance:
        fallback=f(f(high-low)*f(.15));half=f(fallback*.5)
        newheight=max(fallback,f(.1));newbase=f(newbase-half)
    transformed=[]
    for stroke in strokes:
        transformed.append([[f(f(f(y*-sine)+f(x*cosine))+tx),f(f(f(y*cosine)+f(x*sine))+ty)] for x,y in stroke])
    # Homogeneous affine matrix composition is independent of point-by-point scalar evaluation.
    matrix=np.array([[cosine,-sine,tx],[sine,cosine,ty],[0,0,1]],dtype=np.float64)
    via_matrix=np.column_stack((points,np.ones(len(points))))@matrix.T
    actual=np.array([p for s in transformed for p in s])
    assert np.max(abs(via_matrix[:,:2]-actual))<=2e-6*max(1,float(np.max(abs(via_matrix))))
    return dict(baseline=newbase,height=newheight,cosine=cosine,sine=sine,tx=tx,ty=ty,transformed=transformed)


def metadata(strokes,baseline,height,slope,enabled,guideline,row):
    if not enabled:return dict(alignment=0.,aligned_height=0.,guideline_index=-1,row_index=-2147483648)
    xs=[p[0] for s in strokes for p in s];first=f(f(min(xs)*slope)+baseline);last=f(f(max(xs)*slope)+baseline)
    if guideline is None:return dict(alignment=f(f(last+first)*.5),aligned_height=height,guideline_index=-1,row_index=-2147483648)
    rows,base,pitch=guideline[:3];aligned=base;index=0
    if rows!=1:
        index=row
        if index==-2147483648:
            position=f(f(first-base)/pitch);index=math.trunc(f(position+(.5 if position>0 else -.5)))
        center=f(f(last+first)*.5);aligned=f(f(index*pitch)+base)
        if center<aligned:height=f(height+f(aligned-center))
    return dict(alignment=aligned,aligned_height=height,guideline_index=0,row_index=index)


rng=np.random.default_rng(0x169750);controls=[]
for k in range(180):
    scale=float(10**rng.uniform(-2,2));strokes=[]
    for j in range(1+k%3):strokes.append((rng.normal(size=(1+k%17,2))*scale).astype(np.float32).astype(np.float64).tolist())
    baseline=f(rng.uniform(-2,2)*scale);height=[f(0),f(1e-10),f(.1),f(1),f(10),f(-1)][k%6]
    slope=[f(0),f(1e-6),f(-1e-6),f(.2),f(-.7),f(1e5)][k%6]
    enabled=k%7!=0;guideline=None if k%3==0 else [1 if k%3==1 else 5,f(rng.uniform(-3,3)*scale),f(scale*2),f(1)]
    row=-2147483648 if k%2==0 else 2-k%5
    controls.append(dict(strokes=strokes,input_baseline=baseline,input_height=height,slope=slope,enabled=enabled,guideline=guideline,row=row,
                         deskew=deskew(strokes,baseline,height,slope),metadata=metadata(strokes,baseline,height,slope,enabled,guideline,row)))
# Native round-away-from-zero boundaries, negative slope and absent user guidelines.
for first in [-3.,-2.5,-1.5,-.5,0.,.5,1.5,2.5,3.]:
    strokes=[[[0.,0.],[2.,1.]]];g=[3,0.,1.,2.]
    controls.append(dict(strokes=strokes,input_baseline=first,input_height=1.,slope=0.,enabled=True,guideline=g,row=-2147483648,
                         deskew=deskew(strokes,first,1.,0.),metadata=metadata(strokes,first,1.,0.,True,g,-2147483648)))
path=Path(sys.argv[1]);path.parent.mkdir(parents=True,exist_ok=True);path.write_text(json.dumps(controls,allow_nan=False),encoding='utf-8')
print(f'Generated {len(controls)} own postcallback affine and guideline controls')
