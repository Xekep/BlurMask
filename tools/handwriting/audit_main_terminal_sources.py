"""Pinned static0FE2A0 producer audit. Never loads or invokes vendor code."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
b=Binary();fs=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
checks={
 'readerCounts':('1800fcc10','*(longlong *)(param_2 + 0xd0) = lVar2;'),
 'readerTables':('1800fcc10','*(longlong *)(param_2 + 0xd8) = lVar2;'),
 'wordSeparate':('1800fcc10','*(longlong *)(param_2 + 0xa8) = lVar2;'),
 'profileIndex':('1800fe2a0','param_3 = 0x137 - param_3;'),
 'lastUnigram':('1800fe2a0','(ulonglong)(**(int **)(param_2 + 0xd0) - 1) * 4'),
 'unigramOffset':('1800fe2a0','*(longlong *)(param_2 + 0xf0 + lVar26 * 8)'),
 'unigramSlope':('1800fe2a0','*(longlong *)(param_2 + 0x130 + lVar26 * 8)'),
 'cache2534':('1800fe2a0','piVar13[0x94d] = (int)(((float)iVar3 * 0.00012207031 * fVar1 - fVar2) * 8192.0);'),
 'loadedScale':('180092990','*(undefined4 *)((longlong)plVar13 + 0x6c)'),
 'sourceKind':('180092990','*param_3 + 0x58)'),
}
for name,(address,fragment) in checks.items():assert fragment in fs[address],name
_,descriptor=b.descriptor(0x2d1);table=b.qwords(descriptor[3]+8,1)[0]
assert b.qwords(table+0x48,1)[0]==0x1800fe2a0
assert b.qwords(table+0x68,1)[0]==0x1800f7a10
md=Cs(CS_ARCH_X86,CS_MODE_64)
expected=[('mov','rax, qword ptr [rsi + 0xd0]'),('mov','edx, dword ptr [rax]'),
 ('mov','rax, qword ptr [rsi + 0xd8]'),('dec','edx'),('mov','rcx, qword ptr [rax]'),
 ('movd','xmm0, dword ptr [rcx + rdx*4]'),('mov','rcx, qword ptr [rsp + 0x58]'),
 ('cvtdq2ps','xmm0, xmm0'),('mov','rdx, rcx'),('mulss','xmm0, xmm8'),
 ('mulss','xmm0, dword ptr [rdi]'),('subss','xmm0, dword ptr [rcx]'),
 ('mov','qword ptr [r13 + 0x10], r15'),('mov','rcx, r14'),
 ('mov','qword ptr [r13 + 0x28], r12'),('mulss','xmm0, xmm6'),
 ('cvttss2si','eax, xmm0'),('mov','dword ptr [r13 + 0x2534], eax')]
actual=list(md.disasm(b.data[b.off(0x1800ff5d2):b.off(0x1800ff61e)],0x1800ff5d2))
assert [(i.mnemonic,i.op_str) for i in actual]==expected
out=dict(decompilerChecks=len(checks),assemblyInstructions=len(expected),
         producer='2D1.V48=0FE2A0',consumer='2D1.V68=0F7A10 ->2257C0',
         level='D8[0][D0[0]-1], final unigram codebook level; not a word/class ordinal',
         scope='positive finite selected139 scale; actual source135/profile2; no terminal invocation/options/rescue inference')
if len(sys.argv)!=2:raise ValueError('output.json')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
p.write_bytes((json.dumps(out,indent=2)+'\n').encode('utf-8'))
print(f'PASS{len(checks)} static producer/caller bindings and{len(expected)} exact scalar publication instructions; no vendor execution')
