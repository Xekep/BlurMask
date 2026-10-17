"""Read-only pinned13A830: real COMISS flags, stored identity, capacity and marker."""
import hashlib,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();pe_sha=hashlib.sha256(b.data).hexdigest()
assert pe_sha=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x18013a830,0x18013aba0
sha=hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest();assert sha=='9381032ff21ab1220e8bb50266882281a4f17528c00ba036a572e1c13bc940eb'
m=Cs(CS_ARCH_X86,CS_MODE_64);count=0
def exact(a,z,expected):
 global count
 actual=[(i.mnemonic,i.op_str) for i in m.disasm(b.data[b.off(a):b.off(z)],a)]
 assert actual==expected,(hex(a),actual);count+=len(actual)
exact(0x18013a8a0,0x18013a8b8,[('movzx','eax, word ptr [r9 + 0x18]'),('shl','ax, 2'),('movsx','ecx, ax'),('sar','ecx, 4'),('cmp','r13d, ecx'),('jge','0x18013ab20')])
exact(0x18013a98a,0x18013a991,[('comiss','xmm0, dword ptr [rdx + rax*8 + 0xc]'),('ja','0x18013a998')])
exact(0x18013aa7b,0x18013aa82,[('comiss','xmm0, dword ptr [rdx + rcx*8 + 0xc]'),('jbe','0x18013aaf4')])
exact(0x18013aaab,0x18013aab2,[('comiss','xmm0, dword ptr [rdx + rax*8 + 0xc]'),('ja','0x18013aaba')])
exact(0x18013a91f,0x18013a935,[('movzx','ecx, word ptr [rdx + r10*8 + 6]'),('movzx','eax, word ptr [r14 + rdx + 6]'),('xor','ecx, eax'),('test','ecx, 0xffffff00'),('je','0x18013a93d')])
exact(0x18013a9c0,0x18013a9d5,[('cvtsi2sd','xmm0, rax'),('mulsd','xmm0, xmm6'),('cvttsd2si','rax, xmm0'),('cmp','eax, r11d'),('cmovae','r11d, eax')])
assert struct.unpack_from('<d',b.data,b.off(0x1802ed2c0))[0]==1.5
exact(0x18013a9ea,0x18013a9f8,[('call','r10'),('test','rax, rax'),('je','0x18013aa55'),('mov','qword ptr [rdi], rax'),('mov','dword ptr [rdi + 0xc], esi')])
exact(0x18013ab20,0x18013ab28,[('test','r12b, r12b'),('je','0x18013ab5c'),('mov','edx, dword ptr [rdi + 8]')])
exact(0x18013ab46,0x18013ab5c,[('mov','r8, qword ptr [rsp + 0xb0]'),('movzx','eax, r12b'),('test','r8, r8'),('je','0x18013ab60'),('mov','dword ptr [r8], r10d'),('jmp','0x18013ab60')])
assert b.qwords(0x1802dc680+0x60,1)[0]==a
report=dict(PEsha256=pe_sha,pins=[[hex(a),hex(z),sha]],exactInstructions=count,
 comparison='COMISS/JA inserts strictly greater only; COMISS/JBE ignores unordered duplicate',
 growth='max(count+1,trunc(unsignedCapacity*1.5)); realloc failure precedes insertion/capacity change',
 marker='writeback only on overall success; optionalNULL unchanged',
 scope='prepared owner selection; no217670 lazy-refresh or final-wrapper claim; static bytes only')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print(f'PASS {count} exact instructions/one pinned native range and274.V60 binding, no vendor execution.')
