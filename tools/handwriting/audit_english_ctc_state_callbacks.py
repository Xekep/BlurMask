"""Root/default malloc and free(NULL) callback paths, static only.

Composes the immutable zero allocator/table/HeapSize audit. Does not establish
the selected CTC environment origin or execute any vendor instruction.
"""
from pathlib import Path
import argparse,runpy,sys,hashlib,json
from capstone import CS_OP_MEM,CS_AC_WRITE
p=argparse.ArgumentParser(description=__doc__);p.add_argument('--sdk',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
old=sys.argv;tool=Path(__file__).with_name('audit_english_ctc_default_allocator.py');sys.argv=[str(tool),'--sdk',str(a.sdk),'--output',str(a.output.with_name(a.output.stem+'-zero.json'))]
try:up=runpy.run_path(str(tool),run_name='__main__')
finally:sys.argv=old
read=up['read'];md=up['md'];qword=up['qword'];decoded={};regions=[]
for address,size,digest in [
 (0x180037340,193,'4372f5011b76356d1c52c43156aef513b3b2b055d9953d98dafa0fd067f0369f'),
 (0x180037640,114,'bdbd6d29fce7dfeb43db15013d99f6d3c9e4c8423e40d2cfc1b648bdfbf4fbfb'),
 (0x1801bafd0,5,'3ff98486e6e50928d63f393f425001feec630b602d70ba08b260a9375881e7f5'),
 (0x1801c7920,94,'2d915211b3aa22ab72f009cf5a5bcd16db83179bf272c997275ff318426cc2db')]:
 code=read(address,size);assert hashlib.sha256(code).hexdigest()==digest
 ii=list(md.disasm(code,address));assert sum(i.size for i in ii)==size
 decoded.update({i.address:i for i in ii});regions.append(dict(address=hex(address),bytes=size,sha256=digest,instructions=len(ii),registered=address!=0x1801bafd0))
def sequence(at,expected):
 for mnemonic,operands in expected:
  i=decoded[at];assert(i.mnemonic,i.op_str)==(mnemonic,operands),(hex(at),i.mnemonic,i.op_str);at+=i.size
assert qword(0x1801db5a0)==0x180037340 and qword(0x1801db5a8)==0x180037640
assert qword(0x180252018)==0x1801bafd0 # installed as environment60 by prior audit
sequence(0x18003734a,[('mov','rbx, rcx'),('mov','rcx, qword ptr [rcx]'),('test','rcx, rcx'),('je','0x180037368')])
sequence(0x180037368,[('mov','rax, qword ptr [rbx + 0x60]'),('imul','rdx, r8'),('mov','rcx, rdx'),('call','rax'),('mov','rdi, rax'),('test','rax, rax'),('je','0x1800373d2')])
sequence(0x1801bafd0,[('jmp','0x1801c7920')])
sequence(0x1801c7926,[('mov','rbx, rcx'),('cmp','rcx, -0x20'),('ja','0x1801c796b'),('test','rcx, rcx'),('mov','eax, 1'),('cmove','rbx, rax'),('jmp','0x1801c7952')])
sequence(0x1801c7952,[('mov','rcx, qword ptr [rip + 0x8d0df]'),('mov','r8, rbx'),('xor','edx, edx'),('call','qword ptr [rip + 0x13954]'),('test','rax, rax'),('je','0x1801c793d'),('jmp','0x1801c7978')])
sequence(0x1801c7978,[('add','rsp, 0x20'),('pop','rbx'),('ret','')])
i=decoded[0x1801c7952];assert i.address+i.size+i.operands[1].mem.disp==0x180254a38
i=decoded[0x1801c795e];assert i.address+i.size+i.operands[0].mem.disp==0x1801db2b8 and up['imports'][0x1801db2b8]=='HeapAlloc'
# Accounting keeps RDI, only updates counters, and uses the previously audited
# read-only HeapSize callback. Both skip-accounting and accounted exits restore it.
sequence(0x18003737d,[('test','byte ptr [rbx + 0xa8], 8'),('jne','0x1800373f3')])
sequence(0x180037394,[('mov','rax, qword ptr [rbx + 0x70]'),('test','rax, rax'),('je','0x1800373a7'),('mov','rcx, rdi'),('call','rax'),('mov','rcx, rax'),('jmp','0x1800373a9')])
sequence(0x1800373a9,[('mov','rdx, qword ptr [rbx + 0x78]'),('mov','rax, rdi'),('add','rdx, rcx'),('mov','qword ptr [rbx + 0x78], rdx'),('cmp','rdx, qword ptr [rbx + 0x80]'),('jbe','0x1800373f6')])
sequence(0x1800373f3,[('mov','rax, rdi')])
for i in decoded.values():
 if not 0x180037340<=i.address<0x180037401:continue
 for operand in i.operands:
  if operand.type==CS_OP_MEM and operand.access&CS_AC_WRITE:
   m=operand.mem;assert m.index==0 and ((i.reg_name(m.base)=='rsp'and m.disp in(8,0x20))or(i.reg_name(m.base)=='rbx'and m.disp in(0x78,0x80,0x88,0x90))),('unexpected allocator store',hex(i.address))
# NULL free branches straight to RET, before any memory write or nested call.
sequence(0x180037640,[('test','rdx, rdx'),('je','0x1800376b1')]);sequence(0x1800376b1,[('ret','')])
cases=[]
for size in(32,416):
 assert 0<size<2**64-32
 cases.append(dict(unitBytes=size,count=1,requestedBytes=size,heapFlags=0,zeroInitializationProven=False,pointerPreserved=True))
report=dict(status='PASS conditional root/default state callbacks',regions=regions,mallocCases=cases,nullFreeInstructions=3,nullFreeWrites=0,condition='Initialized unmodified root/default callbacks, valid heap and first HeapAlloc success',actualCtcEnvironmentOriginProven=False,wholeConstructorProven=False,runtimePredicateApproved=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(dict(status=report['status'],mallocCases=cases,nullFreeWrites=0)))
