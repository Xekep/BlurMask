"""Own integer/byte controls, no vendor assets or execution.

The recurrence oracle expands compatible prefixes, coalesces a request's best
prefix, and truncates a stable cost sort; production uses native linked ordinal
queries and bounded record insertion. Raw-slot controls include poisoned opaque
bytes, duplicate identity replacement, negative costs and equal-cost ordering.
"""
import json,random,struct,sys
from pathlib import Path
r=random.Random(0x1ee030)
tables=[]
for k in range(512):
 cap=k%9+1;raw=bytearray(r.randbytes(40*cap));count=0;ops=[]
 for i in range(cap):struct.pack_into('<i',raw,i*40,0x4000000)
 initial=raw.hex()
 for j in range(30):
  cost=r.choice([-50,-20,0,0,4,4,8,8,40,200,0x4000000]);identity=r.randrange(5) if j%3 else 0
  fields=[cost,r.randrange(-50,500),r.randrange(256),r.randrange(4),r.randrange(65536),r.randrange(8),r.randrange(65536),bool(j%2),identity,r.randrange(64),r.randrange(32)]
  match=next((i for i in reversed(range(count)) if identity and struct.unpack_from('<Q',raw,i*40+24)[0]==identity),None)
  if match is not None and struct.unpack_from('<i',raw,match*40)[0]<=cost:pass
  else:
   if match is None:
    pos=count
    while pos and struct.unpack_from('<i',raw,(pos-1)*40)[0]>cost:pos-=1
    if pos<cap:
     newcount=min(cap,count+1)
     # Independent whole byte sequence shift, retaining old destination bytes.
     raw[(pos+1)*40:newcount*40]=raw[pos*40:(newcount-1)*40];count=newcount;match=pos
   if match is not None:
    off=match*40;packed=struct.unpack_from('<H',raw,off+14)[0]
    struct.pack_into('<ii',raw,off,fields[0],fields[1]);raw[off+9]=fields[2];raw[off+10]=fields[3]
    struct.pack_into('<H',raw,off+12,fields[6]);struct.pack_into('<H',raw,off+14,(packed&0xfc00)|(fields[5]<<1)|int(fields[7])|(fields[9]<<4))
    struct.pack_into('<H',raw,off+16,fields[4]);struct.pack_into('<Q',raw,off+24,identity);raw[off+32]=(raw[off+32]&0xe0)|fields[10]
  ops.append(dict(fields=fields,count=count,raw=raw.hex()))
 tables.append(dict(capacity=cap,initial=initial,operations=ops))

def c118(g,p,kind):
 if (g['symbol']+p['symbol']+kind+g['node'])%11==0:return None
 return ((g['symbol']*13+p['symbol']*7+g['category']+kind*9)%51)-25
def c140(g,p,kind,incoming):
 if (g['category']+p['category']+kind)%13==12:return None
 return incoming+((g['node']+p['arc']*3+g['arc']*5+kind*4)%23)-11
cases=[]
for k in range(384):
 n=k%8+1;cap=k%3+1;width=k%5+1;inputs=[];rows=[];expected=[]
 for node in range(n):
  requests=[]
  for span in reversed(range(min(cap,node+1))):
   for label in range(10,14):
    requests.append(dict(node=node,arc=span,span=span,symbol=label,category=r.randrange(5),cost=r.randrange(-20,200),likelihood=r.randrange(1,256)))
  # Retain caller request order, including equal-cost/different-category labels.
  inputs.append(requests);candidates=[]
  for g in requests:
   previous=node-g['span']-1
   if previous==-1:
    candidates.append(dict(cost=g['cost'],acoustic=g['cost'],previous=255,pathbit=True,request=g,trace=[g['symbol']]))
   else:
    options=[]
    for i,p in enumerate(rows[previous]):
     costs=[]
     for kind in [0,2]:
      a=c118(g,p['request'],kind);b=None if a is None else c140(g,p['request'],kind,a)
      costs.append(0x4000000 if b is None else b)
     d=min(costs)
     if d<0x4000000:options.append((p['cost']+d+g['cost'],i,costs[1]<costs[0]))
    if options:
     total,i,bit=min(options,key=lambda x:x[0]);p=rows[previous][i]
     candidates.append(dict(cost=total,acoustic=p['acoustic']+g['cost'],previous=i,pathbit=bit,request=g,trace=p['trace']+[g['symbol']]))
  rows.append(sorted(candidates,key=lambda x:x['cost'])[:width]);expected.append(rows[-1])
 cases.append(dict(count=n,cap=cap,width=width,requests=inputs,rows=expected))
target=Path(sys.argv[1]);target.parent.mkdir(parents=True,exist_ok=True)
target.write_text(json.dumps(dict(tables=tables,cases=cases),separators=(',',':')))
