"""Static0283B0 actual275-array/sharedCHAR/WORD-selection proof. No DLL execution."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
b=Binary();m=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
def dis(s,e):return[(i.mnemonic,i.op_str)for i in m.disasm(b.data[b.off(s):b.off(e)],s)]
def pins(s,e,expected):
 actual=dis(s,e)
 for value in expected:assert value in actual,(hex(s),value)
 return actual
regions=[(0x1800283b0,0x180029990),(0x180024f30,0x1800258d0)]
assert hashlib.sha256(b.data[b.off(regions[0][0]):b.off(regions[0][1])]).hexdigest()=='fa2576435b07665bfe72ead6e2da50229aadea857719d8c27ab507ef9b8ab195'
pins(0x180028e25,0x180028ecb,[('call','0x18001b4e0'),('lea','rdi, [rax + 0x18]'),
 ('je','0x180029158'),('mov','qword ptr [rdi + 0x50], rdx'),('mov','qword ptr [rdi + 0x58], 0'),
 ('mov','qword ptr [rdi], rdx'),('mov','dword ptr [rdi + 8], 0xffffffff'),
 ('movabs','rax, 0x7fffffffffffffff'),('mov','qword ptr [rdi + 0x10], rax')])
loop=pins(0x180028f79,0x18002903b,[('call','0x18025ee50'),('btr','eax, 0x17'),('bts','eax, 0x15'),
 ('cmp','rbx, r13'),('jae','0x18002903b'),('mov','r9, qword ptr [rax + 0x18]'),('add','r9, r14'),
 ('mov','qword ptr [rsp + 0x20], rdi'),('call','0x180024f30'),('inc','rbx'),('add','r15, 0x30'),('add','r14, 0x78')])
assert loop.index(('call','0x18025ee50'))<loop.index(('call','0x180024f30'))
pins(0x180029080,0x180029153,[('or','eax, dword ptr [rcx]'),('lea','rcx, [rcx + 0x30]'),
 ('or','eax, dword ptr [rdi + 0x18]'),('bt','eax, 0x15'),('mov','qword ptr [rcx], 0xffffffffffffffff'),
 ('lea','rbx, [rdi + 0x20]'),('mov','r9d, 1'),('call','0x1800210c0'),('cmp','dword ptr [rbx + 8], 1'),
 ('cmp','r9b, 0xff'),('cmp','r10b, 0xff'),('mov','ecx, 0x800000')])
pins(0x180029158,0x18002917b,[('mov','r14, qword ptr [rcx + rax*8]'),
 ('cmp','dword ptr [r14 + 8], eax'),('cmp','dword ptr [r14 + 0xc], eax'),('je','0x1800292f5')])
pins(0x1800292f8,0x180029398,[('mov','dword ptr [rbp + 0xb8], r12d'),('mov','byte ptr [rbp + 0xbc], 0'),
 ('mov','rax, qword ptr [r9 + 0x68]'),('shl','rcx, 4'),('or','edx, dword ptr [rcx + rax + 8]'),
 ('and','edx, 0x3ff'),('lea','rax, [r9 + 0x18]'),('mov','qword ptr [rbp + 0xb0], rax'),('mov','r8d, 0x18')])
pins(0x180024f57,0x180024fa1,[('mov','qword ptr [r12 + 0x10], r14'),('mov','qword ptr [r12 + 0x18], r14'),
 ('mov','qword ptr [r12], r14'),('lea','rdx, [r12 + 0x20]'),('mov','dword ptr [r12 + 8], r14d')])
pins(0x180025125,0x180025135,[('test','r8b, r8b'),('and','dword ptr [rax + 0x18], 0xffdfffff')])
pins(0x180025744,0x1800257d7,[('inc','edi'),('mov','r9d, edi'),('mov','dword ptr [r12 + 0x1c], edi'),
 ('movups','xmmword ptr [rdx], xmm0'),('mov','qword ptr [rdx + 0x10], rax'),
 ('mov','qword ptr [rax + rcx*8 + 8], rdx'),('mov','byte ptr [rax + rcx*8 + 0x14], 0'),
 ('mov','dword ptr [rax + rcx*8 + 0x10], r14d')])
report=dict(regions=[dict(start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest())for s,e in regions],
 scope='fresh configured2 same275 SPAN; all real WORD payloads, shared CHAR/UTF8/context, WORD pending registration only',
 wordSelectionRanges='original WORD key at source node38, not accumulated ordinary union at node48',
 reusedKey='same275 first/last nodes only; different-span remap unsupported',noVendorExecution=True,documentAcceptance=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS2 full PE regions/8 ordered native blocks: actual275 stride120 -> WORD48/CHAR24, same-span reuse, source-key WORD registration; no full0283B0 claim.')
