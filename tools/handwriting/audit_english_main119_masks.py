"""Portable, static-only EN main119 wire/mask binding auditor; vendor DLLs are read, never loaded."""
from pathlib import Path
import argparse,hashlib,json,struct
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--sdk',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();sha=lambda b:hashlib.sha256(b).hexdigest()
pins=json.loads(Path(__file__).with_name('english-main119-mask-pins.json').read_bytes());results=[]
for name,proof in pins.items():
 b=(a.sdk/name).read_bytes();assert sha(b)==proof['sha256'];pe=struct.unpack_from('<I',b,60)[0];assert b[pe:pe+4]==b'PE\0\0';optional=pe+24;assert struct.unpack_from('<H',b,optional)[0]==0x20b
 base=struct.unpack_from('<Q',b,optional+24)[0];table=optional+struct.unpack_from('<H',b,pe+20)[0];sections=[]
 for i in range(struct.unpack_from('<H',b,pe+6)[0]):
  q=table+i*40;vs,rva,size,raw=struct.unpack_from('<IIII',b,q+8);sections.append((rva,size,raw))
 def raw(va,n):
  for rv,size,offset in sections:
   if rv<=va-base and va-base+n<=rv+size:return b[offset+va-base-rv:offset+va-base-rv+n]
  raise AssertionError((hex(va),n))
 for pieces in proof['regions'].values():
  for r in pieces:
   lo=int(r['start'],16);hi=int(r['endExclusive'],16);assert hi-lo==r['bytes']and sha(raw(lo,hi-lo))==r['sha256']
 for r in proof['calls']+proof['data']+proof['boundedCode']:
  value=bytes.fromhex(r['bytes']);assert raw(int(r['at'],16),len(value))==value
 if name=='MyScriptEngine.dll':
  pub=json.loads((a.models/'text-unicode/legacy-text-publication-data.json').read_bytes());rows=pub['scriptRanges'];assert len(rows)==716
  assert raw(0x1801fb6d0,716*12)==b''.join(struct.pack('<iii',*row)for row in rows)
  assert struct.unpack('<Q',raw(0x180202a70+0x30,8))[0]==0x18005f290
  assert struct.unpack('<Q',raw(0x1801de1b0+0x68,8))[0]==0x180052640
  assert struct.unpack('<Q',raw(0x1801de1b0+0x70,8))[0]==0x180052cd0
 else:
  assert struct.unpack('<Q',raw(0x1802b4360+0x30,8))[0]==0x180093b10
  assert struct.unpack('<Q',raw(0x1802e3080,8))[0]==0x1801c2940
 results.append(dict(binary=name,fullGhidraBodies=len(proof['regions']),regions=sum(map(len,proof['regions'].values())),directCalls=len(proof['calls']),dataPins=len(proof['data']),honestBoundedCode=len(proof['boundedCode'])))
ak=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();assert sha(ak)=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
assert struct.unpack_from('<IIIHH',ak,184155)==(0x101fe90,361517,0x21d,0,0x119)
result=dict(status='PASS',nativeExecuted=False,native=results,scope='Actual119 reader/group/mask/source-dispatch and182-to220 impossible-tail closure; no native configured fullroot or neural/linguistic winner claim')
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result))
