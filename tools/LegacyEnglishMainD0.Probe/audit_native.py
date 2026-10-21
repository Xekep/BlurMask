"""Portable read-only PE byte audit. Does not load or invoke vendor binaries."""
import argparse,hashlib,json,struct,csv
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('dll',type=Path);a=p.parse_args();root=Path(__file__).resolve().parent
pins=json.loads((root/'native-regions-and-leaves.json').read_text(encoding='utf-8-sig'));d=a.dll.read_bytes()
assert hashlib.sha256(d).hexdigest()==pins['sdkSha256']
pe=struct.unpack_from('<I',d,0x3c)[0];assert d[pe:pe+4]==b'PE\0\0'
n=struct.unpack_from('<H',d,pe+6)[0];optional=pe+24;size=struct.unpack_from('<H',d,pe+20)[0]
assert struct.unpack_from('<H',d,optional)[0]==0x20b
base=struct.unpack_from('<Q',d,optional+24)[0];sections=[]
for i in range(n):
 at=optional+size+i*40;virtual,rva,rawsize,raw=struct.unpack_from('<IIII',d,at+8);sections.append((rva,rawsize,raw))
def bytes_at(address,count):
 rva=int(address,16)-base
 for start,size,raw in sections:
  if start<=rva and count<=size-(rva-start):return d[raw+rva-start:raw+rva-start+count]
 raise AssertionError('unmapped '+address)
for r in pins['regions']+pins['leaves']:
 assert hashlib.sha256(bytes_at(r['start'],r['size'])).hexdigest()==r['sha256'],r
for r in pins['bindings']:
 raw=bytes_at(r['cell'],8);assert hashlib.sha256(raw).hexdigest()==r['sha256'];assert struct.unpack('<Q',raw)[0]==int(r['target'],16)
rows=list(csv.DictReader((root/'bridge-body-status.tsv').open(encoding='utf-8-sig'),delimiter='\t'))
full=[r for r in rows if r['start']!='UNREGISTERED'];assert len(full)==len(pins['regions'])
for a,b in zip(full,pins['regions']):
 assert int(a['start'],16)==int(b['start'],16) and int(a['byteCount'])==b['size'] and int(a['endInclusive'],16)+1==int(b['start'],16)+b['size']
print(json.dumps(dict(pass_=True,completeRegisteredFunctions=len(set(r['function']for r in full)),completeBodyRegions=len(full),boundedUnregisteredWindows=len(pins['leaves']),actualVirtualBindings=len(pins['bindings']),nativeExecuted=False)))
