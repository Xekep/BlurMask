"""Independent20e6a0 primary-parent byte oracle and signed native control predicate."""
import ast,ctypes,json,random,struct,sys
from pathlib import Path
syntax=ast.parse(Path('tools/LegacyWordContextAlternatives.Probe/reference_controls.py').read_text(encoding='utf-8-sig'))
selected=[n for n in syntax.body if isinstance(n,ast.FunctionDef) and n.name=='compose' or isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='actions' for t in n.targets)]
namespace={};exec(compile(ast.Module(body=selected,type_ignores=[]),'<path-oracle>','exec'),namespace);compose=namespace['compose']
reset=lambda symbol:ctypes.c_int16(symbol).value == -5 or ((ctypes.c_int16(symbol).value+11)&0xfffa)==0 and ctypes.c_int16(symbol).value!=-10
rng=random.Random(0x20e6a0);values=list(range(0xfff5,0x10000))+[0,1,22,70,140,178,179,180,181,182,191,192,256]
costs=[-2147483648,-2147483647,-0x4000000,-100,-1,0,1,100,0x3ffffff,0x4000000,2147483647]
wrap=lambda x:ctypes.c_int32(x).value
cases=[]
for case in range(512):
 n=rng.randrange(1,5);pc=rng.choice([1,2,3,16]);sc=rng.choice([1,2,3,16]);node=rng.randrange(128,512)
 if pc==sc==1:pc=2
 slots=bytearray(rng.randbytes((n+1)*128));p=bytearray(rng.randbytes((n+1)*pc*40));s=bytearray(rng.randbytes((n+1)*sc*40));ep=bytearray(p);es=bytearray(s)
 parents=[];prefixes=[];sourceTokens=[]
 for parent in range(3):
  record=bytearray(rng.randbytes(128));struct.pack_into('<i',record,0x58,0)
  sourceTokens.append(rng.choice(values))
  pp=bytearray(rng.randbytes(pc*40));ps=bytearray(rng.randbytes(sc*40));paths=[]
  for count,pool in ((pc,pp),(sc,ps)):
   row=[]
   for k in range(count):
    struct.pack_into('<ii',pool,k*40,rng.choice(costs),rng.choice(costs))
    row.append([[rng.choice(values) for _ in range(rng.randrange(0,4))] for _ in range(2)])
   paths.append(row)
  prefixes.append(paths);parents.append(dict(record=record.hex(),primary=pp.hex(),secondary=ps.hex()))
 suffixes=[];outputs=[];expectedNodes=[];ordinals=list(range(n));rng.shuffle(ordinals)
 if case%8==0 and n>1:ordinals[-1]=ordinals[0]
 for index in range(n):
  at=index*128;parent=rng.randrange(3);ordinal=ordinals[index];span=rng.randrange(-128,128)
  slots[at+0x20]=span&255;struct.pack_into('<i',slots,at+0x58,ordinal);struct.pack_into('<H',slots,at+0x1e,parent)
  struct.pack_into('<H',slots,at+0x18,node if rng.getrandbits(1) else node+1)
  expectedNodes.append(node-span-1)
  struct.pack_into('<ii',slots,at+0x10,rng.choice(costs),rng.choice(costs))
  suffix=[rng.choice(values) for _ in range(rng.randrange(0,5))];suffixes.append(suffix)
  record=bytes.fromhex(parents[parent]['record']);delta=wrap(struct.unpack_from('<i',slots,at+0x10)[0]-struct.unpack_from('<i',record,0x10)[0]);inc=wrap(struct.unpack_from('<i',slots,at+0x14)[0]-struct.unpack_from('<i',record,0x14)[0])
  isreset=reset(sourceTokens[parent]);append=struct.unpack_from('<H',slots,at+0x18)[0]==node or ctypes.c_int8(slots[at+5]).value>=0
  out=[]
  for count,dst,poolIndex,primary in ((pc,ep,0,True),(sc,es,1,False)):
   writes=[];pool=bytes.fromhex(parents[parent]['primary' if primary else 'secondary']);start=ordinal*count*40
   if count==1:out.append(writes);continue
   struct.pack_into('<H',dst,start+24,parent);dst[start+26]=span&255;dst[start+31]=(dst[start+31]&(192 if primary else 224))|(slots[at+0x21]&31);dst[start+27]&=128
   if not primary:dst[start:start+8]=slots[at+0x10:at+0x18];dst[start+30]=slots[at+0x26]
   for k in range(count):
    offset=start+k*40;valid=struct.unpack_from('<i',pool,k*40)[0]<0x4000000
    if primary and isreset:
     if k==0:
      struct.pack_into('<ii',dst,offset,delta,inc);dst[offset+30]=slots[at+0x26];dst[offset+31]|=32
      mapped=compose([],suffix,True)[0];raw=compose([],suffix,False)[0]
     else:
      struct.pack_into('<ii',dst,offset,0x4000000,0x4000000);dst[offset+30]=0;continue
    elif not primary and k==0:
     mapped=prefixes[parent][poolIndex][k][0];raw=prefixes[parent][poolIndex][k][1]
     if isreset or append:
      mapped=compose(mapped,suffix,not isreset)[0];raw=compose(raw,suffix,False)[0]
    elif valid:
     struct.pack_into('<ii',dst,offset,wrap(struct.unpack_from('<i',pool,k*40)[0]+delta),wrap(struct.unpack_from('<i',pool,k*40+4)[0]+inc))
     struct.pack_into('<H',dst,offset+24,parent);dst[offset+26]=span&255;dst[offset+27]=(dst[offset+27]&128)|(k&127)
     dst[offset+30]=slots[at+0x26] if primary else 0;dst[offset+31]=(dst[offset+31]&(192 if primary else 224))|(slots[at+0x21]&31)
     mapped=prefixes[parent][poolIndex][k][0];raw=prefixes[parent][poolIndex][k][1]
     if append:mapped=compose(mapped,suffix,True)[0];raw=compose(raw,suffix,False)[0]
    else:
     struct.pack_into('<ii',dst,offset,0x4000000,0x4000000);dst[offset+30]=0;continue
    dst[offset+8:offset+24]=bytes(16);writes.append(dict(index=k,mapped=mapped,raw=raw))
   out.append(writes)
  outputs.append(out)
 cases.append(dict(n=n,pc=pc,sc=sc,node=node,slots=slots.hex(),primary=p.hex(),secondary=s.hex(),expectedP=ep.hex(),expectedS=es.hex(),parents=parents,prefixes=prefixes,tokens=sourceTokens,suffixes=suffixes,outputs=outputs,expectedNodes=expectedNodes))
with open(sys.argv[1],'w',encoding='utf-8') as out:json.dump(dict(cases=cases,resetTokens=[v for v in range(65536) if reset(v)]),out,separators=(',',':'))
print('generated512 complete primary-parent pool controls /65536 signed-token predicates')
