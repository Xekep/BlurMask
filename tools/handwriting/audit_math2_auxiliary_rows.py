"""041EC0 assigned-field proof; no invented native padding."""
from pathlib import Path
import hashlib,json,sys
from capstone import Cs,CS_ARCH_X86,CS_MODE_64,CS_AC_WRITE
from capstone.x86 import X86_OP_MEM,X86_REG_RSP
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary('MyScriptDocument.dll');assert hashlib.sha256(b.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
a,z=0x180041ec0,0x180042100;raw=b.data[b.off(a):b.off(z)]
assert hashlib.sha256(raw).hexdigest()=='71dbd4cfc33c068a2ffcba976a6541d1bffc13ef9b62491840957fe51d4e49b6'
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True
decoded=list(md.disasm(raw,a));ins={i.address:(i.mnemonic,i.op_str) for i in decoded}
pins=[(6442720979, 'xor', 'ebp, ebp'), (6442720987, 'mov', 'word ptr [rsp + 0x20], bp'), (6442720992, 'imul', 'ebx, dword ptr [r9 + 8]'), (6442721003, 'mov', 'dword ptr [rsp + 0x24], 0xff7fffff'), (6442721088, 'mov', 'rbx, qword ptr [rsp + 0x20]'), (6442721152, 'cmp', 'byte ptr [r14 + rax], 0'), (6442721157, 'je', '0x1800420bf'), (6442721163, 'mov', 'ebp, r14d'), (6442721166, 'imul', 'ebp, r13d'), (6442721188, 'mov', 'r15d, dword ptr [rdi + 0xa0]'), (6442721195, 'add', 'r15d, r13d'), (6442721328, 'call', '0x1801ac820'), (6442721354, 'shl', 'r8, 3'), (6442721358, 'call', '0x1801ac460'), (6442721363, 'add', 'dword ptr [rdi + 0xa0], r13d'), (6442721392, 'cmp', 'rdx, r13'), (6442721397, 'test', 'rdx, rdx'), (6442721400, 'je', '0x180042098'), (6442721416, 'mov', 'qword ptr [rax + rcx*8], rbx'), (6442721446, 'movzx', 'eax, word ptr [rax + r14*4]'), (6442721451, 'mov', 'word ptr [rcx + rsi*8], ax'), (6442721462, 'mov', 'dword ptr [rax + rsi*8 + 4], ebp'), (6442721471, 'inc', 'r14'), (6442721474, 'add', 'rsi, rcx')]
for address,m,o in pins:assert ins[address]==(m,o),(hex(address),ins.get(address))
stackwrites=[]
for i in decoded:
 if not 0x180041ed3<=i.address<0x1800420f0:continue
 for op in i.operands:
  if op.type==X86_OP_MEM and op.access&CS_AC_WRITE and op.mem.base==X86_REG_RSP:
   first=op.mem.disp;last=first+op.size;stackwrites.append([i.address,first,last])
   assert not(first<0x24 and last>0x22),(hex(i.address),i.op_str)
out=Path(sys.argv[1]).resolve();out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(wholeRegion=[a,z,hashlib.sha256(raw).hexdigest()],instructionPins=len(pins),
 fixedFrameExplicitStackWrites=stackwrites,templateKnownBits='FFFFFFFF0000FFFF',templateDefinedValue='FF7FFFFF00000000',
 semantics=['mask nonzero inserts width rows at unitIndex*width, shifts existing tail and zeros block',
 'first row writes lowU16 output label and score0; zero padding remains from memset',
 'later rows copy local QWORD: lowU16 zero, F32 -FMAX, middleU16 without defining write',
 'preexisting rows retain all original fields; no full native image with unknown padding'],
 adaptation='Prepared consistent block counts and copied semantic fields; width0..128 and1000 units are explicit library bounds, no native allocator-prefix claim',
 missing=['actual auxiliary98/A0 producer','full record EF4 integration'],vendorExecution=False),indent=2)+'\n',encoding='utf8')
print(f'PASS complete041EC0/{len(pins)} pins/no explicit write to template padding word; unknown padding retained')
