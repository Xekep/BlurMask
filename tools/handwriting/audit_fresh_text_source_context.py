"""Read-only C51 registry -> fresh0210C0/C4D transaction proof. No DLL execution."""
import sys,json,hashlib,argparse
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
t=Binary();ink=Binary('MyScriptInk.dll');f=functions();md=Cs(CS_ARCH_X86,CS_MODE_64)
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(ink.data).hexdigest()=='b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a'
def dis(b,s,e):return[(i.mnemonic,i.op_str)for i in md.disasm(b.data[b.off(s):b.off(e)],s)]
assert ink.qwords(0x1800e7850+0x20,1)[0]==0x1800e78a0
assert ink.qwords(0x1800e78a0+0x20,1)[0]==0x18008e870
assert ink.qwords(0x1800e78a0+0x38,1)[0]==0x18009ee20
assert ink.qwords(0x1800e5980,1)[0]==0x18007a5b0
assert ink.qwords(0x1800e5980+0x48,1)[0]==0x18007d9b0
assert dis(ink,0x18008e870,0x18008e875)==[('mov','rax, qword ptr [rdx + 0x18]'),('ret','')]
ctor=dis(ink,0x18009e4c0,0x18009e6d0)
assert ('xor','r8d, r8d') in ctor and ('mov','qword ptr [rsi + 0x18], r8') in ctor
inc=dis(ink,0x18009f1f2,0x18009f20e)
for pin in [('mov','rax, qword ptr [rbp + 0x18]'),('cmp','r15, -1'),('inc','rax'),('cmp','r15, rax'),('jl','0x18009f20e'),('lea','rax, [r15 + 1]'),('mov','qword ptr [rbp + 0x18], rax')]:assert pin in inc,pin
fresh=dis(t,0x18002127a,0x18002133e)
for pin in [('mov','edx, 0xc22'),('mov','rbx, qword ptr [r12 + 0x10]'),('lea','rax, [rbx + 1]'),('mov','qword ptr [r12 + 0x10], rax'),('test','r15, r15'),('je','0x18002133b'),('mov','edx, 0xc4d'),('mov','dword ptr [rsp + 0x38], 0x64')]:assert pin in fresh,pin
assert fresh.index(('mov','qword ptr [r12 + 0x10], rax'))<fresh.index(('test','r15, r15'))
for pin in [('test','r14b, r14b'),('je','0x180021392'),('mov','qword ptr [rcx], rbx')]:assert pin in dis(t,0x18002137a,0x18002139a),pin
producer=f['18005f050'];caller=f['18005f200']
for text in ['*(undefined8 *)(param_3 + 0x40)','+ 0x20','param_2[2] = lVar1','(param_1,0xc4d)','(param_1,0)']:assert text in producer,text
assert 'FUN_18005f050(param_1,&local_b8,local_a0)' in caller
assert '(param_1,plVar12,local_98,param_3,&local_c8,&local_b8,&local_88)' in caller
for off,name in [(0x1802ea8e0,b'TEXT'),(0x1802ea8e8,b'WORD'),(0x1802ea8f0,b'CHAR')]:assert t.data[t.off(off):t.off(off)+len(name)+1]==name+b'\0'
transaction=dis(ink,0x18007da83,0x18007da98)
for pin in [('xor','eax, eax'),('mov','dword ptr [rsi + 8], 7'),('mov','qword ptr [rsi + 0x10], rax'),('mov','qword ptr [rsi + 0x18], rax'),('mov','qword ptr [rsi + 0x20], rax')]:assert pin in transaction,pin
command=dis(ink,0x18007a5b0,0x18007a770)
for pin in [('mov','r8d, 4'),('mov','qword ptr [rdx + 0x18], rsi'),('mov','byte ptr [rdi + 0x28], 1'),('mov','dword ptr [rdi + 0x40], eax'),('mov','qword ptr [rdi + 0x38], rsi')]:assert pin in command,pin
copy=dis(ink,0x180085d53,0x180085dc8)
for pin in [('mov','eax, dword ptr [r14 + 0x10]'),('mov','dword ptr [rbx + 0x20], eax'),('shl','r8, 4'),('call','0x1800d57e0')]:assert pin in copy,pin
regions=[]
for b,s,e in [(t,0x1800210c0,0x18002139a),(t,0x18005f050,0x18005f130),(ink,0x18009e4c0,0x18009e6d0),(ink,0x18009ee20,0x18009f280),(ink,0x18007d9b0,0x18007daa0),(ink,0x18007a5b0,0x18007a770),(ink,0x180085c90,0x180085e10)]:
 regions.append(dict(binary=b.name,start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest()))
report=dict(regions=regions,virtualBindings=5,counter='fresh C51 zero; successful NEW explicit K raises next to K+1; original IDs0..N-1 derive N',
 failure='0210C0 consumes counter before temporary C22 null test; output written only on success',scope='NEW library raw root, pending C4D commands only; no SDK page ID,prior/cached branch,024D40 payload orfullwrapper',noVendorExecution=True)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS7 PE regions/5virtual bindings/native counter-consumption and output-success gate/static only.')
