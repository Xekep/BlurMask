"""Independent owned coordinates/ranges -> ordinary bbox and top-mark facts."""
import json,struct,sys
from pathlib import Path
raw=Path(sys.argv[1]).read_bytes();p=json.loads(Path(sys.argv[2]).read_bytes())
def value(b):return struct.unpack('<f',struct.pack('<I',b&0xffffffff))[0]
def bits(v):return struct.unpack('<I',struct.pack('<f',v))[0]
# Skip raw alphabet trie/spans/UTF8, then original reverse slots to its stored bits.
at=99;header=struct.unpack_from('<8H',raw,at);at+=16
def read(fmt):
 global at
 v=struct.unpack_from('<'+fmt,raw,at)[0];at+=struct.calcsize('<'+fmt);return v
n=read('H')
for inc in (32,64,1):
 vals=[read('H') for _ in range(n)];n=next(v for v in vals[::-1] if v!=65535)+inc
spans=[read('H') for _ in range(n+1)];pairs=[(read('I'),read('I')) for _ in range(spans[-1]+1)]
at+=pairs[-1][0]+header[-1]*2;have=read('B');flags=raw[at:at+(header[-1]+7)//8] if have else bytes((header[-1]+7)//8)
total=0
for source in p['sources']:
 assert not source['ordered']
 points=[(value(x),value(y)) for x,y in source['points']]
 starts=[];offset=0
 for count in source['counts']:starts.append(offset);offset+=count
 assert len(points)==offset
 groups=source['groups'];guide=source['guide'];assert guide[0]>=0 and value(guide[2])>0
 for node,arc,group,line,kind,top,*box in source['arcs']:
  ranges=groups[group];xy=[points[starts[s]+i] for s,a,b in ranges for i in range(a,b)]
  expected=[min(x for x,y in xy),min(y for x,y in xy),max(x for x,y in xy),max(y for x,y in xy)]
  assert [b&0xffffffff for b in box]==list(map(bits,expected))
  assert kind==0 and line==guide[0] and top==int(all(a==0 for s,a,b in ranges));total+=1
 assert source['flags']==[bool(flags[l//8]&(1<<(l%8))) for l in range(192)]
assert p['guards']==24 and p['arcs']==total and len(p['sources'])==4
print(f'PASS independent{total} actual source arc bounds/topmark/line/guide, rawAL flags for4 CASE/DIRECT owners; no prepared geometry substitution')
