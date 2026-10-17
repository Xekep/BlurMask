"""Independent scalar integer geometry, trajectory and raster calculation."""
import math
def div(a,b):return(abs(a)//abs(b))*(-1 if(a<0)!=(b<0)else 1)
def short(a):return(a+32768)%65536-32768
def distance(a,b):
 dx=(b[0]-a[0])*16;dy=(b[1]-a[1])*16;n=dx*dx+dy*dy;shift=0
 if n>=16384:
  shift=2
  while n>>(shift*2)>16383:shift+=1
 return int(math.sqrt(n>>(shift*2))+.5)<<shift
def resample(p,lengths,count):
 if count==1:return[p[0]]
 total=sum(lengths);step=total//(count-1);assert step>0
 output=[p[0]];segment=residual=previous=0
 while len(output)<count:
  while residual<step and segment<len(lengths):previous=residual;residual+=lengths[segment];segment+=1
  assert segment>0 and lengths[segment-1]>0
  i=segment-1;before=residual
  output.append([p[i][j]+div((step-previous)*(p[i+1][j]-p[i][j]),lengths[i])for j in(0,1)])
  residual=lengths[i]-step+previous;multiple=2
  while residual>=step and len(output)<count:
   residual-=step;output.append([p[i][j]+div((p[i+1][j]-p[i][j])*multiple*step,before)for j in(0,1)]);multiple+=1
 return output
def geometry(strokes):
 points=[p for s in strokes for p in s];mins=[min(p[j]for p in points)for j in(0,1)];size=[max(p[j]for p in points)-mins[j]for j in(0,1)]
 extent=max(size);assert extent>0;scale=130048//extent;w,h=[v*scale>>10 for v in size];center=(128-min(w,h)-1)>>1
 coords=[[[((p[j]-mins[j])*scale>>10)+(center if size[j]<size[1-j]else 0)for j in(0,1)]for p in s]for s in strokes]
 lengths=[[distance(a,b)for a,b in zip(s,s[1:])]for s in coords];gaps=[distance(a[-1],b[0])for a,b in zip(coords,coords[1:])]
 total=sum(map(sum,lengths))+sum(gaps);stride=total//30;groups=2*len(strokes)-1;counts=[1]*groups;weights=[]
 for i,s in enumerate(strokes):
  weights.append(sum(lengths[i]))
  if i<len(gaps):weights.append(gaps[i]-2*stride)
 for _ in range(groups,30):
  best=max(range(groups),key=lambda i:weights[i]);weights[best]-=stride;counts[best]+=1
 samples=[];pen=[];parts=[]
 for i,s in enumerate(coords):
  part=resample(s,lengths[i],counts[2*i]);samples+=part;pen +=[1024]*len(part);parts.append(part)
  if i>=len(gaps):continue
  count=counts[2*i+1];gap=gaps[i];part=[]
  if gap==0:assert count==1;part=[samples[-1]]
  else:
   step=gap//(count+1);previous=samples[-1]
   for _ in range(count):
    previous=[previous[j]+div((coords[i+1][0][j]-previous[j])*step,gap)for j in(0,1)];part.append(previous);gap-=step
  samples+=part;pen +=[-1024]*len(part);parts.append(part)
 assert len(samples)==len(pen)==30
 return samples,pen,parts,w,h
def trajectory(samples,pen,width,height,stroke_count):
 delta=[[16*(b[j]-a[j])for j in(0,1)]for a,b in zip(samples,samples[1:])];dist=[distance(a,b)for a,b in zip(samples,samples[1:])]
 directions=[0]*6
 def add(at,sign,weight):
  if sign>0:directions[at]+=weight
  elif sign<0:directions[at+1]+=weight
  else:directions[at]+=weight>>1;directions[at+1]+=weight>>1
 for i,d in enumerate(delta):
  if pen[i]!=1024 or i+1<len(delta)and pen[i+1]!=1024:continue
  add(0,d[0],dist[i]);add(2,d[1],dist[i])
  if i+1<len(delta):add(4,(d[0]*delta[i+1][1]-d[1]*delta[i+1][0])>>8,dist[i]+dist[i+1])
 result=[]
 for i in range(0,6,2):
  total=directions[i]+directions[i+1];result +=[div(2048*directions[j],total)-1024 if total else-1024 for j in(i,i+1)]
 result+=pen+[16*p[0]-1024 for p in samples]+[16*p[1]-1024 for p in samples]
 dx=[div(1024*d[0],n)if n else 0 for d,n in zip(delta,dist)];dy=[div(1024*d[1],n)if n else 0 for d,n in zip(delta,dist)]
 result+=dx+dy+[(ax*bx+ay*by)>>10 for ax,bx,ay,by in zip(dx,dx[1:],dy,dy[1:])]+[(ax*by-bx*ay)>>10 for ax,bx,ay,by in zip(dx,dx[1:],dy,dy[1:])]
 physical=[p for p,state in zip(samples,pen)if state==1024];physicalLength=sum(n for i,n in enumerate(dist)if pen[i]==pen[i+1]==1024);total=sum(dist)
 result +=[1024,min(1024,256*(height+1)//(width+1)-1024),div(16*sum(p[0]for p in physical),len(physical))-1024,
           div(16*sum(p[1]for p in physical),len(physical))-1024,256*stroke_count-1024,max(-1024,min(1024,physicalLength//8-1024)),div(2048*physicalLength,total)-1024 if total else-1024]
 assert len(result)==217;return list(map(short,result))
def raster(parts):
 pixels=set()
 for s in parts[::2]:
  if len(s)==1:
   x,y=[v-2 if v==127 else v-1 if v>0 else 0 for v in s[0]]
   pixels.update((x+dx,y+dy)for dy in range(3)for dx in range(3));continue
  for a,b in zip(s,s[1:]):
   ax,ay=a;bx,by=b
   if ax>bx:ax,bx=bx,ax;ay,by=by,ay
   dx=bx-ax;dy=abs(by-ay);ys=-1 if by<ay else 1;steep=dy>dx;major=max(dx,dy);minor=min(dx,dy);error=2*minor-major
   for _ in range(major+1):
    pixels.add((ax,ay))
    if error<0:
     if steep:ay+=ys
     else:ax+=1
     error+=2*minor
    else:ax+=1;ay+=ys;error+=2*(minor-major)
 assert all(0<=x<128 and 0<=y<128 for x,y in pixels)
 row=[0]*128;col=[0]*128;diag=[0]*255;anti=[0]*255
 left=[128]*128;top=left[:];right=left[:];bottom=left[:]
 sy=[i+1 if i<128 else 255-i for i in range(255)];sx=sy[:];al=sy[:];ah=sy[:]
 for x,y in pixels:
  r=(x+1,y)in pixels;l=(x-1,y)in pixels;up=(x,y-1)in pixels;down=(x,y+1)in pixels
  sw=(x-1,y+1)in pixels;se=(x+1,y+1)in pixels;d=x+y;a=x-y+127
  row[y]+=not r;col[x]+=not up
  if not sw:diag[d]+=1
  elif not down and not l:
   at=a-1;anti[at]+=1;al[at]=min(al[at],x-1,y);ah[at]=min(ah[at],127-x if y+1<x else 126-y)
  if not se:anti[a]+=1
  elif not down and not r:
   at=d+1;diag[at]+=1;sy[at]=min(sy[at],y if at<128 else 126-x);sx[at]=min(sx[at],x if at<128 else 126-y)
  left[y]=min(left[y],x);top[x]=min(top[x],y);right[y]=min(right[y],127-x);bottom[x]=min(bottom[x],127-y)
  sy[d]=min(sy[d],y if d<128 else 127-x);sx[d]=min(sx[d],x if d<128 else 127-y);al[a]=min(al[a],x,y);ah[a]=min(ah[a],127-max(x,y))
 out=[0]*192
 for bank in range(16):
  at=slice(bank*8,bank*8+8)
  out[bank]=max(-1024,min(1024,sum(row[at])*64-1024));out[32+bank]=max(-1024,min(1024,sum(col[at])*64-1024))
  for offset,values in((64,left),(96,top),(128,right),(160,bottom)):out[offset+bank]=sum(values[at])*2-1024
  start=bank*16-(bank>=8);indices=range(start,start+16)
  out[16+bank]=max(-1024,min(1024,sum(diag[i]for i in indices)*32-1024));out[48+bank]=max(-1024,min(1024,sum(anti[i]for i in indices)*32-1024))
  for offset,values in((80,sy),(112,al),(144,sx),(176,ah)):
   total=sum((values[i]<<8)//(i+1 if bank<8 else 255-i)for i in indices);out[offset+bank]=(total*128>>8)-1024
 return list(map(short,out))
def extract(strokes):
 allpoints=[p for s in strokes for p in s]
 if len({tuple(p)for p in allpoints})==1:
  stats=[1024,-768,0,0,-768,1024,-1024]
  return[0]*6+[1024]*30+[0]*174+stats,stats+[-1024]*64+[1024]*128
 samples,pen,parts,w,h=geometry(strokes);t=trajectory(samples,pen,w,h,len(strokes));return t,t[210:]+raster(parts)
