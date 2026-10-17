"""Independent raw-AK / byte-interner / full40-byte import oracle. No target text."""
import base64,hashlib,importlib.util,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=3:raise SystemExit('AK capture.json')
ak=Path(sys.argv[1]).read_bytes();capture=json.loads(Path(sys.argv[2]).read_bytes())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec=importlib.util.spec_from_file_location('raw',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py');raw=importlib.util.module_from_spec(spec);spec.loader.exec_module(raw)
alphabet=raw.read_alphabet(ak);label_count=struct.unpack_from('<H',ak,113)[0];unit=struct.unpack_from('<H',ak,105)[0]
assert label_count==192 and unit==178
D=lambda x:base64.b64decode(x,validate=True)
class Alphabet:
 def __init__(self,stage=None,occurrence=1,residue=0):
  self.entries={};self.labels=[];self.capacity=0;self.root=False;self.middle=set();self.buckets=set();self.invalidations=0;self.special=[65535]*3;self.stage=stage;self.occurrence=occurrence;self.hits=0;self.residue=residue;self.allocations=[]
 def allocate(self,stage,n):
  self.allocations.append([stage,n])
  if stage==self.stage:
   self.hits+=1
   if self.hits==self.occurrence:return False
  return True
 def insert(self,b):
  if b in self.entries:return self.entries[b]
  h=5381
  for value in b:h=((33*h)^value)%(2**32)
  high=(h>>16)&15;prefix=(high,((h>>6)&31))
  def fail():self.invalidations+=bool(self.residue);return 65535
  if not self.root:
   if not self.allocate('HashRoot',128):return fail()
   self.root=True
  if high not in self.middle:
   if not self.allocate('HashMiddle',256):return fail()
   self.middle.add(high)
  if prefix not in self.buckets:
   if not self.allocate('HashBucket',512):return fail()
   self.buckets.add(prefix)
  if not self.allocate('Leaf',len(b)+20):return fail()
  self.entries[b]=65535
  required=len(self.labels)+1
  if required>self.capacity:
   capacity=((required+15)//16)*16
   if self.allocate('SymbolVector',capacity*8):self.capacity=capacity
   else:required=0
  if required:
   self.entries[b]=len(self.labels);self.labels.append(b)
  for i,v in enumerate([b'\n',b' ',b'\xc2\xa0']):
   if b==v:self.special[i]=self.entries[b]
  self.invalidations+=1
  return self.entries[b]
 def state(self):return [len(self.labels),self.capacity,*self.special,int(self.root),len(self.middle),len(self.buckets),len(self.entries),sum(map(len,self.entries)),self.invalidations]
for t in capture['traces']+capture['faults']:
 a=Alphabet(t.get('stage'),t.get('occurrence',1),t.get('residue',0))
 for op in t['operations']:
  assert a.insert(D(op['bytes']))==op['id'];assert a.state()==op['state'],(t.get('seed'),t.get('stage'),op,a.state())
 assert a.labels==list(map(D,t['symbols']))
 if 'allocations' in t:assert t['allocations']==a.allocations
collision=capture['collision'];first,second=D(collision['first']),D(collision['second']);assert first!=second
keys=[]
for value in [first,second]:
 h=5381
 for c in value:h=((33*h)^c)%(2**32)
 keys.append((h&2047)|(((h>>16)&15)<<11))
assert keys==[collision['bucket']]*2
state=Alphabet();assert state.insert(first)==0 and state.insert(second)==1
assert collision['state']==state.state() and list(map(D,collision['symbols']))==[first,second]
assert capture['capacity']['state'][:2]==[65520,65520]
assert capture['inFlightLeaf']['observedBeforeVectorResult']==0
inflight=Alphabet('SymbolVector');inflight.insert(b'x');assert capture['inFlightLeaf']['state']==inflight.state()
reserved={65515+i:chr(0xf0018-i).encode() for i in range(9)};reserved.update({65526:chr(0xf0000).encode(),65529:b'\n',65531:b' ',65532:b'\xc2\xa0'})
def u16(b,p):return struct.unpack_from('<H',b,p)[0]
def nodes(items):return {x['key']:[x['packed'],bytearray(D(x['records']))] for x in items}
def convert(sample):
 source=nodes(sample['before']);expected={key:[packed,bytearray(data)] for key,(packed,data) in source.items()};a=Alphabet(sample.get('faultStage'));processed=changed=0;failure=None
 for key in sorted(expected,key=lambda k:k if k<2**63 else k-2**64):
  packed,data=expected[key];count=((packed<<2)&65535);count=(count-65536 if count>=32768 else count)>>4;assert count>=0 and len(data)==count*40
  for i in range(count):
   pos=i*40;flags=u16(data,pos+4);label=u16(data,pos+8);kind=flags&7;newflags=flags;newlabel=label
   if kind==0:
    text=b'\xc2' if label==unit else (alphabet.get(label).encode() if label<label_count and label in alphabet else None) if label<label_count else reserved.get(label,b'\xef\xbf\xbd')
    if text is None:failure=key|(i<<40);break
    newlabel=a.insert(text)
    if newlabel==65535:failure=key|(i<<40);break
    newflags=(flags&65530)|2
   elif kind==1:
    if label&32764:
     sub=label&3
     if sub==1:failure=key|(i<<40);break
     text={0:b'\n',2:b' ',3:b'\xc2\xa0'}[sub]
     if a.insert(text)==65535:failure=key|(i<<40);break
    newflags=(flags&65531)|3
   if newflags!=flags or newlabel!=label:
    struct.pack_into('<H',data,pos+4,newflags);struct.pack_into('<H',data,pos+8,newlabel);changed+=1
   processed+=1
  if failure is not None:break
 assert sample['succeeded']==(failure is None) and sample['stopped']==(failure is not None)
 assert sample['processed']==processed and sample['changed']==changed and sample['failureReference']==failure
 assert sample['state']==a.state() and list(map(D,sample['symbols']))==a.labels
 actual=nodes(sample['after']);assert actual==expected
 return processed,changed,len(a.labels)
checks=changes=symbols=0
for s in capture['preparedImports']:
 p,c,n=convert(s);checks+=p;changes+=c;symbols+=n
for s in capture['actual']:
 p,c,n=convert(s);assert s['succeeded'];checks+=p;changes+=c;symbols+=n
 records=nodes(s['after'])
 for word in s['words']:
  b=D(word['storage120']);assert len(b)==120 and struct.unpack_from('<i',b,96)[0]==struct.unpack_from('<i',b,112)[0]==0
  for ref in word['references']:
   key=(ref&0xffffff)|(ref&0xffff000000)|(ref&(1<<56));ordinal=(ref>>40)&65535;assert key in records and ordinal<len(records[key][1])//40
for sample in capture['canceledImports']:
 assert not sample['succeeded'] and sample['stopped'] and sample['processed']==sample['changed']==0
 assert nodes(sample['before'])==nodes(sample['after'])
 source=nodes(sample['before']);b=next(iter(source.values()))[1];label=u16(b,8);value=alphabet[label].encode();assert list(map(D,sample['symbols']))==[value]
 state=Alphabet();state.insert(value);assert sample['state']==state.state()
assert len(capture['canceledImports'])==4
assert len(capture['traces'])==256 and len(capture['faults'])==18 and len(capture['preparedImports'])==711 and len(capture['actual'])==8
assert sum(s['nn'] for s in capture['actual'])==631
print(f'PASS independent256byte traces/18failure prefixes/711preparedimports/8actual all-pools/{checks}records/{changes}changed/{symbols}control symbols; full40bytes/IDs/capacity/cache/specialslots, no target text.')
