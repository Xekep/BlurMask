"""Own glyph parallelograms: vectorized affine corner construction, no vendor data."""
import json,sys
import numpy as np
rng=np.random.default_rng(935025)
cases=[]
for index in range(320):
    origin=rng.uniform(-2000,2000,2).astype(np.float32)
    edges=rng.uniform(-400,400,(2,2)).astype(np.float32)
    if index%8==0:edges[0,1]=edges[1,0]=0
    # Four vertices via a basis and two-step float32 translation.
    basis=np.array([[0,0],[1,0],[1,1],[0,1]],np.float32)
    corners=np.tile(origin,(4,1))
    for column in range(2):corners=(corners+basis[:,column,None]*edges[column]).astype(np.float32)
    lines=None if index%3==0 else [float(np.float32(rng.uniform(-2,3))),float(np.float32(rng.uniform(0,2)))]
    cases.append(dict(geometry=[float(v) for v in np.concatenate([origin,edges.ravel()])],lines=lines,corners=[float(v) for v in corners.ravel()]))
with open(sys.argv[1],'w',encoding='utf8')as stream:json.dump(cases,stream,separators=(',',':'))
print('Generated',len(cases),'independent affine quad controls')
