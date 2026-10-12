"""Independent ordinary167210 adjacent-raw dedup and exact15abe0 ratios.
Near-zero oblique edges intentionally distinguish tolerant265 fallback.
"""
import json,random,sys
from pathlib import Path
import numpy as np
f=lambda x:float(np.float32(x))
def changed(a,b):return abs(a)+abs(b)<abs(f(a-b))*8388608.
random.seed(150219);result=[]
for trial in range(256):
    strokes=[]
    for s in range(1+trial%5):
        if trial%2==0:points=[[0.,0.],[10.,f(1e-8)],[20.,0.],[30.,0.]]
        else:points=[[f(random.randrange(-8,9)),f(random.randrange(-8,9))] for i in range(1+(trial+s)%10)]
        if trial%7==0:points+=points[-1:]
        strokes.append(points)
    prepared=[]
    for points in strokes:
        prepared.append([v for i,v in enumerate(points) if i==0 or changed(v[0],points[i-1][0]) or changed(v[1],points[i-1][1])])
    edges=orthogonal=corners=0;nondegenerate=False
    for points in prepared:
        edges+=len(points)-1
        for i in range(1,len(points)):
            xzero=points[i][0]==points[i-1][0];yzero=points[i][1]==points[i-1][1]
            orthogonal+=int(xzero or yzero)
            if i+1<len(points):corners+=int((xzero and points[i][1]==points[i+1][1]) or (yzero and points[i][0]==points[i+1][0]))
            nondegenerate|=not xzero or not yzero
    metrics=[f(orthogonal/edges) if edges else 0.,f(corners/(edges-len(prepared))) if edges>len(prepared) else 0.]
    result.append(dict(strokes=strokes,metrics=metrics,compose=nondegenerate and trial%2!=0))
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(result,separators=(',',':')),encoding='utf-8')
print(f'{len(result)} source metric producer references -> {out}')
