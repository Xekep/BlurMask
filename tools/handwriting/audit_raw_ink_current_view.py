"""Pinned static C1C/C32 XY -> kind0/C49 -> current view -> identity export.
Reads PE bytes only. Page/C51/tag/DAF ownership is deliberately not inferred.
"""
import argparse,hashlib,json,struct,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
b=Binary('MyScriptInk.dll');assert hashlib.sha256(b.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];pin_count=0
def pins(s,e,expected):
 global pin_count
 rows=[(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(s):b.off(e)],s)]
 for v in expected:assert v in rows,(hex(s),v)
 pin_count+=len(expected);regions.append(dict(start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest()))
 return rows
def table(a,slot,target):assert b.qwords(a+slot,1)[0]==target
def text(a):return b.data[b.off(a):].split(b'\0',1)[0]
# C32 is an interface, not a guessed concrete descriptor.
table(0x1800e6fd0,0x18,0x1800988f0)
pins(0x1800988f0,0x180098914,[('cmp','edx, 0xc32'),('lea','rax, [rip + 0x4e70f]'),('cmovne','rax, rcx')])
assert 0x180098901+0x4e70f==0x1800e7010
assert text(0x1800f6b94)==b'X' and text(0x1800f6b98)==b'Y'
assert b.qwords(0x1800e52a0,4)==(0x1800f6b94,0x1800f7e00,0x10420,0x1800f7623)
assert b.qwords(0x1800e52c0,4)==(0x1800f6b98,0x1800f7e00,0x10420,0x1800f7623)
pins(0x180070ee0,0x180071120,[('mov','dword ptr [rdi + 8], ebx')])
# C1C constructor copies real channels and calls layout production; no
# caller "ordinary format" boolean takes the place of the channel model.
pins(0x1800978d0,0x180097b90,[('cmp','r9, 0x18'),('call','0x180098080')])
pins(0x180098080,0x18009836a,[('cmp','dword ptr [rcx + 8], 1'),('cmp','dword ptr [rcx + 8], 2'),
 ('cmp','dword ptr [rcx + 0x18], 0x10420'),('cmp','edi, 1'),('mov','qword ptr [rax + 4], r14'),
 ('mov','dword ptr [rax + 0xc], 8'),('mov','dword ptr [rax + 0x10], 0x10420'),
 ('mov','qword ptr [rax + 0x14], 4'),('mov','dword ptr [rax + 0x1c], 8'),('mov','dword ptr [rax + 0x20], 0x10420')])
# C48 raw-address arithmetic and10420 float load settle header lane meaning.
table(0x1800e6ed0,0x30,0x180096d20)
pins(0x180096d20,0x180096d84,[('imul','r9d, dword ptr [r10 + rcx + 0xc]'),('imul','eax, dword ptr [rdx + 8]'),
 ('add','eax, dword ptr [r10 + rcx + 4]'),('cmp','edx, 0x1e0'),('movss','xmm0, dword ptr [rax + r11 + 0x10]'),('cvtps2pd','xmm0, xmm0')])
table(0x1800e6980,0,0x180090cf0)
pins(0x180090cf0,0x180090df0,[('test','edx, edx'),('mov','edx, 0xc49'),('call','0x18008ddb0'),('cmp','edx, 4'),('mov','edx, 0xc25')])
pins(0x18008dd30,0x18008ddb0,[('cmp','dword ptr [rax - 8], 0'),('imul','ecx, ebx')])
pins(0x18008ddb0,0x18008dea2,[('mov','edx, 0xc1e'),('mov','qword ptr [rbx + 0x18], rax'),('call','0x18008dd30')])
table(0x1800e65b0,0x28,0x18008e000);table(0x1800e65b0,0x30,0x1800222c0)
assert b.data[b.off(0x1800222c0):b.off(0x1800222c0)+3]==bytes.fromhex('33c0c3')
pins(0x18008e000,0x18008e015,[('cmp','dword ptr [r8 + 4], -1'),('mov','ecx, dword ptr [rax + 8]'),('dec','ecx'),('mov','dword ptr [r8 + 4], ecx')])
# Successful full raw append creates current and raw entries, preserving
# original source index and actual current-view start; no registry-ID claim.
pins(0x180065b60,0x180066121,[('mov','ebx, dword ptr [r15 + 0x18]'),('mov','edi, dword ptr [r15 + 0x38]'),
 ('inc','dword ptr [r15 + 0x18]'),('inc','dword ptr [r15 + 0x38]')])
