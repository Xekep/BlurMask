"""Independent scalar2d0 prepared-stage oracle; no DLL, vendor tables or C# calls.

OwnChild is a deliberately original deterministic supplied callback, not child249.
Axis extrema use the decompiled branch, rather than the managed direction helper.
"""
import json, math, random, struct, sys
from pathlib import Path
import numpy as np

f = lambda x: float(np.float32(x))
add = lambda a,b: f(f(a)+f(b))
sub = lambda a,b: f(f(a)-f(b))
mul = lambda a,b: f(f(a)*f(b))
div = lambda a,b: f(f(a)/f(b))
approx = lambda a,b: abs(sub(a,b)) <= max(mul(max(abs(a),abs(b)),2**-22),2**-23)

def axis(points, column):
    anchor = distinct = points[0][column]
    result = []
    for i in range(1,len(points)):
        c,b = points[i][column], points[i-1][column]
        if c == b: continue
        peak = distinct < b and c < b
        if peak:
            turning = True
        else:
            decreasing = b < distinct
            distinct = b
            turning = decreasing and b < c
        if turning:
            result.append(abs(sub(b,anchor))); distinct = anchor = b
    if points[-1][column] != anchor: result.append(abs(sub(points[-1][column],anchor)))
    return result

def scale(strokes,q):
    x,y = [],[]
    for points in strokes: x.extend(axis(points,0)); y.extend(axis(points,1))
    def choose(values):
        if not values: return f(np.finfo(np.float32).max)
        position = mul(len(values)-1,q)
        index = int(sub(position,.5) if position <= 0 else add(position,.5))
        return sorted(values)[min(len(values)-1,index)]
    return min(choose(x),choose(y)),x,y

def rotate(strokes,angle):
    a = float(angle)*.017453292519943295
    if a == 0: return strokes
    cosine,sine = math.cos(a),math.sin(a)
    return [[[f(cosine*x+sine*y),f(cosine*y-sine*x)] for x,y in points] for points in strokes]

