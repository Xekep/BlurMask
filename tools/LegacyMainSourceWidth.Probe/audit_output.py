"""Independent2188F0 row filtering and ordered float-width/double-sum oracle."""
import json,struct,sys
from pathlib import Path
u32=lambda b,o:struct.unpack_from('<I',b,o)[0]
i32=lambda b,o:struct.unpack_from('<i',b,o)[0]
u16=lambda b,o:struct.unpack_from('<H',b,o)[0]
f32=lambda x:struct.unpack('<f',struct.pack('<f',x))[0]
float_bits=lambda x:struct.unpack('<f',struct.pack('<I',x))[0]
double_bits=lambda x:struct.unpack('<d',struct.pack('<Q',x))[0]
bits_double=lambda x:struct.unpack('<Q',struct.pack('<d',x))[0]
bits_float=lambda x:struct.unpack('<I',struct.pack('<f',x))[0]
d=json.loads(Path(sys.argv[1]).read_bytes());total=0
for c in d['prepared']:
 compact=bytes.fromhex(c['compact']);rows=bytes.fromhex(c['rows'])
 count=c['seedCount'];value=double_bits(c['seedSumBits']);calls=[]
 for o in reversed(range(0,len(rows),72)):
  index=i32(rows,o+8)
  if index>=len(compact)//40:continue
  kind=compact[index*40+32]&7
  if kind==2:continue
  if kind not in (4,6) and u16(rows,o+56)!=65535 and (u16(rows,o+58)+6)&65535<=2:continue
  node=i32(rows,o);assert u16(rows,o+4)==node*13
  lo,hi=map(float_bits,c['boxes'][node]);value+=f32(hi-lo)
  count=((count+1+2**31)%2**32)-2**31;calls.append(node)
 assert c['calls']==calls and c['sumBits']==bits_double(value) and c['count']==count
 assert c['averageBits']==bits_float(f32(value/count) if count>0 else 0)
 total+=len(calls)
assert len(d['prepared'])==256 and d['guards']==3
print(f'PASS independent256 prepared2188F0 segments/{total} lazy bbox reads, reverse order, float subtraction/double sum, count wrap and TOP mean; not owning segmentation')
