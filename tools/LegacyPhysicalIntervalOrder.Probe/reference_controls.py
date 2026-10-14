"""Independent scalar interpreter of grouped fresh15F050 interval insertion."""
import json,sys
from pathlib import Path
import numpy as np
f=np.float32

def order(boxes,groups,scale,tolerance):
    result=[]
    for group in groups:
        first=len(result)
        for current in sorted(group):
            end=len(result);threshold=float(f(tolerance))*float(scale)+float(f(boxes[current][1]))
            scan=next((i for i in range(first,end) if threshold<float(f(boxes[result[i]][0]))),end)
            if scan==end:result.append(current);continue
            previous=scan-1
            while previous>=first and f(boxes[current][0])<f(boxes[result[previous]][0]):previous-=1
            if previous>=first:
                anchor=f(boxes[result[previous]][1])
                for i in range(previous+1,scan):
                    if not f(boxes[result[i]][0])<anchor:break
                    previous=i
            result.insert(previous+1,current)
    return result

def main():
    rng=np.random.default_rng(15050014);cases=[]
    for i in range(512):
        n=1+i%20
        if i%8==0:boxes=[[float(f(20*(n-k))),float(f(20*(n-k)+10))] for k in range(n)]
        else:
            starts=rng.integers(-100,100,n).astype(np.float32);width=rng.integers(0,120,n).astype(np.float32)
            boxes=[[float(a),float(f(a+b))] for a,b in zip(starts,width)]
        perm=rng.permutation(n).tolist();g=1+i%min(n,4);groups=[perm[j::g] for j in range(g)]
        scale=float(f([1,10,80][i%3]));tol=float(f([0,.25,1][i%3]))
        cases.append(dict(boxes=boxes,groups=groups,scale=scale,tolerance=tol,expected=order(boxes,groups,scale,tol)))
    #Explicit adjacent-contact/fixed-anchor/strict-left/tolerance-equality guards.
    for boxes in [[[20,30],[0,10],[10,20]],[[0,35],[15,60],[40,45],[5,10]],[[0,10],[30,40],[20,20]],[[0,0],[0,0],[0,0]]]:
        groups=[list(range(len(boxes)))];cases.append(dict(boxes=boxes,groups=groups,scale=80,tolerance=.25,expected=order(boxes,groups,80,.25)))
    if len(sys.argv)!=2 or Path(sys.argv[1]).suffix.lower()!='.json':raise ValueError('Supply one output.json')
    Path(sys.argv[1]).write_bytes((json.dumps(dict(cases=cases),separators=(',',':'))+'\n').encode('utf8'))
    print(f'Wrote {len(cases)} independent interval/tolerance/fixed-anchor controls.')
if __name__=='__main__':main()
