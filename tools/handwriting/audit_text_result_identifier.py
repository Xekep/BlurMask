"""Static native model62 generation/raw-order formatting and result38 join."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
e=Binary('MyScriptEngine.dll');t=Binary('MyScriptText.dll');md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];count=0
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def pins(b,start,end,expected):
 global count
 raw=b.data[b.off(start):b.off(end)];rows=[(x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
 for pair in expected:assert pair in rows,(hex(start),pair)
 count+=len(expected);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
 return rows
assert e.qwords(0x18020f878,2)==(0x180092ac0,0x180092b80)
assert e.data[e.off(0x18020f850):e.off(0x18020f850)+16]==b'0123456789abcdef'
assert struct.unpack_from('<5I',e.data,e.off(0x18020f860))==(8,4,4,4,12)
pins(e,0x180092ac0,0x180092b80,[('mov','edx, 0x62'),('movabs','r9, 0x9e3779b97f4a7c15'),
 ('movabs','r10, 0xbf58476d1ce4e5b9'),('movabs','rdx, 0x94d049bb133111eb'),
 ('mov','qword ptr [rbx], rax'),('mov','qword ptr [rbx + 8], rcx'),
 ('and','byte ptr [rbx + 6], 0xf'),('or','byte ptr [rbx + 6], 0x40'),('and','cl, 0x3f'),('or','cl, 0x80')])
pins(e,0x180092b80,0x180092c20,[('movzx','edx, byte ptr [r11 + r9]'),('inc','r9'),
 ('mov','byte ptr [rax], cl'),('mov','byte ptr [rax + 1], cl'),('mov','byte ptr [rax], 0x2d'),('mov','byte ptr [rcx], 0')])
pins(t,0x18001c010,0x18001c2c0,[('lea','rax, [rdi + 0x38]'),('xorps','xmm0, xmm0'),('movups','xmmword ptr [rax], xmm0')])
pins(t,0x180032550,0x1800326b0,[('mov','edx, 0x62'),('je','0x180032590'),
 ('movups','xmmword ptr [rbx + 0x38], xmm2')])
#Initializer body is pinned solely to distinguish the deliberately unported
#native address/time/thread seed from the library cryptographic adaptation.
raw=e.data[e.off(0x180092cf0):e.off(0x180092de0)];regions.append(dict(dll=e.name,start='0x180092cf0',end='0x180092de0',sha256=hashlib.sha256(raw).hexdigest(),scope='native initializer not emulated'))
a.output.write_bytes((json.dumps(dict(regions=regions,pins=count,bindings=2,format='raw byte order8-4-4-4-12 lowercase; UUIDv4',portableAdaptation='Crypto8B library initial state; not native092CF0 address initialization',vendorExecution=False),indent=2)+'\n').encode())
print(f'PASS{len(regions)} static regions/2 bindings/{count} pins; restored62 algorithm, explicit portable seed adaptation.')
