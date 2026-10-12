"""Independent binary32 source-budget oracle; no vendor code is invoked."""
import base64,ctypes,json,random,struct,sys
from pathlib import Path
f=lambda v:ctypes.c_float(v).value
u=lambda v:v&0xffffffff
i=lambda v:ctypes.c_int32(v).value
scaled=lambda value,weight:max(1,u(int(f(f(value)*f(weight)))))
minimum=lambda value,weight:0 if weight<=0 else scaled(value,weight)
r=random.Random(0x1fbb90);cases=[]
for case in range(512):
 count=case%17
 rows=[]
 for j in range(count):
  kind=r.choice([0,0x800,0xa00,0xc00,0x1e00])
  header=kind|r.randrange(0x200)
  lower=r.choice([0,0,.125,.25,.5,1,2,-.25,-0.0])
  upper=r.choice([0,.125,.5,1,1,2,-.25])
  if (case+j)%5==0:lower,upper=0,1
  rows.append([header,lower,upper])
 if case==0:rows=[[0xa00,0,1]] # Override matches zeroed general source30.
 if case==1:rows=[[0xc00,0,1],[0xa00,0,1]] # Nonzero index cannot match general.
 if case==2:rows=[[0xa00,.25,.5],[0xa00,.125,1]] # Last special source wins.
 if case==3:rows=[[0xc00,0,1]]
 budget=r.choice([0,1,2,3,7,16,64,127,255,65535])
 factor=r.choice([0,.125,.25,.5,.75,1,2,-.25])
 share=f(r.choice([0,0,-0.0,.125,.25,.5,.75,1]))
 if case<4:budget,factor,share=3,.25,0
 packed=0;sub=-1;override=-1
 for idx,(header,lower,upper) in enumerate(rows):
  kind=header&0x1e00
  packed=(packed&~7)|max(packed&7,(header>>2)&7)
  for sourcebit,targetbit in [(5,3),(7,6),(8,7)]:
   if header&(1<<sourcebit):packed|=1<<targetbit
  if header&0x20 and kind!=0x800:packed|=0x10
  if kind==0xa00:packed|=0x20;override=idx
  if kind==0x800:sub=idx
 expanded=u(int(f(f(f(2 if packed&0x10 else 1)*f(factor))*f(budget))))
 fields=[[0]*7];buckets=[];suma=sumb=0
 for idx,(header,lower,upper) in enumerate(rows):
  if lower==0 and upper==1:buckets.append(0);continue
  buckets.append(len(fields)&0xff)
  a=minimum(budget,lower);b=scaled(budget,upper)
  c=minimum(expanded,lower);d=scaled(expanded,upper)
  fields.append([a,b,u(2*b+2),c,d,u(4*d+1),idx]);suma=i(suma+a);sumb=i(sumb+c)
 primary=len(fields);heapOverride=0xffffffff
 if share==0:
  found=next((idx for idx,v in enumerate(fields) if v[6]==override),None)
  if found is not None:
   v=fields[found];heapOverride=len(fields);fields.append([*v[:3],0,0,0,-1]);suma=i(suma+v[0])
 else:
  original=[v.copy()for v in fields];fields.extend([[0]*7 for _ in range(primary)])
  for idx in range(1,primary):
   for destination,fraction in [(idx,f(1-share)),(idx+primary,share)]:
    v=original[idx];fields[destination]=[0 if pos in (0,3) and val==0 else scaled(val,fraction)for pos,val in enumerate(v[:6])]+[v[6]]
 a=u(2*max(2,i(budget-suma)));b=u(4*max(1,i(expanded-sumb)))
 general=[0xffffffff,0xffffffff,a,0xffffffff,0xffffffff,b,-1]
 if share==0:fields[0]=general
 else:
  fields[0]=[0xffffffff,0xffffffff,scaled(a,f(1-share)),0xffffffff,0xffffffff,scaled(b,f(1-share)),-1]
  fields[primary]=[0xffffffff,0xffffffff,scaled(a,share),0xffffffff,0xffffffff,scaled(b,share),0]
 image=bytearray(len(fields)*56)
 for idx,v in enumerate(fields):struct.pack_into('<7I',image,idx*56+0x18,*[u(x)for x in v])
 cases.append(dict(sources=rows,budget=budget,factor=factor,share=share,packed=packed,subword=sub,overrideSource=override,overrideHeap=heapOverride,buckets=buckets,image=base64.b64encode(image).decode()))
limitCases=[]
for case in range(4096):
 prior=[r.randrange(65536)for _ in range(3)];knowledge=[r.choice([-32768,-2,-1,-1,0,1,2,20,32767])for _ in range(3)]
 mode=r.choice([0,0,1,255]);packed=r.randrange(0x10000);arc=r.getrandbits(32);initial=bool(case%2)
 counts=prior.copy()
 if initial:
  counts=[2 if knowledge[0]==-1 else knowledge[0]&0xffff,2 if knowledge[1]==-1 else knowledge[1]&0xffff,
          (1 if mode==0 and packed&7>1 else 2)if knowledge[2]==-1 else knowledge[2]&0xffff]
 if initial and case%7==1:prior[1:]=counts[1:] # Header-only changes do not free pools.
 if initial and case%7==3:prior=counts.copy()
 reset=initial and counts[1:]!=prior[1:]
 limitCases.append(dict(prior=prior,knowledge=knowledge,mode=mode,packed=packed,arc=arc,initial=initial,expected=counts,reset=reset))
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(dict(cases=cases,limits=limitCases),separators=(',',':')),encoding='utf8');print(f'Generated {len(cases)} independent full56-byte preparations /{len(limitCases)} controller limits')
