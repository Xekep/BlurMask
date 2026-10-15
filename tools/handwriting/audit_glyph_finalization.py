"""Pinned2173E0 nontracing finalization audit; no native code is executed."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('output.json')
b=Binary();fs=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
sha=hashlib.sha256(b.data).hexdigest()
assert sha=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
count=0
def exact(start,end,wanted):
    global count
    actual=[(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(start):b.off(end)],start)]
    assert actual==wanted,(hex(start),actual)
    count+=len(wanted)
exact(0x180217432,0x180217437,[('mov','ebx, 0x100')])
exact(0x18021743e,0x180217445,[('mov','dword ptr [rsp + 0x98], ebx')])
exact(0x1802174ce,0x1802174d3,[('call','0x180216bc0')])
exact(0x1802174d3,0x180217513,[
 ('test','al, al'),('je','0x18021764a'),('movsxd','rcx, dword ptr [rsp + 0x98]'),
 ('movzx','edx, byte ptr [rsp + 0x90]'),('test','ecx, ecx'),('jns','0x1802174fe'),
 ('test','dl, dl'),('je','0x1802174fe'),('movzx','r9d, word ptr [rsp + 0xb0]'),
 ('jmp','0x180217513'),('cmp','ecx, ebx'),('jge','0x18021750d'),
 ('movzx','r9d, word ptr [rsp + rcx*2 + 0xb0]'),('jmp','0x180217513'),('mov','r9d, 0xffff')])
exact(0x180217517,0x180217546,[
 ('sub','ebx, ecx'),('and','rax, 0xfffffffffffffffe'),('mov','r8, r15'),('mov','rcx, rdi'),
 ('mov','rax, qword ptr [rax + 8]'),('mov','r10, qword ptr [rax + 0x30]'),
 ('movzx','eax, byte ptr [rsp + 0x91]'),('mov','byte ptr [rsp + 0x30], al'),
 ('mov','byte ptr [rsp + 0x28], dl'),('mov','rdx, qword ptr [r14 + 0x48]'),
 ('mov','dword ptr [rsp + 0x20], ebx'),('call','r10')])
#A failed13AF30 call jumps directly to false, before lookup/score writes.
exact(0x180217546,0x18021754e,[('test','al, al'),('je','0x18021764a')])
exact(0x18021764a,0x18021764c,[('xor','al, al')])
exact(0x18021756b,0x1802175c4,[
 ('movss','xmm0, dword ptr [rsp + 0x9c]'),('mov','r8, rax'),('xor','edx, edx'),
 ('movss','dword ptr [rax + 0xc], xmm0'),('movss','xmm1, dword ptr [rsp + 0xa0]'),
 ('movss','dword ptr [rax + 0x10], xmm1'),('mov','ecx, dword ptr [rsp + 0xa4]'),
 ('mov','dword ptr [rax + 0x14], ecx'),('mov','eax, 8'),('cmp','byte ptr [rbp + 0x3d], dl'),
 ('movzx','ecx, word ptr [r8 + 4]'),('cmova','dx, ax'),('mov','eax, 0xfff7'),
 ('and','cx, ax'),('or','dx, cx'),('mov','word ptr [r8 + 4], dx'),
 ('movzx','eax, word ptr [rsp + 0x94]'),('mov','word ptr [r8 + 6], ax')])
exact(0x1802175c4,0x1802175e3,[
 ('cmp','byte ptr [rsi + 0x2368], 0'),('je','0x180217646'),
 ('movsxd','rax, dword ptr [rbp + 8]'),('mov','rdx, qword ptr [r8 + 0x18]'),
 ('imul','rcx, rax, 0x288'),('add','rcx, qword ptr [rsi + 0x2468]')])
#13AF30's sixth stack argument is componentFlag; seventh is externalVariant.
exact(0x18013af94,0x18013afa3,[('cmp','byte ptr [rsp + 0x38], 0'),
 ('je','0x18013afa3'),('mov','r8d, 4'),('jmp','0x18013b000')])
exact(0x18013afce,0x18013afd7,[('movzx','ecx, byte ptr [rsp + 0x30]'),('dec','r9w')])
assert b.qwords(0x1802dc680+0x28,1)[0]==0x18013ae90
assert b.qwords(0x1802dc680+0x30,1)[0]==0x18013af30
s=fs['1802173e0']
for text in ['FUN_180216bc0','local_260 = 0x100;','0x100 - local_260',
             'local_267[0]','local_268','param_4 + 0x3d','*(ushort *)(lVar3 + 6) = local_264[0]']:
    assert text in s,text
out=dict(sourceSha256=sha,exactInstructions=count,poolBindings=2,preparedProducer='216BC0 result only',
         implemented='2173E0 nontracing symbol/score/mismatch/publication finalization',
         trace='unsupported: stride0x288=648,640-byte copy',
         remaining='216BC0 producer;2498 rows;217DB0 residual association;218560 chain orchestration;full21FFD0')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
p.write_bytes((json.dumps(out,indent=2)+'\n').encode('utf-8'))
print(f'PASS {count} exact2173E0/13AF30 instructions,2 pool bindings and7 prepared-output contracts; no vendor execution')
