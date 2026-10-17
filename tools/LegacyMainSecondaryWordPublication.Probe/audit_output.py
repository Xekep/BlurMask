"""Independent21FE30 packet/reset and275 linked-word publication byte oracle.
Actual glyph rows are inputs from separately checked216BC0/2173E0 producers;
this composition does not assert a transcription or reimplement their NN.
"""
import json,struct,sys
from pathlib import Path
ak=Path(sys.argv[1]).read_bytes();d=json.loads(Path(sys.argv[2]).read_bytes())
import importlib.util
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('actual_alphabet',ROOT/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
alphabet=module.read_alphabet(ak)
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
u64=lambda b,o:struct.unpack_from('<Q',b,o)[0]
def reset(b):
 b=bytearray(b)
 for off,value,fmt in [(8,2**64-1,'Q'),(16,0x3f800000,'I'),(20,0x3f800000,'I'),(24,0,'I'),(28,0,'I'),
  (40,0,'I'),(48,65534,'I'),(53,0,'H'),(55,0,'B'),(60,2**32-1,'I'),(84,2**32-1,'I'),(96,0,'I'),(112,0,'I')]:struct.pack_into('<'+fmt,b,off,value)
 return b
fields=[(0,16,4),(4,24,4),(8,28,4),(12,48,2),(14,50,2),(16,53,1),(17,54,1),(20,60,4),
 (28,68,4),(32,72,4),(36,76,4),(40,80,4),(44,55,1)]
words=callbacks=total=0
for case in d['cases']:
 assert case['ac']==2
 prior=bytes.fromhex(case['initial'])
 for part in case['flushes']:
  callbacks+=1;packet=bytes.fromhex(part['packet']);expected=bytearray(prior)
  for a,b,n in fields:expected[b:b+n]=packet[a:a+n]
  before=bytes.fromhex(part['before']);after=bytes.fromhex(part['after'])
  assert before==expected and after==reset(before) and part['succeeded']
  pool={}
  for node in part['pool']:
   raw=bytes.fromhex(node['records']);assert len(raw)%40==0
   for i in range(len(raw)//40):pool[node['key']|(i<<40)]=raw[i*40:(i+1)*40]
  if part['word'] is not None:
   words+=1;word=bytes.fromhex(part['word']);head=u64(word,8);chain=[]
   while head!=2**64-1:
    assert head in pool and head not in chain;chain.append(head);head=u64(pool[head],32)
   assert list(reversed(chain))==part['references'] and chain
   flags=u16(before,50);span=0;capacity=0
   for n,reference in enumerate(chain,1):
    row=pool[reference];flags|=u16(row,6)
    kind=u16(row,4)&7;span+=((u16(row,8)>>2)&8191) if kind in (1,3) else 1
    if capacity<n:capacity=max(n,int(capacity*1.5))
   assert span>0
   expectedWord=bytearray(before);struct.pack_into('<Q',expectedWord,8,u64(word,8));struct.pack_into('<Q',expectedWord,32,0)
   struct.pack_into('<II',expectedWord,40,len(chain),capacity);struct.pack_into('<H',expectedWord,50,flags&~1024)
   assert word==expectedWord
   #Only actual ordinary kind0/2 descriptors occur in these two paths.
   text=b''
   for reference in part['references']:
    glyph=pool[reference];assert u16(glyph,4)&7 in (0,2)
    text+=alphabet[u16(glyph,8)].encode('utf8')
   assert bytes.fromhex(part['rawUtf8'])==text
   first,last=struct.unpack_from('<ii',packet,48)
   if first!=-1:assert part['span'][0]==first
   if last!=-1:assert part['span'][1]==last
  else:
   rows=bytes.fromhex(part['rows']);assert len(rows)==72
   key=u64(rows,48);matches=[r for ref,r in pool.items() if ref&((1<<56)|((1<<40)-1))==key]
   assert matches and all((u16(r,4)&7) in (1,3) and (u16(r,8)>>2)&8191==0 for r in matches)
  total+=len(before)+len(after)+(120 if part['word'] is not None else 0);prior=after
assert words==d['words'] and words==64 and callbacks==d['flushes'] and callbacks==128 and d['guards']==2*callbacks+4
print(f'PASS independent{callbacks} packet/reset callbacks/{words} complete275 words/{total} WORD bytes, pool links/flat refs/span overrides/zero-span suppression; actual CASE fixture, no expected text')

for a,b in zip(d['cases'][::2],d['cases'][1::2]):
 assert a['b0']==b['b0'] and a['identity']!=b['identity'] and a['selected']!=b['selected']
 wa=[p for p in a['flushes'] if p['word'] is not None]
 wb=[p for p in b['flushes'] if p['word'] is not None]
 assert len(wa)==len(wb)==1 and wa[0]['references']!=wb[0]['references']
 if wa[0]['span']==wb[0]['span']:assert wa[0]['wordIndex']!=wb[0]['wordIndex']
print('PASS32 sameTOP pairs: retained firstB0, distinct actual mapped identities/compact indices/pooledWORD references and raw AK UTF8.')
