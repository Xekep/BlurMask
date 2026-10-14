"""Independent fixed-variance29B EM/intersection/producer oracle."""
import base64,json,math,random,struct,sys
from pathlib import Path
import numpy as np
f=lambda x:float(np.float32(x))
def fit(profile,observations):
 seedm1,seedv1,seedp1,seedm2,seedv2,seedp2=profile;mass0=f(seedp1+seedp2)
 anchor1=f(seedp1/mass0);anchor2=f(seedp2/mass0);previous1=previous2=-1.;attempts=[]
 for retry in range(5):
  m1,m2=seedm1,seedm2;p1,p2=seedp1,seedp2;v1=f(seedv1/(1<<retry));v2=f(seedv2/(1<<retry));iterations=0
  while True:
   iterations+=1
   if iterations>4096:raise ValueError('Convergence portable bound')
   w1,w2,mass=anchor1,anchor2,mass0;s1=m1*anchor1;s2=m2*anchor2
   for value in observations:
    d=value-m1;a=math.exp(((d*-.5)*d)/v1)*p1;d=value-m2;b=math.exp(((d*-.5)*d)/v2)*p2;t=b+a
    if max(abs(t)*4.440892098500626e-16,2.220446049250313e-16)<abs(t):
     mass+=1.;aa=a/t;bb=b/t;w1+=aa;s1+=value*aa;s2+=value*bb;w2+=bb
   m1=f(s1/w1);m2=f(s2/w2);p1=f(w1/mass);p2=f(w2/mass)
   if abs(f(m1-previous1))<1e-6 and abs(f(m2-previous2))<1e-6:break
   previous1,previous2=m1,m2
  a=-.5/v1;b=-.5/v2;quadratic=a-b;linear1=m1*a;linear2=m2*b;half=linear2-linear1
  constant=math.log((p1/math.sqrt(v1*6.283185307179586))/(p2/math.sqrt(v2*6.283185307179586)))+(linear1*m1-linear2*m2)
  discr=half*half-quadratic*constant
  if quadratic==0:
   if half==0:raise ValueError('Nonfinite native intersection')
   root=(constant*-.5)/half
  elif discr>=0:
   r=math.sqrt(discr);plus=(r-half)/quadratic;minus=(-half-r)/quadratic
   d=plus-m1;ap=math.exp(((d*-.5)*d)/v1)*p1;d=minus-m1;am=math.exp(((d*-.5)*d)/v1)*p1
   root=plus if am<=ap else minus
  else:root=f(m1+m2)*.5
  if not math.isfinite(root):raise ValueError('Nonfinite native intersection')
  attempts.append(dict(index=retry,iterations=iterations,firstMean=m1,secondMean=m2,firstVariance=v1,secondVariance=v2,firstPrior=p1,secondPrior=p2,intersection=root))
  if abs(f(m1-m2))>1e-6 and m1<root<m2:break
 return dict(intersection=root,attempts=attempts)
def gap_vectors(boxes,intervals,heights,minimum):
 all_gaps=[];qualified=[];sorted_rows=[]
 def center(row):return f(f(boxes[row][0]+boxes[row][1])*.5)
 def sort(order,start,end):
  while True:
   pivot=center(order[(start+end)//2]);a=start;b=end
   while True:
    while center(order[a])<pivot:a+=1
    while pivot<center(order[b]):b-=1
    if b<a:break
    order[a],order[b]=order[b],order[a];a+=1;b-=1
    if a>b:break
   if start<b:sort(order,start,b)
   start=a
   if end<=start:return
 for line,start,end in intervals:
  order=list(range(start,end));sort(order,0,len(order)-1) if len(order)>1 else None;sorted_rows.append(order);right=-np.finfo(np.float32).max
  for i,row in enumerate(order):
   if i:
    value=f(max(0,min(4,f(f(boxes[row][0]-right)/heights[line]))));all_gaps.append(value)
    if end-start-1>minimum:qualified.append(value)
   right=max(right,boxes[row][1])
 return all_gaps,qualified,sorted_rows
def active(profile,mixture,boxes,intervals,heights):
 minimum,maximum,zero,offset,multiplier,limit=profile;all_gaps,qual,sorted_rows=gap_vectors(boxes,intervals,heights,minimum)
 blend=f(max(0,min(1,f(f(len(qual)-minimum)/f(maximum-minimum)))))
 assert blend>0
 model=fit(mixture,qual);mean=sum(all_gaps)/len(all_gaps);variance=sum((v-mean)**2 for v in all_gaps)/len(all_gaps)
 ordinary=mean+(offset-math.sqrt(variance)*multiplier);value=f(min(limit,f(1-blend)*ordinary+f(f(model['intersection'])*blend)))
 return dict(gap=value,blend=blend,mean=mean,variance=variance,observations=all_gaps,qualifying=qual,sorted=sorted_rows,mixture=model)
if __name__=='__main__':
 if len(sys.argv)!=3 or Path(sys.argv[2]).suffix.lower()!='.json':raise ValueError('AK.res output.json')
 ak=Path(sys.argv[1]).read_bytes();node=ak[188050:188108];gaussian=list(struct.unpack_from('<6f',node,16));profile=[*node[40:42],*struct.unpack_from('<4f',node,42)]
 rng=random.Random(0x175d26);fits=[];active_cases=[]
 for trial in range(192):
  model=gaussian[:] if trial<128 else list(map(f,[.10,.06,.65,1.70,.14,.35])) if trial<160 else list(map(f,[1.5,.1,.2,.15,.1,.8])) if trial<184 else list(map(f,[.2,.1,.5,.2,.1,.5]))
  observations=[f(0.05+((i+trial)%7)*.03) if i%3 else f(1.0+(trial%7)*.18+rng.random()*.1) for i in range(trial%49)]
  if trial%11==0:observations=[f(trial%5)]*(1+trial%31)
  try:expected=fit(model,observations)
  except ValueError:expected=None
  fits.append(dict(profile=model,observations=observations,expected=expected))
 for trial in range(64):
  boxes=[];intervals=[];heights=[]
  for line in range(1+trial%2):
   start=len(boxes);count=12+trial%17;cursor=f(0);h=f(15+trial%31);heights.append(h)
   for i in range(count):
    width=f(h*(.2+(i%3)*.05));boxes.append([cursor,f(cursor+width)])
    gap=f(h*(.1+((i+trial)%11==0)*1.5+rng.random()*.1));cursor=f(cursor+f(width+gap))
   intervals.append((line,start,len(boxes)))
  ds=[];gs=[];rs=[]
  for line,start,end in intervals:
   b=bytearray(88)
   for at,v in [(16,line),(52,start),(56,end)]:struct.pack_into('<i',b,at,v)
   ds.append(base64.b64encode(b).decode())
  for h in heights:
   b=bytearray(40);struct.pack_into('<f',b,4,h);gs.append(base64.b64encode(b).decode())
  for left,right in boxes:
   b=bytearray(720);struct.pack_into('<i',b,16,-1);struct.pack_into('<ff',b,24,left,right);rs.append(base64.b64encode(b).decode())
  active_cases.append(dict(descriptors=ds,guides=gs,rows=rs,expected=active(profile,gaussian,boxes,intervals,heights)))
 Path(sys.argv[2]).write_bytes((json.dumps(dict(fits=fits,cases=active_cases),separators=(',',':'))+'\n').encode())
 print(f'Generated{len(fits)} independent EM controls +{len(active_cases)} active source720 producer controls, attempts up to{max(len(c["expected"]["attempts"]) for c in fits if c["expected"])}.')
