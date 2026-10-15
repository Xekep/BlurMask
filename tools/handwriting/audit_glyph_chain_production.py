"""Pinned216BC0 protocol caller and218560 chain writes; static PE only."""
from pathlib import Path
import hashlib,json,sys
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('output.json')
b=Binary();f=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
controls=0;pins=[]
def exact(a,z,wanted):
 global controls
 actual=[(i.mnemonic,i.op_str)for i in md.disasm(b.data[b.off(a):b.off(z)],a)];assert actual==wanted,(hex(a),actual);controls+=len(actual)
def pin(a,z,expected):
 actual=hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest();assert actual==expected
 pins.append(dict(start=hex(a),end=hex(z),sha256=actual))
pin(0x180218560,0x1802188e1,'9b322a862d1f0a9b22edef7d3eff93c8ff94be6c7853d13c69e71dba89468744')
pin(0x180216bc0,0x1802173e0,'84da9213fea4000119cd81f85d9f1f73376012b0362565b8e2435ad1a4b2f70f')
exact(0x18021860a,0x180218632,[('cmp','byte ptr [r14 + 0x44], 0'),('mov','ebp, dword ptr [r14 + 8]'),('mov','r13d, dword ptr [r14 + 0xc]'),('mov','rdi, qword ptr [r14 + 0x30]'),('jne','0x180218630'),('test','rcx, rdi'),('jne','0x180218630'),('cmp','byte ptr [rsp + 0x108], 0'),('jne','0x180218630'),('xor','dl, dl'),('jmp','0x180218632'),('mov','dl, 1')])
exact(0x180218632,0x18021864a,[('movsxd','rcx, dword ptr [r14]'),('mov','r9, rdi'),('mov','rax, qword ptr [rsp + 0xc8]'),('mov','rax, qword ptr [rax + 0x2478]'),('sub','ebp, dword ptr [rax + rcx*4]')])
exact(0x180218719,0x180218730,[('mov','rcx, qword ptr [rsp + 0x58]'),('cmp','rcx, -1'),('je','0x18021878e'),('cmp','r13d, -1'),('je','0x18021878e'),('cmp','qword ptr [rax + 0x20], rcx'),('je','0x18021878e')])
exact(0x1802187bd,0x1802187ce,[('mov','rcx, qword ptr [rsp + 0x58]'),('mov','byte ptr [rsp + 0x108], 1'),('mov','qword ptr [rax + 0x20], rcx')])
exact(0x1802187e4,0x180218808,[('test','r15, r15'),('jle','0x180218800'),('mov','rax, qword ptr [rsp + 0xc8]'),('mov','rax, qword ptr [rax + 0x2498]'),('movzx','ecx, word ptr [rax + r12 - 0x10]'),('jmp','0x180218808'),('movzx','ecx, word ptr [rsp + 0xe0]')])
exact(0x180218848,0x18021886f,[('call','0x1802173e0'),('test','al, al'),('je','0x1802188cf'),('cmp','byte ptr [rsp + 0x50], 0'),('jne','0x180218877'),('mov','rax, qword ptr [rsp + 0xf8]'),('test','rax, rax'),('je','0x180218877'),('mov','qword ptr [rax], rbx'),('mov','byte ptr [rsp + 0x50], 1'),('jmp','0x180218877')])
exact(0x18021889c,0x1802188aa,[('cmp','r12, -1'),('jne','0x1802188aa'),('mov','qword ptr [r15 + 8], rdi'),('mov','al, 1'),('jmp','0x1802188d1')])
exact(0x1802188c7,0x1802188cf,[('mov','qword ptr [rax + 0x20], rdi'),('mov','al, 1'),('jmp','0x1802188d1')])
exact(0x180216f57,0x180216f8c,[('mov','ecx, 0xfffc'),('cmp','r14w, cx'),('jne','0x180216f67'),('xor','r13d, r13d'),('jmp','0x180216f8c'),('mov','ecx, 0xfffb'),('cmp','r14w, cx'),('jne','0x180216f7a'),('mov','r13d, 1'),('jmp','0x180216f8c'),('mov','ecx, 0xfffa'),('cmp','r14w, cx'),('mov','ecx, 2'),('cmove','r13d, ecx')])
exact(0x180217003,0x180217044,[('mov','eax, dword ptr [rbp + 0xd8]'),('mov','word ptr [rbp + 0x1c], cx'),('mov','rcx, qword ptr [rbp - 0x38]'),('mov','dword ptr [rbp + 0x34], edx'),('mov','rdx, qword ptr [rdi + 0x20]'),('mov','dword ptr [rbp + 0x2c], eax'),('mov','eax, dword ptr [rbx]'),('mov','dword ptr [rbp + 0x28], r13d'),('mov','r13, qword ptr [rbp - 0x10]'),('mov','dword ptr [rbp + 0x38], r8d'),('lea','r8, [rbp]'),('mov','byte ptr [rbp + 0x3c], r9b'),('lea','r9, [rbp - 0x58]'),('mov','qword ptr [rbp - 0x58], rcx'),('mov','rcx, r13'),('mov','dword ptr [rbp - 0x50], eax'),('movss','dword ptr [rbp + 0x30], xmm0')])
exact(0x180217054,0x18021706a,[('call','r10'),('mov','eax, dword ptr [rbp - 0x50]'),('mov','rcx, qword ptr [rbp - 0x18]'),('mov','dword ptr [rbx], eax'),('movzx','eax, byte ptr [rbp - 0x4c]'),('mov','rbx, qword ptr [rbp - 0x68]'),('mov','byte ptr [rcx], al')])
exact(0x18021706a,0x18021707f,[('movzx','r8d, word ptr [rsp + 0x74]'),('xor','r11d, r11d'),('lea','eax, [r15 - 3]'),('test','al, 0xfd'),('je','0x180217289')])
#AA's owning fresh/default branch is not inferred from a caller boolean.
exact(0x18000c8d9,0x18000c8e0,[('mov','qword ptr [rdi + 0xa4], rax')])
exact(0x18020c6c4,0x18020c6d1,[('movzx','ecx, byte ptr [rax + 0xaa]'),('mov','byte ptr [rbx + 0x2540], cl')])
exact(0x180216ed7,0x180216ee0,[('cmp','byte ptr [rdi + 0x2540], 0'),('je','0x180216f54')])
exact(0x18020c274,0x18020c2a0,[('mov','r9d, 1'),('mov','word ptr [rbx + 0x24be], ax'),('lea','r8, [rip + 0xdd8f8]'),('mov','rdx, rsi'),('mov','rcx, rdi'),('call','0x180018f70'),('mov','r9d, 2'),('mov','word ptr [rbx + 0x24c0], ax')])
assert b.data[b.off(0x1802e9b80):b.off(0x1802e9b80)+2]==b'-\0'
exact(0x180216d8f,0x180216da6,[('movzx','eax, word ptr [rdi + 0x24c0]'),('cmp','word ptr [r9 + rcx*8 + 0x1c], ax'),('mov','ecx, dword ptr [rsp + 0x70]'),('jne','0x180216e45')])
assert '*(undefined8 *)(param_2 + 0xa4) = 0' in f['18000c860']
assert '*(undefined1 *)(param_2 + 0x2540) = *(undefined1 *)(param_3[1] + 0xaa)' in f['18020c1c0']
#Selected main247 preparation modifies the documented scalar overrides only;
#external property-copy/setter/event paths are excluded by the sealed policy.
assert '+ 0xaa)'not in f['1801d60d0']
assert 'FUN_180217db0' in f['180218560']
result=dict(exactInstructions=controls,pins=pins,scope='preparedpostassociationsegments;freshordinaryDIRECT;type0/type3;no trace;no class10;no segmentationclaim')
Path(sys.argv[1]).write_bytes((json.dumps(result,indent=2)+'\n').encode())
print(f'PASS{controls} pinnedinstructions,2 wholebodypins, AA/default forwarding and full218560 write/collision/cursor contracts; no vendor execution.')
