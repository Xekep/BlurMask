"""Read-only PE proof for a NEW library-owned fresh textField INPUT context.
No SDK page, layout, existing-registry state, or document acceptance inferred.
"""
import argparse,hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
sha={'MyScriptText.dll':'18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f',
     'MyScriptInk.dll':'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a',
     'MyScriptEngine.dll':'d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c',
     'MyScriptDocument.dll':'17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'}
bs={n:Binary(n) for n in sha}
for n,b in bs.items():assert hashlib.sha256(b.data).hexdigest()==sha[n]
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];pin_count=0;tables=0
def pins(n,s,e,expected):
 global pin_count
 b=bs[n];raw=b.data[b.off(s):b.off(e)];rows=[(i.mnemonic,i.op_str) for i in md.disasm(raw,s)]
 for v in expected:assert v in rows,(n,hex(s),v)
 pin_count+=len(expected);regions.append(dict(dll=n,start=hex(s),end=hex(e),sha256=hashlib.sha256(raw).hexdigest()))
 return rows
def table(n,a,slot,target):
 global tables
 assert bs[n].qwords(a+slot,1)[0]==target,(n,hex(a),hex(slot));tables+=1
def string(n,a):
 b=bs[n];return b.data[b.off(a):].split(b'\0',1)[0]
I='MyScriptInk.dll';E='MyScriptEngine.dll';T='MyScriptText.dll';D='MyScriptDocument.dll'
# Actual Text2BC extension registration, not a kind inferred from raw points.
table(T,0x180275f50,8,0xbe1000002bc);table(T,0x180275cb0,8,0x180276000)
pins(T,0x180003091,0x1800030d7,[('lea','rdx, [rip + 0x272eb8]'),('call','r8')])
assert 0x180003098+0x272eb8==0x180275f50
table(T,0x180276000,0,0x1802eaf70);table(T,0x180276000,0x10,0x18003ff40);table(T,0x180276000,0x18,0x180045740)
assert string(T,0x1802eaf70)==b'http://www.myscript.com/ns/myscript/page'
assert string(T,0x1802eacd0)==b'textField'
table(T,0x180275e98,0,0x1802eacd0);table(T,0x180275bd0,0,0x180042ba0)
pins(T,0x18003ff40,0x18003ff9f,[('lea','r9, [rip + 0x235f43]'),('call','r10')])
assert 0x18003ff55+0x235f43==0x180275e98
pins(T,0x180045740,0x180045772,[('cmp','r8, rax'),('cmp','r9d, 0xc98'),('lea','rax, [rip + 0x230474]')])
assert 0x18004575c+0x230474==0x180275bd0
pins(T,0x180042ba0,0x180042baa,[('mov','dword ptr [r8], 0'),('mov','al, 1'),('ret','')])
# The live BCD interface getter dispatches the actual node's bound namespace
# handler; BE0 iterates its registered extensions, not a constructor kind.
table(E,0x18021c930,0x30,0x1800d2a10);table(E,0x18021ca20,0x28,0x1800d31c0)
pins(E,0x1800d2a10,0x1800d2ab4,[('cmp','cl, 3'),('mov','rcx, qword ptr [r8 + 0x28]'),
 ('mov','rdx, qword ptr [r8 + 0x20]'),('mov','r10, qword ptr [rax + 0x28]'),('jmp','r10')])
pins(E,0x1800d31c0,0x1800d323a,[('mov','rbx, qword ptr [rdx + 0x10]'),
 ('mov','r11, qword ptr [r10 + 0x18]'),('call','r11')])
# addItems reads actual kind, adds decimal100, creates registry selection,
# and writes returned ID to input.tag. Existing span children are rejected.
pins(T,0x18004190f,0x180041a14,[('mov','r8d, 0xc98'),('add','ecx, 0x64'),
 ('mov','r9, r14'),('mov','dword ptr [rsp + 0x48], ecx'),('mov','r10, qword ptr [rax + 0xe0]'),
 ('mov','qword ptr [rsp + 0x40], 0xffffffffffffffff'),('mov','r10, qword ptr [rcx + 0x128]')])
