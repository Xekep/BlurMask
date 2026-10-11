"""Vendor-free geometry oracle: exhaustive endpoint pairs, shoelace area and ray crossing."""
import argparse,json,math,random,struct
from pathlib import Path
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def add(a,b):return f(f(a)+f(b))
def sub(a,b):return f(f(a)-f(b))
def mul(a,b):return f(f(a)*f(b))
def div(a,b):return f(f(a)/f(b)) if b else float('nan')
def dist(a,b):return f(math.hypot(sub(a[0],b[0]),sub(a[1],b[1])))
def inside(poly,q):
    crossings=0
    for a,b in zip(poly,poly[1:]+poly[:1]):
        if (a[1]>q[1])!=(b[1]>q[1]):
            x=(b[0]-a[0])*(q[1]-a[1])/(b[1]-a[1])+a[0]
            if q[0]<x:crossings+=1
    return crossings%2!=0
def evaluate(strokes,ordinal):
    p=strokes[ordinal];n=len(p)
    if n>500 or n<2:return 0.
    arc=[0.]*n;total=0.;i=1
    while i+3<n:
        a=dist(p[i-1],p[i]);arc[i]=add(a,total)
        b=add(add(dist(p[i],p[i+1]),a),total);arc[i+1]=b
        c=add(dist(p[i+1],p[i+2]),b);arc[i+2]=c
        total=add(dist(p[i+2],p[i+3]),c);arc[i+3]=total;i+=4
    while i<n:total=add(total,dist(p[i-1],p[i]));arc[i]=total;i+=1
    turns=[1]*n;directions=[0,0];last=[-1.,-1.];threshold=mul(total,.01)
    for i in range(1,n):
        for axis in range(2):
            delta=sub(p[i][axis],p[i-1][axis]);sign=1 if delta<0 else (-1 if delta>0 else 0)
            if sign:
                if directions[axis]==-sign and (last[axis]==-1 or abs(sub(p[i-1][axis],last[axis]))>threshold):
                    turns[i-1]+=1;last[axis]=p[i-1][axis]
                directions[axis]=sign
        turns[i]=turns[i-1]
    turns[-1]+=1
    quarter=mul(total,.25);best=0.;bestpoly=None
    for end in range(n-1,0,-1):
        tail=sub(total,arc[end])
        if tail>=quarter:continue
        for start in range(end-1,-1,-1):
            outside=add(arc[start],tail)
            if not(arc[start]<quarter and outside<quarter):continue
            dx=sub(p[start][0],p[end][0]);dy=sub(p[start][1],p[end][1]);sq=add(mul(dx,dx),mul(dy,dy))
            if sq>=mul(mul(total,total),.1) or turns[end]-turns[start]>=11:continue
            closure=f(math.sqrt(sq));path=sub(arc[end],arc[start]);penalty=div(add(add(closure,closure),outside),add(closure,path))
            if not(penalty<.5 and div(closure,path)<.25):continue
            perimeter=add(dist(p[end-1],p[start]),sub(arc[end-1],arc[start]));poly=p[start:end]
            # Independent polygon area calculation, rather than native central-y differences.
            area=abs(sum(a[0]*b[1]-a[1]*b[0] for a,b in zip(poly,poly[1:]+poly[:1])))*.5
            score=div(f(area),mul(perimeter,perimeter))
            if score>f(.0159):
                score=mul(sub(1.,penalty),score)
                if score>best:best=score;bestpoly=poly
    if bestpoly is None:return 0.
    if any(sum(inside(bestpoly,q) for q in stroke)*2>len(stroke) for j,stroke in enumerate(strokes) if j!=ordinal):return mul(best,f(12.566))
    return 0.
def main():
    ap=argparse.ArgumentParser();ap.add_argument('output');a=ap.parse_args();rng=random.Random(927104);cases=[]
    for i in range(384):
        n=rng.randint(8,96);rx=rng.uniform(4,50);ry=rng.uniform(4,50);cx=rng.uniform(-60,60);cy=rng.uniform(-60,60)
        angle=rng.uniform(-math.pi,math.pi);delta=math.tau*rng.uniform(.8,1.13)
        poly=[[f(cx+rx*math.cos(angle+delta*j/n)),f(cy+ry*math.sin(angle+delta*j/n))] for j in range(n+1)]
        if i%4==0:poly[-1]=poly[0].copy()
        if i%13==0:poly=poly[::-1]
        strokes=[poly]
        for j in range(rng.randint(1,3)):
            stroke=[]
            for k in range(rng.randint(1,7)):
                qx=rng.uniform(-1.6,1.6);qy=rng.uniform(-1.6,1.6)
                stroke.append([f(cx+rx*qx),f(cy+ry*qy)])
            strokes.append(stroke)
        if i%11==0:strokes=strokes[::-1]
        cases.append({'Strokes':strokes,'Expected':[evaluate(strokes,j) for j in range(len(strokes))]})
    out=Path(a.output);out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps({'Cases':cases}),encoding='utf8')
    print('Generated',len(cases),'vendor-free enclosure controls')
if __name__=='__main__':main()