def sort_desc(records):
    def visit(first,last):
        while first < last:
            left,right = first,last
            pivot = records[(first+last)//2][1]
            while left <= right:
                while records[left][1] > pivot: left += 1
                while records[right][1] < pivot: right -= 1
                if left > right: break
                records[left],records[right] = records[right],records[left]
                left += 1; right -= 1
            if first < right: visit(first,right)
            first = left
    visit(0,len(records)-1)
    return records

def score(strokes,angles,s):
    result = []
    for index,angle in enumerate(angles):
        rotated = rotate(strokes,angle); ys = [p[1] for points in rotated for p in points]
        lo,hi = min(ys),max(ys); width = div(s,10)
        bins = [0]* (int(div(sub(hi,lo),width))+1)
        for y in ys: bins[int(div(sub(y,lo),width))] += 1
        value = 0.
        for count in bins:
            if count: value = add(value,mul(f(math.log(count)),count))
        result.append([index,value])
    return sort_desc(result)

def box(points):
    return (min(p[0] for p in points),min(p[1] for p in points),max(p[0] for p in points),max(p[1] for p in points))
def center(points):
    length,x,y = 0.,0.,0.
    for previous,current in zip(points,points[1:]):
        dx,dy = sub(current[0],previous[0]),sub(current[1],previous[1])
        arc = math.hypot(dx,dy); length += arc
        x += add(current[0],previous[0])*arc; y += add(current[1],previous[1])*arc
    return points[0] if length <= 0 else [f(x*.5/length),f(y*.5/length)]

def reject(strokes,s,p):
    last = None; eligible = forward = above = below = 0
    for points in strokes:
        b=box(points); width,height=sub(b[2],b[0]),sub(b[3],b[1])
        if max(width,height)<mul(s,.5) or height<mul(s,.35): continue
        eligible += 1; x,y=center(points)
        if last:
            if y <= last[1] or last[3] <= y:
                if last[0]<x<last[2]:
                    mid = mul(add(last[3],last[1]),.5)
                    if y > mid: above += 1
                    elif y < mid: below += 1
            else:
                mid = mul(add(last[2],last[0]),.5)
                if x < mid if p[9] else x > mid: forward += 1
        last=b
    return eligible>8 and (div(forward,eligible)<p[7] or div(above,eligible)>p[8] or div(below,eligible)>p[8])

def coverage(strokes,s):
    boxes=list(map(box,strokes)); first=len(boxes)-1
    for i,b in enumerate(boxes):
        if mul(s,.5)<sub(b[2],b[0]) or mul(s,.5)<sub(b[3],b[1]): first=i;break
    lo,top,hi,bottom=boxes[first]; widths=heights=0.
    for nlo,ntop,nhi,nbottom in boxes[first+1:]:
        if sub(nhi,nlo)<mul(s,.5) and sub(nbottom,ntop)<mul(s,.5): continue
        if ntop<=bottom: lo,top,hi,bottom=min(lo,nlo),min(top,ntop),max(hi,nhi),max(bottom,nbottom)
        else: widths=add(widths,sub(hi,lo));heights=add(heights,sub(bottom,top));lo,top,hi,bottom=nlo,ntop,nhi,nbottom
    heights=add(heights,sub(bottom,top))
    return 0 if approx(heights,0) else div(add(sub(hi,lo),widths),heights)

def own_child(strokes,power):
    total=0.; count=0
    for points in strokes:
        for x,y in points:
            total=add(total,add(mul(abs(x),.25),mul(abs(y),.75))); count+=1
    value=math.floor(add(total,.5));quotient=value//count
    product=mul(f(math.pow(len(strokes),power)),quotient)
    return -2147483648 if not math.isfinite(product) or product>=2147483648 or product< -2147483648 else int(product)

def decide(strokes,angles,s,p,ranked):
    selected=[i for i,_ in ranked[:int(p[5])]]
    if len(strokes)>8: selected=[i for i in selected if not reject(rotate(strokes,angles[i]),s,p)]
    if not selected: return 0,0,selected,[]
    if len(selected)==1: return angles[selected[0]],0,selected,[]
    v=coverage(rotate(strokes,angles[selected[0]]),s)
    if v>=p[6]:
        first=selected[0]
        if not approx(abs(angles[first]),90):
            if len(strokes)>=7: return angles[first],v,selected,[]
            selected=[angles.index(0),first]
        else:
            plus=any(approx(angles[i],90) for i in selected); minus=any(approx(angles[i],-90) for i in selected)
            selected=[]
            if plus:selected.append(angles.index(90))
            if minus:selected.append(angles.index(-90))
            if len(strokes)<7 and angles.index(0) not in selected:selected.append(angles.index(0))
            if len(selected)==1:return angles[selected[0]],v,selected,[]
    elif len(strokes)<7 and angles.index(0) not in selected:selected.append(angles.index(0))
    costs=[[i,own_child(rotate(strokes,angles[i]),p[4])] for i in selected]
    best=min(range(len(costs)),key=lambda i:costs[i][1])
    return angles[costs[best][0]],v,selected,costs

random.seed(31040); controls=[]
for trial in range(384):
    strokes=[]
    for s in range(1+trial%12):
        if trial%8==0:
            points=[[f(s*17+x),f((s%3)*25+y)] for x,y in [(0,0),(0,20),(12,20),(12,0),(12,0),(4,0),(4,8)]]
        else:
            points=[[f(random.randrange(-80,81)+s*24),f(random.randrange(-70,71)+(s%4)*30)] for _ in range(2+trial%7)]
        strokes.append(points)
    p=[-90.,90.,15. if trial%3 else 30.,[0.,.5,.95,1.][trial%4], [.0,.5,1.][trial%3], [1,3,9,13][trial%4], [0.,2.5,1000.][trial%3], .2,.3,trial%2]
    p=list(map(f,p))
    angles=list(map(float,range(-90,91,int(p[2]))));s,x,y=scale(strokes,p[3]);ranked=score(strokes,angles,s)
    winner,v,retained,costs=decide(strokes,angles,s,p,ranked)
    rotated=[]
    for a in [0.,90.,-90.,15.]:
        r=rotate(strokes,a);rotated.append(dict(angle=a,strokes=r,coverage=coverage(r,s),reject=reject(r,s,p)))
    controls.append(dict(strokes=strokes,profile=p,scale=s,samplesX=x,samplesY=y,angles=angles,ranked=ranked,rotated=rotated,winner=winner,decisionCoverage=v,retained=retained,costs=costs))
destination=Path(sys.argv[1]);destination.parent.mkdir(parents=True,exist_ok=True)
destination.write_text(json.dumps(controls,separators=(',',':')),encoding='utf-8')
print(f'{len(controls)} independent orientation controls -> {destination}')
