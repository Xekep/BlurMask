"""Pinned static21FFD0 flush/packet write audit; no vendor execution."""
import hashlib,json,re,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();f=functions();m=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins=[];count=0
for a,z,sha in [(0x18021ffd0,0x180221840,'14325c5b19f568cdbafb14eacabd9e6765fe012c2e49478fec697ae09d80637a'),
 (0x18020e230,0x18020e700,'a6f91d5c155561cd32f95d5cb975f4e0c5d60bf6d5ce79118dfe0621a002f33b'),
 (0x180221840,0x180221a50,'317121113ed91d4bacc56d16c246352ceaffc0d6652502bcaf431a152d7a567a')]:
 assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==sha;pins.append([hex(a),hex(z),sha])
def exact(a,z,wanted):
 global count
 got=[(i.mnemonic,i.op_str) for i in m.disasm(b.data[b.off(a):b.off(z)],a)]
 assert got==wanted,(hex(a),got);count+=len(got)
exact(0x1802203ca,0x180220429,[
 ('cmp','qword ptr [rbp - 0x58], 0'),('je','0x180220429'),('mov','eax, dword ptr [r9 + 0x24]'),('cmp','eax, -1'),('je','0x180220429'),
 ('mov','r8, qword ptr [rsi + 0x30]'),('lea','rcx, [rbp + 0xb0]'),('mov','dword ptr [rbp + 0xe4], eax'),('mov','rdx, r15'),
 ('mov','byte ptr [rsp + 0x28], 0'),('mov','rax, qword ptr [r8]'),('mov','r10, qword ptr [rax + 0x70]'),
 ('movzx','eax, word ptr [r9 + 0x1c]'),('mov','r9, qword ptr [r9]'),('mov','word ptr [rsp + 0x20], ax'),('call','r10'),
 ('mov','rdx, qword ptr [rsi + 0x2460]'),('movups','xmm0, xmmword ptr [rax]'),('movups','xmmword ptr [rbp + 0xec], xmm0'),
 ('movzx','eax, byte ptr [rdx + rbx*8 + 0x20]'),('shr','al, 7'),('mov','byte ptr [rbp + 0xfc], al')])
exact(0x1802210a6,0x1802210fa,[
 ('mov','eax, dword ptr [rbp + 0x238]'),('mov','rdx, qword ptr [rsi + 0x20]'),('sub','eax, edi'),('movd','xmm1, eax'),
 ('mov','eax, dword ptr [rsp + 0x70]'),('sub','eax, r13d'),('mov','r13, qword ptr [rbp - 0x70]'),('shl','eax, 0xd'),
 ('mov','rcx, r13'),('cvtdq2ps','xmm1, xmm1'),('movd','xmm0, eax'),('mov','eax, dword ptr [rbp + 0x240]'),
 ('cvtdq2ps','xmm0, xmm0'),('sub','eax, r15d'),('divss','xmm1, xmm0'),('movd','xmm0, eax'),
 ('movss','dword ptr [rbp + 0xd4], xmm1'),('cvtdq2ps','xmm0, xmm0'),('mulss','xmm0, xmm10'),('movss','dword ptr [rbp + 0xd8], xmm0')])
exact(0x1802218ab,0x1802218b7,[('movzx','r8d, word ptr [rcx + 0x32]'),('mov','edx, dword ptr [rcx + 0x3c]'),('add','rcx, 0x44')])
exact(0x180220166,0x18022018b,[('movzx','eax, word ptr [rbp + 0x248]'),('movaps','xmm0, xmmword ptr [r14]'),
 ('mov','rdx, qword ptr [rsi + 0x2460]'),('mov','word ptr [rbp + 0xde], ax'),('mov','eax, dword ptr [rbp + 0x250]'),('mov','dword ptr [rbp + 0xe4], eax')])
exact(0x180220ee0,0x180220ef4,[('lea','eax, [rdi + 7]'),('mov','edx, 0xfffd'),('test','dx, ax'),('je','0x180220f09'),('cmp','byte ptr [rsi + 0x22f8], 0')])
exact(0x180221262,0x18022127a,[('mov','rax, qword ptr [rsi + 0x2498]'),('movzx','ecx, word ptr [rax + 0x38]'),
 ('mov','word ptr [rbp + 0xfe], cx'),('mov','dword ptr [rbp + 0x10c], ebx')])
exact(0x18020e265,0x18020e277,[('mov','dword ptr [rdx + 0x25f8], esi'),('cmp','dword ptr [rdx + 0x2558], esi'),('je','0x18020e660')])
exact(0x18020e298,0x18020e2a8,[('and','eax, 0x1e00'),('cmp','eax, 0xa00'),('je','0x18020e355')])
s=f['18021ffd0'];rules=[
 'local_128 = (ulonglong)param_9 << 0x20;', 'uStack_fc = 0xffffffff;',
 'uStack_11c = (undefined4)*param_10;', 'uStack_110 = (undefined4)((ulonglong)param_10[1] >> 0x20);',
 'bStack_10c = 0;', '== -0xb', '== -6', 'local_138 = 1.0;',
 'fStack_134 = 0.0;', 'uStack_12c = 0xffff;', 'uStack_fc = uVar9;',
 'local_298 = (uint)local_258;', 'uStack_10a = *(undefined2 *)(*(longlong *)(param_2 + 0x2498) + 0x38);']
for rule in rules:assert rule in s,rule
writers={key for key,body in f.items() if re.search(r'\*\([^\n]*param_2 \+ 0x25f8\) =',body)}
assert writers=={'18020baa0','18020e230'},writers
# Frontend168/170 ordinary graph B8/C0 bindings are pinned by the earlier
# numeric audit; no coordinates or source IDs are inferred from glyph labels.
assert b.qwords(0x1802de610+0x168,1)[0]==0x180152a10
assert b.qwords(0x1802de610+0x170,1)[0]==0x180152a30
report=dict(exactInstructions=count,pins=pins,decompiledContracts=len(rules),source25F8Writers=sorted(writers),
 source70='only compact+0x24 != -1; fulltag, raw U16+0x1c, byte boundary0',
 packetSeed='WORD32->packet0E;WORD3C->packet14;WORD44..53->packet1C..2B;packet3C initially literal-1',
 scope='fresh ordinary CASE flush plan; no callback, association, pool or publication',
 unresolved=['source0 metadata','class12 extra flush policy','unflushed terminal tail','actual source70 event fixture','complete21FFD0'])
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print(f'PASS {count} exact instructions/3 native ranges/{len(rules)} contracts/2 boundary bindings; no vendor execution.')
