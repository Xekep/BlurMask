"""Static sourceV90/VA0 remainder production and normal1A9AE0 export."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins=[]
for a,z,sha in [(0x1801a8e60,0x1801a9070,'60343601da38a868f3ff3f3790ac48fc2331e6df96be4ce8a29cf88050feeeec'),
 (0x1801a9ae0,0x1801aa470,'f1c1d83c50d0d8a9dd275272dc40d1b454c5dd5958a624dfca35bf5579c8de55')]:
 assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==sha;pins.append([hex(a),hex(z),sha])
m=Cs(CS_ARCH_X86,CS_MODE_64);count=0
def exact(a,z,wanted):
 global count
 got=[(i.mnemonic,i.op_str) for i in m.disasm(b.data[b.off(a):b.off(z)],a)]
 assert got==wanted,(hex(a),got);count+=len(got)
exact(0x1801a8f62,0x1801a8f82,[('lea','rcx, [rsi + 0x19]'),('mov','r10, qword ptr [rax + 0x90]'),('mov','r14d, r12d'),('shl','r14, 5'),('add','rcx, r14'),('mov','qword ptr [rsp + 0x20], rcx'),('mov','rcx, rbp'),('call','r10')])
exact(0x1801a8f86,0x1801a8fb9,[('lea','rcx, [rsi + 0x1a]'),('mov','rax, qword ptr [r13 + 0x170]'),('add','rcx, r14'),('mov','r9d, dword ptr [rbx + rdi*4 + 8]'),('mov','r8d, dword ptr [rbx + rdi*4]'),('dec','r9d'),('mov','rdx, qword ptr [r13 + 0x168]'),('mov','r10, qword ptr [rax + 0xa0]'),('mov','qword ptr [rsp + 0x20], rcx'),('mov','rcx, rbp'),('call','r10')])
exact(0x1801a8fbe,0x1801a8ff1,[('lea','rcx, [rsi + 0x1b]'),('mov','rax, qword ptr [r13 + 0x170]'),('add','rcx, r14'),('mov','r9d, dword ptr [rbx + rdi*4 + 8]'),('mov','r8d, dword ptr [rbx + rdi*4]'),('dec','r9d'),('mov','rdx, qword ptr [r13 + 0x168]'),('mov','r10, qword ptr [rax + 0x90]'),('mov','qword ptr [rsp + 0x20], rcx'),('mov','rcx, rbp'),('call','r10')])
exact(0x1801aa404,0x1801aa42d,[('movzx','eax, byte ptr [r9 + 0x19]'),('lea','r8d, [r12 + r12]'),('mov','byte ptr [r8 + r11], al'),('lea','ecx, [r8 + 1]'),('movzx','edx, byte ptr [r9 + 0x18]'),('mov','r8, qword ptr [rbp + 0x77]'),('not','dl'),('and','edx, 1'),('movzx','eax, byte ptr [rdx + r9 + 0x1a]'),('mov','byte ptr [rcx + r11], al')])
report=dict(pins=pins,exactInstructions=count,scope='actual normal forward source-key and remainder export, not completeV140/lazy217670',endpointSelection='first19;tail1A/nonTail1B',vendorExecution=False)
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((json.dumps(report,indent=2)+'\n').encode());print(f'PASS {count} exact instructions/two native ranges; actual nonzero endpoint metadata, no vendor execution.')
