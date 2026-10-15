"""Read-only PE pins for2067C0/206960/207CB0; never loads vendor code."""
import argparse,hashlib,json,struct
from pathlib import Path
parser=argparse.ArgumentParser()
parser.add_argument('output')
parser.add_argument('--binary',default=str(Path(__file__).resolve().parents[2]/'research/myscript/sdk/build/x64/MyScriptText.dll'))
args=parser.parse_args()
b=Path(args.binary).read_bytes()
assert hashlib.sha256(b).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pe=struct.unpack_from('<I',b,0x3c)[0];sections=struct.unpack_from('<H',b,pe+6)[0]
header=pe+24+struct.unpack_from('<H',b,pe+20)[0]
def off(va):
 rva=va-0x180000000
 for index in range(sections):
  h=header+index*40;start,size,raw=struct.unpack_from('<III',b,h+12)
  if start<=rva<start+size:return raw+rva-start
 raise ValueError(hex(va))
pins={
 'source_node_ushort_then_signed_flag':(0x1802067e1,'410fb7440918413bc0743c41807c0905007d34'),
 'context_difference_index_minus_one':(0x180206922,'41c74102ffffffff'),
 'merge_left_unsigned_bound':(0x180206a28,'2bc183f8050f8714010000'),
 'merge_right_unsigned_bound':(0x180206a3f,'2bc283f8050f87fd000000'),
 'normalize_empty_still_writes_index':(0x180206cc7,'4863c2488d0c4041897c8808'),
 'beam_marker_clear':(0x180207fa3,'80641006fd'),
 'beam_marker_set':(0x180208534,'42804c010602'),
 'beam_compact_index_write1':(0x18020b3b3,'6644894c0822'),
 'beam_compact_index_write2':(0x18020b629,'664489440822'),
 'frontend220_to_backend150':(0x180152a50,'488b5278488b024c8b90500100000fb64424288844242849ffe2'),
 'ordinary150_to_graphc8':(0x1801c2510,'488b9280010000488b024c8b90c80000000fb64424288844242849ffe2')
}
for name,(va,expected) in pins.items():
 actual=b[off(va):off(va)+len(expected)//2]
 assert actual.hex()==expected,(name,actual.hex())
raw=b[off(0x1802e7b20):off(0x1802e7b20)+144]
assert hashlib.sha256(raw).hexdigest()=='ad45c41cff7e88ad53dac9d8c09ba8cd86d89eeeb18d0fa835432024e71ac9a6'
rows=[list(struct.unpack_from('<6i',raw,row*24)) for row in range(6)]
report=dict(binarySha256=hashlib.sha256(b).hexdigest(),instructionPins=len(pins),
 sixTokenTable=dict(address='1802e7b20',order='FFFC..FFF7',sha256=hashlib.sha256(raw).hexdigest(),rows=rows),
 beamWrites=[dict(address=f'{va:x}',bytes=value,meaning=name) for name,(va,value) in pins.items() if name.startswith('beam_')],
 scope='Pins corroborate the separately reviewed helper/alias inventory; not a proof of complete207CB0 orchestration or arbitrary callback behavior.')
Path(args.output).write_text(json.dumps(report,indent=2),encoding='utf-8')
print(f'PASS{len(pins)} instruction spans +144-byte six-token table; read-only PE audit')
