"""Independent raw-owner UTF8 and024F30/C21/source metadata composition.

The captured original positions are already independently verified by the
TextChoices source-position probe. No expected word labels enter this audit.
"""
import base64,hashlib,importlib.util,json,struct,sys
from pathlib import Path

root=Path(__file__).resolve().parents[2]
def load(name,path):
 spec=importlib.util.spec_from_file_location(name,root/path)
 module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
alphabet=load('alphabet','tools/LegacyRussianGlyphSymbol.Probe/audit_output.py').read_alphabet(
 (root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes())
ops=load('ranges','tools/LegacyRawInkRangeOperations.Probe/reference_controls.py')
data=json.loads((root/'models/text-unicode/legacy-text-publication-data.json').read_bytes())
literal={bytes.fromhex(r[0]):r[1]for r in data['literalRows']}
def script(cp):return next((v for a,b,v in data['scriptRanges']if a<=cp<=b),0)
b64=base64.b64decode
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
def same(a,b):return a>>16==b>>16 and(a>>8&255==b>>8&255 or a>>8&255==255 or b>>8&255==255)
def positive(reference):return reference>>24&0x8000==0
def skipped(row):return u16(row,4)&7==1 and u16(row,8)&0x7ffc!=0 and u16(row,8)&3==3
f=lambda v:struct.unpack('<f',struct.pack('<f',v))[0]
def position(source,value,mode,count):
 integer=int(value);remainder=f(value-f(integer))
 if remainder==0 and integer==count-1:fraction=255 if mode in(3,5)else 0
 else:
  fraction=int(f(f(remainder*200)+f(.5)))
  if fraction==200:integer+=1;fraction=255 if mode in(3,5)and integer==count-1 else 0
 return(source<<32)|(integer<<16)|(fraction<<8)|mode

def word_projection(data,counts):
 count=len(data)//26;assert len(data)==count*26;projected=[]
 for i in range(count):
  group,first,point,last_group,last,end=struct.unpack_from('<6i',data,i*24)
  assert group==last_group==0
  for source in range(first,last+1):
   start=f(f(point)-f(f(data[count*24+i*2])/f(200)))if source==first else f(0)
   stop=f(f(end)-f(f(data[count*24+i*2+1])/f(200)))if source==last else f(counts[source]-1)
   projected.append([position(source,start,3,counts[source]),position(source,stop,5,counts[source])])
 original_end=position(len(counts)-1,counts[-1]-1,5,counts[-1]);result=[]
 for a,b in projected:
  closed=same(b,original_end)or b>>8&255==255 or (b>>16&65535)==0 and(b>>8&255)==0 or same(a,b)
  result.append([a,(b&~255)|(5 if closed else 1)])
 return result

def classify(raw,flags):
 text=raw.decode('utf-8');assert len(text)==1,'Actual coverage is ordinary single scalar labels'
 cp=ord(text);flags&=1023;sc=script(cp)
 if sc in(7,8,9)or sc!=10 and 0x3000<=cp<=0x303f:flags|=0x40000
 elif sc==10:flags|=0x80000
 special={13:0x2000,10:0x2000,0x2028:0x2000,0x2029:0x2000,
  9:0x1000,11:0x1000,12:0x1000,32:0x1000,160:0x1000,0xad:0x4000,0x200c:0x8000}.get(cp,0)
 if cp in[0x61c,0x200e,0x200f,*range(0x202a,0x202f),*range(0x2066,0x206a)]:special=0x20000
 if special:flags|=special
 elif literal.get(raw,-1)>=0:flags|=0x10000
 if cp in(0xf0000,0xf0001):flags|=0x100000
 return flags

def emission(glyph):
 owner=b64(glyph['owner40']);selection=b64(glyph['selectionRows4']);marker=glyph['initialMarker'];emitted=[]
 for offset in range(0,len(selection),4):
  ordinal=u16(selection,offset);row=owner[ordinal*40:(ordinal+1)*40];assert len(row)==40
  assert u16(row,4)&7==0,'Nonordinary token emitter needs its own independent coverage'
  raw=alphabet[u16(row,8)].encode()
  if not raw:
   if len(emitted)<marker:marker-=1
   continue
  if raw=='\u2639'.encode():raw='\U0001f641'.encode()
  elif raw=='\u263a'.encode():raw='\U0001f642'.encode()
  emitted.append((raw,classify(raw,u16(row,6))))
 return emitted,marker

capture=json.loads(Path(sys.argv[1]).read_bytes());total=alternatives=reused=negative=overlaps=0
for sample in capture['samples']:
 counts=sample['counts'];next_id=len(counts);pending=[];nodes={};strings={}
 original=[[3,((len(counts)-1)<<32)|((counts[-1]-1)<<16)|0xff05]]
 for word in sample['words']:
  assert word['wordClip']==word_projection(b64(word['wordSource26']),counts)
  inputs=word['inputs'];ranges=[];skip=[];ispositive=[];joiner=[]
  for item in inputs:
   row=b64(item['initialGlyph40']);s=skipped(row);p=positive(item['reference'])
   skip.append(s);ispositive.append(p)
   joiner.append(u16(row,4)&7==0 and alphabet[u16(row,8)]=='\u200d')
   key=item.get('originalRanges',[]);ranges.append(ops.intersect(key,word['wordClip'])if p else key)
  assert word['sourceFlags']==(0 if any(p and not s for p,s in zip(ispositive,skip))else 0x200000)
  expected_indices=[i for i,s in enumerate(skip)if not s]
  assert [g['index']for g in word['glyphs']]==expected_indices
  ordinary=[];selected_flags=0
  for glyph in word['glyphs']:
   i=glyph['index'];key=ranges[i];p=ispositive[i]
   if p:
    if len(key)==1 and same(*key[0])and not any(ops.n(a)<=ops.n(key[0][0])<=ops.n(b)and key[0][1]<=b for a,b in original):
     p=False;key=[[(key[0][0]&~255)|4,(key[0][1]&~255)|4]]
    elif joiner[i]:
     end=key[0][1]
     if end>>8&255==255:end&=~0xff00
     end=(end&~255)|4;key=[[end,end]];p=False
    else:
     for later in range(i+1,len(inputs)):
      if skip[later]or not ispositive[later]:continue
      overlap=ops.intersect(ops.union([],ranges[later]),key)
      if not overlap:continue
      overlaps+=1
      if key!=overlap:key=ops.subtract(key,overlap,counts)
      else:ranges[later]=ops.subtract(ranges[later],overlap,counts)
      break
   ranges[i]=key;assert glyph['key']==key and glyph['positive']==p
   negative+=not p
   packed=tuple(tuple(r)for r in key);existing=nodes.get(packed)
   assert glyph['reused']==(existing is not None)
   emitted,marker=emission(glyph)
   if existing is None:
    node=len(nodes)+1;records=[];aggregate=0
    for raw,flags in emitted:
     identity=strings.setdefault(raw,len(strings)+1);records.append((raw,flags,identity));aggregate|=flags
    selection_id=next_id if p else -1
    if p:pending.append(dict(id=next_id,name='CHAR',ranges=key));next_id+=1
    # Fresh raw C4A traversal clears the prior flag; an empty traversal keeps1.
    visited=any(r[0]<=r[3]and r[1]<=r[4]for r in glyph['sourceRefs24'])
    prior=p and not visited
    flags=(((int(prior)<<24)^((0 if p else 0x200000)|(aggregate&0xce1fffff)|
     (0x800000 if p and len(key)==1 and same(*key[0])else 0)))&0xfdffffff)
    existing=dict(node=node,records=records,key=key,selection=selection_id,flags=flags)
    nodes[packed]=existing
   else:
    reused+=1;old_count=len(existing['records']);marker_out=-1
    for index,(raw,flags)in enumerate(emitted):
     newly=raw not in strings;identity=strings.setdefault(raw,len(strings)+1)
     match=next((j for j,r in enumerate(existing['records'][:old_count])if r[2]==identity),None)if not newly else None
     if match is None:
      match=len(existing['records']);existing['records'].append((raw,flags,identity));existing['flags']|=flags
     if index==marker:marker_out=match
    marker=marker_out
   assert glyph['node']==existing['node']and glyph['storedKey']==existing['key']
   assert glyph['selectionId']==existing['selection']and glyph['flags']==existing['flags']
   assert glyph['line']==(sample['line']if ispositive[i]else -1)
   assert len(existing['records'])==len(glyph['records'])
   for (raw,flags,identity),actual in zip(existing['records'],glyph['records']):
    storage=bytearray(40);struct.pack_into('<I',storage,8,flags);struct.pack_into('<I',storage,32,len(raw))
    assert b64(actual['scalar40'])==storage and b64(actual['utf8'])==raw and actual['stringId']==identity
    alternatives+=1
   assert glyph['marker']==marker and b64(glyph['selectedUtf8'])==existing['records'][marker][0]
   selected_flags|=existing['records'][marker][1]
   if p:ordinary=ops.union(ordinary,existing['key'])
   total+=1
  assert word['ordinaryRanges']==ordinary and word['selectedFlags']==selected_flags
 assert sample['nextId']==next_id and sample['pending']==pending
if 'inputAssumption'in capture:
 assert len(capture['samples'])==9 and total==31
else:assert len(capture['samples'])==7 and total==26
assert reused==0 and negative==0
print(f'PASS independent{len(capture["samples"])} raw candidates/{total} processed keys/{alternatives} full40B rows/CHAR metadata/intern ownership; {overlaps} overlap branches. Actual reused/negative keys0; no0283B0 acceptance claim.')
