"""Read-only C8F transport and distinct05F720 acceptance receiver contracts."""
import argparse, hashlib, json, struct, sys
from pathlib import Path
import pefile
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
from capstone.x86 import X86_OP_IMM, X86_OP_MEM, X86_REG_RIP

root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
p=argparse.ArgumentParser(description=__doc__);p.add_argument('output',type=Path);a=p.parse_args()
names=['MyScriptText.dll','MyScriptDocument.dll','MyScriptEngine.dll','MyScriptInk.dll']
hashes=['18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f',
 '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f',
 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c',
 'b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a']
bs=[Binary(n)for n in names]
for b,h in zip(bs,hashes):assert hashlib.sha256(b.data).hexdigest()==h
t,d,e,ink=bs;md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True
regions=[];bindings=[];pins=0
def bind(b,table,slot,target):
 assert b.qwords(table+slot,1)==(target,)
 bindings.append(dict(dll=b.name,table=hex(table),slot=hex(slot),target=hex(target)))
def region(b,first,end,expected):
 global pins
 raw=b.data[b.off(first):b.off(end)];rows=[(x.address,x.mnemonic,x.op_str)for x in md.disasm(raw,first)]
 for row in expected:assert row in rows,(b.name,row)
 pins+=len(expected);regions.append(dict(dll=b.name,first=hex(first),end=hex(end),sha256=hashlib.sha256(raw).hexdigest()))

#The actual parameterized constructor is metadata.ctorTable+10, not the
#nearby CA7 interface test at11BB20. Its16-byte branch retains the supplied
#receiver and sets ownership bit0 from the actual second argument's byte0.
bind(d,0x180209630,0x18,0x1802095f8)
bind(d,0x1802095f8,8,0x1802095b0);bind(d,0x1802095f8,0x10,0x1802095e0)
bind(d,0x1802095e0,0x10,0x18011b450);bind(d,0x1802095b0,0x18,0x18011b860)
region(d,0x18011b450,0x18011b530,[
 (0x18011b4b2,'cmp','rbp, 0x10'),(0x18011b4bc,'mov','rdx, qword ptr [rsi]'),
 (0x18011b4c6,'mov','qword ptr [rbx + 0x10], rax'),(0x18011b4ca,'cmp','rax, qword ptr [rsi]'),
 (0x18011b4d1,'mov','qword ptr [rbx + 0x18], rax'),(0x18011b4d7,'cmp','byte ptr [rsi + 8], al'),
 (0x18011b4dd,'mov','dword ptr [rbx + 0x20], ecx'),(0x18011b4e4,'mov','qword ptr [rbx + 0x40], 0')])
region(d,0x18011b860,0x18011b950,[
 (0x18011b886,'lea','edi, [rax + 1]'),(0x18011b892,'mov','r8d, 0x28'),
 (0x18011b8a0,'mov','r9d, edi'),(0x18011b8b0,'je','0x18011b8c2'),
 (0x18011b8de,'movups','xmmword ptr [rdx], xmm0'),(0x18011b8e1,'movups','xmmword ptr [rdx + 0x10], xmm0'),
 (0x18011b8e5,'mov','qword ptr [rdx + 0x20], rax'),(0x18011b8e9,'inc','dword ptr [rsi + 0x40]'),
 (0x18011b905,'mov','qword ptr [rcx + rdx + 0x10], rax'),(0x18011b912,'mov','qword ptr [rcx + rdx + 0x18], rax'),
 (0x18011b91f,'mov','qword ptr [rcx + rdx + 0x20], rax'),(0x18011b927,'mov','qword ptr [rdx + rax], r14'),
 (0x18011b92e,'mov','qword ptr [rax + rdx + 8], rbp')])
region(d,0x18011b6c0,0x18011b860,[
 (0x18011b6c7,'test','byte ptr [rdx + 0x20], 1'),(0x18011b6d1,'je','0x18011b7c2'),
 (0x18011b708,'mov','r15, qword ptr [rax + rcx*8 + 0x10]'),(0x18011b736,'mov','r8, qword ptr [rax + 0x18]'),
 (0x18011b73d,'mov','r15, qword ptr [r14 + 0x18]'),(0x18011b773,'mov','r14, qword ptr [r14 + 0x20]'),
 (0x18011b79c,'mov','r8, qword ptr [rax + 0x50]'),(0x18011b7e2,'mov','rdx, qword ptr [rbp + 0x10]')])

