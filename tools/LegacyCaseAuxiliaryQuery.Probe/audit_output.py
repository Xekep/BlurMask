"""Native231AE0 copies only five case bits; no forms, costs or other fields."""
import json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise ValueError('captures.json')
c=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'));source=bytes.fromhex(c['source'])
assert c['nativeRootRecords']==0 and 'NOT native092990' in c['stressAuxiliaryMap']
assert len(source)>0 and len(source)%32==0 and len(c['states'])==32
for sample in c['states']:
 state=sample['state'];actual=bytes.fromhex(sample['records']);assert len(actual)==len(source)
 for i in range(0,len(source),32):
  expected=bytearray(source[i:i+32]);expected[7]=(expected[7]&0x83)|(state<<2)
  assert actual[i:i+32]==expected and actual[i+5]&2
  assert struct.unpack_from('<i',actual,i+16)[0]==struct.unpack_from('<i',source,i+16)[0]
print(f'PASS independent32 states/{len(source)//32*32} terminal records, exact masked byte7 and unchanged source/context/price bytes')
