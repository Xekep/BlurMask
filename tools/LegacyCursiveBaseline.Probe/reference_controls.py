"""Independent190350 baseline-stage float-bit interpreter on own ink."""
import json,math,random,struct,sys
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def add(a,b):return f(a+b)
def sub(a,b):return f(a-b)
def mul(a,b):return f(a*b)
def bits(v):return struct.unpack('<i',struct.pack('<f',v))[0]
def forward(rows,i):
    p,n=rows[i:i+2];dx=sub(n[0],p[0]);dy=sub(n[1],p[1]);p[2:5]=[dx,dy,f(math.sqrt(float(dy)*dy+float(dx)*dx))]
def make(rng):
    strokes=[[[f(rng.randint(-32,32)/4),f(rng.randint(-32,32)/4)] for _ in range(rng.randint(4,20))] for _ in range(rng.randint(1,3))];counts=[len(s) for s in strokes];tags=[];rows=[]
    for stroke,points in enumerate(strokes):
        for point,p in enumerate(points):
            tag=1 if not point else rng.choice([0,0,4,6,7,8,9,11,12,12,12,13,14]);tags.append(tag);rows.append(p+[0.,0.,0.,tag,stroke,len(rows)])
    for i in range(len(rows)-1):forward(rows,i)
    stroke=rng.randrange(len(strokes));start=sum(counts[:stroke]);height=f(rng.choice([1,2,4,8,12]));baseline=rng.choice([None,-2.,0.,2.]);radius=mul(height,f(.35));i=start;pending=False;pending_at=0;anchor=start
    while i<start+counts[stroke]:
        tag=rows[i][5]
        if tag==12:pending=True;pending_at=i
        elif tag and tag not in [13,14]:
            if pending and rows[i-1][5] not in [13,14]:
                if tag in [6,8] and baseline is not None:
                    skip=False
                    if rows[anchor][5]==9:
                        if abs(sub(rows[anchor][1],baseline))<abs(sub(rows[pending_at][1],baseline)):skip=True
                        else:rows[anchor][5]=14
                    if not skip:
                        if rows[pending_at][1]<=baseline:
                            distance=0.
                            for j in range(i-1,pending_at-1,-1):distance=add(distance,rows[j][4])
                            if radius<distance:rows[pending_at][5]=2
                        else:
                            distance=0.;at=i
                            while pending_at<at:
                                distance=add(distance,rows[at-1][4])
                                if radius<distance:break
                                at-=1
                            if rows[at][1]<=baseline:
                                for j in range(pending_at,i):
                                    if rows[j][1]<baseline:rows[j][5]=2;break
                            else:rows[at][5]=2
                elif tag==11:
                    total=0.
                    for j in range(pending_at,i):total=add(total,rows[j][4])
                    distance=0.
                    for j in range(pending_at,i):
                        distance=add(distance,rows[j][4])
                        if mul(total,.5)<distance:
                            p=rows[j];t=f(sub(mul(total,.5),sub(distance,p[4]))/p[4]);x=add(mul(t,p[2]),p[0]);y=add(mul(t,p[3]),p[1]);at=j+1
                            rows.insert(at,[x,y,0.,0.,0.,2,stroke,rows[at-1][7]]);counts[stroke]+=1;forward(rows,at-1);forward(rows,at);i+=1;break
                elif tag in [7,4] and rows[i][0]>rows[pending_at][0]:
                    distance=0.
                    for j in range(pending_at,i):distance=add(distance,rows[j][4])
                    if height<distance:rows[pending_at][5]=2
            anchor=i;pending=False
        i+=1
    return dict(strokes=strokes,tags=tags,stroke=stroke,height=height,baseline=baseline,counts=counts,expected=[[bits(v) for v in r[:5]]+r[5:] for r in rows])
rng=random.Random(0x297190350)
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump([make(rng) for _ in range(768)],stream,separators=(',',':'))
print('Wrote768 independent baseline-stage controls')
