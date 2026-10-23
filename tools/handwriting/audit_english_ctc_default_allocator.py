"""Static root/default allocator contract; never loads or executes the SDK.

Condition: initialized root environment retains the audited default tables and
callbacks, valid heap, first allocation succeeds. This is not caller-origin or
whole-constructor proof. Windows HeapAlloc semantics are the external contract.
"""
from pathlib import Path
import argparse,hashlib,json,struct
from capstone import Cs,CS_ARCH_X86,CS_MODE_64,CS_OP_MEM,CS_AC_WRITE

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk',type=Path,required=True)
p.add_argument('--output',type=Path,required=True)
a=p.parse_args();raw=(a.sdk/'MyScriptEngine.dll').read_bytes()
assert hashlib.sha256(raw).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
pe=struct.unpack_from('<I',raw,60)[0];optional=pe+24
image=struct.unpack_from('<Q',raw,optional+24)[0]
table=optional+struct.unpack_from('<H',raw,pe+20)[0]
sections=[struct.unpack_from('<IIII',raw,table+i*40+8)for i in range(struct.unpack_from('<H',raw,pe+6)[0])]
def read(address,size):
 for _,va,length,offset in sections:
  if va<=address-image and address-image+size<=va+length:return raw[offset+address-image-va:offset+address-image-va+size]
 raise AssertionError('Unbacked address')
def qword(address):return struct.unpack('<Q',read(address,8))[0]
regions=[
 (0x180033c70,197,'3323cad3cf0e5ba8c64707f5de5b0ad049d1de1ad6315706a86fea4ed7316a3f'),
 (0x180034010,490,'bb92a6978700410bd69ec7d6e9c44864968228a0e6ff6e4b1594342f76df8c88'),
 (0x180037410,190,'6826968c53e501d2d61454f63fd340a60e9f5ec099c58b8d681721514fb6c0ae'),
 (0x1801c71c0,117,'6ac558292ba661f5fdb665cd46557e63035e564ebec22b19ff1661323afacdc7'),
 (0x1801bafe0,57,'fb8670872a60b5797b9ba26863cc40e894d47e912c40b3e729be97cfbea17d15'),
 (0x1801ba950,5,'6293aa158a12190b181ae80bc02df18fd528804febcee5c257fd495783b897ef'),
 (0x1801bb020,5,'cb3ed1c9527fa2be24fb1cc3e4c1c904aff956a9b97324a02ae98fa29e78d00c'),
 (0x180252000,48,'fc4102026f1c8368186f3c7a129cc6c00e51bf512f6408577813fdfe4565767a'),
 (0x180252030,8,'a5cefb29f0033597e2bb4aa2abacbb261ddb21a45916d748df236b7added7e9a'),
 (0x1801db680,8,'3a55801e49f1c6e5dbdce04bb720e316b283cf8e9d7b27dd7846be2123c699ac'),
 (0x1801db5a0,64,'ec2946f68e178162eba1819655ede346306ca22ccfeead1a2f67accfb6cb9798')]
for address,size,digest in regions:assert hashlib.sha256(read(address,size)).hexdigest()==digest
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True;decoded={};bodies=[]
for address,size,digest in regions[:7]:
 ii=list(md.disasm(read(address,size),address));assert sum(i.size for i in ii)==size
 decoded.update({i.address:i for i in ii});bodies.append(dict(address=hex(address),bytes=size,instructions=len(ii),sha256=digest,registered=address not in (0x1801ba950,0x1801bb020)))
def sequence(at,expected):
 for m,o in expected:
  i=decoded[at];assert(i.mnemonic,i.op_str)==(m,o),(hex(at),i.mnemonic,i.op_str);at+=i.size

