"""Own prepared moment samples; independent scalar fit and exhaustive partitions, no SDK execution."""
import itertools, json, math, random, struct, sys
from pathlib import Path
def f32(x): return struct.unpack('<f',struct.pack('<f',x))[0]

def score(groups, axis, scale, p):
    # Each prepared group's moments contribute its normalized raw moments once.
    n=len(groups); a=0 if axis=='X' else 1; b=1-a
    means=[(g[3]/g[2],g[4]/g[2]) for g in groups]
    xx=sum(g[5]/g[2] for g in groups); yy=sum(g[6]/g[2] for g in groups)
    xy=sum(g[7]/g[2] for g in groups); sums=[sum(t[k] for t in means) for k in (0,1)]
    squares=[xx,yy]
    variance=(squares[a]*n-sums[a]**2)/(n*n*scale*scale)
    gap=adjgap=0; basic=p[1] if n==1 and groups[0][12] else p[0]
    for g,h in zip(groups,groups[1:]):
        distance=(g[8]-h[9]) if a==0 else (h[10]-g[11])
        adjacent=g[1]+1==h[0]
        gap+=max(0,distance); adjgap+=max(0,distance) if adjacent else 0
        if adjacent: basic+=p[2]
    basic+=(gap*p[3+a]/scale)**2+(adjgap*p[5]/scale)**2
    penalty=lambda v: min(p[7]/v,p[8])+p[6]*v if v>0 else p[8]
    result=basic+penalty(variance)
    determinant=squares[b]*n-sums[b]**2
    if p[9]>0 and determinant>0 and abs(sums[b]**2)+abs(squares[b]*n)<=abs(determinant)*8388608:
        slope=(xy*n-sums[a]*sums[b])/determinant; intercept=(sums[a]-slope*sums[b])/n
        residual=(((slope*slope*squares[b]+squares[a]+intercept*n*intercept)-2*slope*xy-2*intercept*sums[a]+2*slope*intercept*sums[b])/(n*scale*scale))
        blend=min(1,sum(g[2] for g in groups)/scale/p[9]); mixed=(1-blend)*variance+blend*residual
        result=basic+penalty(mixed)
        if mixed>0 and (a==0 or variance>0):
            # Sorted-set interval closure, intentionally independent of native insertion algorithm.
            intervals=sorted((f32(g[10]),f32(g[11])) if a==0 else (f32(g[8]),f32(g[9])) for g in groups)
            union=[]
            for lo,hi in intervals:
                if union and lo<=f32(union[-1][1]+1): union[-1]=(union[-1][0],max(union[-1][1],hi))
                else: union.append((lo,hi))
            gaps=sum(f32(union[i][0]-union[i-1][1]) for i in range(len(union)-1,0,-1))
            result=min(basic+penalty(variance), basic+(gaps*p[4-a]/scale)**2+penalty(mixed))
    return result

def exhaustive(gs,axis,scale,p):
    best=math.inf; winner=None; n=len(gs)
    for mask in range(1<<(n-1)):
        starts=[0]+[i+1 for i in range(n-1) if mask&(1<<i)]+[n]
        ranges=list(zip(starts, [i-1 for i in starts[1:]]))
        costs=[score(gs[lo:hi+1],axis,scale,p) for lo,hi in ranges]
        value=sum(costs)
        if value<best: best=value;winner=ranges
    return best,winner

rng=random.Random(682950); output=[]
def arc_moments(points,index):
    weighted=[]
    for (x,y),(u,v) in zip(points,points[1:]):
        dx=f32(u-x);dy=f32(v-y)
        weighted.append((math.hypot(dx,dy),float(f32(x+u))*.5,float(f32(y+v))*.5))
    weighted.append((1,*points[-1]))
    xs=[p[0] for p in points];ys=[p[1] for p in points]
    return [index,index,sum(t[0] for t in weighted),sum(w*x for w,x,y in weighted),sum(w*y for w,x,y in weighted),sum(x*x*w for w,x,y in weighted),sum(y*y*w for w,x,y in weighted),sum(y*x*w for w,x,y in weighted),min(xs),max(xs),min(ys),max(ys),False,-2147483648,-1]
for case in range(160):
    n=1+case%8; gs=[];strokes=[];moments=[]
    for i in range(n):
        points=[(rng.uniform(-50,50)+i*8,rng.uniform(-20,20)+i*6) for _ in range(2+case%9)]
        if case%17==0: points=[(i*6,4)]*3
        prepared=[(f32(x),f32(y)) for x,y in points]
        strokes.append(prepared);moments.append(arc_moments(prepared,i))
        weight=len(points); xs=[t[0] for t in points]; ys=[t[1] for t in points]
        gs.append([i*2,i*2+int(case%3==0),weight,sum(xs),sum(ys),sum(x*x for x in xs),sum(y*y for y in ys),sum(x*y for x,y in points),min(xs),max(xs),min(ys),max(ys),case%9==0,-2147483648,-1])
    p=[f32(rng.uniform(.1,3)) for _ in range(10)]
    p[9]=0 if case%3==0 else f32(rng.uniform(.1,40)); scale=rng.uniform(.1,50)
    for axis in ('X','Y'):
        expected=score(gs,axis,scale,p);cost,ranges=exhaustive(gs,axis,scale,p)
        output.append(dict(groups=gs,profile=p,axis=axis,scale=scale,expected=expected,searchCost=cost,strokes=strokes,moments=moments))
Path(sys.argv[1]).parent.mkdir(parents=True,exist_ok=True)
Path(sys.argv[1]).write_text(json.dumps(output,separators=(',',':')),encoding='utf8')
print('Wrote',len(output),'own moment costs and exhaustive partition minima')
