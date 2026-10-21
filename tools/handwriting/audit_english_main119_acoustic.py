"""Static-only actual EN MAIN119 NN/profile/positive-cache dependency audit. Never loads SDK."""
from pathlib import Path
import argparse,hashlib,json,struct
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();sha=lambda b:hashlib.sha256(b).hexdigest();proof=json.loads(Path(__file__).with_name('english-main119-acoustic-pins.json').read_bytes());b=(a.sdk/proof['binary']).read_bytes();assert sha(b)==proof['sha256'];pe=struct.unpack_from('<I',b,60)[0];assert b[pe:pe+4]==b'PE\0\0';o=pe+24;assert struct.unpack_from('<H',b,o)[0]==0x20b;base=struct.unpack_from('<Q',b,o+24)[0];table=o+struct.unpack_from('<H',b,pe+20)[0];sections=[]
for i in range(struct.unpack_from('<H',b,pe+6)[0]):
 q=table+i*40;vs,rv,n,raw=struct.unpack_from('<IIII',b,q+8);sections.append((rv,n,raw))
def read(va,n):
 for rv,size,offset in sections:
  if rv<=va-base and va-base+n<=rv+size:return b[offset+va-base-rv:offset+va-base-rv+n]
 raise AssertionError((hex(va),n))
for pieces in proof['regions'].values():
 for r in pieces:
  lo=int(r['start'],16);hi=int(r['endExclusive'],16);assert hi-lo==r['bytes']and sha(read(lo,hi-lo))==r['sha256']
for r in proof['sourceCountReaderDependency']['regions']:
 lo=int(r['start'],16);hi=int(r['endExclusive'],16);assert hi-lo==r['bytes']and sha(read(lo,hi-lo))==r['sha256']
for r in proof['calls']+proof['data']+proof['boundedCode']:
 raw=bytes.fromhex(r['bytes']);assert read(int(r['at'],16),len(raw))==raw
assert struct.unpack('<Q',read(0x1802b6b10+0x10,8))[0]==0x1800b5760
assert struct.unpack('<Q',read(0x1802b75a0+0x10,8))[0]==0x1800be270
assert struct.unpack('<QQQ',read(0x1802b75a0+0x38,24))==(0x1800bedf0,0x1800bf090,0x1800bff40)
assert struct.unpack('<Q',read(0x1802e1ff0+0x70,8))[0]==0x1801a7850
assert struct.unpack('<d',read(0x1802ed2b0,8))[0]==1.2000000476837158
raw=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();assert sha(raw)=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'and raw[184281:184283]==bytes([1,2]) and raw[545671]==0 and raw[545658]==1
assert struct.unpack('<QQ',read(0x1802ba2c0+0x78,16))==(0x18004a2d0,0x18004a2d0)
import math
assert struct.unpack('<48i',read(0x180260fd0,192))==tuple(int(math.floor(1024*math.tanh(3*i/47)))for i in range(48))
assert struct.unpack('<128i',read(0x180261090,512))==tuple([int(math.floor(1024/(1+math.exp(-8*i/127))))for i in range(127)]+[1024])
assert struct.unpack('<352I',read(0x180260a50,1408))==tuple(int(25*math.exp(-4+22*i/351))for i in range(352))
r=dict(status='PASS',scope='Actual2B1 dispatch/saturating assembly,current-frame and span/power native dependency only; NEW raw MAIN acoustic runtime dependency; no worddecoder or accuracy claim',nativeFunctions=len(proof['regions']),bodyRegions=sum(map(len,proof['regions'].values())),sourceCountReaderDependencyRegions=len(proof['sourceCountReaderDependency']['regions']),directCalls=len(proof['calls']),dataPins=len(proof['data']),honestBoundedCode=len(proof['boundedCode']),nativeExecuted=False)
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r))
