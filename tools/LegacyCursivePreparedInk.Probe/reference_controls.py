"""Independent native297 record preparation/insertion float-bit controls."""
import json,random,struct,math,sys
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def bits(v):return struct.unpack('<i',struct.pack('<f',v))[0]
def forward(a,b):
    dx=f(b[0]-a[0]);dy=f(b[1]-a[1]);a[2:5]=[dx,dy,f(math.sqrt(float(dy)*dy+float(dx)*dx))]
def encoded(rows):return [[bits(v) for v in r[:5]]+r[5:] for r in rows]
def make(rng):
    strokes=[[[f(rng.randint(-100,100)/4),f(rng.randint(-100,100)/4)] for _ in range(rng.randint(2,18))] for _ in range(rng.randint(1,5))]
    if rng.randrange(5)==0:strokes[0][0]=[-0.,0.]
    rows=[];counts=[]
    for stroke,points in enumerate(strokes):
        counts.append(len(points))
        for point,p in enumerate(points):rows.append(p+[0.,0.,0.,int(point==0),stroke,len(rows)])
    for i in range(len(rows)-1):forward(rows[i],rows[i+1])
    initial=encoded(rows);inserts=[]
    for _ in range(rng.randint(1,5)):
        stroke=rng.randrange(len(strokes));local=rng.randrange(1,counts[stroke]);at=sum(counts[:stroke])+local
        x=f(rng.randint(-100,100)/4);y=f(rng.randint(-100,100)/4);tag=rng.choice([2,3,5,7,8])
        rows.insert(at,[x,y,0.,0.,0.,tag,stroke,rows[at-1][7]])
        forward(rows[at-1],rows[at]);forward(rows[at],rows[at+1]);counts[stroke]+=1
        inserts.append(dict(stroke=stroke,point=local,x=x,y=y,tag=tag,expected=encoded(rows)))
    return dict(strokes=strokes,prepared=initial,inserts=inserts)
rng=random.Random(0x29718dde0)
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump([make(rng) for _ in range(512)],stream,separators=(',',':'))
print('Wrote512 independent prepared-record controls')
