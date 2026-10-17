"""Independent complete raw-pipeline WORD links/source spans. No text labels."""
import json,struct,sys,hashlib,importlib.util
from pathlib import Path
d=json.loads(Path(sys.argv[1]).read_bytes())
root=Path(__file__).resolve().parents[2]
ak=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec=importlib.util.spec_from_file_location('quote_raw_alphabet',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);alphabet=m.read_alphabet(ak)
labels={text:label for label,text in alphabet.items()}
labelCount=struct.unpack_from('<H',ak,113)[0];headerE=struct.unpack_from('<H',ak,105)[0]
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
u32=lambda b,o:struct.unpack_from('<I',b,o)[0]
u64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
signed=lambda n,w:(n^(1<<(w-1)))-(1<<(w-1))
def readScalar(row,scratch):
 kind=u16(row,4)&7;label=u16(row,8)
 if kind==3:
  if not label&0x7ffc:return
  assert label&3==1,'Nonempty control requires unprovided TOP owner'
  scratch[2]=0;return
 label=((label&3)-7)&65535 if kind==1 else label if kind in (0,2,4) else 65535
 struct.pack_into('<H',scratch,0,label);scratch[2]=int(kind==4)

words=0
for sample in d['inputs']:
 if sample.get('multipleLinesRefused'):
  assert sample['input']==1
  continue
 assert sample['unrelatedWordRefused']
 assert sample['source720']>0 and sample['firstWords']>0
 leading=bytearray.fromhex('c14ea573');trailing=bytearray.fromhex('d25fb684')
 before={g['reference']:bytearray.fromhex(g['recordBeforeQuotes']) for node in sample['words'] for g in node['glyphs']}
 for node in sample['words']:
  chain=list(reversed([g['reference'] for g in node['glyphs']]))
  for key in chain:
   readScalar(before[key],leading);label=u16(leading,0)
   if leading[2]==0 and label==headerE:continue
   if leading[2]==0 and label in (labelCount+1,labelCount+2):
    struct.pack_into('<H',before[key],8,labels["'" if label==labelCount+1 else '"'])
   break
  readScalar(before[chain[-1]],trailing)
  if trailing[2]==0 and u16(trailing,0)==labelCount+1:struct.pack_into('<H',before[chain[-1]],8,labels["'"])
 assert leading.hex().upper()==sample['quoteLeading'] and trailing.hex().upper()==sample['quoteTrailing']
 for node in sample['words']:
  for glyph in node['glyphs']:assert before[glyph['reference']]==bytes.fromhex(glyph['record'])
 graph=sample['graph']
 for node in sample['words']:
  words+=1;word=bytes.fromhex(node['storage']);packet=bytes.fromhex(node['packet'])
  assert len(word)==120 and len(packet)==64
  refs=[g['reference'] for g in node['glyphs']]
  records={g['reference']:bytes.fromhex(g['record']) for g in node['glyphs']}
  assert refs and len(refs)==len(records) and all(len(r)==40 for r in records.values())
  chain=[];current=u64(word,8)
  while current!=(1<<64)-1:
   assert current in records and current not in chain
   chain.append(current);current=u64(records[current],32)
  assert chain==list(reversed(refs)) and u32(word,40)==len(refs)
  capacity=0;flags=0;span=0
  for count,key in enumerate(chain,1):
   r=records[key];flags|=u16(r,6)
   span+=((u16(r,8)>>2)&8191) if u16(r,4)&7 in (1,3) else 1
   if capacity<count:capacity=max(count,int(capacity*1.5))
  assert span>0 and u32(word,44)==capacity
  assert u16(word,50)==(u16(packet,14)|flags)&~1024
  def source(key):return signed(key&0xffffff,24),signed((key>>24)&65535,16)
  firstKey=next((key for key in refs if source(key)[1]>=0),chain[0])
  left,la=source(firstKey);right,ra=source(chain[0])
  first=graph[left][la][0] if la>=0 else left+la
  last=graph[right][ra][1] if ra>=0 else right
  ownerMask=(1<<56)|((1<<40)-1)
  if la<0 and firstKey&ownerMask==chain[0]&ownerMask:first,last=last,first
  firstOverride,lastOverride=struct.unpack_from('<ii',packet,48)
  if firstOverride!=-1:first=firstOverride
  if lastOverride!=-1:last=lastOverride
  assert node['span']==[first,last]
preparedWrites=0
for case in d['preparedQuotes']:
 leading=bytearray.fromhex('c14ea573');trailing=bytearray.fromhex('d25fb684');expected=[]
 for word in case['before']:
  rows=[bytearray.fromhex(value) for value in word]
  for row in rows:
   readScalar(row,leading);label=u16(leading,0)
   if leading[2]==0 and label==headerE:continue
   if leading[2]==0 and label in (labelCount+1,labelCount+2):
    struct.pack_into('<H',row,8,labels["'" if label==labelCount+1 else '"']);preparedWrites+=1
   break
  readScalar(rows[-1],trailing)
  if trailing[2]==0 and u16(trailing,0)==labelCount+1:
   struct.pack_into('<H',rows[-1],8,labels["'"]);preparedWrites+=1
  expected.append([row.hex().upper() for row in rows])
 assert expected==case['after']
 assert leading.hex().upper()==case['leading'] and trailing.hex().upper()==case['trailing']
assert len(d['preparedQuotes'])==8 and preparedWrites>10
assert len(d['preparedTrailingCancellation'])==2
assert {case['published'] for case in d['preparedTrailingCancellation']}=={False,True}
for case in d['preparedTrailingCancellation']:
 assert case['lookups']==2 and case['before']==case['after']
 assert case['leading']=='4A000073' and case['trailing']=='4A000084'
assert words==d['words']==5 and d['completed']==5 and d['rejectedLines']==1 and d['guards']==57
print('PASS independent5 raw-pipeline WORD chains/flat order/source graph spans/packet overrides/count-capacity/flags and actual published quote scratch/record bytes; no accuracy claim')
