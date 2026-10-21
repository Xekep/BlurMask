"""Portable English sparse101 field/native binding auditor. SDK is only read.
Full native regions originate from Ghidra getBody (not first unwind fragments).
"""
from pathlib import Path
import argparse,hashlib,json,struct
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--sdk',type=Path);p.add_argument('--output',type=Path,required=True);a=p.parse_args();sha=lambda b:hashlib.sha256(b).hexdigest()
d=(a.models/'en-US/en_US-lk-text.lite.res').read_bytes()
assert len(d)==3300812 and sha(d)=='9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9'
assert struct.unpack_from('<IIIHH',d,1252412)==(0x101fe90,1580296,0x2d2,0,0x101)
assert struct.unpack_from('<HIII',d,1252428)==(0x102,260074,0xffffffff,0xffffffff)
assert struct.unpack_from('<IIIIIII',d,1252514)==(3,7,260075,1024,1024,113643,155484)
native=[]
if a.sdk:
 pins=json.loads(Path(__file__).with_name('english-lk-sparse101-pins.json').read_bytes())
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
  for entry in proof['calls']+proof['tables']+proof['leaves']+proof['boundedCode']:
   q=at(int(entry['at'],16));raw=bytes.fromhex(entry['bytes']);assert b[q:q+len(raw)]==raw
  native.append(dict(binary=name,sha256=sha(b),fullGhidraBodies=len(proof['regions']),regions=sum(map(len,proof['regions'].values())),calls=len(proof['calls']),tables=len(proof['tables']),boundedRetLeaves=len(proof['leaves']),boundedOtherCode=len(proof['boundedCode'])))
result=dict(status='PASS actual sparse English2D2/v101 boundary',lexicalSha256=sha(d),native=native,nativeExecuted=False,scope='Raw field and selected-main fresh numeric binding boundary only; no configured collection, query engine, masks or recognition')
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n');print('PASS English sparse101 fields'+(' and complete native body/binding pins'if native else'; native bytes not requested'))
