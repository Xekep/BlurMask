"""Read-only NULL05AFA0 -> ordinary2C1 -> C21 normalized key -> TEXT proof."""
import argparse,hashlib,json,sys
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_64
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
t=Binary();ink=Binary('MyScriptInk.dll');f=functions();m=Cs(CS_ARCH_X86,CS_MODE_64)
for b,h in[(t,'18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'),(ink,'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a')]:assert hashlib.sha256(b.data).hexdigest()==h
def dis(b,s,e):return[(i.mnemonic,i.op_str)for i in m.disasm(b.data[b.off(s):b.off(e)],s)]
def pins(b,s,e,expected):
 rows=dis(b,s,e)
 for x in expected:assert x in rows,(hex(s),x)
 return rows
caller=pins(t,0x180028771,0x18002881e,[('xor','r15d, r15d'),('lea','rcx, [r14 + 0xc0]'),
 ('mov','qword ptr [rsp + 0x40], rcx'),('mov','qword ptr [rsp + 0x28], r15'),
 ('mov','qword ptr [rsp + 0x30], r15'),('mov','qword ptr [rsp + 0x38], r15'),('call','r10')])
assert caller.index(('xor','r15d, r15d'))<caller.index(('mov','qword ptr [rsp + 0x28], r15'))
null=pins(t,0x18005afde,0x18005b046,[('test','rcx, rcx'),('jne','0x18005b046'),
 ('cmp','edi, dword ptr [rsi + 8]'),('mov','edx, 0x2c1'),('lea','rcx, [rax + rax*4]'),
 ('lea','rbx, [rax + rcx*8]'),('xor','r9d, r9d'),('mov','r8, rbx'),
 ('mov','r10, qword ptr [rax + 0x18]'),('call','r10'),('inc','edi'),('jne','0x18005aff0')])
assert null.index(('test','rcx, rcx'))<null.index(('mov','edx, 0x2c1'))<null.index(('call','r10'))
assert t.qwords(0x180276978+0x18,1)[0]==0x180053f90
assert ink.qwords(0x1800e5ac0+0xa8,1)[0]==0x1800802e0
assert dis(ink,0x1800802e0,0x1800802fc)==[('cmp','r8b, 4'),('je','0x1800802f9'),
 ('cmp','dword ptr [rdx + 8], 0'),('jbe','0x1800802f4'),('mov','rax, qword ptr [rdx]'),
 ('cmp','byte ptr [rax], 4'),('je','0x1800802f9'),('jmp','0x18007faf0'),('mov','al, 1'),('ret','')]
source=f['180053f90']
for x in['if (lVar4 != 0)','param_3[7] != 0xc25','param_4 != (float *)0x0','(fVar1 - (float)param_3[4])','+ 0xa8)']:assert x in source,x
pins(t,0x180029605,0x18002964c,[('lea','rax, [r14 + 0xb0]'),('lea','rcx, [r14 + 0xc0]'),
 ('mov','qword ptr [rsp + 0x28], rax'),('mov','qword ptr [rsp + 0x20], rcx'),('mov','r9d, 2'),('call','0x1800210c0'),('call','0x180025e40')])
assert t.data[t.off(0x1802ea8e0):t.off(0x1802ea8e0)+5]==b'TEXT\0'
regions=[(t,0x180028771,0x18002881e),(t,0x18005afa0,0x18005b046),(t,0x180053f90,0x180054340),
 (ink,0x1800802e0,0x1800802fc),(ink,0x18007faf0,0x1800802c4),(t,0x180029605,0x18002964c),(t,0x1800210c0,0x18002139a)]
report=dict(regions=[dict(binary=b.name,start=hex(s),end=hex(e),sha256=hashlib.sha256(b.data[b.off(s):b.off(e)]).hexdigest())for b,s,e in regions],
 bindings=2,nullIsNotEmpty=True,projection='each actual selected40 input, no overrides; VA8 suppressesmode4 or normalizes07FAF0; consecutive full objects may join',
 selection='source2DA.C0 ->0210C0 kind2 ->source2DA.B0; pending C4D only',scope='NEW raw root/ordinary inputs/no cached orC25/no accepted document',vendorExecution=False)
a.output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print('PASS7 static PE regions/2virtual bindings/NULL-vs-empty branch/full selected-input stride40/C21VA8 normalization/kind2TEXT call before025E40.')
