"""Functional canonical-prefix model, native pair descent and independent full-byte writes."""
import ast,ctypes,json,random,struct,sys
from pathlib import Path
syntax=ast.parse(Path('tools/LegacyWordContextAlternatives.Probe/reference_controls.py').read_text(encoding='utf-8-sig'))
selected=[n for n in syntax.body if isinstance(n,ast.FunctionDef) and n.name=='compose' or isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='actions' for t in n.targets)]
namespace={};exec(compile(ast.Module(body=selected,type_ignores=[]),'<path-model>','exec'),namespace);compose=namespace['compose']

def tail(current,reference):
 depth=len(reference)
 while depth:
  for size in range(len(current),-1,-1):
   if current[:size]==reference[:depth] or current[:size]==reference[:depth-1]:
    return dict(reference=reference[:depth],parent=reference[:depth-1],matched=current[:size],keys=list(reversed(current[size:])))
  depth-=2
 return None

rng=random.Random(0x21222a);values=list(range(0xfff5,0x10000))+[0,1,22,70,140,178,179,180,181,182,191,192,256]
paths=[]
for i in range(1024):
 current=[rng.choice(values) for _ in range(rng.randrange(0,9))]
 reference=[rng.choice(values) for _ in range(rng.randrange(1,9))]
 if i%2==0:
  common=[rng.choice(values) for _ in range(rng.randrange(1,5))]
  current=common+current;reference=common+reference
 if i==0:current=list(range(100));reference=[500]
 paths.append(dict(current=current,reference=reference,expected=tail(current,reference)))
costs=[-2147483648,-0x4000000,-100,-1,0,1,100,0x3ffffff,0x4000000,2147483647];wrap=lambda v:ctypes.c_int32(v).value
cases=[]
for i in range(512):
 count=rng.choice([2,3,16]);anchorNode=rng.randrange(200,500);span=rng.randrange(-128,128);ancestor=anchorNode-span-1
 slots=bytearray(rng.randbytes(256));struct.pack_into('<i',slots,0x58,0);struct.pack_into('<H',slots,0x1e,0);struct.pack_into('<H',slots,0x3e,3);struct.pack_into('<ii',slots,0x10,rng.choice(costs),rng.choice(costs))
 record=[bytearray(rng.randbytes(128)) for _ in range(4)]
 for data in record:struct.pack_into('<i',data,0x58,0)
 struct.pack_into('<i',record[0],0x38,anchorNode);struct.pack_into('<H',record[0],0x3c,1)
 record[1][0x20]=span&255;struct.pack_into('<H',record[1],0x1e,2)
 while True:
  common=[rng.choice(values)] if i%2 else []
  current=common+[rng.choice(values) for _ in range(rng.randrange(0,7))]
  reference=common+[rng.choice(values) for _ in range(rng.randrange(1,7))]
  projection=tail(current,reference)
  if projection is not None:break
 raw=[rng.choice(values) for _ in range(rng.randrange(0,6))];suffix=[rng.choice(values) for _ in range(rng.randrange(0,5))]
 pools=[bytearray(rng.randbytes(count*40)) for _ in range(4)];prefixes=[]
 for k in range(count):
  struct.pack_into('<ii',pools[3],k*40,rng.choice(costs),rng.choice(costs))
  prefixes.append([rng.choice(values) for _ in range(rng.randrange(0,5))])
 previous=bytearray(rng.randbytes(count*40*2));expected=bytearray(previous);writes=[]
 delta=wrap(struct.unpack_from('<i',slots,0x10)[0]-struct.unpack_from('<i',record[3],0x10)[0]);inc=wrap(struct.unpack_from('<i',slots,0x14)[0]-struct.unpack_from('<i',record[3],0x14)[0])
 for k in range(count):
  at=k*40;c=struct.unpack_from('<i',pools[3],at)[0]
  if c>0x3ffffff:
   for later in range(k,count):struct.pack_into('<i',expected,later*40,0x4000000)
   break
  struct.pack_into('<ii',expected,at,wrap(c+delta),wrap(struct.unpack_from('<i',pools[3],at+4)[0]+inc))
  expected[at+8:at+24]=bytes(16);struct.pack_into('<HH',expected,at+24,0,0x00ff);expected[at+28:at+31]=bytes([255,255,0]);expected[at+31]=(expected[at+31]&224)|(slots[0x21]&31);struct.pack_into('<H',expected,at+32,3);expected[at+34]=k&255
  # Retained keys are direct literal index insertions, without six-control folds.
  mapped=prefixes[k]+list(reversed(projection['keys']));mapped=compose(mapped,suffix,True)[0] if suffix else mapped
  resultRaw=compose(raw,suffix,False)[0] if suffix else raw
  writes.append(dict(index=k,mapped=mapped,raw=resultRaw))
 cases.append(dict(count=count,anchorNode=anchorNode,ancestor=ancestor,slots=slots.hex(),records=[r.hex() for r in record],pools=[p.hex() for p in pools],current=current,reference=reference,raw=raw,suffix=suffix,prefixes=prefixes,previous=previous.hex(),expected=expected.hex(),writes=writes))
with open(sys.argv[1],'w',encoding='utf-8') as out:json.dump(dict(paths=paths,cases=cases),out,separators=(',',':'))
print('generated1024 reference-pair controls /512 complete anchored pools')