# Selected child copies original raw slots, then self-view -> that copied raw
# owner. Rebase/crop branches are separate; this runtime only admits full views.
table(0x1800e6980,0x18,0x180092430)
pins(0x180092430,0x180092438,[('mov','r8b, 1'),('jmp','0x180091700')])
pins(0x180092022,0x1800920cc,[('mov','edx, 0xc49'),('call','0x18008ddb0'),('sub','r15d, 0xc49'),('je','0x18009222a')])
table(0x1800e65b0,0x20,0x180052740)
assert b.data[b.off(0x180052740):b.off(0x180052740)+4]==bytes.fromhex('488bc2c3')
pins(0x18006d4fd,0x18006d542,[('mov','dword ptr [r14 + rdi*8 + 0xc], eax'),('mov','r8, qword ptr [rax + 0x18]'),
 ('call','r8'),('mov','qword ptr [r14 + rdi*8], rax')])
pins(0x18006d9eb,0x18006da49,[('mov','r8, qword ptr [rax + 0x20]'),('cmp','rax, rbx'),('jne','0x18006da1f')])
table(0x1800e5050,0x1a8,0x18006f080)
pins(0x18006b975,0x18006ba2a,[('mov','word ptr [rbp - 0x41], 0x101'),('mov','dword ptr [rbp - 0x4d], 0xffffffff'),
 ('mulss','xmm0, xmm6'),('addss','xmm0, xmm7'),('cvttss2si','ecx, xmm0')])
pins(0x18006ba2a,0x18006baf7,[('cmp','edx, dword ptr [rbp - 0x51]'),('comiss','xmm0, xmm1'),('test','edx, edx'),('cmp','r8d, eax')])
missing=pins(0x18006bc00,0x18006bc70,[('test','r12, r12'),('je','0x18006bc70')])
assert missing.index(('test','r12, r12'))<missing.index(('je','0x18006bc70'))
table(0x1800e3fe0,0,0x18004d580)
pins(0x18004d580,0x18004d5a4,[('movabs','rcx, 0x3ff0000000000000'),('mov','qword ptr [rdx], rcx'),
 ('mov','qword ptr [rdx + 0x20], rcx'),('mov','qword ptr [rdx + 0x28], rax')])
pins(0x18006f194,0x18006f21f,[('call','0x1800369a0'),('call','0x18006b7f0'),('ucomisd','xmm1, xmm7'),
 ('ucomisd','xmm5, xmm6'),('jp','0x18006f21f'),('je','0x18006f5b5')])
# Exact full contiguous Ghidra function bodies. A PE unwind entry can stop
# at the first prologue fragment; it must not be called a whole-function hash.
# Bounds below were checked against the retained functions.tsv body_bytes and
# complete linear disassembly. Keep fullFunctions report keys for consumers.
full=[]
full_bounds=[(6442913504, 6442914075, '5e5f03f2dfefccc5ee7e1e9123633eaf289bc1d276ec51dad669d5382ebf7e89'),
 (6443071696, 6443072522, '88a071a9148bc8c0a088da9621eef3fb9554c79637b55cd4c2d60586de124b27'),
 (6443073664, 6443074410, '982b126728085dedf3754caf5dd666999e1cb6bd6039c7c6e4fbcfcfe5c19aff'),
 (6443044080, 6443044322, 'e723515fdb88d8ef19fd9fdf28a0bb10358adc2afbd1ce6be989811c878b34ee'),
 (6443031984, 6443032226, 'a66d10803f9f318596e2ede7f5e6f0e1c40032f32ddddf53b415f7cdfc80f493'),
 (6442867008, 6442870025, '83385e2e4d504549e6457502815b81ec1c1a67cea8360b0d43d08d1465164e09'),
 (6442891248, 6442892455, '6d84d78f54adca5e5ccc9cb3094501ce19bb94dfb2ad087f36c8b96513941636'),
 (6442905728, 6442907287, '4d624881c600a68865a8965f94542cc16cffc9e4103c16ac1c82f1ab82f8dcf3'),
 (6442898112, 6442901782, 'bd472ece3e3c7a5d41de2a62ceaece4a0807d607157548b9362845698c7e5591'),
 (6443046656, 6443049596, '1e787357ebd5ca94970011136f35231b2b5a8a9873bec508c5285a0fc7af9b08')]
for s,e,h in full_bounds:
 raw=b.data[b.off(s):b.off(e)];assert hashlib.sha256(raw).hexdigest()==h,(hex(s),hex(e))
 xs=list(md.disasm(raw,s));assert xs[-1].address+xs[-1].size==e
 full.append(dict(start=hex(s),end=hex(e),sha256=h,extentSource='complete contiguous Ghidra body; not first PE unwind fragment'))
report=dict(peSha256=hashlib.sha256(b.data).hexdigest(),regions=regions,fullFunctions=full,pins=pin_count,
 scope='new library-owned XY root,kind0/C49 full current views,checked06B7F0 matching and exact identity exclusion only',
 pending=['native SDK page observers/history','source tag/C51 registry counter','actual037E80 caller range and result binding','DAF input32 source rows','026EF0/prior/cache/final2E7 handoff'],vendorExecution=False,documentAcceptance=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print(f'PASS{len(regions)} static PE regions/{len(full)} full-function hashes/{pin_count} exactpins; C32XY/C49/currentcoverage/identity; no vendor execution.')
