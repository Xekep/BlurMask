"""Independent native-stage controls; no resource or vendor binary execution."""
import json,random,struct,sys
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def back(tags,lengths,end,radius):
    # Mirror the native four-record unroll, unlike the managed scalar loop.
    distance=0.;j=end
    while j>=0:
        for _ in range(min(4,j+1)):
            distance=f(distance+lengths[j])
            if radius<=distance or tags[j]==1:return
            tags[j]=0;j-=1
def control(rng):
    counts=[rng.randint(1,45) for _ in range(rng.randint(1,8))];n=sum(counts)
    tags=[rng.choice([0,0,2,3,7,8,10,11,12,13,14,15]) for _ in range(n)]
    dx=[f(rng.choice([-3,-1,0,0,1,3])) for _ in range(n)]
    dy=[f(rng.choice([-3,-1,0,0,1,3])) for _ in range(n)]
    lengths=[f(rng.choice([0,0,.125,.5,1,3,rng.random()*4])) for _ in range(n)]
    at=0
    for points in counts:tags[at]=1;at+=points
    original=tags.copy();at=0
    for points in counts:
        direction=0
        for i in range(at,at+points-1):
            if dy[i]<0:
                if direction==-1:tags[i]=7
                direction=1
            elif dy[i]>0:
                if direction==1 and i>0 and dx[i-1]>0:tags[i]=8
                direction=-1
        at+=points
    reversed_=tags.copy();height=f(rng.choice([0,1,2,4,10,rng.random()*15]));relative=f(rng.choice([0,.1,.5,1]));radius=f(height*relative)
    for i in range(n):
        if tags[i]!=1:continue
        if i>0:
            if tags[i-1]!=1:tags[i-1]=0
            back(tags,lengths,i-2,radius)
        distance=0.
        for j in range(i+1,n):
            distance=f(distance+lengths[j-1])
            if radius<=distance or tags[j]==1:break
            tags[j]=0
    if tags[-1]!=1:tags[-1]=0
    back(tags,lengths,n-2,radius)
    markers=[];at=0
    for stroke,points in enumerate(counts):
        for point in range(points):
            kind=tags[at+point]
            if kind and ((kind-11)&0xffffffff)>3:markers.append([kind,stroke,point])
        at+=points
    return dict(counts=counts,tags=original,dx=dx,dy=dy,lengths=lengths,height=height,radius=relative,reversed=reversed_,pruned=tags,markers=markers)
rng=random.Random(297191950)
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump([control(rng) for _ in range(1024)],stream,separators=(',',':'))
print('Wrote1024 independent geometry controls')
