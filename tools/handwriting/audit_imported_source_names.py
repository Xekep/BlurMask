"""13C1D0/1458F0 detached names and actual source count; static PE only."""
from pathlib import Path
import hashlib,json,subprocess,sys
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
pins={
0x18013c1e9:('cmp','qword ptr [rdx + 0xa0], 0'),0x18013c1fd:('cmp','qword ptr [rdx + 0x10], 0'),
0x18013c23a:('mov','r12, qword ptr [r13 + 0x10]'),0x18013c27d:('mov','r12, qword ptr [rax + 0x28]'),
0x18013c28a:('mov','r14d, dword ptr [r12 + 0x22d8]'),0x18013c292:('mov','ebp, 1'),
0x18013c2a1:('mov','r13d, 0xffffffff'),0x18013c2b4:('cmp','edi, r14d'),0x18013c2be:('cmove','ebx, r13d'),
0x18013c2dd:('mov','r9, qword ptr [rax + 8]'),0x18013c2e9:('lea','rdx, [rax + 0x14]'),
0x18013c305:('add','r15d, eax'),0x18013c318:('lea','ebx, [r14 + 1]'),0x18013c323:('inc','r8'),
0x18013c326:('shl','rbx, 3'),0x18013c32a:('add','r8, rbx'),0x18013c330:('add','r8, r14'),
0x18013c33c:('call','r9'),0x18013c34a:('je','0x18013c421'),0x18013c358:('call','0x18025ee50'),
0x18013c361:('lea','rbx, [rbx + r14*8]'),0x18013c380:('cmove','edi, eax'),
0x18013c3ae:('lea','rdi, [rax + 0x14]'),0x18013c3d2:('mov','qword ptr [r13 + rcx*8], rbx'),
0x18013c3ee:('call','0x18025f210'),0x18013c3f6:('mov','byte ptr [rbx], 0'),
0x18013c3fb:('inc','rbx'),0x18013c401:('jbe','0x18013c371'),
0x18013c411:('mov','qword ptr [r13 + 0xa0], rdi'),0x18013c418:('mov','dword ptr [r13 + 0xa8], r14d'),
0x180145912:('mov','ecx, dword ptr [r8 + 0xa8]'),0x18014591b:('je','0x180145952'),
0x18014592d:('movsx','eax, word ptr [r9 + rdx + 0x30]'),0x180145933:('cmp','eax, -1'),
0x180145936:('cmove','eax, ecx'),0x180145939:('cmp','eax, -2'),0x180145940:('jmp','0x18014598f'),
0x180145945:('mov','rax, qword ptr [r8 + 0xa0]'),0x18014594c:('mov','rbx, qword ptr [rax + rcx*8]'),
0x18014598a:('mov','dword ptr [rsi], edi'),
0x180236820:('mov','eax, dword ptr [rdx + 0x58]'),0x180236823:('inc','eax'),
0x180236838:('lea','rax, [rdx + 0x10]'),0x18023683d:('lea','eax, [r8 - 1]'),
0x180236849:('mov','rax, qword ptr [rax + rdx*8 + 0x10]'),
0x180199bc7:('mov','r8, qword ptr [rax + 0x20]'),0x1801af6c7:('mov','r9, qword ptr [rax + 0x28]'),
0x18020ddbc:('mov','r8, qword ptr [rax + 0x20]'),0x18020ddc3:('mov','dword ptr [rdi + 0x22d8], eax'),
0x18020debd:('mov','r9, qword ptr [rax + 0x28]'),0x18020dee8:('mov','qword ptr [rcx + rbp*8], rax'),
0x180227d80:('cmp','r8d, dword ptr [rdx + 0x22d8]'),0x180227d8c:('cmp','r8d, -1'),
0x180227d92:('mov','rax, qword ptr [rdx + 0x2300]'),0x180227d9a:('mov','rax, qword ptr [rdx + 0x22e0]'),
0x180227da8:('mov','rax, qword ptr [rax + 0x10]'),
0x1801d26e8:('mov','qword ptr [rsp + 0x20], 0x38'),0x1801d272f:('cmp','eax, -1'),
0x1801d22e0:('mov','dword ptr [rsp + 0x18], 0x77191214'),
0x1801d28aa:('lea','rdx, [rbp + 0x14]'),0x1801d28b7:('call','0x1801d22e0'),
0x180235f4b:('xor','eax, eax'),0x180235f60:('mov','qword ptr [rdx + 0x58], rax'),
0x1802360fc:('lea','rdi, [rbx + 0x50]'),0x180236100:('mov','ecx, dword ptr [rdi + 8]'),
0x180236106:('inc','ecx'),0x1802361c9:('inc','dword ptr [rdi + 8]'),
0x180236528:('mov','eax, dword ptr [rbx + 0x58]'),
0x18020d6d1:('mov','edx, 0x28d'),0x18020d6f2:('mov','r9, qword ptr [rax]'),0x18020d6f5:('call','r9'),
}
for address,expected in pins.items():
 i=next(md.disasm(b.data[b.off(address):b.off(address)+16],address));assert(i.mnemonic,i.op_str)==expected,(hex(address),i.mnemonic,i.op_str,expected)
bindings=[(0x1802e7c60+0x20,0x1802e7af0),(0x1802e7af0+8,0x180227d80),
 (0x1802dc740+0x38,0x18013c1d0),(0x1802e8bb0+0x20,0x180236820),(0x1802e8bb0+0x28,0x180236830),
 (0x1802e8c60+0x20,0x180199bc0),(0x1802e8c60+0x28,0x1801af6c0),
 (0x1802e8b60+0x18,0x1802e8b28),(0x1802e8b28+0x10,0x1802e8c40),(0x1802e8c40,0x180235f30),
 (0x1802e8b60+0x20,0x1802e8b00),(0x1802e8b00,0x180236020)]
for address,value in bindings:assert b.qwords(address,1)[0]==value
regions=[]
for lo,hi in [(0x18013c1d0,0x18013c436),(0x1801458f0,0x1801459a9),(0x180227d80,0x180227dad),
 (0x180236820,0x18023684f),(0x18020d8f0,0x18020e070),(0x1801d22e0,0x1801d2534),
 (0x180235f30,0x180235f6a),(0x180236020,0x1802361f5),(0x18020d6cd,0x18020d6f8)]:
 regions.append(dict(start=hex(lo),end=hex(hi),sha256=hashlib.sha256(b.data[b.off(lo):b.off(hi)]).hexdigest()))
p=Path(sys.argv[1]);p.parent.mkdir(parents=True,exist_ok=True)
dependencies=[]
for name in ['audit_fresh_case_main_source.py','audit_imported_word_bytes.py']:
 out=Path(str(p)+'.'+name+'.json');subprocess.run([sys.executable,str(root/'tools/handwriting'/name),str(out)],check=True,stdout=subprocess.DEVNULL);dependencies.append(json.loads(out.read_bytes()))
p.write_text(json.dumps(dict(noVendorExecution=True,pins=[dict(address=hex(a),instruction=' '.join(v)) for a,v in pins.items()],bindings=[[hex(a),hex(v)] for a,v in bindings],regions=regions,dependencies=dependencies,scope='Fresh copied V38 metadata only; N=selected+implicit0; no Document acceptance; unwritten last allocation byte omitted'),indent=2)+'\n')
print(f'PASS {len(pins)} instruction pins/{len(bindings)} bindings/source count and detach-order dependencies; static-only')
