"""Read-only old/final tuple counter -> native07AAF0 pending command proof."""
import argparse,hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
t=Binary('MyScriptText.dll');i=Binary('MyScriptInk.dll')
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(i.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];count=0
def pins(b,start,end,expected):
 global count
 raw=b.data[b.off(start):b.off(end)];rows=[(x.mnemonic,x.op_str)for x in md.disasm(raw,start)]
 for pair in expected:assert pair in rows,(hex(start),pair)
 count+=len(expected);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
 return rows
assert i.qwords(0x1800e5980+0x10,1)[0]==0x18007aaf0
assert i.qwords(0x1800e78a0+0x20,1)[0]==0x18008e870
assert i.data[i.off(0x18008e870):i.off(0x18008e870)+5]==bytes.fromhex('488b4218c3')
pins(t,0x18005f050,0x18005f124,[('mov','r8, qword ptr [rax + 0x20]'),('mov','qword ptr [rbx + 0x10], rax')])
r=pins(t,0x1800283b0,0x180029990,[('mov','rax, qword ptr [rbp + 0x30]'),('mov','qword ptr [rbp + 0xd0], rax'),
 ('call','0x180026ef0'),('cmp','rsi, qword ptr [rbp + 0x30]'),('je','0x1800298e7'),
 ('mov','r9, qword ptr [rbp + 0x30]'),('mov','r8, rsi'),('mov','r10, qword ptr [rax + 0x10]')])
assert r.index(('mov','qword ptr [rbp + 0xd0], rax'))<r.index(('call','0x180026ef0'))<r.index(('cmp','rsi, qword ptr [rbp + 0x30]'))
pins(t,0x18002127a,0x18002133e,[('lea','rax, [rbx + 1]'),('mov','qword ptr [r12 + 0x10], rax')])
pins(i,0x18007aaf0,0x18007aba0,[('mov','edx, 0x50'),('mov','r8d, 7'),('call','0x180077230'),
 ('mov','qword ptr [rdx + 0x20], rsi'),('mov','qword ptr [rdx + 0x18], rdi'),
 ('mov','qword ptr [rax + 0x10], rdx'),('mov','qword ptr [rcx + 8], rdx'),('mov','qword ptr [rax + 0x18], rdx')])
assert struct.unpack_from('<I',i.data,i.off(0x1800772a0)+7*4)[0]==0x77281
pins(i,0x180077230,0x1800772a0,[('mov','byte ptr [rdx], r8b'),('lea','r8, [rdx + 0x18]'),
 ('xorps','xmm0, xmm0'),('movups','xmmword ptr [r8], xmm0'),('movups','xmmword ptr [r8 + 0x10], xmm0'),
 ('movups','xmmword ptr [r8 + 0x20], xmm0'),('mov','qword ptr [r8 + 0x30], rax')])
#The later executor is separate from append: only there can C20.VF8 changeC51.
assert i.qwords(0x1800e5050+0xf8,1)[0]==0x18006a100
pins(i,0x180078701,0x180078772,[('cmp','qword ptr [r14 + 0x20], -1'),('cmp','qword ptr [r14 + 0x18], -1'),
 ('mov','r8, qword ptr [r14 + 0x18]'),('cmp','r8, rax'),('jle','0x18007864d'),('mov','r10, qword ptr [rax + 0xf8]')])
a.output.write_bytes((json.dumps(dict(regions=regions,pins=count,bindings=3,scope='same tuple initial/final counters -> pending operation7; executor intentionally not run',vendorExecution=False),indent=2)+'\n').encode())
print(f'PASS{len(regions)} static regions/3 bindings/{count} pins; native pending-counter append, no transaction execution.')