#These are THREE receivers, not one presumed C20: reader.V200() for
#178/180/C8F's retained10; request+8 for188; original reader for218.
region(t,0x18005f720,0x18005f950,[
 (0x18005f742,'mov','r13, qword ptr [rax + 0x10]'),(0x18005f74d,'mov','r9, qword ptr [rax + 0x200]'),
 (0x18005f768,'mov','r15, rax'),(0x18005f77f,'mov','r8, qword ptr [rax]'),
 (0x18005f7bb,'mov','rdx, r15'),(0x18005f7be,'mov','r10, qword ptr [rcx + 0x178]'),
 (0x18005f7d8,'mov','qword ptr [rsp + 0x30], r15'),(0x18005f7e2,'mov','byte ptr [rsp + 0x38], 1'),
 (0x18005f7f3,'mov','edx, 0xc8f'),(0x18005f821,'mov','r9d, 0x10'),
 (0x18005f834,'mov','qword ptr [r14 + 0x20], rax'),(0x18005f858,'call','0x18005f200'),
 (0x18005f867,'cmp','dword ptr [rax + 0x40], 1'),(0x18005f874,'mov','rdx, qword ptr [r14 + 8]'),
 (0x18005f880,'mov','r8, qword ptr [rbx + 0x20]'),(0x18005f887,'mov','r10, qword ptr [rax + 0x188]'),
 (0x18005f898,'mov','rdx, r13'),(0x18005f8af,'mov','r10, qword ptr [rax + 0x218]'),
 (0x18005f8e9,'xor','r8d, r8d'),(0x18005f8ef,'mov','rdx, r15'),
 (0x18005f8f5,'mov','r9, qword ptr [rax + 0x180]'),(0x18005f8ff,'test','al, al')])
region(t,0x18005f4dd,0x18005f52b,[
 (0x18005f4dd,'mov','rdx, qword ptr [r15 + 0x20]'),(0x18005f50f,'call','r10'),
 (0x18005f521,'mov','qword ptr [rax], rsi'),(0x18005f524,'mov','qword ptr [rbp - 0x11], rsi')])

#A fresh parameterized BCC cannot supply the missing request acceptance
#receiver: its200 returnsNULL. BEF instead retains explicit second ctor arg28.
bind(e,0x180227080,0x200,0x180031900);bind(e,0x1802273a0,0x200,0x18000c0a0)
bind(e,0x1802273a0,0x218,0x1800efb10);bind(e,0x180227330,0x10,0x1800f0520)
region(e,0x180031900,0x180031903,[(0x180031900,'xor','eax, eax'),(0x180031902,'ret','')])
region(e,0x18000c0a0,0x18000c0a5,[(0x18000c0a0,'mov','rax, qword ptr [rdx + 0x28]'),(0x18000c0a4,'ret','')])
region(e,0x1800f0520,0x1800f0600,[
 (0x1800f0587,'mov','rdx, qword ptr [rdi + 8]'),(0x1800f058b,'test','rdx, rdx'),
 (0x1800f05a9,'mov','qword ptr [rbx + 0x28], rdx'),(0x1800f05b1,'mov','qword ptr [rbx + 0x30], 0'),
 (0x1800f05ce,'cmp','rdx, qword ptr [rdi + 8]')])

#Ink C20's180 signature contradicts pretending that returned receiver is
#this object: withR8==0 it writes throughR9, but05F720 usesR9 ascalltarget.
bind(ink,0x1800e5050,0x180,0x18006ec60);bind(ink,0x1800e5050,0x188,0x18006ed30)
bind(ink,0x1800e5980,0x38,0x18007e210)
region(ink,0x18006ec60,0x18006ed30,[
 (0x18006ec82,'test','r8, r8'),(0x18006ec85,'jne','0x18006ec9c'),(0x18006ec8c,'mov','byte ptr [r9], 1')])
