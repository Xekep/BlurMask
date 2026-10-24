"""Literal single-stroke native2D temporal prefix. Derived from original C/PE,
not the managed implementation. No model weights are evaluated."""
import math,struct
f=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
def div(a,b):return (abs(a)//abs(b))*(-1 if (a<0)!=(b<0) else 1)
def short(a):return (a+32768)%65536-32768
def norm(n):
 n&=0xffffffff
 if n<16384:return int(math.sqrt(n)+.5)
 shift=2
 while (n>>(shift*2))>=16384:shift+=1
 return int(math.sqrt(n>>(shift*2))+.5)<<shift

def normalize(points):
 p=[[f(c)for c in q]for q in points];lo=[min(q[i]for q in p)for i in(0,1)];hi=[max(q[i]for q in p)for i in(0,1)]
 w,h=[f(hi[i]-lo[i])for i in(0,1)];s=min(f(8192/w),f(8192/h))if w and h else f(1)
 return [[math.trunc(f(f(f(f(q[i]-lo[i])*s)+0)+.5))for i in(0,1)]for q in p]

def calculate(points,minimum,maximum):
 #02ED20 converts original short XY into the private128-unit centered grid.
 minx,miny=[min(p[i]for p in points)for i in(0,1)];maxx,maxy=[max(p[i]for p in points)for i in(0,1)]
 w,h=maxx-minx,maxy-miny;factor=div(127*1024,max(w,h));w,h=w*factor>>10,h*factor>>10
 px=(127-w)>>1 if w<h else 0;py=(127-h)>>1 if h<w else 0
 x=[((p[0]-minx)*factor>>10)+px for p in points];y=[((p[1]-miny)*factor>>10)+py for p in points]
 edge=[norm(((x[i+1]-x[i])*16)**2+((y[i+1]-y[i])*16)**2)for i in range(len(x)-1)]
 #02F3D0: retain the original cursor/carry variables and native denominator.
 samplex=[x[0]];sampley=[y[0]];step=div(sum(edge),29)
 index=0;point=0;carry=0;before=0;saved_index=0;saved_point=0;produced=0
 while produced<29:
  accumulated=carry
  if accumulated<step:
   while True:
    old=carry;accumulated=old
    if point>=len(x)-1:break
    point+=1;accumulated=old+edge[index];carry=accumulated;index+=1;before=old
    if accumulated>=step:break
   saved_point=point;saved_index=index
  assert index>0 and edge[index-1]>0
  produced+=1;j=index-1;dx=x[index]-x[j];dy=y[index]-y[j]
  samplex.append(div((step-before)*dx,edge[j])+x[j]);sampley.append(div(dy*(step-before),edge[j])+y[j]);carry=edge[j]-step+before
  repeat=2
  if step<=carry:
   while True:
    if produced>=29:break
    carry-=step;produced+=1;samplex.append(div(dx*repeat*step,accumulated)+x[j]);sampley.append(div(dy*repeat*step,accumulated)+y[j]);repeat+=1
    if step>carry:break
   point=saved_point;index=saved_index
 assert len(samplex)==len(sampley)==30
 ddx=[(samplex[i+1]-samplex[i])*16 for i in range(29)];ddy=[(sampley[i+1]-sampley[i])*16 for i in range(29)]
 lengths=[norm(ddx[i]*ddx[i]+ddy[i]*ddy[i])for i in range(29)];total=sum(lengths)
 #02FB40/02FA10 summary6, then seven-channel temporal layout.
 masses=[[0,0]for _ in range(3)]
 def add(k,sign,weight):
  if sign>0:masses[k][0]+=weight
  elif sign<0:masses[k][1]+=weight
  else:masses[k][0]+=weight>>1;masses[k][1]+=weight>>1
 for i in range(29):
  add(0,ddx[i],lengths[i]);add(1,ddy[i],lengths[i])
  if i<28:add(2,(ddx[i]*ddy[i+1]-ddy[i]*ddx[i+1])>>8,lengths[i]+lengths[i+1])
 out=[]
 for pos,neg in masses:
  s=pos+neg;out.extend([-1024,-1024]if s==0 else [short(div(2048*pos,s)-1024),short(div(2048*neg,s)-1024)])
 out.extend([1024]*30);out.extend([short(c*16-1024)for c in samplex]);out.extend([short(c*16-1024)for c in sampley])
 out.extend([0 if lengths[i]==0 else short(div(1024*ddx[i],lengths[i]))for i in range(29)])
 out.extend([0 if lengths[i]==0 else short(div(1024*ddy[i],lengths[i]))for i in range(29)])
 out.extend([short((out[97+i]*out[96+i]+out[126+i]*out[125+i])>>10)for i in range(28)])
 out.extend([short((out[96+i]*out[126+i]-out[97+i]*out[125+i])>>10)for i in range(28)])
 assert len(out)==210
 #030020 actual3F flags; then0327A0 calibrates217 prefix, skips baseline3.
 out.extend([1024,short(min(1024,div(256*(h+1),w+1)-1024)),short(div(16*sum(samplex),30)-1024),short(div(16*sum(sampley),30)-1024),-768,short(max(-1024,min(1024,(2*total>>4)-1024))),1024 if total else -1024])
 for i in range(217):
  denominator=maximum[i]-minimum[i]
  if denominator:out[i]=short(div((out[i]-minimum[i])<<11,denominator)-1024)
 out.extend([0,0,0,1024,1024]);assert len(out)==222
 return [[samplex[i],sampley[i]]for i in range(30)],out
