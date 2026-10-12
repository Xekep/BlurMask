"""Original scalar range costs + exhaustive suffix partitions; no vendor execution."""
import argparse, functools, itertools, json, math, random, struct
from pathlib import Path

def f(v): return struct.unpack('<f', struct.pack('<f', v))[0]
def plus(a,b): return f(a+b)
def sub(a,b): return f(a-b)
def mul(a,b): return f(a*b)
def div(a,b): return f(a/b)

def moments(points):
    out=[0.0]*6
    for a,b in zip(points,points[1:]):
        x,y=plus(a[0],b[0])*0.5,plus(a[1],b[1])*0.5
        dx,dy=sub(b[0],a[0]),sub(b[1],a[1]);w=math.sqrt(dy*dy+dx*dx)
        vals=[w,w*x,w*y,x*x*w,y*y*w,y*x*w]
        out=[v+old for v,old in zip(vals,out)]
    x,y=points[-1]
    return [v+old for v,old in zip([1,x,y,x*x,y*y,y*x],out)]+[min(p[0] for p in points),max(p[0] for p in points),min(p[1] for p in points),max(p[1] for p in points)]

def union_intervals(items):
    # Independent sorted connected-component union. Native inputs use one-unit tolerance.
    result=[]
    for start,end in sorted(items):
        if result and sub(start,1)<=result[-1][1]: result[-1]=(result[-1][0],max(end,result[-1][1]))
        else: result.append((start,end))
    return result

def union_cost(items,weight,scale):
    intervals=union_intervals(items)
    gaps=[sub(b[0],a[1]) for a,b in zip(intervals,intervals[1:])]
    total=0.0
    for v in reversed(gaps):total+=v
    return (total*(weight/scale))**2

def range_data(c,h,first,last):
    p=c['p'];raw=[moments(x) for x in c['strokes'][first:last+1]]
    sums=raw[0][:6];box=raw[0][6:];penalty=extra=0.0;s=c['scale'];primary=c['primary'];flags=c['flags'][first:last+1]
    def overlap(weight,depth,lower):
        length=f(weight/s);blend=div(length,p[12]) if length<=p[12] else 1.0
        return mul(mul(blend,p[11] if lower else p[10]),min(depth,p[13]))
    for i,r in enumerate(raw[1:],1):
        xlo,xhi,ylo,yhi=r[6:];xmin,xmax,ymin,ymax=box
        if h:
            if c['directional']:
                gap=div(sub(xlo,xmax),f(s))
                v=mul(gap,p[9]) if gap>0 else overlap(r[0],f(-gap),plus(ymin,ymax)<=plus(ylo,yhi))
            elif xlo>xmax:v=mul(div(sub(xlo,xmax),f(s)),p[9])
            elif xmin>xhi:v=mul(div(sub(xmin,xhi),f(s)),p[9])
            else:v=overlap(r[0],div(min(sub(xmax,xlo),sub(xhi,xmin)),f(s)),plus(ymin,ymax)<=plus(ylo,yhi))
            for j,earlier in enumerate(raw[:i]):
                if not(flags[i] or flags[j]) or xlo>earlier[7] or earlier[6]>xhi:continue
                gap=max(0.0,sub(ylo,earlier[9]),sub(earlier[8],yhi))
                if gap>0:
                    value=mul(gap,p[3])/primary
                    if flags[i] and flags[j]:value+=value
                    extra=value+extra
        else:
            gap=div(sub(ylo,ymax),f(s));v=mul(gap,p[9]) if gap>0 else overlap(r[0],f(-gap),plus(xlo,xhi)<=plus(xmin,xmax))
        v=plus(v,0);penalty=mul(v,v)+penalty
        sums=[v+old for v,old in zip(r[:6],sums)];box=[min(xmin,xlo),max(xmax,xhi),min(ymin,ylo),max(ymax,yhi)]
    w,sx,sy,sxx,syy,sxy=sums
    dep,square,ind,indsq=(sy,syy,sx,sxx) if h else (sx,sxx,sy,syy)
    variance=(square*w-dep*dep)/(w*w*s*s)
    reg=0.0 if c['external'] else p[8]
    determinant=indsq*w-ind*ind
    if reg>0 and determinant>0:
        slope=(sxy*w-dep*ind)/determinant;intercept=(dep-slope*ind)/w
        blend=min(1.0,(w/s)/reg)
        residual=(((((slope*slope*indsq+square+intercept*w*intercept)-(slope+slope)*sxy)-(intercept+intercept)*dep)+(slope+slope)*intercept*ind)/(s*w*s))
        variance=variance*(1-blend)+blend*residual
    cost=p[7] if variance<=0 else min(p[6]/variance,p[7])+p[5]*variance
    basic=p[1]+penalty+(extra if h else 0)+cost
    # Ordered interval start is the suffix minimum; end is the prefix maximum.
    intervals=[(r[8],r[9]) if h else (r[6],r[7]) for r in raw]
    starts=[min(a for a,b in intervals[i:]) for i in range(len(intervals))]
    ends=[max(b for a,b in intervals[:i+1]) for i in range(len(intervals))]
    ordered=0.0
    for i in range(1,len(intervals)):
        d=sub(starts[i],ends[i-1])
        if d>0:ordered=plus(ordered,d)
    chronology=(p[4]/s)*ordered
    cost=chronology*chronology+union_cost([(r[6],r[7]) for r in raw],p[2],s)+basic+union_cost([(r[8],r[9]) for r in raw],p[3],s)
    extent=sub(box[3],box[2]) if h else sub(box[1],box[0])
    return dict(own=cost,moments=sums+box,extent=extent)

