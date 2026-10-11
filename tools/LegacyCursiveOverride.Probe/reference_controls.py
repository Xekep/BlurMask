"""Independent1908c0 RU106 geometry override on own finite ink."""
import json,math,random,struct,sys

def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def add(a,b):return f(a+b)
def sub(a,b):return f(a-b)
def mul(a,b):return f(a*b)
def bits(v):return struct.unpack('<i',struct.pack('<f',v))[0]
def forward(rows,i):
 p,n=rows[i:i+2];dx=sub(n[0],p[0]);dy=sub(n[1],p[1]);p[2:5]=[dx,dy,f(math.sqrt(float(dy)*dy+float(dx)*dx))]
def intersect(p,q,r,s):
 a=sub(q[0],p[0]);b=sub(s[0],r[0]);c=sub(p[1],q[1]);d=sub(r[1],s[1]);e=sub(mul(-c,p[0]),mul(a,p[1]));g=sub(mul(-d,r[0]),mul(b,r[1]));det=sub(mul(b,c),mul(d,a))
 if not abs(det)>=f(1e-10):return None
 x=f(sub(mul(g,a),mul(b,e))/det);y=f(sub(mul(d,e),mul(g,c))/det)
 if all(min(a[k],b[k])<=v<=max(a[k],b[k]) for a,b in [(p,q),(r,s)] for k,v in [(0,x),(1,y)]):return [x,y]
 return None
def make(rng,index):
 target=[[f(i*1.5),f(rng.randint(-2,2)/8)] for i in range(rng.randint(3,14))]
 if rng.randrange(2):target.reverse()
 strokes=[target]+[[[f(rng.randrange(0,len(target))*1.5),-2.],[f(rng.randrange(0,len(target))*1.5),2.]] for _ in range(rng.randint(1,8))]
 if index%7==0:rng.shuffle(strokes)
 stroke=next(i for i,s in enumerate(strokes) if s is target);counts=[len(s) for s in strokes];starts=[0]
 for n in counts:starts.append(starts[-1]+n)
 tags=[];rows=[]
 for si,s in enumerate(strokes):
  for i,p in enumerate(s):
   tag=1 if i==0 else rng.choice([0,2,4,7,11,12,13,14]);tags.append(tag);rows.append(p+[0.,0.,0.,tag,si,len(rows)])
 for i in range(len(rows)-1):forward(rows,i)
 height=f(rng.choice([1,2,4,8]));baseline=rng.choice([None,0.,2.,4.]);first=rng.randrange(stroke+1);last=rng.randrange(stroke,len(strokes));crosses=[];farthest=0.;best=[0,0]
 if baseline is not None and height<sub(max(p[0] for p in target),min(p[0] for p in target)) and sub(max(p[1] for p in target),min(p[1] for p in target))<mul(height,.5) and max(p[1] for p in target)<sub(baseline,mul(height,.5)):
  for other in range(first,last+1):
   if other==stroke:continue
   for i in range(starts[other],starts[other+1]-1):
    for j in range(starts[stroke],starts[stroke+1]-1):
     point=intersect(rows[i],rows[i+1],rows[j],rows[j+1])
     if point is None or len(crosses)==10:continue
     crosses.append((point,j))
     for a in range(len(crosses)):
      for b in range(len(crosses)):
       if a==b:continue
       dy=sub(crosses[a][0][1],crosses[b][0][1]);dx=sub(crosses[a][0][0],crosses[b][0][0]);distance=add(mul(dy,dy),mul(dx,dx))
       if farthest<distance:farthest=distance;best=[a,b]
  if len(crosses)>1 and mul(mul(height,height),.25)<farthest:
   lo=crosses[min(best)][1];hi=crosses[max(best)][1];total=0.
   for i in range(lo,hi+1):total=add(total,rows[i][4])
   midpoint=mul(total,.5);distance=0.
   for i in range(lo,hi):
    distance=add(distance,rows[i][4])
    if midpoint<distance:
     p=rows[i];fraction=f(sub(midpoint,sub(distance,p[4]))/p[4]);x=add(mul(fraction,p[2]),p[0]);y=add(mul(fraction,p[3]),p[1]);at=i+1
     rows.insert(at,[x,y,0.,0.,0.,2,stroke,p[7]]);counts[stroke]+=1;forward(rows,at-1);forward(rows,at);break
  else:
   for i in range(starts[stroke]+1,starts[stroke+1]):rows[i][5]=0
 return dict(strokes=strokes,tags=tags,stroke=stroke,height=height,baseline=baseline,first=first,last=last,counts=counts,expected=[[bits(v) for v in r[:5]]+r[5:] for r in rows])
rng=random.Random(0x2971908c0)
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump([make(rng,i) for i in range(768)],stream,separators=(',',':'))
print('Wrote768 independent Russian106 override controls')
