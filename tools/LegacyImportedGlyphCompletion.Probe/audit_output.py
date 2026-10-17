"""Independent raw AK -> imported2A -> pre-canonical WORD bytes; no target text."""
from pathlib import Path
import base64,hashlib,importlib.util,json,struct,sys
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=4:raise SystemExit('AK capture.json circled-payload-model.json')
ak=Path(sys.argv[1]).read_bytes();capture=json.loads(Path(sys.argv[2]).read_bytes())
assert hashlib.sha256(ak).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
spec=importlib.util.spec_from_file_location('raw',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py');raw=importlib.util.module_from_spec(spec);spec.loader.exec_module(raw)
alphabet=raw.read_alphabet(ak);count=struct.unpack_from('<H',ak,113)[0];unit=struct.unpack_from('<H',ak,105)[0]
assert count==192 and unit==178
D=lambda v:base64.b64decode(v,validate=True)
U=lambda b,p:struct.unpack_from('<H',b,p)[0]
reserved={65515+i:chr(0xf0018-i).encode() for i in range(9)}
reserved.update({65526:chr(0xf0000).encode(),65529:b'\n',65531:b' ',65532:b'\xc2\xa0'})
def fallback(label):return reserved.get(label,b'\xef\xbf\xbd')
def original_text(label):
 if label==unit:return b'\xc2'
 if label<count:return alphabet.get(label).encode() if label in alphabet else None
 return fallback(label)
def pool_nodes(items):return {x['key']:bytearray(D(x['records'])) for x in items}
def import_original(nodes):
 result={k:bytearray(b) for k,b in nodes.items()};symbols=[];ids={};success=True
 def insert(v):
  if v not in ids:ids[v]=len(symbols);symbols.append(v)
  return ids[v]
 for key in sorted(result,key=lambda k:k if k<2**63 else k-2**64):
  rows=result[key]
  for p in range(0,len(rows),40):
   flags,label=U(rows,p+4),U(rows,p+8);kind=flags&7
   if kind==0:
    text=original_text(label)
    if text is None:success=False;break
    struct.pack_into('<H',rows,p+8,insert(text));struct.pack_into('<H',rows,p+4,(flags&65530)|2)
   elif kind==1:
    if label&32764:
     sub=label&3
     if sub==1:success=False;break
     insert({0:b'\n',2:b' ',3:b'\xc2\xa0'}[sub])
    struct.pack_into('<H',rows,p+4,(flags&65531)|3)
  if not success:break
 return result,symbols,success
def valid(b):
 try:b.decode('utf-8','strict');return True
 except UnicodeDecodeError:return False
def expected(row,symbols):
 kind,label=U(row,4)&7,U(row,8);n=((label>>2)&8191) if kind in (1,3) else 1
 if n==0:return b'',0,False
 if kind==4:return None
 if kind==1:label=((label&3)-7)&65535
 elif kind==3:
  text={0:b'\n',2:b' ',3:b'\xc2\xa0'}.get(label&3)
  assert text is not None
  label=symbols.index(text) if text in symbols else 65535
 elif kind not in (0,2):label=65535
 value=symbols[label] if label<len(symbols) else fallback(label)
 return value*n,n,value==b'\xc2'
glyphs=words=raw_units=circles=0
def check_glyph(g,symbols):
 global glyphs,raw_units,circles
 row=D(g['record']);assert len(row)==40;e=expected(row,symbols);assert e is not None
 b,n,u=e;assert D(g['bytes'])==b and g['tokens']==n and g['valid']==valid(b) and g['unit']==u
 glyphs+=1;raw_units+=u;circles+=chr(0xf0000).encode() in b
 return b
for sample in capture['prepared']:
 before={7:bytearray(D(sample['original']))};after,symbols,success=import_original(before)
 assert pool_nodes(sample['nodes'])==after and list(map(D,sample['symbols']))==symbols and sample['succeeded']==success
 if success:
  e=expected(after[7],symbols)
  if e is None:assert sample['refusedAlternate']
  else:assert D(sample['record'])==bytes(after[7]);check_glyph(sample,symbols)
for sample in capture['actual']:
 after,symbols,success=import_original(pool_nodes(sample['originalNodes']))
 assert success and sample['succeeded'] and pool_nodes(sample['nodes'])==after and list(map(D,sample['symbols']))==symbols
 for word in sample['words']:
  data=bytearray();u=False
  for g in word['glyphs']:
   ref=g['reference'];key=(ref&0xffffff)|(ref&0xffff000000)|(ref&(1<<56));index=(ref>>40)&65535
   assert D(g['record'])==after[key][index*40:index*40+40]
   data.extend(check_glyph(g,symbols));u|=g['unit']
  assert D(word['bytes'])==data and word['valid']==valid(data) and word['unit']==u;words+=1
assert len(capture['prepared'])==320 and len(capture['actual'])==8 and sum(s['nn'] for s in capture['actual'])==631
assert raw_units>=1 and circles>=1
import unicodedata
payload=json.loads(Path(sys.argv[3]).read_bytes());assert payload['tableSha256']=='a162c735d60ca0c0d0581cc40a4e3882e1de805de205fb18cfa8d50f983e45cc'
lookup={bytes.fromhex(text).decode('utf-8'):chr(cp) for text,cp in payload['literalPayloads']}
#Independent official Unicode circle decompositions for native ASCII arithmetic.
for cp in list(range(0x2460,0x2500))+list(range(0x3251,0x3260))+list(range(0x32b1,0x32c0)):
 d=unicodedata.decomposition(chr(cp)).split()
 if d and d[0]=='<circle>':
  text=''.join(chr(int(c,16)) for c in d[1:])
  if text.isascii() and (text.isalpha() and len(text)==1 or text.isdecimal() and 0<=int(text)<=50):lookup[text]=chr(cp)
start,end=chr(0xf0000),chr(0xf0001)
def complete(original):
 text=original.decode('utf-8','strict')
 #Stable known characters in this joining corpus; native037E/0387 and gap-CCC
 #cases are separately covered by the existing helper's full oracle.
 assert '\u037e' not in text and '\u0387' not in text
 scratch=unicodedata.normalize('NFC',text.removeprefix('\ufeff')).replace('\ufffe','\x1a').replace('\uffff','\x1a')
 cursor=0;copied=0;out='';changed=False
 while True:
  begin=scratch.find(start,cursor)
  if begin<0:break
  finish=scratch.find(end,begin+1)
  if finish<0:break
  target=lookup.get(scratch[begin+1:finish])
  if target is not None:out+=scratch[copied:begin]+target;copied=finish+1;changed=True
  cursor=finish+1
 return ((out+scratch[copied:]).encode('utf-8') if changed else original),changed
completed_count=changed_count=refused=0
def check_completion(g):
 global completed_count,changed_count,refused
 if not g['valid']:assert g['refusedCodec'];refused+=1;return None
 expected,changed=complete(D(g['bytes']));assert D(g['completed'])==expected and g['changed']==changed,(g,expected,changed)
 completed_count+=1;changed_count+=changed;return expected
for sample in capture['prepared']:
 if 'record' in sample:check_completion(sample)
for sample in capture['actual']:
 for word in sample['words']:
  data=b''.join(check_completion(g) for g in word['glyphs']);assert D(word['completed'])==data and word['changed']==any(g['changed'] for g in word['glyphs'])
  assert D(word['completed'])==D(word['bytes']) and not word['changed'] #No positive actual circled winner invented.
discriminators=0
for group in capture['grouped']:
 assert list(map(D,group['parts']))==[D(g['bytes']) for g in group['glyphs']]
 data=b''.join(check_completion(g) for g in group['glyphs']);assert D(group['completed'])==data
 whole,_=complete(b''.join(map(D,group['parts'])))
 discriminators+=whole!=data
assert len(capture['grouped'])==276 and discriminators>=4 and refused==1 and changed_count>=230
b=capture['completedBoundaries'];assert b['aggregateBytes']==8_388_608 and b['acceptedWords']==64 and b['rejectedWords']==65
assert b['sha256']==hashlib.sha256(b'x'*8_388_608).hexdigest() and b['acceptedReferences']==65536 and b['rejectedReferences']==65537
expanded_input=('\u0958'*1024+start+'A'+end).encode();expanded,changed=complete(expanded_input)
assert changed and b['expandedRawPerGlyph']==len(expanded_input)==3081 and b['expandedCompletedPerGlyph']==len(expanded)==6147
assert b['acceptedExpandedWords']==42 and b['rejectedExpandedWords']==43
assert len(expanded_input)*43*32<8_388_608 and len(expanded)*42*32<=8_388_608<len(expanded)*43*32
assert hashlib.sha256(expanded*(42*32)).hexdigest()==b['expandedSha256']
print(f'PASS {completed_count}per-glyph completions/{changed_count}changed/{refused}C2refusal/{discriminators}wholeWORD discriminators; raw/import/8actualWORDs unchanged.')
