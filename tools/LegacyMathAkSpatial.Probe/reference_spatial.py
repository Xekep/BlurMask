"""Literal configured1211 spatial branch: native030400/030FE0/0315F0.
The reference builds a complete bitmap then traverses it in row order. Native
record visitation order is immaterial to bounded modular counts/minima after
bitmap construction; failure-prefix or scratch-allocation equivalence excluded.
"""
from reference_temporal import div,short,calculate as temporal

def spatial(points,minimum,maximum):
 lo=[min(p[i]for p in points)for i in(0,1)];hi=[max(p[i]for p in points)for i in(0,1)]
 w,h=hi[0]-lo[0],hi[1]-lo[1];factor=div(127*1024,max(w,h));w,h=w*factor>>10,h*factor>>10
 pad=[(127-w)>>1 if w<h else 0,(127-h)>>1 if h<w else 0]
 q=[tuple(((p[i]-lo[i])*factor>>10)+pad[i]for i in(0,1))for p in points]
 bitmap=bytearray(130*130);pixels=set()
 def put(x,y):
  assert 0<=x<128 and 0<=y<128
  bitmap[x+1+(y+1)*130]=1;pixels.add((x,y))
 #Literal directional step tables and iVar13>=0 branch from030400.
 orthx=[1,0,1,0];orthy=[0,1,0,-1];diagx=[1]*4;diagy=[1,1,-1,-1]
 for first,last in zip(q,q[1:]):
  if first[0]>last[0]:first,last=last,first
  x,y=first;dx=last[0]-x;dy=last[1]-y
  dyabs=abs(dy);kind=(2 if dy<0 else 0)|(1 if dyabs>dx else 0)
  major=max(dx,dyabs);minor=min(dx,dyabs);error=2*minor-major
  for _ in range(major+1):
   put(x,y)
   if error<0:x+=orthx[kind];y+=orthy[kind];error+=2*minor
   else:x+=diagx[kind];y+=diagy[kind];error+=2*(minor-major)
 p={k:[0]*(128 if k in(0x148,0x158)else 255)for k in(0x148,0x150,0x158,0x160)}
 for k in(0x168,0x170,0x178,0x180):p[k]=[128]*128
 for k in(0x188,0x190,0x198,0x1a0):p[k]=[min(i+1,255-i)for i in range(255)]
 def inc(k,i):p[k][i]=short(p[k][i]+1)
 def mn(k,i,v):
  assert 0<=i<len(p[k]) and 0<=v<=128
  if v<p[k][i]:p[k][i]=v
 for x,y in sorted(pixels,key=lambda q:(q[1],q[0])):
  at=x+1+(y+1)*130
  left,downleft,down,downright,right,up=[bitmap[at+d]for d in(-1,129,130,131,1,-130)]
  if not right:inc(0x148,y)
  if not up:inc(0x158,x)
  if not downleft:inc(0x150,x+y)
  elif not down and not left:
   i=x-y+126;inc(0x160,i);mn(0x190,i,min(y,x-1));mn(0x1a0,i,127-x if y+1<x else 126-y)
  if not downright:inc(0x160,x-y+127)
  elif not down and not right:
   i=x+y+1;inc(0x150,i);mn(0x188,i,126-x if i>=128 else y);mn(0x198,i,126-y if i>=128 else x)
  mn(0x168,y,x);mn(0x170,x,y);mn(0x178,y,127-x);mn(0x180,x,127-y)
  i=x+y;mn(0x188,i,127-x if i>=128 else y);mn(0x198,i,127-y if i>=128 else x)
  i=x-y+127;mn(0x190,i,min(x,y));mn(0x1a0,i,127-max(x,y))
 out=[0]*192
 def sm(values):
  n=0
  for v in values:n=short(n+v)
  return n
 for key,group in[(0x148,0),(0x158,2),(0x168,4),(0x170,6),(0x178,8),(0x180,10)]:
  for b in range(16):
   n=sm(p[key][8*b:8*b+8]);v=short(n*(64 if group<4 else 2)-1024)
   out[group*16+b]=min(1024,max(-1024,v))if group<4 else v
 for key,group in[(0x150,1),(0x160,3),(0x188,5),(0x190,7),(0x198,9),(0x1a0,11)]:
  for b in range(16):
   start=b*16 if b<8 else 127+(b-8)*16
   if group<4:v=min(1024,max(-1024,short(sm(p[key][start:start+16])*64-1024)))
   else:
    acc=sm([((p[key][i]&65535)<<8)//min(i+1,255-i)for i in range(start,start+16)])
    v=short(((acc*128)&4294967295)>>8)-1024;v=short(v)
   out[group*16+b]=v
 _,tv=temporal(points,minimum,maximum)
 calibrated=[v if maximum[i+220]==minimum[i+220]else short(div((v-minimum[i+220])<<11,maximum[i+220]-minimum[i+220])-1024)for i,v in enumerate(out)]
 return tv[210:220]+calibrated+[1024,1024],len(pixels),out
