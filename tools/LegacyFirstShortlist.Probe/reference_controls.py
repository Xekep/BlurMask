"""Independent fresh A8 defined-byte/first-arc oracle; no vendor assets."""
from pathlib import Path
import json,struct,random,sys,math
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
r=random.Random(0x1ef070)
cases=[]
for _ in range(512):
 width=r.randint(1,12);cap=r.randint(1,5);count=r.randint(0,width)
 primary=bytearray(r.randbytes(width*40));active=[];providers={}
 for i in range(count):
  arc=r.randint(0,width//2);cost=r.randint(-10000,100000)
  if i==count-1 and r.randrange(5)==0:cost=67108864
  acoustic=r.randint(-10000,100000);struct.pack_into('<ii',primary,i*40,cost,acoustic);struct.pack_into('<H',primary,i*40+16,arc)
  active.append((arc,cost,acoustic))
  if arc not in providers:
   symbols=r.sample(range(100,150),r.randint(0,20));p=[]
   for s in symbols:p.append([s,f(r.choice([0,1,1/255,.5,r.random()]))])
   if p and r.randrange(4)==0:p.insert(r.randrange(len(p)),[65535,0]);p=p[:20]
   providers[arc]=p
 initial=bytearray(r.randbytes(width*cap*40));expected=initial[:];owners=[];counts=[];seen=set()
 for ordinal,(arc,cost,acoustic) in enumerate(active):
  if cost>=67108864:break
  if arc in seen:continue
  seen.add(arc);group=len(owners);n=0
  for symbol,p in providers[arc]:
   if n==cap or symbol==65535:break
   if symbol in [100,101]:continue
   pos=(group*cap+n)*40;struct.pack_into('<ii',expected,pos,cost,acoustic);expected[pos+8]=max(1,min(255,math.trunc(f(p*255))))
   struct.pack_into('<HH',expected,pos+12,symbol,struct.unpack_from('<H',expected,pos+14)[0]);struct.pack_into('<H',expected,pos+16,arc);n+=1
  owners.append(ordinal);counts.append(n)
 for g in range(width):
  for a in range(counts[g] if g<len(counts) else 0,cap):struct.pack_into('<i',expected,(g*cap+a)*40,67108864)
 cases.append(dict(primary=primary.hex(),count=count,cap=cap,providers={str(k):v for k,v in providers.items()},initial=initial.hex(),expected=expected.hex(),owners=owners,counts=counts))
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(cases,separators=(',',':')),encoding='utf-8')
print('Generated512 fresh A8 first-encounter/finite-stop/top20/opaque-byte controls.')
