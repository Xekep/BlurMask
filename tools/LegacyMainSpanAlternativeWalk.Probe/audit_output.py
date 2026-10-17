"""Independent raw compact direct-parent218A30 reconstruction, not219580 eligibility."""
import base64,collections,importlib.util,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
s=importlib.util.spec_from_file_location('alphabet',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py');m=importlib.util.module_from_spec(s);s.loader.exec_module(m)
ak=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes();alphabet=m.read_alphabet(ak)
def label(text):return next((i for i,v in alphabet.items()if v==text),65535)
marker,joiner,closed,unit,soft=label('\3'),label('\u200d'),label('\U000f0001'),struct.unpack_from('<H',ak,105)[0],label('\u00ad')
b64=base64.b64decode;end=(1<<64)-1
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
def ref(b):
 v=struct.unpack_from('<Q',b,8)[0];n=v&0xffffff
 return(n-0x1000000 if n&0x800000 else n,v>>24&65535)
def pack(n,a):return n&0xffffff|(a&65535)<<24
def span(a,b,alt):return a&0xffffff|((b-a)&65535)<<24|int(alt)<<56
stats=collections.Counter();samples=json.loads(Path(sys.argv[1]).read_bytes())
control=Path(sys.argv[1]+'.closed.bin').read_bytes();position=0
for closed_id in[191,65535]:
 for flags in[0,16]:
  for prior in[0,1,0xff00]:
   for sym in range(65536):
    state=(prior&255)if closed_id!=65535 else prior
    if sym==closed_id and closed_id!=65535:state=1
    accepted=True
    if sym==65526:
     if state&255:state=0xff00
     else:accepted=not flags&16
    assert control[position]==accepted and struct.unpack_from('<H',control,position+1)[0]==state
    position+=3
assert position==len(control)==786432*3
for sample in samples:
 compact=b64(sample['compact']);record=lambda i:compact[i*40:(i+1)*40];arcs=sample['arcs'];residue=b64(sample['residue'])
 assert len(compact)==sample['count']*40
 #No flag is patched to create a false actual219580 candidate.
 assert not any(record(i)[32]&8 for i in range(sample['count']))and sample['nativeEligible']==0
 for case in sample['walks']:
  for emit in[False,True]:
   index=case['index'];minimum=case['minimum'];lastnode=-1;parent=i32(record(index),16);lastkey=end;flags=65534 if emit else 65535
   visited=[];rows=[];residual=[];prior=(-1,65535);lastregular=65535;first=True;closed_state=0;initialflags=record(index)[32]
   while True:
    r=record(index);node,arc=ref(r)
    if node==-1 or node<minimum:status=4 if node==-1 else 1;break
    assert index not in visited;visited.append(index);lastnode=node;parent=i32(r,16);direct=(-1,65535)if parent==-1 else ref(record(parent));sym=u16(r,28);type=r[32]&7
    accept=False
    if marker==65535 or sym!=marker:
     if sym==65530 and not rows and node==sample['nodes']-1:stats['finalFffa']+=1
     elif sym in(65531,65529,65525,65530):status=2;stats['protocolStop']+=1;break
     elif sym not in(joiner,65533):
      if closed!=65535:
       closed_state&=255
       if sym==closed:closed_state=1
      accept=True
      if sym==65526:
       if closed_state&255:closed_state=0xff00;stats['closedCarry']+=1
       elif initialflags&16:accept=False;stats['skipFff6']+=1
      elif sym==65535:accept=False
    if accept:
     if sym==unit:
      if emit:residual.append(pack(node,arc))
      stats['unit']+=1
     elif not emit:lastkey=pack(node,arc)
     else:
      assert type<=3
      if type<3:lastregular=sym
      look=parent;earlier=direct
      while look!=-1:
       nxt=record(look)
       if nxt[32]&7<3 and u16(nxt,28)not in(65535,unit):break
       look=i32(nxt,16);earlier=(-1,65535)if look==-1 else ref(record(look))
      normalized=soft if sym==65524 else 65529 if sym in(65525,65527,65528)else sym
      if type<3:key=pack(node,arc);first=False
      elif(normalized+7)&65535<4 or normalized==soft:
       if earlier[0]>=0 and prior[0]>=0:
        a,b=arcs[prior[0]][prior[1]][0],arcs[earlier[0]][earlier[1]][1]
        if a==0xffffffff:a,b=b,arcs[earlier[0]][earlier[1]][0]
        key=span(a,b,False)
       else:
        alt=not first or earlier[0]==-1;selected=(node,arc)if alt else earlier
        a,b=arcs[selected[0]][selected[1]][1],arcs[selected[0]][selected[1]][0];key=span(a,b,alt)
      else:
       alt=not first or earlier[0]==-1;selected=(node,arc)if alt else earlier
       a,b=arcs[selected[0]][selected[1]][1],arcs[selected[0]][selected[1]][0];key=span(a,b,alt)
      out=bytearray(residue)
      for o,v in[(0,node),(8,index),(12,direct[0]),(20,parent),(24,earlier[0]),(32,look),(36,prior[0]),(64,0)]:struct.pack_into('<i',out,o,v)
      for o,v in[(4,arc),(16,direct[1]),(28,earlier[1]),(40,prior[1]),(56,lastregular),(58,normalized)]:struct.pack_into('<H',out,o,v)
      struct.pack_into('<Q',out,48,key);out[60]=type;out[61]=r[32]>>6&1;out[62]=r[33];out[68]=0;out[69]=r[7]>>1&1;rows.append(out)
    if emit and r[4]!=255:flags=r[4]
    prior=node,arc
    if parent==-1:status=4;break
    index=parent
   if emit and flags==65534:flags=65535
   actual=case['emit'if emit else'inspect'];stats['status'+str(status)]+=1
   assert(status,lastnode,parent,lastkey,flags)==(actual['status'],actual['minimum'],actual['parent'],actual['source'],actual['wordFlags']),(case['index'],emit,'scalars')
   assert visited==actual['visited']and residual==actual['residuals']
   assert b''.join(rows)==b64(actual['rows']),(case['index'],emit,'rows72')
   stats['walks']+=1;stats['rows']+=len(rows)
assert len(samples)==7 and stats['walks']>0
print('PASS independent7 actual compact owners/full72B direct-parent diagnostics:',dict(stats),'; native219580 eligible0, no augmentation claim.')
