"""Own deterministic float32 ink/config oracle. No SDK or vendor data is loaded."""
import json, math, random, struct, sys
from pathlib import Path

def f(x): return struct.unpack('<f', struct.pack('<f', x))[0]
def add(a,b): return f(a+b)
def sub(a,b): return f(a-b)
def mul(a,b): return f(a*b)
def div(a,b): return f(a/b)
def distance(a,b):
    x,y=sub(a[0],b[0]),sub(a[1],b[1])
    return f(math.sqrt(add(mul(y,y),mul(x,x))))

def reference(source,p):
    n,radius,excluded,same,sine,center,slope,lower,upper=p
    arc=0.
    for stroke in source:
        for a,b in zip(stroke,stroke[1:]): arc=add(arc,distance(a,b))
    step=div(arc,n-1)
    points=[]; owners=[]
    def emit(point,owner): points.append(point); owners.append(owner)
    if step==0:
        for owner,stroke in enumerate(source):emit(stroke[0],owner)
    else:
        carry=0.
        for owner,stroke in enumerate(source):
            if owner:emit(stroke[0],owner)
            for a,b in zip(stroke,stroke[1:]):
                total=add(carry,distance(a,b))
                if total<step:carry=total;continue
                length=sub(total,carry)
                if abs(length)<=max(mul(abs(length),f(2.3841858e-7)),f(1.1920929e-7)):
                    emit(b,owner);carry=length;continue
                t=div(sub(step,carry),length)
                remaining=add(carry,sub(length,step))
                while True:
                    emit([add(mul(sub(1.,t),a[d]),mul(t,b[d])) for d in (0,1)],owner)
                    t=add(t,div(step,length))
                    if t>=1:break
                    remaining=sub(remaining,step)
                carry=remaining
        if div(carry,step)>.5:emit(source[-1][-1],len(source)-1)
    extent=max(sub(max(p[0] for p in points),min(p[0] for p in points)),sub(max(p[1] for p in points),min(p[1] for p in points)))
    outer=mul(extent,radius); near=mul(extent,excluded)
    deltas=[[sub(b[d],a[d]) for d in (0,1)] for a,b in zip(points,points[1:])]
    mids=[[mul(add(a[d],b[d]),.5) for d in (0,1)] for a,b in zip(points,points[1:])]
    lengths=[f(math.sqrt(add(mul(d[0],d[0]),mul(d[1],d[1])))) for d in deltas]
    def square(a,b):
        x,y=sub(mids[a][0],mids[b][0]),sub(mids[a][1],mids[b][1])
        return add(mul(y,y),mul(x,x))
    total=0.
    for i,d in enumerate(deltas):
        if owners[i]!=owners[i+1] or lengths[i]==0:continue
        left=i-1
        while left>=0 and owners[left]==owners[i] and square(i,left)<=mul(near,near):left-=1
        right=i+1
        while right<len(deltas) and owners[right]==owners[i] and square(i,right)<=mul(near,near):right+=1
        best=0.
        for j in list(range(left,-1,-1))+list(range(right,len(deltas))):
            if owners[j]!=owners[j+1] or lengths[j]==0:continue
            sq=square(i,j)
            if sq>=mul(outer,outer):continue
            cross=abs(sub(mul(d[0],deltas[j][1]),mul(d[1],deltas[j][0])))
            angle=div(cross,mul(lengths[j],lengths[i]))
            if angle>=sine:continue
            score=mul(div(sub(outer,f(math.sqrt(sq))),outer),div(sub(sine,angle),sine))
            if owners[i]==owners[j]:score=mul(score,same)
            best=max(best,score)
        total+=best
    mean=total/(len(points)-1) if len(points)>1 else total
    if lower==upper:lower,upper=0.,1.
    mode2=f(1/(1+math.exp(((1-mean)-center)*-slope))*abs(sub(upper,lower))+lower)
    return [f(mean),sub(1.,mode2),mode2,len(points)]

r=random.Random(257)
cases=[]
for index in range(512):
    strokes=[]
    for s in range(r.randrange(1,5)):
        points=[];x=f(r.randrange(-100,101));y=f(r.randrange(-40,41))
        for q in range(r.randrange(1,9)):
            x=add(x,f(r.randrange(-10,21)*.25));y=add(y,f(r.randrange(-6,7)*.25))
            points.append([x,y])
        strokes.append(points)
    profile=[r.randrange(3,50),r.uniform(.05,1.5),r.uniform(0,.15),r.uniform(.1,1),r.uniform(.1,.8),r.uniform(0,1),r.uniform(-8,8),r.uniform(0,.3),r.uniform(.7,1)]
    profile=[int(profile[0])]+list(map(f,profile[1:]))
    if index%31==0:profile[-1]=profile[-2]
    cases.append({'profile':profile,'strokes':strokes,'expected':reference(strokes,profile)})
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps({'cases':cases},separators=(',',':')),encoding='utf-8')
print('Generated',len(cases),'own float32 controls:',out)
