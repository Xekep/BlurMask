"""Independent design-row normal-equation oracle. Requires NumPy; never invokes vendor code."""
import json,random,sys
from pathlib import Path
import numpy as np
r=random.Random(0x20ed0);out=[]
for k in range(512):
 rows=[];design=[];targets=[];mom=np.zeros(10)
 for i in range(4+k%13):
  x=r.uniform(-15,15);q=r.uniform(-1,1);dq=r.uniform(.2,3);dy=r.uniform(1,20);y=r.uniform(-50,50);w=r.uniform(.1,3)
  rows.append([x,y-dy*.5,y+dy*.5,q-dq*.5,q+dq*.5,w])
  # Equivalent midpoint + span rows, preserving the native span contribution.
  design.extend([[1,x,q],[0,0,dq]]);targets.extend([y,dy])
  mom+=np.array([w,x*w,y*w,q*w,x*x*w,y*x*w,q*x*w,y*y*w,y*q*w+dy*dq*w,q*q*w+dq*dq*w])
 weights=np.repeat([a[5]for a in rows],2);a=np.asarray(design);b=np.asarray(targets)
 normal=a.T@(weights[:,None]*a);normal[1,1]+=1e-12;rhs=a.T@(weights*b)
 # Solve the independent matrix in [intercept,slope,height] order.
 expected=np.linalg.solve(normal,rhs);intercept,slope,height=expected
 if abs(slope)<1e-5:continue
 out.append(dict(rows=rows,solution=[float(height),float(slope),float(intercept)],bounds=[-100,100]))
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(out,separators=(',',':')),encoding='utf8');print(f'Generated {len(out)} independent design-matrix line solves')
