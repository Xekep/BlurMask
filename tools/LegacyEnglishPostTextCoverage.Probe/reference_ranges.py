"""Independent linear interval oracle for canonical native C21 inputs."""
from pathlib import Path
import random,json,sys,hashlib
MASK=2**64-1

def n(x):return x&~0xff00 if(x>>8)&255==255 else x
def cross(a,b):return(((a>>32)+1)&0xffffffff)==b>>32 and((a+253)&0xfffd)==0 and(b&0xffffffff)in(3,0xff03)
def union(old,extra):
 if not extra or(old and old[0][0]&255==4)or extra[0][0]&255==4:return old[:]
 for added in extra:
  seq=[(a,b,1)for a,b in old]+[(added[0],added[1],0)]
  seq.sort(key=lambda r:(n(r[0]),-r[2]))
  out=[]
  for a,b,old_end in seq:
   if out and(n(a)<=((n(out[-1][1])+2)&MASK)or cross(out[-1][1],a)):
    x,y,old_y=out[-1]
    if n(b)>n(y)or(n(b)==n(y)and old_end>old_y):y=b;old_y=old_end
    out[-1]=(x,y,old_y)
   else:out.append((a,b,old_end))
  old=[list(r[:2])for r in out]
 return old

def intersect(old,filters):
 if not old or not filters or old[0][0]&255==4 or filters[0][0]&255==4:return[]
 out=[]
 for a,b in filters:
  for x,y in old:
   if n(y)<n(a)or n(x)>n(b):continue
   out.append([a if n(x)<=n(a)<=n(y)else x,b if n(x)<=n(b)<=n(y)else y])
 return out

def count(counts,k):return counts[k]if 0<=k<len(counts)else 0x8000
def previous(counts,a):
 k=a>>32;p=(a>>16)&65535;f=(a>>8)&255;m=a&255
 if k and(a&0xffffffff)in(3,0xff03):return((k-1)<<32)|(((count(counts,k-1)-1)&65535)<<16)|0xff05
 if m==3:return(a&~255&(~0xff00 if f==255 else MASK))|1
 return(a&~0xffff)|0xff05 if k<len(counts)and p==count(counts,k)-1 else(a&~255)|5

def following(counts,b):
 k=b>>32;p=(b>>16)&65535;f=(b>>8)&255;m=b&255
 if f==255:
  k=(k+1)&0xffffffff
  return(k<<32)|3|(0xff00 if count(counts,k)==1 else 0)
 if m==5:return(b&~255)|7
 return(b&~0xffff)|0xff03 if k<len(counts)and p==count(counts,k)-1 else(b&~255)|3

def subtract(old,cuts,counts):
 for a,b in cuts:
  if a>>32==b>>32 and(a>>16)&65535==(b>>16)&65535 and(((a>>8)&255)==((b>>8)&255)or((a>>8)&255)==255 or((b>>8)&255)==255)and a&255==b&255==4:continue
  before=previous(counts,a);after=following(counts,b);out=[]
  for x,y in old:
   l=n(x)<=n(before)<=n(y);r=n(x)<=n(after)<=n(y)
   if l:out.append([x,before])
   if r:out.append([after,y])
   if not l and not r and(n(y)<n(before)or n(x)>n(after)):out.append([x,y])
  old=out
 return old

def make(output):
 rng=random.Random(0xc21024f30);tests=[]
 for i in range(4096):
  counts=[rng.randrange(1,16)for _ in range(rng.randrange(1,9))]
  def interval():
   def coordinate():
    k=rng.randrange(len(counts));point=rng.randrange(counts[k]);fraction=0 if point==counts[k]-1 else rng.choice([0,0,1,40,199])
    return k,point,fraction
   a,b=sorted([coordinate(),coordinate()]);ma=rng.choice([3,7]);mb=rng.choice([1,5])
   if a==b:ma,mb=3,5
   def pack(t,mode):
    k,point,fraction=t
    if fraction==0 and point==counts[k]-1 and mode in(3,5):fraction=255
    return(k<<32)|(point<<16)|(fraction<<8)|mode
   return[pack(a,ma),pack(b,mb)]
  target=union([], [interval()for _ in range(rng.randrange(9))])
  extra=[interval()for _ in range(rng.randrange(5))];filters=[interval()for _ in range(rng.randrange(4))];cuts=[interval()for _ in range(rng.randrange(4))]
  if i%16==0:target=[[4,4]]
  if i%32==1:extra=[[4,4]]
  if i%32==2:filters=[[4,4]]
  if i%32==3:cuts=[[4,4]]
  tests.append(dict(counts=counts,target=target,extra=extra,filters=filters,cuts=cuts,
   union=union(target,extra),intersect=intersect(target,filters),subtract=subtract(target,cuts,counts),
   contains=[any(n(a)<=n(x)<=n(b)and y<=b for a,b in target)for x,y in filters]))
 data=(json.dumps(dict(tests=tests),separators=(',',':'))+'\n').encode();Path(output).write_bytes(data)
 print('PASS4096 independent linear C21 cases SHA256',hashlib.sha256(data).hexdigest())
if __name__=='__main__':make(sys.argv[1])
