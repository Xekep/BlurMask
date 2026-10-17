"""Static NEW2DA/selected INPUT -> post023260 transform swap proof."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
b=Binary('MyScriptText.dll');assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];count=0
def pins(start,end,expected):
 global count
 raw=b.data[b.off(start):b.off(end)];rows=[(x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
 for pair in expected:assert pair in rows,(hex(start),pair)
 count+=len(expected);regions.append(dict(start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
 return rows
assert b.qwords(0x180275518+8,1)[0]==0x180275550
assert b.qwords(0x180275550+0x18,1)[0]==0x1800283b0
assert b.qwords(0x180275550+0x28,1)[0]==0x18002d8c0
assert b.qwords(0x1802754b0,1)[0]==0x18001c010
pins(0x18001c010,0x18001c2c0,[('xor','ebp, ebp'),('mov','dword ptr [rdi + 0xd8], ebp'),
 ('mov','qword ptr [rdi + 0x108], rbp'),('mov','qword ptr [rdi + 0x110], rbp'),
 ('mov','edx, 0x3e'),('mov','qword ptr [rdi + 0x118], rax')])
pins(0x18002d8c0,0x18002e1c0,[('call','0x18001c460'),('mov','qword ptr [rsi + 0x30], rax')])
pins(0x18002df2b,0x18002df65,[('mov','rdx, qword ptr [rdi + 0x38]'),
 ('cmp','qword ptr [rdx + 0x10], rax'),('call','0x180016120'),('mov','qword ptr [rsi + 0x30], rax')])
r=pins(0x1800283b0,0x180029990,[('call','0x180023260'),('mov','r8, qword ptr [r14 + 0x30]'),
 ('mov','r9, qword ptr [rax + 0x128]'),('mov','rcx, qword ptr [rax + 0x18]'),('mov','eax, dword ptr [rax + 0x20]'),
 ('mov','byte ptr [rsp + 0x20], 0'),('call','0x180037e80'),('call','0x180026ef0')])
assert r.index(('call','0x180023260'))<r.index(('call','0x180037e80'))<r.index(('call','0x180026ef0'))
r=pins(0x180037e80,0x1800381b0,[('cmp','dword ptr [rdx + 0xd8], 0'),('mov','byte ptr [rsp + 0x28], 1'),
 ('mov','r10, qword ptr [rax + 0x30]'),('call','r10'),('movups','xmm0, xmmword ptr [r14 + 0x108]'),
 ('setne','r15b'),('movups','xmmword ptr [rsp + 0x30], xmm0'),('movups','xmmword ptr [r14 + 0x108], xmm1'),
 ('cmp','byte ptr [rsp + 0x80], 0'),('je','0x180038098')])
# Export status is saved; it does not guard the vector replacement.
start=r.index(('call','r10'));swap=r.index(('movups','xmmword ptr [r14 + 0x108], xmm1'))
assert all(not op.startswith('j')for op,_ in r[start+1:swap])
pins(0x180038098,0x1800380fb,[('add','rbx, 0x50'),('call','r8'),('movzx','eax, r15b')])
pins(0x180026ef0,0x180026f1a,[('cmp','dword ptr [r8 + 8], 0'),('jne','0x180026f1a'),('mov','al, 1'),('ret','')])
a.output.write_bytes((json.dumps(dict(peSha256=hashlib.sha256(b.data).hexdigest(),regions=regions,pins=count,bindings=4,
 scope='Fresh selected INPUT seed with shared3E; actualD8 alternatives; writeDomfalse; failed export still swaps; empty actualDAF returns before writes',
 vendorExecution=False),indent=2)+'\n').encode())
print(f'PASS{len(regions)} static regions/4 bindings/{count} pins; NEW2DA seed and unconditional transform-vector replacement.')