# NULL memory callbacks select the default48-byte table. The root parent is0;
# three literal16-byte copies install its fields at environment48..77.
sequence(0x180033c84,[('test','rcx, rcx'),('lea','rdi, [rip + 0x21e372]'),('mov','rbp, rdx'),('mov','rsi, r8'),('cmovne','rdi, rcx')])
sequence(0x180033cb0,[('xor','eax, eax'),('mov','qword ptr [rbx], rax'),('movups','xmm0, xmmword ptr [rdi]'),('movups','xmmword ptr [rbx + 0x48], xmm0'),('movups','xmm1, xmmword ptr [rdi + 0x10]'),('movups','xmmword ptr [rbx + 0x58], xmm1'),('movups','xmm0, xmmword ptr [rdi + 0x20]'),('movups','xmmword ptr [rbx + 0x68], xmm0')])
assert qword(0x180252000)==1 and qword(0x180252008)==0x1801ba950 and qword(0x180252028)==0x1801bb020
# Initial allocator service installation, distinct from proving later callbacks
# cannot replace it. The initialized/unmodified condition remains explicit.
assert qword(0x180252030)==0x1801db660 and qword(0x1801db680)==0x1801db5a0
assert qword(0x1801db5b8)==0x180037410
sequence(0x18003402b,[('lea','rax, [rip + 0x21fb1e]'),('mov','qword ptr [rcx + 8], rax')])
sequence(0x180034047,[('mov','rax, qword ptr [rip + 0x21dfe2]'),('mov','qword ptr [rip + 0x21fb03], rdx'),('mov','r10, qword ptr [rcx + 8]'),('mov','rdx, qword ptr [rax + 0x20]'),('mov','qword ptr [r10], rdx')])
sequence(0x18003741a,[('mov','rbx, rcx'),('mov','rcx, qword ptr [rcx]'),('test','rcx, rcx'),('je','0x180037439')])
sequence(0x180037439,[('mov','rax, qword ptr [rbx + 0x50]'),('mov','rcx, r8'),('call','rax'),('mov','rdi, rax'),('test','rax, rax'),('je','0x18003749f')])
sequence(0x1801ba950,[('jmp','0x1801c71c0')])
# The positive400-byte request reaches HeapAlloc with flag8. Overflow/error/
# new-handler branches are not used under the first-success condition.
sequence(0x1801c71c6,[('mov','r8, rdx'),('mov','rbx, rcx'),('test','rcx, rcx')])
sequence(0x1801c71d1,[('xor','edx, edx'),('lea','rax, [rdx - 0x20]'),('div','rbx'),('cmp','rax, r8'),('jb','0x1801c7222'),('imul','rbx, r8')])
sequence(0x1801c7206,[('mov','rcx, qword ptr [rip + 0x8d82b]'),('mov','r8, rbx'),('mov','edx, 8'),('call','qword ptr [rip + 0x1409d]'),('test','rax, rax'),('je','0x1801c71f1'),('jmp','0x1801c722f')])
assert decoded[0x1801c7215].address+decoded[0x1801c7215].size+decoded[0x1801c7215].operands[0].mem.disp==0x1801db2b8
# Successful accounting retains the allocated pointer in RDI. The default size
# callback's valid/nonNULL path is a read-only HeapSize query, not a mutator.
sequence(0x180037461,[('mov','rax, qword ptr [rbx + 0x70]'),('test','rax, rax'),('je','0x180037474'),('mov','rcx, rdi'),('call','rax')])
sequence(0x18003747a,[('mov','rax, rdi'),('add','rdx, rcx'),('mov','qword ptr [rbx + 0x78], rdx'),('cmp','rdx, qword ptr [rbx + 0x80]'),('jbe','0x1800374c3')])
sequence(0x1800374c0,[('mov','rax, rdi')])
for i in decoded.values():
 if not 0x180037410<=i.address<0x1800374ce:continue
 for operand in i.operands:
  if operand.type==CS_OP_MEM and operand.access&CS_AC_WRITE:
   memory=operand.mem;base=i.reg_name(memory.base)
   assert memory.index==0 and ((base=='rsp' and memory.disp in (8,0x20))or(base=='rbx' and memory.disp in (0x78,0x80,0x88,0x90))),('Unexpected allocator write',hex(i.address))
sequence(0x1801bb020,[('jmp','0x1801bafe0')])
sequence(0x1801bafe4,[('test','rcx, rcx'),('jne','0x1801bb002')])
sequence(0x1801bb002,[('mov','r8, rcx'),('xor','edx, edx'),('mov','rcx, qword ptr [rip + 0x99a2a]'),('add','rsp, 0x28'),('jmp','qword ptr [rip + 0x20207]')])
assert decoded[0x1801bb012].address+decoded[0x1801bb012].size+decoded[0x1801bb012].operands[0].mem.disp==0x1801db220
def rip_target(at):
 i=decoded[at];return i.address+i.size+i.operands[1].mem.disp
assert rip_target(0x1801c7206)==rip_target(0x1801bb007)==0x180254a38
# Authenticate the IAT names rather than interpreting a numeric slot as an API.
imports_rva=struct.unpack_from('<I',raw,optional+112+8)[0]
imports={};d=imports_rva
while any(struct.unpack('<5I',read(image+d,20))):
 original,_,_,name,first=struct.unpack('<5I',read(image+d,20));index=0
 while qword(image+(original or first)+8*index):
  value=qword(image+(original or first)+8*index)
  if value>>63==0:
   at=image+value+2;namebytes=bytearray()
   while read(at,1)!=b'\0':namebytes+=read(at,1);at+=1
   imports[image+first+8*index]=namebytes.decode('ascii')
  index+=1
 d+=20
assert imports[0x1801db2b8]=='HeapAlloc' and imports[0x1801db220]=='HeapSize'
cases=[]
for count,unit in [(5,80),(4,8)]:
 assert count>0 and unit<=(2**64-32)//count
 cases.append(dict(count=count,unitBytes=unit,requestedBytes=count*unit,heapFlags=8,zeroInitialized=True))
report=dict(status='PASS STATIC conditional root default zero-allocation contract',regions=bodies,cases=cases,condition='Initialized unmodified ROOT environment/default callbacks; valid heap; first HeapAlloc succeeds; documented Windows API contract',rootDefaultCallbackProven=True,firstAttemptOnly=True,actualCtcEnvironmentOriginProven=False,completeConstructorProven=False,runtimePredicateApproved=False,vendorExecuted=False,apiContract='https://learn.microsoft.com/en-us/windows/win32/api/heapapi/nf-heapapi-heapalloc')
a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(dict(status=report['status'],cases=cases)))
