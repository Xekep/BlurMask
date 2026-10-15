"""Static fresh00ED90/00F460 ->2271F0 ->2257C0 call audit; no DLL execution."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
b=Binary();fs=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
checks={
 'fresh98':('18000c860','*(undefined8 *)(param_2 + 0x98) = 0;'),
 'setterOverride':('18000dd60','*(char *)(param_2 + 0x9c) = cVar3;'),
 'normalCaller':('18000ed90','FUN_18000f460(param_1,param_2,*(undefined1 *)(param_2 + 0x9c),0)'),
 'actualResultInvocation':('18000f460','*(undefined8 *)(*(longlong *)(param_2 + 0x78) + 0x10),1,param_3,param_4)'),
 'conditionalFilter':('1802271f0','local_214 = (uint)param_5;'),
 'flagEc':('1802271f0','local_238 = 0.0 < *(float *)(*(longlong *)(param_2 + 0x18) + 0xec);'),
 'loopBound':('1802271f0','(int)uVar18 < (int)(uint)uVar1) || (local_238'),
 'emptyEndSkip':('1802271f0','if ((uint)uVar15 != local_210)'),
 'required6':('1802271f0','uVar18 == uVar1'),
 'rescueRequested7':('1802271f0','uVar18 == 0'),
 'native7Gate':('1802257c0',"(param_7 != '\\0') && (*(char *)(param_2 + 0x22f6) == '\\0')"),
 'native7SourceGate':('1802257c0','(*(uint *)(param_2 + 0x22e8) & 0x20) != 0'),
 'onlyClear8':('1802257c0','*param_8 = 0;'),
 'sentinelMarker':('18020c1c0','&DAT_1802eb3a0,4'),
}
for name,(a,t) in checks.items():assert t in fs[a],name
#The scoped single-alternative branch cannot turnEC0's in/outflagtrue.
assert fs['1802257c0'].count('*param_8 =')==1
md=Cs(CS_ARCH_X86,CS_MODE_64)
def instructions(start,end):return [(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(start):b.off(end)],start)]
assert instructions(0x180227325,0x18022733f)==[
 ('movzx','ecx, byte ptr [rbp + 0x1e0]'),('movzx','r15d, al'),('xor','r12d, r12d'),
 ('test','r15b, r15b'),('mov','eax, r12d'),('cmove','eax, ecx'),('mov','dword ptr [rbp - 0x5c], eax')]
assert instructions(0x180227570,0x180227593)==[
 ('movzx','ebx, word ptr [r14 + 0x2404]'),('cmp','r15d, ebx'),('jl','0x180227587'),
 ('cmp','byte ptr [rbp - 0x80], r12b'),('je','0x18022795d'),('test','r15d, r15d'),
 ('sete','cl'),('cmp','r15d, ebx'),('sete','dl')]
assert b.qwords(0x1802e7cb0+0x80,1)[0]==0x1802271f0
assert b.data[b.off(0x1802eb3a0):b.off(0x1802eb3a0)+4]==chr(0xf0009).encode('utf-8')
assert b.qwords(0x1802de610+0x240,1)[0]==0x180152b40
assert instructions(0x180152b40,0x180152b51)==[
 ('mov','rdx, qword ptr [rdx + 0x70]'),('mov','rax, qword ptr [rdx]'),
 ('mov','r10, qword ptr [rax + 0x160]'),('jmp','r10')]
assert b.qwords(0x1802e1ff0+0x160,1)[0]==0x1801a97a0
out=dict(checks=len(checks),assemblyInstructions=20,context='fresh DIRECT/no-external-property NEW2B3 normal00ED90 request',
         native7=True,rescue='inactive becauseactualowner22E8.bit20clear',
         maximumInvocations=1,flags='9Cfalse makesFilter4false underbothV240 outcomes; EC0 leavesResult8zero; index0requires6false',
         unresolved='Whole21B token-stream ownership, reused/property/case paths and actual2520/currentnode records are separate')
if len(sys.argv)!=2:raise ValueError('output.json')
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((json.dumps(out,indent=2)+'\n').encode('utf-8'))
print(f'PASS{len(checks)} caller/fresh-policy bindings +20 exact conditional/loop/frontend instructions; native7true gate scopedinactive')
