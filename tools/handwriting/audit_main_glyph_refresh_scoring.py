"""Static actual ordinary217670 scoring/preparation contract, no vendor execution."""
import hashlib,json,subprocess,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('output.json')
output=Path(sys.argv[1]);output.parent.mkdir(parents=True,exist_ok=True)
preparation=output.with_name(output.stem+'-preparation.json')
subprocess.run([sys.executable,str(ROOT/'tools/handwriting/audit_main_lazy_preparation.py'),str(preparation)],check=True)
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=Cs(CS_ARCH_X86,CS_MODE_64);pins=0
regions=[
 (0x180217670,0x180217db0,'8321a0ed9770f4e6f4ff036643ff8d1490ad6989dd616084b739dd5a2c0b176b'),
 (0x1801c1300,0x1801c13a0,'d553ea2e1172201343c1b0a18510f24bdd96dc4f3407750aa0695bacf7d88113'),
 (0x1801c13a0,0x1801c1520,'b9319d885c0b3698680031ce1204f244b2fd3be28a8c2ff3582c388a74b932fa'),
 (0x1801c1520,0x1801c156c,'cd9621ec4d7eaacccdc3dc86127a3932bbb12da5184dfd9ce76c9b850d86701b')]
for start,end,expected in regions:assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()==expected
def exact(a,z,wanted):
 global pins
 actual=[(i.mnemonic,i.op_str) for i in md.disasm(b.data[b.off(a):b.off(z)],a)]
 assert actual==wanted,(hex(a),actual);pins+=len(actual)
exact(0x18021790f,0x180217925,[('movzx','r9d, cx'),('mov','rcx, rsi'),('mov','r10, qword ptr [rax + 0x158]'),('call','r10'),('movss','dword ptr [rbp + 0x1c], xmm0')])
exact(0x180217af7,0x180217b09,[('mov','r10, qword ptr [rcx + 0x1b8]'),('mov','rcx, rsi'),('call','r10'),('movss','dword ptr [rdi + 0x10], xmm0')])
exact(0x180217b20,0x180217b6f,[
 ('cmp','bp, word ptr [rbx + 0x24dc]'),('jne','0x180217b2d'),('mov','al, 0xff'),('jmp','0x180217b6f'),
 ('mov','rax, qword ptr [rbx + 0x2460]'),('mov','r11, qword ptr [rsp + 0xa8]'),('test','byte ptr [rax + r14*8 + 0x20], 0x40'),('je','0x180217b5d'),
 ('mov','rax, qword ptr [r11 + 0x58]'),('movsx','rdx, byte ptr [rbp + rax]'),('cmp','dl, 0xff'),('je','0x180217b5d'),
 ('mov','rax, qword ptr [r11 + 0x78]'),('movzx','eax, byte ptr [rdx + rax]'),('jmp','0x180217b6f'),
 ('mov','rax, qword ptr [r11 + 0x50]'),('movsx','r8, byte ptr [rax + rbp]'),('mov','rax, qword ptr [r11 + 0x78]'),('movzx','eax, byte ptr [r8 + rax]')])
exact(0x180217b6f,0x180217bc7,[
 ('mov','rcx, qword ptr [rsp + 0x98]'),('lea','r8, [rsp + 0xc0]'),('mov','word ptr [rsp + 0xc4], cx'),('mov','rdx, r9'),('mov','ecx, 0xffff'),
 ('mov','byte ptr [rsp + 0xca], al'),('mov','word ptr [rsp + 0xc6], cx'),('xor','eax, eax'),('mov','rcx, rsi'),
 ('mov','dword ptr [rsp + 0xc0], r13d'),('mov','word ptr [rsp + 0xc8], bp'),('mov','byte ptr [rsp + 0xcb], 0'),('mov','dword ptr [rsp + 0xcc], eax'),('call','r10'),('jmp','0x180217bcc')])
exact(0x1801c1550,0x1801c155d,[('mov','rax, qword ptr [rcx + rax + 0x20]'),('mov','rcx, qword ptr [rax + 0x10]'),('movss','xmm0, dword ptr [rcx]')])
exact(0x1801c14ef,0x1801c14fb,[('movsx','rdx, byte ptr [rdi + 0xa]'),('mov','rcx, qword ptr [r8 + 0x10]'),('add','eax, dword ptr [rcx + rdx*4]')])
bindings=[(0x1802de610,0x158,0x1801529c0),(0x1802de610,0x1b8,0x180152f50),(0x1802de610,0x1c0,0x180152f90),
          (0x1802e4180,0x90,0x1801c1520),(0x1802e4180,0x80,0x1801c1300),(0x1802e4180,0x88,0x1801c13a0)]
for table,slot,target in bindings:assert b.qwords(table+slot,1)[0]==target
report=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in regions],exactInstructions=pins,
 bindings=len(bindings),preparation=json.loads(preparation.read_text()),
 scope='same actual immutable main source; explicit normalized216860 symbol; no descriptor/pool mutation')
output.write_bytes((json.dumps(report,indent=2)+'\n').encode())
print(f'PASS {len(regions)} scoring regions/{pins} exact instructions/{len(bindings)} bindings plus scoped preparation proof.')
