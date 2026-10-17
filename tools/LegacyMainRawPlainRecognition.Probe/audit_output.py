"""Independent complete raw-pipeline WORD links/source spans. No text labels."""
import json,struct,sys
from pathlib import Path
d=json.loads(Path(sys.argv[1]).read_bytes())
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
u32=lambda b,o:struct.unpack_from('<I',b,o)[0]
u64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
signed=lambda n,w:(n^(1<<(w-1)))-(1<<(w-1))
words=0
for sample in d['inputs']:
 if sample.get('multipleLinesRefused'):
  assert sample['input']==1
  continue
 assert sample['source720']>0 and sample['firstWords']>0
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
assert words==d['words']==5 and d['completed']==5 and d['rejectedLines']==1 and d['guards']==30
print('PASS independent5 raw-pipeline WORD chains/flat order/source graph spans/packet overrides/count-capacity/flags; no accuracy claim')