region(ink,0x18006ed30,0x18006ede4,[
 (0x18006ed51,'test','r8, r8'),(0x18006ed67,'mov','rsi, qword ptr [rdx + 0x58]'),
 (0x18006ed77,'jne','0x18006edbd'),(0x18006ed8d,'mov','r10, qword ptr [rax + 0x40]'),
 (0x18006edd5,'mov','r10, qword ptr [rax + 0x38]')])
region(ink,0x18007e210,0x18007e2e0,[
 (0x18007e241,'mov','r10, qword ptr [rax + 0x78]'),(0x18007e24c,'mov','rdx, qword ptr [rsi + 0xf8]'),
 (0x18007e259,'je','0x18007e293'),(0x18007e274,'mov','byte ptr [rsi + 0x108], dil'),
 (0x18007e27b,'call','0x180078090'),(0x18007e2ae,'mov','r10, qword ptr [rcx + 0x80]')])

#A concrete different Document CBB receiver forwards178/180 to the20/28
#interface of its retained inner+B8. This is a candidate provenance chain,
#not evidence that the unregistered05F720 request was produced by it.
bind(d,0x180208f90,0x178,0x180116970);bind(d,0x180208f90,0x180,0x180116990)
bind(d,0x1802091b0,0x18,0x180209508);bind(d,0x180209508,8,0x180208f90)
bind(d,0x180209508,0x10,0x180209230);bind(d,0x180209230,0x10,0x18011af60)
region(d,0x180116970,0x1801169b0,[
 (0x180116974,'mov','rdx, qword ptr [rax + 0xb8]'),(0x18011697e,'mov','r10, qword ptr [rax + 0x20]'),
 (0x180116994,'mov','rdx, qword ptr [rax + 0xb8]'),(0x18011699e,'mov','r9, qword ptr [rax + 0x28]')])
region(d,0x18011af60,0x18011b073,[
 (0x18011b006,'mov','qword ptr [rbx + 0x28], rcx'),(0x18011b00a,'mov','rdx, qword ptr [rbx + 0x18]'),
 (0x18011b012,'mov','qword ptr [rbx + 0x10], rax'),(0x18011b016,'cmp','rdx, qword ptr [rsi]'),
 (0x18011b033,'mov','rdx, qword ptr [rax + 0xb8]'),(0x18011b03d,'mov','r10, qword ptr [rax + 0x10]'),
 (0x18011b046,'je','0x18011b05c')])

#Bounded negative entry evidence: conventional relocatable QWORD function
#pointers, PE exports and decoded function operands do not reference05F720.
#No claim about arbitrary computed addresses or an external SDK caller.
pe=pefile.PE(data=t.data);base=pe.OPTIONAL_HEADER.ImageBase;target=0x18005f720
assert t.data.find(struct.pack('<Q',target))<0
exports=[x.address for x in getattr(pe,'DIRECTORY_ENTRY_EXPORT',type('Empty',(),{'symbols':[]})()).symbols]
assert target-base not in exports
references=[];functions=0
for entry in pe.DIRECTORY_ENTRY_EXCEPTION:
 first,end=base+entry.struct.BeginAddress,base+entry.struct.EndAddress
 if not first<end:continue
 functions+=1
 for ins in md.disasm(t.data[t.off(first):t.off(end)],first):
  for op in ins.operands:
   absolute=op.imm if op.type==X86_OP_IMM else ins.address+ins.size+op.mem.disp if op.type==X86_OP_MEM and op.mem.base==X86_REG_RIP else None
   if absolute==target:references.append(dict(address=hex(ins.address),mnemonic=ins.mnemonic,operand=ins.op_str))
assert references==[],references
a.output.write_bytes((json.dumps(dict(regions=regions,bindings=bindings,pins=pins,entrySearch=dict(exceptionFunctions=functions,decodedReferences=references,absoluteQwords=0,peExports=0),
 scope='C8F ownership and receiver separation only; no05F720 registration or C20 acceptance claim',vendorExecution=False),indent=2)+'\n').encode())
print(f'PASS{len(regions)} regions/{len(bindings)} bindings/{pins} pins;{functions} decoded functions/no conventional05F720 entry reference.')
