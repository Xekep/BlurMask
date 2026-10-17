"""Independent raw AK -> imported2A -> pre-canonical WORD bytes; no target text."""
from pathlib import Path
import base64,hashlib,importlib.util,json,struct,sys
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=3:raise SystemExit('AK capture.json')
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
boundary=capture['aggregateBoundary'];symbol=D(boundary['symbol'])
assert len(symbol)==4096 and boundary['words']==64 and boundary['referencesPerWord']==32 and boundary['refusedWords']==65
repeated=symbol*(boundary['words']*boundary['referencesPerWord'])
assert len(repeated)==boundary['bytes']==8_388_608 and hashlib.sha256(repeated).hexdigest()==boundary['sha256']
assert capture['referenceBoundary']==dict(accepted=65536,refused=65537,bytes=0,glyphs=65536)
print(f'PASS rawAK/2A/full40bytes {glyphs}glyphs/{words}actualWORDs/{raw_units}genuineC2/{circles}rawcircle markers; pre-canonical only.')
