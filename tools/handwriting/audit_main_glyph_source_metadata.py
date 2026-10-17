"""Static ordinary1AA470/V140 target/base identity and source26-byte layout."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins=[]
for a,z,sha in [(0x1801aa470,0x1801aaa60,'622eeeff6e994ec9ccb864f33e15fd66a88008d4920e6f7de3312bf523b7dc5b'),
 (0x180217670,0x180217db0,'8321a0ed9770f4e6f4ff036643ff8d1490ad6989dd616084b739dd5a2c0b176b')]:
 assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==sha;pins.append([hex(a),hex(z),sha])
assert b.qwords(0x1802e1ff0+0x140,1)[0]==0x1801aa470
m=Cs(CS_ARCH_X86,CS_MODE_64);count=0
def exact(a,z,wanted):
 global count
 actual=[(i.mnemonic,i.op_str) for i in m.disasm(b.data[b.off(a):b.off(z)],a)]
 assert actual==wanted,(hex(a),actual);count+=len(actual)
exact(0x1801aa532,0x1801aa53a,[('mov','qword ptr [rbx], r10'),('call','0x1801a9ae0')])
exact(0x1801aa55c,0x1801aa56c,[('cmp','byte ptr [rdx + r15], 0'),('mov','rbp, qword ptr [rdx + r15 + 8]'),('jne','0x1801aa881')])
exact(0x1801aaa2f,0x1801aaa3f,[('mov','rcx, qword ptr [rax + rcx + 0x18]'),('mov','rax, qword ptr [rsp + 0xb8]'),('mov','qword ptr [rax], rcx')])
exact(0x1801aa833,0x1801aa858,[('movzx','eax, byte ptr [r9 + 0x19]'),('lea','r8d, [rdi + rdi]'),('mov','byte ptr [r8 + r11], al'),('lea','ecx, [r8 + 1]'),('movzx','edx, byte ptr [r9 + 0x18]'),('not','dl'),('and','edx, 1'),('movzx','eax, byte ptr [rdx + r9 + 0x1a]'),('mov','byte ptr [rcx + r11], al')])
exact(0x18021786e,0x180217873,[('imul','r8, qword ptr [rbp + 0x30], 0x1a')])
exact(0x18021788f,0x1802178ae,[('mov','rcx, qword ptr [rbp + 0x30]'),('lea','r9, [rsp + 0x90]'),('mov','r8, qword ptr [rbp + 0x10]'),('lea','rdx, [rcx + rcx*2]'),('lea','rcx, [rax + rdx*8]'),('mov','rdx, r15'),('mov','qword ptr [rbp + 0x38], rcx')])
report=dict(exactInstructions=count,pins=pins,binding='ordinary21B.V140=1AA470',identity='target owner source key distinct from higher.originalLast/base scorer key',layout='24*N reference bytes followed by2*N actual endpoint bytes',scope='bounded ordinary base/higher-kind0 metadata; no negative/protocol dispatch, actual217670 allocation/failure/scoring or fullwrapper claim',vendorExecution=False)
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((json.dumps(report,indent=2)+'\n').encode());print(f'PASS{count} exact instructions/two native ranges/21B.V140 binding; target/base/source layout retained.')
