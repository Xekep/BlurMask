"""Pinned static-PE ordinary local graph metadata audit; never executes DLLs."""
import argparse,hashlib,json,pathlib,sys
import capstone
p=argparse.ArgumentParser();p.add_argument('--output',required=True);args=p.parse_args()
root=pathlib.Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=capstone.Cs(capstone.CS_ARCH_X86,capstone.CS_MODE_64)
blocks=[
 (0x1801a3450,0x1801a3593,'e1e9c689ae9f5b646b43d300df6b54038dbcb888ffa93d81b590b7f4fb6d9eaa'),
 (0x1801a35a0,0x1801a37a3,'a4dccccbd472c12005ce8785432678cff0546497943a3ac288b0b91a835dcdd1'),
 (0x1801a3833,0x1801a390f,'db6d17146a486f5d2285b34a952cf7d713b8ec8c8f207e724dd05083cc5bd660'),
 (0x1801a4399,0x1801a44d2,'3d9f1d014f3a295f415bd9f2ff20edc5d270910161991bee88b81c32f8822f3d'),
 (0x1801a6105,0x1801a6112,'e78ed2831fb93494151a44fb13277f6f7429e071f62eb6787a55e9eb82597cb4')]
instructions={}
for a,z,h in blocks:
 raw=b.data[b.off(a):b.off(z)];assert hashlib.sha256(raw).hexdigest()==h
 ins=list(md.disasm(raw,a));assert ins[-1].address+ins[-1].size==z
 instructions.update({i.address:(i.mnemonic,i.op_str) for i in ins})
pins={
0x1801a3490:('sar','eax, 1'),0x1801a349c:('mov','rcx, qword ptr [rax + rdx*8]'),
0x1801a34a0:('mov','eax, dword ptr [rcx]'),0x1801a34a2:('mov','r11d, dword ptr [rcx + 4]'),
0x1801a34c6:('jl','0x1801a34d0'),0x1801a34ce:('jge','0x1801a34db'),
0x1801a34e9:('jg','0x1801a34f3'),0x1801a34f1:('jle','0x1801a34ff'),
0x1801a3502:('jg','0x1801a3557'),0x1801a3531:('movups','xmmword ptr [r8], xmm0'),
0x1801a3544:('movups','xmmword ptr [rcx + rdx], xmm3'),0x1801a3565:('call','0x1801a3450'),
0x1801a364d:('call','0x1800cfcd0'),0x1801a367e:('call','0x1801a3450'),
0x1801a3691:('mov','r9, qword ptr [rax + 0x58]'),
0x1801a383b:('movzx','esi, word ptr [rcx + 0x18]'),0x1801a3844:('movups','xmmword ptr [r15], xmm0'),
0x1801a3848:('movups','xmmword ptr [r15 + 0x10], xmm0'),0x1801a38ae:('mov','word ptr [r15 + 0x18], si'),
0x1801a38c2:('mov','byte ptr [r15 + 4], cl'),0x1801a38ca:('mov','dword ptr [r15 + 0x10], ecx'),
0x1801a38d2:('mov','dword ptr [r15 + 0x14], ecx'),
0x1801a38f1:('cmp','cl, 0xc8'),0x1801a38f4:('jae','0x1801a38fc'),0x1801a38f6:('inc','cl'),
0x1801a43c8:('movups','xmmword ptr [rax], xmm0'),0x1801a43db:('movups','xmmword ptr [rax + 0x50], xmm0'),
0x1801a4425:('mov','word ptr [rdi + rbx], ax'),0x1801a4434:('mov','eax, 0xffff'),
0x1801a4439:('mov','word ptr [rdi + rbx + 2], ax'),0x1801a443e:('mov','word ptr [rdi + rbx + 6], 0'),
0x1801a444d:('mov','byte ptr [rdi + rbx + 8], 0'),0x1801a4452:('movups','xmmword ptr [rdi + rbx + 0x10], xmm0'),
0x1801a445f:('mov','byte ptr [rdi + rbx + 0x50], 0'),0x1801a446a:('movups','xmmword ptr [rdi + rbx + 0x20], xmm0'),
0x1801a4482:('mov','byte ptr [rdi + rbx + 0x51], al'),0x1801a4486:('movzx','eax, word ptr [rdx + 4]'),
0x1801a448a:('mov','word ptr [rdi + rbx + 4], ax'),0x1801a44ce:('mov','byte ptr [rdi + rbx + 0x52], al'),
0x1801a6109:('cmp','al, 0xc8'),0x1801a610b:('jae','0x1801a6112'),0x1801a610d:('dec','al')}
for address,pin in pins.items():assert instructions[address]==pin,(hex(address),instructions.get(address),pin)
report=dict(blocks=[dict(start=hex(a),endExclusive=hex(z),sha256=h) for a,z,h in blocks],
 pins=[f'{a:x} {m} {o}' for a,(m,o) in pins.items()],
 semantics='Signed first-range physical/start middle-pivot quicksort incl equal swaps; concatenate histories; zeroed merged ordinary header/arc, left descriptor/auxiliary/context inheritance, placement0, saturated reference byte and ordered deletions.',
 upstream='Actual19F850 initial header, pending19EB10, source70/80/F0 fresh ownership and seed topology are separately pinned by descriptor/append/source metadata gates.',
 scope='Actual ordinary local same-source noNN allocation and mode0 pruning, then19D470 append only; cross-source merge/boundary pruning/global NN graph remain absent.')
pathlib.Path(args.output).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS static {len(pins)} local graph allocation/history/reference pins in{len(blocks)} exact Text PE blocks')
