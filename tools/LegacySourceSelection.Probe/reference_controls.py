"""Independent source-list/count ordering and binary32 descriptor-weight oracle."""
import argparse,json,random,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('output',type=Path);a=p.parse_args();r=random.Random(0x236490)
def f32(x):return struct.unpack('<f',struct.pack('<f',x))[0]
out=[];uid=0
for case in range(384):
 sources=[];ops=[]
 for step in range(24):
  if sources and r.randrange(4)==0:
   index=r.randrange(len(sources));sources.pop(index);ops.append(dict(remove=index))
  else:
   order=r.randrange(4);count=r.choice([0,1,1024,2048,0x7fffffff,0x80000000,0xffffffff]) if order else None
   weight=f32(f32(r.randint(-2000000,2000000))/65536)
   if case<8:weight=r.choice([0,f32(.25),f32(.5),1,f32(1+2**-23),f32(-2**-23)])
   uid+=1;source=dict(flags=r.choice([0x800,0xc00,0x200,0x600,0x1000])|order*4,order=order,count=count,weight=weight,other=f32(.75),owner=uid)
   sources.append(source);ops.append(dict(add=source))
  prepared=[]
  for index,s in enumerate(sources):
   priority=s['count'] if s['flags']&0x1e00 in (0x800,0xc00) and s['count'] is not None else 1
   prepared.append(dict(index=index,priority=priority,weight=s['weight'],other=s['other'],owner=s['owner'],flags=s['flags'],order=s['order']))
  prepared.sort(key=lambda x:(x['priority'],x['index']),reverse=True)
  total=0
  for s in prepared:total=f32(total+s['weight'])
  if total>1:
   for s in prepared:s['weight']=f32(s['weight']/total)
  for s in prepared:s['weightBits']=struct.unpack('<I',struct.pack('<f',s.pop('weight')))[0]
  ops[-1]['expected']=prepared
 out.append(ops)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(out,separators=(',',':')),encoding='utf8');print(f'Generated {len(out)} independent source traces / {sum(len(x) for x in out)} add-remove-prepare cases')
