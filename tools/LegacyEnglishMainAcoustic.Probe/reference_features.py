"""Independent MAIN feature joins: source normalization, chronology permutation,
pen-up selection, two baseline profiles and saturated2B1. Pure old scalar
references supply independently tested coordinate/statistics kernels.
"""
import ast,math,struct
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
def defs(name):
 p=ROOT/'tools'/name/'reference_controls.py';t=ast.parse(p.read_text(encoding='utf-8-sig'));t.body=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];d={};exec(compile(t,str(p),'exec'),d);return d
coord=defs('LegacyDirectionalCoordinates.Probe');land=defs('LegacyDirectional.Probe')['reference'];cross=defs('LegacyIntersection.Probe')['classification'];globalstat=defs('LegacyDirectionalStatistics.Probe')['reference'];temporalstat=defs('LegacyDirectionalWindow.Probe')['reference'];spatialstat=defs('LegacySpatialStatistics.Probe')['reference'];basestat=defs('LegacyDirectionalBaseline.Probe')['reference']
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def div(a,b):return (-1 if(a<0)!=(b<0)else 1)*(abs(a)//abs(b))
def i32(x):return (x+2**31)%2**32-2**31
def sh(x):return (x+32768)%65536-32768
def sat(x):return max(-32768,min(32767,x))
def clamp(x):return max(-1024,min(1024,sh(x)))
def distance(a,b):return coord['distance']([16*x for x in a],[16*x for x in b])
def geometry(strokes):
 points=[p for s in strokes for p in s];minimum=[min(p[i]for p in points)for i in(0,1)];extent=[max(p[i]for p in points)-minimum[i]for i in(0,1)];scale=130048//max(extent);width,height=[v*scale>>10 for v in extent];center=(127-min(width,height))>>1;cy=center if height<width else 0;cx=center if width<height else 0
 normalized=[[[((p[0]-minimum[0])*scale>>10)+cx,((p[1]-minimum[1])*scale>>10)+cy]for p in s]for s in strokes];lengths=[[distance(a,b)for a,b in zip(s,s[1:])]for s in normalized];totals=list(map(sum,lengths));gaps=[distance(a[-1],b[0])for a,b in zip(normalized,normalized[1:])];stride=(sum(totals)+sum(gaps))//30
 allocation=[1]*(2*len(strokes)-1);weights=[]
 for i,v in enumerate(totals):
  weights.append(v)
  if i<len(gaps):weights.append(gaps[i]-2*stride)
 for unused in range(len(weights),30):
  j=max(range(len(weights)),key=lambda j:weights[j]);weights[j]-=stride;allocation[j]+=1
 samples=[];pen=[]
 for i,stroke in enumerate(normalized):
  count=allocation[2*i];output=[stroke[0]]
  if count>1:
   step=totals[i]//(count-1);segment=residual=previous=0
   while len(output)<count:
    while residual<step and segment<len(lengths[i]):previous=residual;residual+=lengths[i][segment];segment+=1
    j=segment-1;length=lengths[i][j];before=residual;output.append([stroke[j][axis]+div((step-previous)*(stroke[j+1][axis]-stroke[j][axis]),length)for axis in(0,1)]);residual=length-step+previous;multiple=2
    while residual>=step and len(output)<count:
     residual-=step;output.append([stroke[j][axis]+div((stroke[j+1][axis]-stroke[j][axis])*multiple*step,before)for axis in(0,1)]);multiple+=1
  samples+=output;pen+=[1024]*count
  if i<len(gaps):
   count=allocation[2*i+1]
   if gaps[i]==0:assert count==1;samples.append(samples[-1]);pen.append(-1024)
   else:
    remaining=gaps[i];step=remaining//(count+1)
    for unused in range(count):samples.append([samples[-1][axis]+div((normalized[i+1][0][axis]-samples[-1][axis])*step,remaining)for axis in(0,1)]);pen.append(-1024);remaining-=step
 assert len(samples)==30
 return dict(points=samples,pen=pen,allocation=allocation,width=width,height=height,minY=minimum[1],scale=scale,centerY=cy,strokes=len(strokes))
def trajectory(g):
 p=g['points'];pen=g['pen'];deltas=[[(b[i]-a[i])*16 for i in(0,1)]for a,b in zip(p,p[1:])];lengths=[distance(a,b)for a,b in zip(p,p[1:])];directions=[0]*6
 for i,(dx,dy)in enumerate(deltas):
  if pen[i]!=1024 or(i+1<len(deltas)and pen[i+1]!=1024):continue
  terms=[(dx,lengths[i]),(dy,lengths[i])]
  if i+1<len(deltas):terms.append(((dx*deltas[i+1][1]-dy*deltas[i+1][0])>>8,lengths[i]+lengths[i+1]))
  for j,(value,length)in enumerate(terms):
   #Native AddDirection retains the exponent-scaled absolute motion.
   if value:directions[2*j+(value<0)]+=length
   else:directions[2*j]+=length>>1;directions[2*j+1]+=length>>1
 result=[]
 for j in(0,2,4):
  total=sum(directions[j:j+2]);result+=[-1024 if total==0 else div(2048*v,total)-1024 for v in directions[j:j+2]]
 result+=pen+[16*q[0]-1024 for q in p]+[16*q[1]-1024 for q in p]
 vx=[0 if d==0 else div(1024*v[0],d)for v,d in zip(deltas,lengths)];vy=[0 if d==0 else div(1024*v[1],d)for v,d in zip(deltas,lengths)];vx=list(map(sh,vx));vy=list(map(sh,vy));result+=vx+vy
 result+=[sh((ax*bx+ay*by)>>10)for ax,bx,ay,by in zip(vx,vx[1:],vy,vy[1:])];result+=[sh((ax*by-bx*ay)>>10)for ax,bx,ay,by in zip(vx,vx[1:],vy,vy[1:])]
 real=[q for q,state in zip(p,pen)if state==1024];physical=sum(d for i,d in enumerate(lengths)if pen[i]==pen[i+1]==1024);total=sum(lengths)
 result+=[1024,min(1024,256*(g['height']+1)//(g['width']+1)-1024),16*sum(q[0]for q in real)//len(real)-1024,16*sum(q[1]for q in real)//len(real)-1024,256*g['strokes']-1024,max(-1024,min(1024,physical//8-1024)),-1024 if total==0 else 2048*physical//total-1024]
 return list(map(sh,result))
def pixels(g):
 lit=set();at=0
 def line(a,b):
  if a[0]>b[0]:a,b=b,a
  x,y=a;dx=b[0]-x;dy=abs(b[1]-y);step=-1 if b[1]<y else 1;major,minor=max(dx,dy),min(dx,dy);error=2*minor-major
  for i in range(major+1):
   lit.add((x,y))
   if error<0:
    if dy>dx:y+=step
    else:x+=1
    error+=2*minor
   else:x+=1;y+=step;error+=2*(minor-major)
 for group,n in enumerate(g['allocation']):
  if group%2==0:
   points=g['points'][at:at+n]
   if n==1:
    x,y=[v-2 if v==127 else v-1 if v>0 else 0 for v in points[0]];lit.update((x+i,y+j)for i in range(3)for j in range(3))
   else:
    for a,b in zip(points,points[1:]):line(a,b)
  at+=n
 return lit

def baseline(g,lit):
 y=(i32((0-g['minY'])*g['scale'])>>10)+g['centerY'];h=100*g['scale']>>10;real=[q for q,s in zip(g['points'],g['pen'])if s==1024];physical=sum(distance(a,b)for i,(a,b)in enumerate(zip(g['points'],g['points'][1:]))if g['pen'][i]==g['pen'][i+1]==1024);result=[0]*9
 if h==0:
  for i in(0,1,2,3,7,8):result[i]=1024
 else:
  result[0]=min(1024,sh(div(512*(g['width']+1),h)-1024));result[1]=min(1024,sh(div(512*(g['height']+1),h)-1024));result[2]=clamp(div(256*(sum(q[1]for q in real)//len(real)-y),h));result[3]=min(1024,sh(div(256*(physical>>4),h)-1024));result[7]=clamp(div(512*(y-h-g['centerY']),h)-1024);result[8]=clamp(div(512*(g['height']+g['centerY']-y),h)-1024)
 zone=[sum(p[1]<y-h for p in lit),sum(y-h<=p[1]<=y for p in lit),sum(p[1]>y for p in lit)];result[4:7]=[clamp(2048*n//len(lit)-1024)for n in zone]if lit else [1024]*3
 return result

def projection(lit):
 row=[0]*128;col=[0]*128;diag=[0]*255;anti=[0]*255;left=[128]*128;top=left.copy();right=left.copy();bottom=left.copy();dy=[i+1 if i<128 else 255-i for i in range(255)];dx=dy.copy();al=dy.copy();ah=dy.copy()
 for x,y in sorted(lit,key=lambda p:(p[1],p[0])):
  r=(x+1,y)in lit;up=(x,y-1)in lit;down=(x,y+1)in lit;l=(x-1,y)in lit;sw=(x-1,y+1)in lit;se=(x+1,y+1)in lit;d=x+y;a=x-y+127
  row[y]+=not r;col[x]+=not up
  if not sw:diag[d]+=1
  elif not down and not l:
   extra=a-1;anti[extra]+=1;al[extra]=min(al[extra],x-1,y);ah[extra]=min(ah[extra],127-x if y+1<x else 126-y)
  if not se:anti[a]+=1
  elif not down and not r:
   extra=d+1;diag[extra]+=1;dy[extra]=min(dy[extra],y if extra<128 else 126-x);dx[extra]=min(dx[extra],x if extra<128 else 126-y)
  left[y]=min(left[y],x);top[x]=min(top[x],y);right[y]=min(right[y],127-x);bottom[x]=min(bottom[x],127-y);dy[d]=min(dy[d],y if d<128 else 127-x);dx[d]=min(dx[d],x if d<128 else 127-y);al[a]=min(al[a],x,y);ah[a]=min(ah[a],127-max(x,y))
 result=[0]*192
 for k in range(16):
  bins=range(k*8,k*8+8);result[k]=max(-1024,min(1024,sum(row[i]for i in bins)*64-1024));result[32+k]=max(-1024,min(1024,sum(col[i]for i in bins)*64-1024))
  for array,start in[(left,64),(top,96),(right,128),(bottom,160)]:result[start+k]=sum(array[i]for i in bins)*2-1024
  bins=range(k*16-(k>=8),k*16-(k>=8)+16);result[16+k]=max(-1024,min(1024,sum(diag[i]for i in bins)*32-1024));result[48+k]=max(-1024,min(1024,sum(anti[i]for i in bins)*32-1024))
  for array,start in[(dy,80),(al,112),(dx,144),(ah,176)]:result[start+k]=(sum((array[i]<<8)//(i+1 if k<8 else 255-i)for i in bins)*128>>8)-1024
 return list(map(sh,result))
def assembly(global_,windows,base,spatial):
 def ratio(v,d):return div(i32(v*2048),d)-1024
 result=[1024]
 for i,v in enumerate(global_):result.append(sat(div(i32(v*2048-2048),5)-1024 if i==2 else i32(v*2)if i==8 else ratio(v,3072 if i==3 else 3 if 9<=i<=15 else 1024)))
 for window in windows:
  for i,v in enumerate(window):result.append(sat(ratio(v,256)if spatial and i in(0,1,2,15)else i32(v*2048-1024)if i in(4,5,9,10,11,12,13,14)else i32(v*2)if i==8 else ratio(v,128 if i==15 else 1024)))
 result+=[sat(ratio(i32(v+(4096 if i==3 else 0)),8192 if i in(2,3)else 1024 if i>=6 else 4096))for i,v in enumerate(base)];assert len(result)==283;return result

def directional(strokes):
 normalized=coord['norm'](strokes);samples=coord['resample'](normalized,100);flags=land(samples['strokes'])['flags'];case=dict(strokes=samples['strokes'],width=samples['width'],height=samples['height'],step=samples['step'],gridScale=10);intersections,loops=cross(case);case.update(arc=samples['cumulative'][-1],flags=flags,intersections=intersections,intersectionCount=loops,cumulative=samples['cumulative'],shift=normalized['shift'],unit=True)
 global_=globalstat(case);shift=normalized['shift'];scale=lambda v:v<<shift if shift>=0 else v>>-shift
 if samples['step']==0:by,bh=0,100
 else:by,bh=scale(-normalized['minY']),max(1,scale(100))
 base=basestat(dict(case,baselineY=by,baselineHeight=bh));n=len(flags);windows=[[]for _ in range(16)];lengths=samples['cumulative']
 if n<=16:
  for i in range(n):windows[i]=[i]
 else:
  step=(lengths[-1]<<10)//16;boundary=step+512;start=k=0
  for i in range(1,n):
   if lengths[i]>boundary>>10:windows[k]=list(range(start,i));k+=1;start=i;boundary+=step
  windows[k]=list(range(start,n))
 temporal=[temporalstat(dict(case,members=indices))for indices in windows];spatial=[[]for _ in range(16)]
 for i,(x,y)in enumerate([p for s in samples['strokes']for p in s]):
  sx=(samples['width']<<10)//4;sy=(samples['height']<<10)//4;ix=x*1024//sx;iy=y*1024//sy;ix=ix if 0<=ix<4 else 3;iy=iy if 0<=iy<4 else 3;spatial[iy*4+ix].append(i)
 return assembly(global_,temporal,base,False),assembly(global_,[spatialstat(dict(case,members=indices))for indices in spatial],base,True)

def audit(row,g):
 ranges=g['Ordered'];raw=[[[f(v)for v in p]for p in row['CutInk'][s][a:z]]for s,a,z in ranges];minimum=min(p[0]for s in raw for p in s);baseline_,height=f(row['Baseline']),f(row['Height']);normalized=[[[int(min(32767,f(f(f(p[0]-minimum)*100)/height))),int(min(32767,f(f(f(p[1]-baseline_)*100)/height)))]for p in s]for s in raw];assert normalized==g['Normalized'],('normalization',row['Name'],g['Group'])
 assert g['Ordered']==[g['Stored'][i]for i in g['Order']]
 if 'StoredStartKeys'in g:
  keys=g['StoredStartKeys'];indices=list(range(len(keys)))
  def sortkeys(lo,hi):
   l,r=lo,hi;pivot=keys[indices[(lo+hi)//2]]
   while l<=r:
    while keys[indices[l]]<pivot:l+=1
    while keys[indices[r]]>pivot:r-=1
    if l>r:break
    indices[l],indices[r]=indices[r],indices[l];l+=1;r-=1
   if lo<r:sortkeys(lo,r)
   if l<hi:sortkeys(l,hi)
  sortkeys(0,len(keys)-1);assert indices==g['Order']
 assert len(set(r[0]for r in g['Stored']))==g['SourceCount']
 boundaries=[1024 if g['Stored'][0][1]==0 else -1024,1024 if g['Stored'][-1][2]==row['CutCounts'][g['Stored'][-1][0]]else -1024];assert boundaries==g['Boundary']
 #Pen-up threshold10: independent descending quicksort ties preserve native order.
 kept=list(range(len(normalized)-1))
 if len(normalized)>10:
  gaps=[[sum((a-b)**2 for a,b in zip(normalized[i][-1],normalized[i+1][0])),i]for i in kept]
  def sort(lo,hi):
   left,right=lo,hi;pivot=gaps[(lo+hi)//2][0]
   while left<=right:
    while gaps[left][0]>pivot:left+=1
    while gaps[right][0]<pivot:right-=1
    if left>right:break
    gaps[left],gaps[right]=gaps[right],gaps[left];left+=1;right-=1
   if lo<right:sort(lo,right)
   if left<hi:sort(left,hi)
  sort(0,len(gaps)-1);kept=sorted(i for _,i in gaps[:9])
 merged=[];current=[]
 for i,s in enumerate(normalized):
  current+=s
  if i==len(normalized)-1 or i in kept:merged.append(current);current=[]
 assert kept==g['RetainedPenUps']and merged==g['Merged'];point=merged[0][0];degenerate=all(p==point for s in merged for p in s);assert degenerate==g['Degenerate']
 if degenerate:
  y=point[1];base=[-1024,-1024,clamp(div(256*y,100)),-1024,0,0,0,0,0]
  if y>0:base[4:]=[-1024,-1024,1024,-1024,sh(div(512*(-y-100),100)-1024)]
  else:base[4:]=[1024,-1024,-1024,sh(div(512*y,100)-1024),-1024]
  stats=[1024,-768,0,0,-768,1024,-1024];trajectory_=[0]*6+[1024]*30+[0]*174+stats+base;raster=stats+base+[-1024]*64+[1024]*128
 else:
  geometry_=geometry(merged);lit=pixels(geometry_);base=baseline(geometry_,lit);trajectory_=trajectory(geometry_)+base;raster=trajectory_[210:217]+base+projection(lit)
 assert trajectory_+boundaries==g['Inputs'][0],('trajectory',row['Name'],g['Group'],[(i,a,b)for i,(a,b)in enumerate(zip(trajectory_,g['Inputs'][0]))if a!=b][:10])
 assert raster+boundaries==g['Inputs'][1],('raster',row['Name'],g['Group'])
 a,b=directional(merged);assert a+boundaries==g['Inputs'][2],('temporal',row['Name'],g['Group']);assert b+boundaries==g['Inputs'][3],('spatial',row['Name'],g['Group'])
