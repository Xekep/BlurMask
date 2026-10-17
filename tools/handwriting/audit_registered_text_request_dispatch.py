"""Registered C8E/F03/130 C9E request and2C1-owned2DA provenance, static only."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
d=Binary('MyScriptDocument.dll');t=Binary('MyScriptText.dll')
assert hashlib.sha256(d.data).hexdigest()=='17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
assert hashlib.sha256(t.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
md=Cs(CS_ARCH_X86,CS_MODE_64);regions=[];bindings=[];count=0
def bind(b,table,slot,target):
 assert b.qwords(table+slot,1)==(target,);bindings.append(dict(dll=b.name,table=hex(table),slot=hex(slot),target=hex(target)))
def region(b,start,end,pins):
 global count
 raw=b.data[b.off(start):b.off(end)];rows=[(i.address,i.mnemonic,i.op_str)for i in md.disasm(raw,start)]
 for pin in pins:assert pin in rows,(b.name,pin)
 count+=len(pins);regions.append(dict(dll=b.name,start=hex(start),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))
bind(d,0x1801b6330,0x28,0x18000dba0);bind(d,0x1801b6330,0x40,0x18000e310)
bind(d,0x1801fd2e0,0,0x180022840);bind(d,0x1801fd2e0,8,0x1800228c0)
bind(d,0x1801fd258,0,0x18004d700)
bind(t,0x180276370,0,0x180049370);bind(t,0x180276370,8,0x1800479a0)
bind(t,0x180276a08,0,0x180053c30);bind(t,0x1802769a0,0x10,0x180054340);bind(t,0x1802769a0,0x18,0x180054a30)
bind(d,0x1801b6db0,0x10,0x180020b00);bind(d,0x1801b6db0,0x18,0x180020a60)
region(d,0x18004dc20,0x18004dc44,[
 (0x18004dc2a,'lea','rax, [rip + 0x1af6af]'),(0x18004dc31,'cmp','edx, 0xc9e'),
 (0x18004dc37,'cmovne','rax, rcx'),(0x18004dc3c,'lea','rax, [rip + 0x1af615]')])
assert 0x18004dc31+0x1af6af==0x1801fd2e0 and 0x18004dc43+0x1af615==0x1801fd258
region(t,0x18004a630,0x18004a664,[
 (0x18004a642,'lea','rax, [rip + 0x22bd27]'),(0x18004a649,'cmp','edx, 0xc9e'),(0x18004a64f,'cmovne','rax, rcx')])
assert 0x18004a649+0x22bd27==0x180276370
region(d,0x1800116b0,0x180011f83,[(0x180011890,'mov','edx, 0xf03')])
region(d,0x18004d700,0x18004d91d,[
 (0x18004d874,'mov','dword ptr [rbp + 0x10], 1'),(0x18004d87b,'mov','edx, 0x130')])
region(d,0x1800126db,0x180012736,[
 (0x1800126df,'cmp','qword ptr [r13 + 0xc0], 0'),(0x1800126ed,'mov','r8d, 0xc9e'),
 (0x180012716,'mov','r8, qword ptr [rax + 8]'),(0x18001271a,'call','r8'),
 (0x180012725,'test','rax, rax'),(0x180012728,'mov','qword ptr [r13 + 0xc0], rax')])
region(d,0x180012945,0x1800129d8,[
 (0x180012945,'mov','rax, qword ptr [r13 + 0x80]'),(0x18001294c,'mov','qword ptr [rbp - 0x80], rax'),
 (0x180012950,'mov','rax, qword ptr [r13 + 0x78]'),(0x180012954,'mov','qword ptr [rbp - 0x78], rax'),
 (0x180012958,'mov','rax, qword ptr [r13 + 0xc0]'),(0x18001295f,'mov','qword ptr [rbp - 0x70], rax'),
 (0x180012997,'mov','r8d, 0xc9e'),(0x1800129bc,'lea','r8, [rbp - 0x80]'),
 (0x1800129c3,'mov','r10, qword ptr [rax]'),(0x1800129cf,'call','r10')])
region(t,0x1800479a0,0x1800479e1,[(0x1800479c4,'mov','edx, 0x2c1'),(0x1800479de,'jmp','rax')])
#Constructor is split into unwind fragments at053C73; include its actual
#continuation rather than treating the first.pdata EndAddress as the end.
region(t,0x180053c30,0x180053de0,[
 (0x180053c78,'mov','edx, 0x2da'),(0x180053cbb,'mov','qword ptr [rdi + 0x10], rax'),
 (0x180053cbf,'test','rax, rax'),(0x180053cc2,'jne','0x180053d32')])
region(t,0x180049370,0x180049ec0,[
 (0x1800493aa,'mov','rax, qword ptr [r8 + 0x10]'),(0x1800493bc,'mov','qword ptr [rbp - 0x58], rax'),
 (0x1800495f7,'cmp','qword ptr [rbp - 0x58], r15'),(0x1800495fb,'jne','0x18004968e'),
 (0x18004960e,'mov','edx, 0xcbd'),(0x180049644,'mov','qword ptr [r14 + 0x10], rax'),
 (0x180049667,'mov','r9, rdi'),(0x18004966a,'mov','r8, r14'),
 (0x180049670,'mov','r10, qword ptr [rax + 0x10]'),(0x18004967e,'call','r10'),
 (0x180049681,'mov','qword ptr [r13 + 0x10], rax'),(0x180049695,'mov','rdx, qword ptr [r13 + 0x10]'),
 (0x1800496a5,'call','r8'),
 (0x180049d75,'cmp','qword ptr [rbp - 0x58], r15'),(0x180049d79,'jne','0x180049d7f'),
 (0x180049d7b,'mov','qword ptr [r13 + 0x10], r15'),(0x180049d95,'call','r8')])
region(d,0x180020b00,0x180020d9b,[
 (0x180020b2c,'mov','r8d, 0xc9e'),(0x180020b43,'call','r10'),
 (0x180020b67,'mov','r14, qword ptr [rsp + 0x80]'),(0x180020b6f,'test','r14, r14'),
 (0x180020b72,'jne','0x180020ba9'),(0x180020b7a,'call','0x180020a60'),
 (0x180020b8e,'mov','rax, qword ptr [r15 + 8]'),(0x180020b99,'call','rax'),
 (0x180020ba3,'mov','qword ptr [rbp + 0x18], rax'),
 (0x180020be2,'cmp','rdi, qword ptr [rbp + 0x28]'),
 (0x180020c99,'mov','qword ptr [rbp + 0x28], rax'),
 (0x180020cc6,'mov','qword ptr [rbp + 0x18], rax'),
 (0x180020cd7,'mov','edx, 0xc23'),(0x180020d18,'mov','qword ptr [rbp + 0x30], rax')])
region(d,0x180020a60,0x180020afb,[
 (0x180020a76,'mov','rdx, qword ptr [rdx + 0x28]'),(0x180020a89,'call','r8'),
 (0x180020a8c,'mov','rdx, qword ptr [rdi + 0x18]'),(0x180020aa9,'call','r8'),
 (0x180020aac,'mov','rdx, qword ptr [rdi + 0x20]'),(0x180020ac6,'call','r8'),
 (0x180020ac9,'mov','rdx, qword ptr [rdi + 0x30]'),(0x180020ae4,'call','r8'),
 (0x180020aec,'mov','qword ptr [rdi + 0x30], rsi')])
a.output.write_bytes((json.dumps(dict(regions=regions,bindings=bindings,pins=count,vendorExecution=False,
 scope='Registered native dispatch/context provenance; no05F720 caller, no automatic binding of library Seed to native2C1, no C20 acceptance'),indent=2)+'\n').encode())
print(f'PASS{len(regions)} regions/{len(bindings)} bindings/{count} pins; registered130 request with2C1-owned2DA, not05F720.')
