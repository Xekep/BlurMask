"""Independent ordinary empty-guideline postselection references; own scalar geometry only."""
import json
from pathlib import Path
import random
import sys
import numpy as np
f=np.float32

def key_sort(items):
    if len(items)<2:return items[:]
    out=items[:];p=out[(len(out)-1)//2][1];a,b=0,len(out)-1
    while a<=b:
        while out[a][1]<p:a+=1
        while out[b][1]>p:b-=1
        if a<=b:out[a],out[b]=out[b],out[a];a+=1;b-=1
    return key_sort(out[:b+1])+out[b+1:a]+key_sort(out[a:])

def sum32(values):
    result=f(0)
    for v in values:result=f(result+v)
    return result

def edges(group,records,x,side):
    chosen=[records[i][5] for i in group if x<=records[i][0] or x<records[i][2]] if side==0 else [records[i][5] for i in group if records[i][2]<=x or records[i][0]<x]
    # IEEE nan for an empty edge subset is retained, matching native comparisons.
    return f(sum32(chosen)/f(len(chosen))) if chosen else f('nan')

def post(records,groups,scale):
    groups=[g[:] for g in groups];scale=f(scale)
    order=key_sort([(i,f(sum32(records[p][5] for p in g)/f(len(g)))) for i,g in enumerate(groups)])
    def box(g):return [min(records[i][0] for i in g),min(records[i][1] for i in g),max(records[i][2] for i in g),max(records[i][3] for i in g)]
    def compatible(a,b,ba,bb):
        ra=f(ba[2]-min(f(ba[2]-ba[0]),f(scale*f(5))))
        lb=f(bb[0]+min(f(bb[2]-bb[0]),f(scale*f(5))))
        ea,eb=edges(a,records,ra,0),edges(b,records,lb,1)
        ma=f(sum32(records[i][5] for i in a)/f(len(a)));mb=f(sum32(records[i][5] for i in b)/f(len(b)))
        if abs(f(ea-ma))>f(scale*f(.25)) or abs(f(eb-mb))>f(scale*f(.25)):return abs(f(eb-ea))<f(scale*f(1.75))
        return abs(f(mb-ma))<scale
    def coverage(a,b):
        union=[]
        for i in a:
            start,end=records[i][0],records[i][2]
            left=next((j for j,v in enumerate(union) if v[1]>=f(start-f(1))),len(union))
            right=next((j for j,v in enumerate(union) if v[0]>f(end+f(1))),len(union))
            if left<right:union[left:right]=[(min(start,union[left][0]),max(end,union[right-1][1]))]
            else:union.insert(left,(start,end))
        result=f(0);eps=f(2**-23)
        for i in b:
            start,end=records[i][0],records[i][2]
            intersect=[v for v in union if v[0]<=f(end+eps) and v[1]>=f(start-eps)]
            if not intersect:amount=f(0)
            elif len(intersect)==1:amount=f(end-start)
            else:
                amount=f(intersect[0][1]-start)
                for v in intersect[1:-1]:amount=f(amount+f(v[1]-v[0]))
                amount=f(amount+f(end-intersect[-1][0]));t=max(f(abs(amount)*f(2**-22)),eps)
                if abs(amount)<=t:amount=f(0)
            result=f(result+amount)
        return result
    for k in range(len(order)-1,0,-1):
        l,r=order[k-1][0],order[k][0];a,b=groups[l],groups[r];ba,bb=box(a),box(b)
        dy=abs(f(order[k-1][1]-order[k][1]));overlap=bb[3]>=ba[1] and ba[3]>=bb[1]
        if dy>=scale and not overlap:continue
        merge=(ba[2]<=bb[0] and compatible(a,b,ba,bb)) or (bb[2]<=ba[0] and compatible(b,a,bb,ba))
        if not merge and dy<scale:merge=coverage(a,b)<f(scale*f(.5))
        if merge:groups[l]+=b;groups[r]=[];order[k]=(-1,order[k][1])
    output=[groups[i] for i,key in order if i>=0];membership=[-1]*len(records)
    for g,ids in enumerate(output):
        for i in ids:membership[i]=g
    return output,membership

def main():
    rng=random.Random(78507255);cases=[]
    for trial in range(512):
        records=[];n=1+trial%23;scale=f([10,40,120][trial%3])
        for i in range(n):
            x=f(rng.randint(-20,90));y=f(rng.randint(-30,120) if trial%5 else 20);w=f(rng.randint(0,50));h=f(rng.randint(0,90))
            s=[x,y,f(x+w),f(y+h),f(x+w/f(2)),f(y+h/f(2)),f(20)]+[f(0)]*10;records.append(s)
        ids=list(range(n));rng.shuffle(ids);groups=[]
        while ids:
            take=min(len(ids),1+rng.randrange(4));groups.append(ids[:take]);ids=ids[take:]
        if trial%7==0:groups=groups[:-1] # explicit unselected strokes
        expected,member=post(records,groups,scale)
        cases.append(dict(statistics=[[float(v) for v in s] for s in records],scale=float(scale),input=groups,groups=expected,membership=member))
    output=Path(sys.argv[1]);output.parent.mkdir(parents=True,exist_ok=True);output.write_text(json.dumps(cases,separators=(',',':')),encoding='utf8');print(f'Wrote {len(cases)} independent postselection cases.')
if __name__=='__main__':main()
