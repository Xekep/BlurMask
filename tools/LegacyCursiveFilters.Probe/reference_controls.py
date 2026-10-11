"""Independent191950 pre/post-baseline tag filters on own ink."""
import json,random,struct,sys

def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def sub(a,b):return f(a-b)
def make(rng):
 strokes=[[[f(rng.randint(-20,20)/4),f(rng.randint(-20,20)/4)] for _ in range(rng.randint(3,20))] for _ in range(rng.randint(1,4))];flat=[p for s in strokes for p in s];counts=[len(s) for s in strokes];tags=[rng.choice([0,1,2,4,5,6,7,8,9,11,12,13,14]) for p in flat];initial=tags.copy();stroke=rng.randrange(len(strokes));start=sum(counts[:stroke]);end=start+counts[stroke];anchor=None
 for i in range(start,end-1):
  t=tags[i]
  if t in [4,7]:anchor=i;continue
  if t in [0,13,14]:continue
  if anchor is not None and t==11 and sub(flat[anchor][1],flat[i][1])>0:
   dy=sub(flat[anchor][1],flat[i][1]);dx=sub(flat[i][0],flat[anchor][0]);found=False
   for j in range(anchor+1,i+1):
    x=sub(flat[j][0],flat[anchor][0])
    if dx==0:outside=x<0
    elif x==0:outside=dx>0
    else:outside=f(sub(flat[anchor][1],flat[j][1])/x)>f(dy/dx)
    if outside:found=True;break
   if found:tags[i]=2
  anchor=None
 anchor=None
 for i in range(start,end):
  if tags[i]==11:
   if anchor is None or i==0 or sub(flat[i][0],flat[i-1][0])<=0:anchor=i
   else:
    if anchor!=0:tags[anchor]=0
    tags[i]=2;anchor=None
  elif tags[i] in [1,12]:anchor=None
 before=tags.copy();tags=initial.copy();height=f(rng.choice([1,2,4,8]));baseline=rng.choice([None,-2.,0.,2.]);anchor=start
 for i in range(start+1,end):
  if tags[i] in [0,1,13,14]:continue
  if baseline is not None and flat[i][1]<0 and f(baseline+f(height*.5))<flat[anchor][1] and flat[anchor][0]<flat[i][0] and tags[anchor] in [4,5,7]:
   for j in range(anchor,i):
    if flat[j][1]<baseline:tags[j-1]=2;tags[anchor]=0;break
  anchor=i
 previous_y=flat[start][1]
 for i in range(start+1,end-1):
  if tags[i] in [0,1,13,14]:continue
  if tags[i]==11 and tags[i+1]==13 and f(height*f(.2))<sub(flat[i][1],previous_y):tags[i]=2
  previous_y=flat[i][1]
 return dict(strokes=strokes,tags=initial,stroke=stroke,height=height,baseline=baseline,before=before,after=tags)
rng=random.Random(0x297192200)
with open(sys.argv[1],'w',encoding='utf8') as stream:json.dump([make(rng) for _ in range(1024)],stream,separators=(',',':'))
print('Wrote1024 independent controller-filter controls')
