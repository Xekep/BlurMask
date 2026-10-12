"""Independent full-byte higher-arc reset branch and scalar predicate controls."""
import ast,ctypes,json,random,struct,sys
from pathlib import Path
syntax=ast.parse(Path('tools/LegacyWordContextAlternatives.Probe/reference_controls.py').read_text(encoding='utf-8-sig'))
selected=[n for n in syntax.body if isinstance(n,ast.FunctionDef) and n.name=='compose' or isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='actions' for t in n.targets)]
namespace={};exec(compile(ast.Module(body=selected,type_ignores=[]),'<context-reference>','exec'),namespace);compose=namespace['compose']
rng=random.Random(0x20ed);values=list(range(0xfff5,0x10000))+[0,1,22,70,140,178,179,180,181,182,191,192,256];costs=[-2147483648,-0x4000000,-1,0,1,0x3ffffff,0x4000000,2147483647];wrap=lambda v:ctypes.c_int32(v).value
cases=[]
for i in range(512):
 pc=rng.choice([1,2,3,16]);sc=rng.choice([1,2,3,16]);node=rng.randrange(128,512);span=rng.randrange(-128,128)
 if pc==sc==1:pc=2
 slots=bytearray(rng.randbytes(256));slots[0x20]=span&255;struct.pack_into('<i',slots,0x58,0);struct.pack_into('<H',slots,0x1e,3);struct.pack_into('<H',slots,0x24,1);struct.pack_into('<ii',slots,0x10,rng.choice(costs),rng.choice(costs))
 parent=bytearray(rng.randbytes(128));struct.pack_into('<i',parent,0x58,0)
 pp=bytearray(rng.randbytes(pc*40*2));sp=bytearray(rng.randbytes(sc*40*2));ep=bytearray(pp);es=bytearray(sp);parentPool=bytearray(rng.randbytes(sc*40))
 suffix=[rng.choice(values) for _ in range(rng.randrange(0,6))];prefixes=[[rng.choice(values) for _ in range(rng.randrange(0,5))] for _ in range(2)]
 resetSource=slots[4] if i%2 else rng.getrandbits(32);header=(rng.getrandbits(32)&~28)|rng.choice([8,12,16,20,24,28]);written=[]
 for count,dst,primary in ((pc,ep,True),(sc,es,False)):
  if count==1:written.append(None);continue
  struct.pack_into('<H',dst,24,3);dst[26]=span&255;dst[27]&=128;dst[30]=slots[0x26];dst[31]=(dst[31]&(192 if primary else 224))|(slots[0x21]&31)
  if primary:
   struct.pack_into('<ii',dst,0,wrap(struct.unpack_from('<i',slots,0x10)[0]-struct.unpack_from('<i',parent,0x10)[0]),wrap(struct.unpack_from('<i',slots,0x14)[0]-struct.unpack_from('<i',parent,0x14)[0]))
   dst[31]=(dst[31]&~32)|((slots[5]&1)<<5 if slots[4]==resetSource else 32)
   ids=[compose([],suffix,True)[0],compose([],suffix,False)[0]]
  else:
   dst[:8]=slots[0x10:0x18];ids=[compose(prefix,suffix,False)[0] for prefix in prefixes]
  dst[8:24]=bytes(16);written.append(ids)
  for k in range(1,count):struct.pack_into('<ii',dst,k*40,0x4000000,0x4000000);dst[k*40+30]=0
 cases.append(dict(pc=pc,sc=sc,node=node,expectedNode=node-span-1,slots=slots.hex(),parent=parent.hex(),parentPool=parentPool.hex(),primary=pp.hex(),secondary=sp.hex(),expectedP=ep.hex(),expectedS=es.hex(),suffix=suffix,prefixes=prefixes,written=written,resetSource=resetSource,header=header))
predicates=[]
for mode in (0,1):
 for bits in range(256):
  for equal in (False,True):predicates.append(dict(mode=mode,header=bits|0xffaa0000,label=5,special=5 if equal else 6,result=mode==0 and (bits&28)>7 and not equal))
with open(sys.argv[1],'w',encoding='utf-8') as out:json.dump(dict(cases=cases,predicates=predicates),out,separators=(',',':'))
print('generated512 complete higher-arc reset pools /1024 header-mode-label predicates')