# C20 default construction enables its OWN C4D and C5E, not page CB7.
pins(I,0x180063ec0,0x180063ecb,[('mov','r9b, 1'),('xor','r8d, r8d'),('jmp','0x1800638a0')])
pins(I,0x1800638a0,0x180063bd0,[('mov','edx, 0xc51'),('mov','edx, 0xc4d'),('mov','edx, 0xc5e'),('mov','r9d, 0x6e'),('mov','r9d, 0x78')])
table(I,0x1800e5860,0x10,0x180078950);table(I,0x1800e5860,0x18,0x180078ac0)
table(I,0x1800e5860,0x90,0x1800797f0);table(I,0x1800e5860,0x98,0x180079a10)
for slot in (0x10,0x18,0x90,0x98):table(I,0x1800e55e0,slot,0x180020d60)
table(I,0x1800e59f0,0x10,0x180077530);table(I,0x1800e59f0,0x28,0);table(I,0x1800e59f0,0x30,0)
# Base70 initializes parent/listener60 NULL. External properties are not
# admitted by this new owner. History queues allocate commands, not C51 tags.
pins(E,0x1800292e0,0x180029510,[('xor','ebp, ebp'),('mov','qword ptr [rdi + 0x60], rbp')])
pins(E,0x180003200,0x18000327d,[('mov','qword ptr [rcx + 0x28], rax'),('mov','qword ptr [rcx], rax')])
table(E,0x18023c7e0,8,0x1800299d0);table(E,0x18023c7e0,0x90,0x18002b0a0)
pins(E,0x1800299d0,0x180029a10,[('call','0x180028720')])
pins(E,0x18002b0a0,0x18002b170,[('call','0x180029e90'),('mov','edx, dword ptr [rdx + 0x50]')])
pins(E,0x180029e90,0x180029fe0,[('call','0x18002b8f0')])
pins(I,0x180077530,0x180077570,[('call','0x1800772d0'),('jmp','0x180076920')])
# Raw append callbacks retain input, append to history; no AddSelection.
pins(I,0x180078950,0x180078ac0,[('call','0x180076780'),('call','0x180077230')])
pins(I,0x180078ac0,0x180078c40,[('xor','ebp, ebp'),('mov','rdi, rbp'),('cmp','dword ptr [r8], edi')])
pins(I,0x1800797f0,0x180079a10,[]);pins(I,0x180079a10,0x180079b40,[])
# Fresh C51 counters. The counter increments only after actual09BB20 success.
pins(I,0x18009e594,0x18009e68e,[('xor','r8d, r8d'),('movabs','rax, 0x8000000000000000'),
 ('mov','qword ptr [rsi + 0x20], rax'),('mov','qword ptr [rsi + 0x18], r8'),('mov','byte ptr [rsi + 0x28], r8b')])
pins(I,0x18009f1ba,0x18009f23f,[('call','0x18009bb20'),('je','0x18009f26a'),
 ('mov','qword ptr [rbp + 0x20], r12'),('inc','rax'),('mov','qword ptr [rbp + 0x18], rax'),
 ('mov','dword ptr [r14 + 0x18], edi')])
table(I,0x1800e5050,0x128,0x18006c880);table(I,0x1800e78a0,0x30,0x18009ede0)
pins(I,0x18006c880,0x18006c8e8,[('mov','r9, qword ptr [rax + 0x30]')])
# Selected clone creates C22 against target C20; retains full original ID/name,
# passes new C22 at stack20, exact category at40, then checks returned ID.
table(I,0x1800e5050,0x170,0x18006e120)
pins(I,0x18006de38,0x18006df2b,[('mov','edx, 0xc22'),('mov','r9d, 0x18'),
 ('mov','r9, qword ptr [r13]'),('mov','r8, rbx'),('mov','dword ptr [rsp + 0x40], eax'),
 ('mov','qword ptr [rsp + 0x20], r14'),('cmp','rbx, rax')])
pins(I,0x18006dfab,0x18006dfe2,[('mov','r8, qword ptr [rax + 0x20]'),('mov','r9, qword ptr [rcx + 0x28]')])
table(I,0x1800e78a0,0x20,0x18008e870);table(I,0x1800e78a0,0x28,0x18009ed40)
pins(I,0x18009edce,0x18009ede0,[('cmp','qword ptr [r11 + 0x20], rdi'),('jl','0x18009edd8'),('mov','qword ptr [r11 + 0x18], rdi')])
#030FB0 discovers input/span BEFORE allocating a32-byte row. No spans ->
# direct return, not an invented empty range used as a full-source key.
pins(D,0x180031023,0x1800310e2,[('mov','r10, qword ptr [rcx + 0x1a0]'),('test','rbx, rbx'),
 ('je','0x18003131f'),('mov','r8d, 0x20'),('inc','dword ptr [r14 + 8]')])
report=dict(peSha256=sha,regions=regions,pins=pin_count,virtualBindings=tables,
 scope='NEW library-owned fresh textField/C20;default observers;INPUT category100;separate C51/C22;full selected copy;no span children',
 pending=['existing SDK page/layout observers','cropped/reused/auxiliary selections','actual2DA/037E80 result binding','full0283B0 acceptance'],vendorExecution=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print(f'PASS{len(regions)} static regions/{tables} bindings/{pin_count} pins; actual textField C98/default observers/C51/C22 clone; no vendor execution.')