def reference(c,h):
    n=len(c['strokes']);data={(i,j):range_data(c,h,i,j) for i in range(n) for j in range(i,n)}
    labels=c['labels']
    if all(x==-1 for x in labels):
        # Exhaustive full partition oracle; this does not call the native backwards DP.
        @functools.lru_cache(None)
        def paths(start):
            if start==n:return ((0.0,()),)
            result=[]
            for end in range(start,n):
                d=data[start,end]
                if end>start and (end-start>c['budget']+1 or d['extent']>c['p'][0]*c['scale']):break
                for cost,groups in paths(end+1):result.append((d['own']+cost,(end,)+groups))
            return tuple(result)
        rows=[]
        for start in range(n):
            cost,path=min(paths(start),key=lambda v:v[0]);end=path[0]
            rows.append(dict(end=end,cluster=-1,suffix=cost,**data[start,end]))
        return rows
    # Separate imperative metadata oracle: native gate consults BEST label, not all candidate labels.
    rows=[None]*n
    for first in reversed(range(n)):
        d=data[first,first];best=dict(end=first,cluster=labels[first],suffix=d['own']+(rows[first+1]['suffix'] if first+1<n else 0),**d);unknown=0
        for last in range(first+1,n):
            label=labels[last]
            if label!=-1 and best['cluster']!=-1 and label!=best['cluster'] or c['budget']<unknown:break
            if label==-1:unknown+=1
            d=data[first,last]
            if d['extent']>c['p'][0]*c['scale']:break
            cost=d['own']+(rows[last+1]['suffix'] if last+1<n else 0)
            if cost<best['suffix']:best=dict(end=last,cluster=label if label!=-1 else best['cluster'],suffix=cost,**d)
        rows[first]=best
    return rows

parser=argparse.ArgumentParser();parser.add_argument('output',type=Path);a=parser.parse_args()
r=random.Random(0x06f6e0);cases=[]
for index in range(384):
    n=r.randint(1,8);strokes=[]
    for j in range(n):
        x,y=r.randint(-20,100),r.randint(-20,100)
        if index%11==0:x,y=100000+x,100000+y
        points=[[f(x),f(y)]]
        for k in range(r.randint(0,5)):points.append([f(points[-1][0]+r.randint(-15,25)+.125),f(points[-1][1]+r.randint(-15,25)+.375)])
        strokes.append(points)
    # MaximumExtent,Base,XGap,YGap,ordered,variance,inverse,zero,regression,gap,upper,lower,blend,clamp.
    p=[r.choice([0,1.3,10]),r.choice([0,1.3,3]),.2,3,8,60,.01,5,r.choice([0,2,20]),.5,.5,1,4,5]
    p=[f(x) for x in p]
    c=dict(strokes=strokes,p=p,budget=r.choice([0,1,3,200]),directional=bool(index%2),horizontal=index%3!=2,vertical=index%3!=1,primary=r.choice([.25,8,55,100]),scale=r.choice([.25,8,55,100]),external=bool(index%4),flags=[bool(r.getrandbits(1)) for _ in range(n)],labels=[-1]*n if index<256 else [r.choice([-1,-1,0,1,2]) for _ in range(n)])
    c['x']=reference(c,True) if c['horizontal'] else []
    c['y']=reference(c,False) if c['vertical'] else []
    c['selectedX']=bool(c['x']) and (not c['y'] or c['x'][0]['suffix']<c['y'][0]['suffix'])
    cases.append(c)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(cases,separators=(',',':')),encoding='utf8')
print(f'Generated {len(cases)} raw-source controls, including256 exhaustive suffix-partition cases and128 labeled best-state gate cases')
