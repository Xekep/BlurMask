"""Own finite-ink loops, independent native scalar arithmetic reference."""
import json,math,random,struct,sys
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def add(a,b):return f(a+b)
def sub(a,b):return f(a-b)
def mul(a,b):return f(a*b)
def div(a,b):return f(a/b)
def bits(v):return struct.unpack('<i',struct.pack('<f',v))[0]
def forward(rows,i):
    a,b=rows[i:i+2];dx=sub(b[0],a[0]);dy=sub(b[1],a[1]);a[2:5]=[dx,dy,f(math.sqrt(float(dy)*dy+float(dx)*dx))]
def intersect(p,q,r,s):
    a=sub(q[0],p[0]);b=sub(s[0],r[0]);c=sub(p[1],q[1]);d=sub(r[1],s[1]);e=sub(mul(-c,p[0]),mul(a,p[1]));g=sub(mul(-d,r[0]),mul(b,r[1]));det=sub(mul(b,c),mul(d,a))
    if abs(det)<f(1e-10):return None
    x=div(sub(mul(g,a),mul(b,e)),det);y=div(sub(mul(d,e),mul(g,c)),det)
    if all(min(u,v)<=value<=max(u,v) for value,u,v in [(x,p[0],q[0]),(y,p[1],q[1]),(x,r[0],s[0]),(y,r[1],s[1])]):return [x,y]
def count(delta,coordinate,spacing,direction,last,total):
    sign=1 if delta<0 else -1 if delta>0 else direction
    if delta and direction==-sign and (last==-1 or spacing<abs(sub(coordinate,last))):last=coordinate;total+=1
    return sign,last,total
def make(rng):
    strokes=[[[f(rng.randint(-24,24)/4),f(rng.randint(-24,24)/4)] for _ in range(rng.randint(4,20))] for _ in range(rng.randint(1,3))];counts=[len(s) for s in strokes];tags=[];rows=[]
    for stroke,points in enumerate(strokes):
        for point,p in enumerate(points):
            tag=1 if not point else rng.choice([0,0,0,7,8,4,11,12,13,14]);tags.append(tag);rows.append(p+[0.,0.,0.,tag,stroke,len(rows)])
    for i in range(len(rows)-1):forward(rows,i)
    stroke=rng.randrange(len(strokes));start=sum(counts[:stroke]);height=f(rng.choice([1,2.5,3,5,6,7.5,10,12]));spacing=mul(height,f(.1));i=start
    def insert(at,p,tag):
        rows.insert(at,p+[0.,0.,0.,tag,stroke,rows[at-1][7]]);counts[stroke]+=1;forward(rows,at-1);forward(rows,at)
    while i<start+counts[stroke]-1:
        end=start+counts[stroke]
        for j in range(i+2,end-1):
            a,b,c,d=rows[i],rows[i+1],rows[j],rows[j+1]
            p=intersect(a,b,c,d)
            if p is None:continue
            xd=yd=xc=yc=0;lastx=lasty=-1.;turn=0.
            for k in range(i,j):
                r,n=rows[k:k+2]
                if r[4]>0 and n[4]>0:turn=add(turn,div(sub(mul(r[2],n[3]),mul(r[3],n[2])),mul(r[4],n[4])))
                xd,lastx,xc=count(r[2],r[0],spacing,xd,lastx,xc);yd,lasty,yc=count(r[3],r[1],spacing,yd,lasty,yc)
            if xc>2 or yc>2:continue
            insert(j+1,p,12);insert(i+1,p,11);nested=any(11<=r[5]<=14 for r in rows[i+2:j+2])
            for k in range(i+2,j+2):
                old=rows[k][5]
                if turn<0:
                    if old==0 or not nested and old in [7,8]:rows[k][5]=13
                elif nested:
                    if old==0:rows[k][5]=14
                else:rows[k][5]=9 if old==7 else 14
            i+=2;break
        i+=1
    return dict(strokes=strokes,tags=tags,stroke=stroke,height=height,counts=counts,expected=[[bits(v) for v in r[:5]]+r[5:] for r in rows])
rng=random.Random(0x29718e1f0)
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump([make(rng) for _ in range(768)],stream,separators=(',',':'))
print('Wrote768 independent loop-stage controls')
