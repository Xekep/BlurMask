"""Independent29B175260 sort/frontier/statistics oracle; no vendor execution."""
import base64,json,math,random,struct,sys
from pathlib import Path
import numpy as np
f=lambda x:float(np.float32(x))
def oracle(boxes,intervals,guides,incoming,profile):
 minimum,maximum,zero,offset,multiplier,limit=profile;qualified=sum(end-start-1 for _,start,end in intervals if end-start-1>minimum)
 if qualified>minimum:return None
 count=sum(end-start-1 for _,start,end in intervals)
 if not count:return dict(gap=zero,assigned=True,mean=0.,variance=0.,observations=[],sorted=[])
 observed=[];sorted_rows=[]
 def sort(order,first,last):
  while True:
   pivot=f(f(boxes[order[(first+last)//2]][0]+boxes[order[(first+last)//2]][1])*.5);a=first;b=last
   def center(index):return f(f(boxes[order[index]][0]+boxes[order[index]][1])*.5)
   while True:
    while center(a)<pivot:a+=1
    while pivot<center(b):b-=1
    if b<a:break
    order[a],order[b]=order[b],order[a];a+=1;b-=1
    if a>b:break
   if first<b:sort(order,first,b)
   first=a
   if last<=first:return
 for line,start,end in intervals:
  order=list(range(start,end))
  if len(order)>1:sort(order,0,len(order)-1)
  sorted_rows.append(order);front=-np.finfo(np.float32).max
  for i,row in enumerate(order):
   left,right=boxes[row]
   if i:observed.append(f(max(0,min(4,f(f(left-front)/guides[line])))))
   front=max(front,right)
 mean=sum(observed)/len(observed);variance=sum((v-mean)**2 for v in observed)/len(observed)
 assigned=len(observed)>=minimum or variance>=f(.1)
 gap=f(min(limit,mean+(offset-math.sqrt(variance)*multiplier))) if assigned else incoming
 return dict(gap=gap,assigned=assigned,mean=mean,variance=variance,observations=observed,sorted=sorted_rows)
if __name__=='__main__':
 if len(sys.argv)!=3:raise ValueError('AK.res controls.json')
 ak=Path(sys.argv[1]).read_bytes();node=ak[188050:188108];assert struct.unpack_from('<I',node,8)[0]==0x29b
 profile=[*node[40:42],*struct.unpack_from('<4f',node,42)];rng=random.Random(0x175350);controls=[]
 for n in range(160):
  boxes=[];intervals=[];guides=[]
  for line in range(1+n%3):
   start=len(boxes);count=1+(n+line)%12
   for r in range(count):
    center=f(rng.randrange(-10,40)*5);half=f(rng.randrange(0,60));boxes.append([f(center-half),f(center+half)])
   intervals.append((line,start,len(boxes)));guides.append(f(5+(n+line)%45))
  incoming=f(-1 if n%2 else 1.27);expected=oracle(boxes,intervals,guides,incoming,profile)
  ds=[];gs=[];rs=[]
  for line,start,end in intervals:
   b=bytearray(88)
   for at,value in [(16,line),(52,start),(56,end)]:struct.pack_into('<i',b,at,value)
   ds.append(base64.b64encode(b).decode())
  for h in guides:
   b=bytearray(40);struct.pack_into('<f',b,4,h);gs.append(base64.b64encode(b).decode())
  for left,right in boxes:
   b=bytearray(720);struct.pack_into('<ff',b,24,left,right);rs.append(base64.b64encode(b).decode())
  controls.append(dict(descriptors=ds,guides=gs,rows=rs,incoming=incoming,expected=expected))
 Path(sys.argv[2]).write_bytes((json.dumps(controls,separators=(',',':'))+'\n').encode())
 print(f'Generated{len(controls)} independent29B cases; {sum(c["expected"] is None for c in controls)} actual active-EM refusals.')
