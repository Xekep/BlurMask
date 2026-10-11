"""Independent191430 record/memmove-equivalent final marker controls."""
import json,math,random,struct,sys
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def add(a,b):return f(a+b)
def sub(a,b):return f(a-b)
def mul(a,b):return f(a*b)
def visible(tag):return tag!=0 and not 11<=tag<=14
def make(rng):
    strokes=[];tags=[]
    for _ in range(rng.randint(1,3)):
        p=[[f(rng.randint(-24,24)/4),f(rng.randint(-24,24)/4)] for _ in range(rng.randint(7,30))]
        if rng.randrange(2)==0:p[-1]=p[0].copy()
        strokes.append(p);tags.extend([rng.choice([1,1,1,2])]+[rng.choice([0,2,3,4,5,6,7,8,9,10,11,12,13,14]) for _ in p[1:]])
    flat=[p for stroke in strokes for p in stroke];lengths=[]
    for i,p in enumerate(flat):
        if i==len(flat)-1:lengths.append(0.);continue
        n=flat[i+1];dx=sub(n[0],p[0]);dy=sub(n[1],p[1]);lengths.append(f(math.sqrt(float(dy)*dy+float(dx)*dx)))
    original=tags.copy();height=f(rng.choice([1,2,4,8,12,24]));cap=rng.choice([None,0,1,2,4,6]);start=0
    for stroke in strokes:
        end=start+len(stroke)
        marker_count=sum(visible(t) for t in tags[start:end])
        if tags[start]==1 and marker_count>6:
            width=sub(max(p[0] for p in stroke),min(p[0] for p in stroke));target=int(f(mul(width,4)/height))
            dx=sub(stroke[0][0],stroke[-1][0]);dy=sub(stroke[0][1],stroke[-1][1]);distance=math.sqrt(add(mul(dy,dy),mul(dx,dx)))
            if cap is not None and distance<mul(height,f(.8)) and cap<target:target=cap
            if target<marker_count:
                indices=[start];weights=[lengths[start]]
                for i in range(start+1,end):
                    if visible(tags[i]):indices.append(i);weights.append(lengths[i])
                    weights[-1]=add(weights[-1],lengths[i])
                for _ in range(marker_count-target):
                    best=f(3.4028234663852886e38);selected=-1
                    for k in range(1,len(indices)):
                        right,left=weights[k],weights[k-1]
                        if right<=left:left=float(left)*.1
                        else:right=float(right)*.1
                        cost=f(right+left)
                        if cost<best and tags[indices[k]] not in [7,4]:best=cost;selected=k
                    if selected>=0:
                        tags[indices[selected]]=0;del indices[selected];del weights[selected];weights[selected-1]=best
        start=end
    a=f(rng.uniform(0,1e8));b=f(rng.uniform(0,1e8));merge=f(min(a,b)+max(a,b)*.1)
    return dict(merge_left=a,merge_right=b,merge_bits=struct.unpack("<i",struct.pack("<f",merge))[0],strokes=strokes,tags=original,height=height,closed_cap=cap,expected=tags)
rng=random.Random(0x297191430)
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump([make(rng) for _ in range(1024)],stream,separators=(',',':'))
print('Wrote1024 independent marker-limit controls')
