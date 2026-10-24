"""Independent literal03BCE0/03C2E0 selected299 reference from original bodies.
No learned neural evaluation. Private geometry normalization is00FB90 mode1/1."""
import math,struct

def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def add(a,b):return f(f(a)+f(b))
def sub(a,b):return f(f(a)-f(b))
def mul(a,b):return f(f(a)*f(b))
def div(a,b):return f(f(a)/f(b))
def round_short(x):
 y=add(x,-.5 if x<=0 else .5)
 if not math.isfinite(y)or not -32768<=y<32768:raise ValueError('short domain')
 return math.trunc(y)

def normalized(strokes):
 src=[[[f(c)for c in p]for p in row]for row in strokes]
 lo=[min(p[k]for row in src for p in row)for k in(0,1)]
 hi=[max(p[k]for row in src for p in row)for k in(0,1)]
 w,h=[sub(hi[i],lo[i])for i in(0,1)];scale=min(div(8192,w),div(8192,h))if w!=0 and h!=0 else f(1)
 return [[[round_short(add(mul(sub(p[k],lo[k]),scale),0))for k in(0,1)]for p in row]for row in src]

def length(a,b):
 x=f(b[0]-a[0]);y=f(b[1]-a[1]);return f(math.sqrt(add(mul(y,y),mul(x,x))))

def resample(rows):
 total=f(0)
 for row in rows:
  for a,b in zip(row,row[1:]):total=add(total,length(a,b))
 step=div(total,29);groups=[]
 def append(x,y):
  if sum(len(g)for g in groups)>=30:raise ValueError('over30')
  groups[-1].append([f(x),f(y)])
 if step==0:
  groups=[[]]
  for _ in range(30):append(*rows[0][0])
  return groups
 residual=f(0)
 for s,row in enumerate(rows):
  pending=s!=0
  if s==0:groups.append([]);append(*row[0])
  for i in range(1,len(row)):
   before=residual;after=add(before,length(row[i-1],row[i]))
   if step<=after:
    segment=sub(after,before)
    tolerance=max(mul(abs(segment),f(2**-22)),f(2**-23))
    if abs(segment)<=tolerance:
     if pending:groups.append([]);pending=False
     append(*row[i]);after=segment
    else:
     fraction=div(sub(step,before),segment);after=add(before,sub(segment,step))
     while True:
      if pending:groups.append([]);pending=False
      inv=sub(1,fraction)
      append(*[add(mul(row[i-1][k],inv),mul(row[i][k],fraction))for k in(0,1)])
      successor=add(fraction,div(step,segment))
      if 1<=successor:break
      if not fraction<successor:raise ValueError('nonprogress')
      fraction=successor;after=sub(after,step)
   residual=after
 if .5<div(residual,step):append(*rows[-1][-1])
 return groups

def features(strokes,weights):
 groups=resample(normalized(strokes));samples=[p for row in groups for p in row]
 low=[min(p[k]for p in samples)for k in(0,1)];high=[max(p[k]for p in samples)for k in(0,1)]
 width,height=[sub(high[k],low[k])for k in(0,1)];extent=max(width,height,f(1))
 ox=mul(add(sub(height,high[0]),low[0]),.5)if width<height else f(0)
 oy=mul(add(sub(width,high[1]),low[1]),.5)if height<width else f(0)
 grouped=[]
 for group in groups:
  target=[]
  for i,p in enumerate(group):
   xy=[round_short(div(mul(add(sub(p[k],low[k]),[ox,oy][k]),8192),extent))for k in(0,1)]
   target.append([8192 if i==0 else -8192,*xy,0,0,0,0])
  grouped.append(target)
 for group in grouped:
  for i,row in enumerate(group):
   left=group[max(0,i-1)];right=group[min(i+1,len(group)-1)]
   dx=right[1]-left[1];dy=right[2]-left[2];n=f(math.sqrt(dx*dx+dy*dy))
   tol=max(mul(abs(n),f(2**-22)),f(2**-23))
   row[3:5]=[0,0]if abs(n)<=tol else [round_short(div(f(dx*8192),n)),round_short(div(f(dy*8192),n))]
 for group in grouped:
  for i,row in enumerate(group):
   left=group[max(0,i-1)];right=group[min(i+1,len(group)-1)]
   row[5]=(left[3]*right[3]+left[4]*right[4])>>13
   row[6]=(left[3]*right[4]-left[4]*right[3])>>13
 rows=[r for group in grouped for r in group]
 count=len(rows)
 while len(rows)<30:rows.append(rows[-1].copy())
 values=[round_short(mul(c,weights[i]))for row in rows for i,c in enumerate(row)]
 return values,count,len(groups),groups

def raw_prototypes(b):
 assert struct.unpack_from('<HHBBB',b,1772560)==(30,0,4,1,127)
 weights=struct.unpack_from('<7f',b,1772567);p=1772595;n=struct.unpack_from('<H',b,p)[0];p+=2;result=[]
 for label in range(n):
  count=b[p];p+=1;entries=[]
  for ordinal in range(count):
   values=struct.unpack_from('<210h',b,p);p+=420;nt=b[p];p+=1
   targets=struct.unpack_from('<'+'H'*nt,b,p);p+=nt*2
   factor=struct.unpack_from('<f',b,p)[0]if nt else None;p+=4 if nt else 0
   entries.append(dict(ordinal=ordinal,values=list(values),targets=list(targets),factor=factor))
  result.append(dict(label=label,entries=entries))
 assert p==1789539
 return weights,result

def distances(values,entries):
 return [div(sum(abs(a-b)for a,b in zip(values,e['values'])),30*8192)for e in entries]
