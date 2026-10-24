"""PURE CVTT/limiter branch DATA, not caller-authorized sampler input."""
import math,struct,json,sys
from pathlib import Path
def f(x):
 # Python struct rejects finite overflow; SSE/C# F64-to-F32 conversion yields signed infinity.
 try:return struct.unpack('<f',struct.pack('<f',x))[0]
 except OverflowError:return math.copysign(float('inf'),x)
def cvtt(v):
 b=struct.unpack('<I',struct.pack('<f',v))[0];sign=b>>31;exp=(b>>23)&255
 if exp==255 or v>=2147483648. or v< -2147483648.:return -2147483648
 return math.trunc(v)
def i32(v):return ((v+2**31)%2**32)-2**31
def divide(a,b):
 if b==0:
  if a==0:return float('nan')
  return math.copysign(float('inf'),a*math.copysign(1.,b))
 return f(a/b)
def merge(left,right):return f(float(right)+float(left)*.1)if right<=left else f(float(right)*.1+float(left))
def limit(tags,weights,target):
 m=len(tags);entries=list(zip(range(m),weights));removed=[];attempts=i32(m-target)if target<m else 0;visits=0
 for _ in range(max(attempts,0)):
  visits+=1;best=f(3.4028234663852886e38);selected=-1
  for k in range(1,len(entries)):
   cost=merge(entries[k-1][1],entries[k][1]);tag=tags[entries[k][0]]
   if cost<best and tag not in [7,4]:best=cost;selected=k
  if selected<0:continue
  removed.append(entries[selected][0]);entries.pop(selected);entries[selected-1]=(entries[selected-1][0],best)
 return dict(attempts=attempts,visits=visits,removed=removed,remaining=[i for i,w in entries])
def controls():
 values=[0.,-0.,5.9,-5.9,float('nan'),float('inf'),-float('inf'),2147483648.,f(2147483520.),-2147483648.]
 converted=[cvtt(v)for v in values];assert converted==[0,0,5,-5,-2147483648,-2147483648,-2147483648,-2147483648,2147483520,-2147483648]
 heights=[]
 for width,height in [(0.,0.),(1.,0.),(0.,-0.),(1.,-0.),(2.,2.)]:
  scaled=divide(f(width*4),height);target=cvtt(scaled);r=limit([1,2,2,2,2,2,2],[1.]*7,target)
  assert r['visits']==(3 if width==height==2 else 0)
  heights.append(dict(width=width,height=height,scaledBits=struct.unpack('<I',struct.pack('<f',scaled))[0],target=target,result=r))
 protected=limit([1,7,4,7,4,7,4],[1.]*7,1);assert protected['visits']==6 and protected['removed']==[] and protected['remaining']==list(range(7))
 inf=limit([1]+[2]*6,[float('inf')]*7,1);nan=limit([1]+[2]*6,[float('nan')]*7,1)
 assert inf['visits']==nan['visits']==6 and inf['removed']==nan['removed']==[]
 tie=limit([1]+[2]*6,[1.]*7,6);assert tie['removed']==[1]
 maximum=f(3.4028234663852886e38)
 positive=merge(maximum,maximum);negative=merge(-maximum,-maximum)
 assert math.isinf(positive) and positive>0 and math.isinf(negative) and negative<0
 assert merge(10.,20.)==merge(20.,10.)==12.
 finite=limit([1]+[2]*6,[maximum]*7,1)
 assert finite['visits']==6 and finite['removed']==[] and finite['remaining']==list(range(7))
 overflow=dict(positiveMergeBits=struct.unpack('<I',struct.pack('<f',positive))[0],negativeMergeBits=struct.unpack('<I',struct.pack('<f',negative))[0],asymmetricOperandResults=[merge(10.,20.),merge(20.,10.)],finiteMaxNoSelection=finite)
 return dict(finiteOverflow=overflow,cvttInputsBits=[struct.unpack('<I',struct.pack('<f',v))[0]for v in values],cvttResults=converted,heightCases=heights,protectedNoSelection=protected,infiniteCostNoSelection=inf,nanCostNoSelection=nan,strictTie=tie,totalControls=24,networkForwards=0,sourceAuthority=False)
if __name__=='__main__':Path(sys.argv[1]).write_text(json.dumps(controls(),indent=2)+'\n',encoding='utf8')
