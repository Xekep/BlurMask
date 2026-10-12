"""Independent semantic context oracle plus complete211eb0 byte-slice writer."""
import ast, ctypes, json, random, struct, sys
from pathlib import Path

# Reuse the frozen independent functional oracle, not managed implementation.
syntax=ast.parse(Path('tools/LegacyWordContextAlternatives.Probe/reference_controls.py').read_text(encoding='utf-8-sig'))
selected=[n for n in syntax.body if isinstance(n,ast.FunctionDef) and n.name=='compose' or isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='actions' for t in n.targets)]
namespace={};exec(compile(ast.Module(body=selected,type_ignores=[]),'<semantic-path-reference>','exec'),namespace)
compose=namespace['compose']
rng=random.Random(0x211eb0)
values=list(range(0xfff6,0x10000))+[0,1,22,70,140,178,179,180,181,182,191,192,255,256]
costs=[-2147483648,-2147483647,-0x4000000,-100,-1,0,1,100,0x3ffffff,0x4000000,2147483647]
wrap=lambda value:ctypes.c_int32(value).value
cases=[]
for case in range(512):
 n=rng.randrange(1,6);count=rng.choice([1,2,3,16]);node=rng.randrange(0,100)
 slots=bytearray(rng.randbytes((n+2)*128));pool=bytearray(rng.randbytes((n+2)*count*40));expected=bytearray(pool)
 parents=[];parentPrefixes=[];suffixes=[];outMapped=[];outRaw=[];ordinals=list(range(n));rng.shuffle(ordinals)
 if case%8==0 and n>1:ordinals[-1]=ordinals[0]
 for parent in range(3):
  record=bytearray(rng.randbytes(128));struct.pack_into('<i',record,0x58,0)
  alternatives=bytearray(rng.randbytes(count*40));prefixes=[]
  for index in range(count):
   struct.pack_into('<ii',alternatives,index*40,rng.choice(costs),rng.choice(costs))
   prefixes.append([[rng.choice(values) for _ in range(rng.randrange(0,4))] for _ in range(2)])
  parents.append(dict(record=record.hex(),alts=alternatives.hex()));parentPrefixes.append(prefixes)
 for index in range(n):
  at=index*128;parent=rng.randrange(3);ordinal=ordinals[index]
  struct.pack_into('<i',slots,at+0x58,ordinal);struct.pack_into('<H',slots,at+0x1e,parent);struct.pack_into('<H',slots,at+0x3e,0xffff)
  struct.pack_into('<ii',slots,at+0x10,rng.choice(costs),rng.choice(costs))
  suffix=[rng.choice(values) for _ in range(rng.randrange(0,5))];suffixes.append(suffix)
  mapped=[];raw=[];src=bytes.fromhex(parents[parent]['alts']);record=bytes.fromhex(parents[parent]['record'])
  for k in range(count):
   mapped.append(compose(parentPrefixes[parent][k][0],suffix,True)[0] if suffix else parentPrefixes[parent][k][0])
   raw.append(compose(parentPrefixes[parent][k][1],suffix,False)[0] if suffix else parentPrefixes[parent][k][1])
  outMapped.append(mapped);outRaw.append(raw)
  if count==1:continue
  for k in range(count):
   dst=ordinal*count*40+k*40;offset=k*40;c=struct.unpack_from('<i',src,offset)[0]
   if c>0x3ffffff:
    for tail in range(k,count):struct.pack_into('<i',expected,ordinal*count*40+tail*40,0x4000000)
    break
   cost=wrap(struct.unpack_from('<i',slots,at+0x10)[0]-struct.unpack_from('<i',record,0x10)[0]+c)
   inc=wrap(struct.unpack_from('<i',src,offset+4)[0]-struct.unpack_from('<i',record,0x14)[0]+struct.unpack_from('<i',slots,at+0x14)[0])
   struct.pack_into('<ii',expected,dst,cost,inc);expected[dst+8:dst+24]=bytes(16)
   struct.pack_into('<H',expected,dst+24,parent);expected[dst+26]=255;expected[dst+27]=k&127
   expected[dst+28:dst+31]=bytes([255,255,0]);expected[dst+31]=(expected[dst+31]&224)|(slots[at+0x21]&31)
   expected[dst+32:dst+35]=bytes([255,255,255])
 heap=rng.randrange(0,5);sourceindex=rng.choice([-1,0,1,2]);header=rng.getrandbits(8);packed=rng.getrandbits(8)
 prepass=bool(packed&16) if heap==0 else sourceindex>=0 and bool(header&32)
 succeeds=not prepass or bool(rng.getrandbits(1))
 if not succeeds:expected=bytearray(pool)
 cases.append(dict(n=n,count=count,node=node,slots=slots.hex(),pool=pool.hex(),expected=expected.hex(),parents=parents,prefixes=parentPrefixes,suffixes=suffixes,mapped=outMapped,raw=outRaw,heap=heap,sourceindex=sourceindex,header=header,packed=packed,prepass=prepass,succeeds=succeeds))
ordinal=[]
for i in range(256):
 cap=rng.randrange(1,20);n=rng.randrange(0,cap+1);cont=bool(rng.getrandbits(1));first=rng.randrange(0,65536) if cont else rng.getrandbits(32)
 if i%8==0:first=0xffff if cont else 0xffffffff
 storage=bytearray(rng.randbytes(cap*128));expected=bytearray(storage);current=first
 for index in range(n):
  struct.pack_into('<I',expected,index*128+0x58,current);current=(current+1)&(0xffff if cont else 0xffffffff)
 ordinal.append(dict(n=n,cont=cont,first=first,storage=storage.hex(),expected=expected.hex(),next=current))
with open(sys.argv[1],'w',encoding='utf-8') as out:json.dump(dict(cases=cases,ordinals=ordinal),out,separators=(',',':'))
print('generated512 continuation pool controls /256 full-byte ordinal controls')
