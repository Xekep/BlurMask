"""Pinned219580 SSE cost transfer; static bytes only, never load vendor code."""
import hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x180219580,0x180219f80;raw=b.data[b.off(a):b.off(z)]
assert hashlib.sha256(raw).hexdigest()=='9825287ed9aabcefcad22ff893fad2eb058e8be88a54e7614f82990a2e5a7f64'
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True;ins={i.address:i for i in md.disasm(raw,a)}
pins={
0x180219b7e:('mov','r12d, dword ptr [rdx + rax*8 + 0x14]'),
0x180219b83:('mov','r15d, dword ptr [rdx + rax*8 + 0x18]'),
0x180219b8d:('cmp','eax, -1'),
0x180219b96:('sub','r12d, dword ptr [rdx + rcx*8 + 0x14]'),
0x180219b9b:('sub','r15d, dword ptr [rdx + rcx*8 + 0x18]'),
0x180219bc7:('cvtdq2ps','xmm6, xmm6'),
0x180219be3:('sub','ecx, ebx'),
0x180219be5:('add','eax, ecx'),
0x180219bea:('shl','eax, 0xd'),
0x180219bf1:('cvtdq2ps','xmm0, xmm0'),
0x180219bf4:('divss','xmm6, xmm0'),
0x180219c2b:('cmp','eax, 0x135'),
0x180219c32:('movaps','xmm0, xmm8'),
0x180219c38:('movaps','xmm0, xmm9'),
0x180219c3c:('xorps','xmm0, xmm10'),
0x180219c43:('mulss','xmm0, dword ptr [rbp - 0x38]'),
0x180219c48:('movss','dword ptr [rbp - 0x40], xmm0'),
0x180219c55:('cvtdq2ps','xmm0, xmm0'),
0x180219c58:('mulss','xmm0, xmm11'),
0x180219c5d:('movss','dword ptr [rbp - 0x34], xmm0')}
for at,expected in pins.items():assert(ins[at].mnemonic,ins[at].op_str)==expected,(hex(at),ins[at].op_str)
constants={0x180219a07:0x80000000,0x180219a10:0x40a00000,0x180219a19:0x3f800000,0x180219a22:0x39000000}
resolved={}
for at,expected in constants.items():
    i=ins[at];target=i.address+i.size+i.disp
    value=struct.unpack_from('<I',b.data,b.off(target))[0];assert value==expected,(hex(at),hex(value))
    resolved[hex(at)]={'target':hex(target),'bits':hex(value)}
Path(sys.argv[1]).write_text(json.dumps(dict(regionSha256=hashlib.sha256(raw).hexdigest(),pins=len(pins),constants=resolved,
    scope='Prepared219580 costs only; no actual candidate selection, chain success or WORD acceptance inferred',vendorExecution=False),indent=2)+'\n')
print('PASS full219580 PE/20 cost instructions/4 exact SSE constants; no vendor execution')
