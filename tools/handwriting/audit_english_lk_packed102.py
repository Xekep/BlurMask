"""Portable EN LK packed102 static boundary auditor; no vendor execution.
Optional PE paths are read as bytes. No research/Ghidra/E-path dependency.
"""
from pathlib import Path
import argparse,hashlib,json,struct
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--sdk',type=Path);p.add_argument('--output',type=Path,required=True);a=p.parse_args();sha=lambda b:hashlib.sha256(b).hexdigest()
d=(a.models/'en-US/en_US-lk-text.lite.res').read_bytes();ak=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes()
assert len(d)==3300812 and sha(d)=='9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9'
assert sha(ak)=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
checks=[(0,0x13f,0x102,3300812),(92,0x13d,0x10a,182),(184,0x13b,0x300,2832716),(2572,0x2b4,0x102,1252412),(1252412,0x2d2,0x101,2832716)]
for off,k,v,end in checks:
 magic,size,kind,enc,version=struct.unpack_from('<IIIHH',d,off);assert(magic,kind,enc,version,off+8+size)==(0x101fe90,k,0,v,end)
assert struct.unpack_from('<I',d,88)[0]==21 and struct.unpack_from('<HH',d,200)==(0x301,0x100)
name=bytes(b^b'\x14\x12\x19\x77'[i%4]for i,b in enumerate(d[208:264])).rstrip(b'\0');assert name==b'en_US-lk-text.lite.lex'
assert struct.unpack_from('<III',d,276)==(0,1,0x3f800000) and struct.unpack_from('<IH',d,288)==(0x2b,0x104)
assert struct.unpack_from('<H',d,308)[0]==104 and struct.unpack_from('<I',d,2568)[0]==1
assert struct.unpack_from('<I',d,2600)[0]==struct.unpack_from('<I',d,669161)[0]==0x1000001
assert d[2608:2612]==bytes.fromhex('0801ffff')and d[669161:669176].hex()=='01000001ffffffb32b0a000701ffff'
assert struct.unpack_from('<III',d,2588)==(0,666547,43)and d[669160]==17 and struct.unpack_from('<I',d,1252408)[0]==260074
native=[]
if a.sdk:
 pins=json.loads(Path(__file__).with_name('english-lk-packed102-pins.json').read_bytes())
 for name,proof in pins.items():
  b=(a.sdk/name).read_bytes();assert sha(b)==proof['sha256'];pe=struct.unpack_from('<I',b,60)[0];assert b[pe:pe+4]==b'PE\0\0';optional=pe+24;assert struct.unpack_from('<H',b,optional)[0]==0x20b
  base=struct.unpack_from('<Q',b,optional+24)[0];table=optional+struct.unpack_from('<H',b,pe+20)[0];sections=[]
  for i in range(struct.unpack_from('<H',b,pe+6)[0]):
   q=table+i*40;vs,rva,size,raw=struct.unpack_from('<IIII',b,q+8);sections.append((rva,size,raw))
  def at(va):
   r=va-base
   for rv,size,raw in sections:
    if rv<=r<rv+size:return raw+r-rv
   raise AssertionError(hex(va))
  for parts in proof['regions'].values():
   for region in parts:
    begin=int(region['start'],16);end=int(region['endExclusive'],16);raw=b[at(begin):at(begin)+end-begin];assert len(raw)==region['bytes']and sha(raw)==region['sha256']
  for entry in proof['calls']+proof['tables']:
   q=at(int(entry['at'],16));raw=bytes.fromhex(entry['bytes']);assert b[q:q+len(raw)]==raw
  native.append(dict(binary=name,sha256=sha(b),fullGhidraBodies=len(proof['regions']),regions=sum(map(len,proof['regions'].values())),calls=len(proof['calls']),tables=len(proof['tables'])))
result=dict(status='PASS actual English LK packed102 boundary',lexicalSha256=sha(d),acousticSha256=sha(ak),native=native,nativeExecuted=False,scope='Packed dictionary/root field boundary only; not configured English masks/language owner or recognition')
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n');print('PASS actualEN LK root13F/main13B/AL104/packed102 fields'+(' and full native byte bindings'if native else'; native bytes not requested'))
