"""Conditional constructor state allocation, abstract instructions only.

Requires the separately audited root/default zero-allocation contract, fresh
disjoint successful malloc callbacks, preserving free(NULL) callbacks and
optional packet count58=0. Actual caller
origin, subsequent initializers and whole constructor remain unproved.
"""
from pathlib import Path
import argparse,runpy,sys,json,struct
from capstone import CS_OP_REG

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True)
args=p.parse_args();repo=Path(__file__).resolve().parents[2]
def compose(name,suffix,models=False):
 old=sys.argv;sys.argv=[str(repo/'tools/handwriting'/name),'--sdk',str(args.sdk),'--output',str(args.output.with_name(args.output.stem+suffix+'.json'))]
 if models:sys.argv+=['--models',str(args.models)]
 try:return runpy.run_path(sys.argv[0],run_name='__main__')
 finally:sys.argv=old
allocation=compose('audit_english_ctc_default_allocator.py','-allocator')
up=compose('audit_english_ctc_header_origin.py','-headers',True)
Header=up['HeaderOrigin'];Address=up['Address'];plus=up['plus'];signed=up['signed'];instructions=up['instructions'];read=up['read']

class StateAllocation(Header):
 def __init__(self,alignment,flagsPresent,mutation=None):
  super().__init__(alignment,flagsPresent);super().run()
  assert self.pc==0x1801944ec
  self.mutation=mutation;self.stateCalls=[];self.allocated={'headers'}
  # Compose zero-fill with the preceding proof, which did not read these bytes.
  # Preserve every explicit constructor store; only untouched bytes become0.
  self.initial=True
  covered={a.offset+i for a,n in self.memory if a.domain=='headers'for i in range(n)}
  for i in range(400):
   if i not in covered:self.store(Address('headers',i),1,0)
  self.store(Address('packet',88),4,0)
  self.initial=False;self.tailSeed=self.memory.copy();self.tailWriteStart=len(self.writes)
  if mutation=='header-not-zero':self.store(Address('headers',48),4,1)
 def load(self,address,size):
  if isinstance(address,int):
   if address in (0x8000,0x8008):assert size==8;return 0xABD if address==0x8000 else 0xABE
   if 0x1801946f8<=address and address+size<=0x180194720:assert size==4;return struct.unpack('<I',read(address,size))[0]
   return super().load(address,size)
  if (address,size) in self.memory:return super().load(address,size)
  # Merge fully defined scalar subcells only. Never manufacture bytes in malloc.
  b={}
  for (a,n),value in self.memory.items():
   if a.domain!=address.domain or not isinstance(value,int):continue
   for j,x in enumerate((value&((1<<(8*n))-1)).to_bytes(n,'little')):b[a.offset+j]=x
  assert all(address.offset+j in b for j in range(size)),('undefined scalar read',hex(self.pc),address,size)
  value=int.from_bytes(bytes(b[address.offset+j]for j in range(size)),'little')
  self.reads.append((address,size));return value
 def store(self,address,size,value):
  if hasattr(self,'allocated')and address.domain not in ('stack','graph','headers','packet'):
   assert address.domain=='state-pointers' and (value==Address('state'+str(address.offset//8))or value==0)
   # Header.store admits only graph/headers after initialization; keep the same
   # bounds and overlap semantics while allowing this one newly allocated table.
   was=self.initial;self.initial=True
   try:super().store(address,size,value)
   finally:self.initial=was
   if not was:self.writes.append(dict(pc=hex(self.pc),domain=address.domain,offset=address.offset,size=size,value=value))
  else:super().store(address,size,value)
 def call(self,target):
  assert self.getreg('rcx')==Address('environment')
  if target==0xABC:
   assert self.getreg('edx')==8 and self.getreg('r8')==4
   name='state-pointers';bound=32;zero=True
  elif target==0xABD:
   index=sum(x['kind']=='malloc'for x in self.stateCalls);name='state'+str(index)
   bound=32 if index==0 else 416;zero=False
   assert self.getreg('edx')==bound and self.getreg('r8')==1,(hex(self.pc),index,self.getreg('edx'),self.getreg('r8'))
   if self.mutation=='undersized-state'and index==1:bound-=1
  elif target==0xABE:
   assert self.getreg('rdx')==0,('free must receive calloc NULL',self.getreg('rdx'))
   self.stateCalls.append(dict(kind='free',pointer=0))
   for r in ('rax','rcx','rdx','r8','r9','r10','r11'):self.setreg(r,None)
   self.zf=None;self.last_cmp=None;return
  else:raise AssertionError(('unexpected call',hex(self.pc),target))
  assert name not in self.allocated
  self.allocated.add(name);self.bounds[name]=bound
  result=Address(name)
  if self.mutation=='alias-allocation'and name=='state1':result=Address('headers')
  if self.mutation=='null-allocation'and name=='state1':result=0
  assert result==Address(name),('fresh disjoint successful allocation required',name,result)
  if zero:
   was=self.initial;self.initial=True
   try:
    for off in range(0,bound,8):self.store(Address(name,off),8,0)
   finally:self.initial=was
  self.stateCalls.append(dict(kind='calloc'if zero else'malloc',domain=name,bytes=bound,zeroInitialized=zero))
  for r in ('rcx','rdx','r8','r9','r10','r11'):self.setreg(r,None)
  self.setreg('rax',result);self.zf=None;self.last_cmp=None
 def run_tail(self):
  while self.pc!=0x1801946a9:
   self.steps+=1;assert self.steps<8192
   i=instructions[self.pc];m=i.mnemonic;o=i.operands;nxt=self.pc+i.size
   if m in ('add','and','xor','inc','call','test'):self.last_cmp=None
   if m=='mov':self.assign(i,o[0],self.value(i,o[1]))
   elif m=='movsxd':self.assign(i,o[0],signed(self.value(i,o[1]),32))
   elif m=='lea':self.assign(i,o[0],self.address(i,o[1]))
   elif m=='cdqe':self.setreg('rax',signed(self.getreg('eax'),32))
   elif m in ('add','and','xor'):
    if m=='xor'and o[0].type==o[1].type==CS_OP_REG and o[0].reg==o[1].reg:v=0
    else:
     a=self.value(i,o[0]);b=self.value(i,o[1]);v=plus(a,b)if m=='add'else a&b if m=='and'else a^b
    self.assign(i,o[0],v);self.zf=None if isinstance(v,Address)else self.value(i,o[0])==0
   elif m=='inc':self.assign(i,o[0],self.value(i,o[0])+1);self.zf=self.value(i,o[0])==0
   elif m=='cmp':
    a=self.value(i,o[0]);b=self.value(i,o[1]);assert isinstance(a,int)and isinstance(b,int);self.last_cmp=(a,b,o[0].size*8);self.zf=a==b
   elif m=='test':
    a=self.value(i,o[0]);b=self.value(i,o[1]);assert a==b;self.zf=False if isinstance(a,Address)else a==0
    if isinstance(a,int):self.last_cmp=(a,0,o[0].size*8)
   elif m=='cmovl':
    assert self.last_cmp is not None;a,b,bits=self.last_cmp
    if signed(a,bits)<signed(b,bits):self.assign(i,o[0],self.value(i,o[1]))
   elif m in ('jle','jl','jg','ja'):
    assert self.last_cmp is not None
    a,b,bits=self.last_cmp
    condition=signed(a,bits)<=signed(b,bits)if m=='jle'else signed(a,bits)<signed(b,bits)if m=='jl'else signed(a,bits)>signed(b,bits)if m=='jg'else(a&((1<<bits)-1))>(b&((1<<bits)-1))
    if condition:nxt=self.value(i,o[0])
   elif m in ('je','jne','jmp'):
    assert m=='jmp'or self.zf is not None
    if m=='jmp'or self.zf==(m=='je'):nxt=self.value(i,o[0])
   elif m=='call':self.call(self.value(i,o[0]))
   elif m=='nop':pass
   else:raise AssertionError(('unsupported tail',hex(self.pc),m,i.op_str))
   self.pc=nxt
  # Independent final images: input32 + three recurrent416; output has no state.
  assert [(c['kind'],c.get('bytes'))for c in self.stateCalls]==[('calloc',32)]+[x for n in (32,416,416,416)for x in [('free',None),('malloc',n)]]
  assert self.load(Address('graph',16),8)==Address('state-pointers')and self.load(Address('graph',48),4)==0 and self.load(Address('graph',64),4)==5
  for index,bound in enumerate((32,416,416,416)):
   assert self.bounds['state'+str(index)]==bound
   assert self.load(Address('state-pointers',8*index),8)==Address('state'+str(index))
   assert self.load(Address('headers',80*index+44),4)==self.load(Address('headers',80*index+48),4)==1
   assert not any(a.domain=='state'+str(index)for a,n in self.memory),('ordinary malloc bytes were invented',index)
  predicted=self.tailSeed.copy()
  for off,size,value in [(16,8,Address('state-pointers')),(48,4,0),(64,4,5)]:predicted[Address('graph',off),size]=value
  # Compare scalar-byte images after exact overlapping stores; symbolic pointers
  # remain typed cells and cannot be reinterpreted as arbitrary integers.
  def image(mem):
   out={}
   for (a,n),v in mem.items():
    if a.domain=='stack':continue
    if isinstance(v,Address)or v is None:out[a,n]=v
    else:
     for j,b in enumerate((v&((1<<(8*n))-1)).to_bytes(n,'little')):out[Address(a.domain,a.offset+j),1]=b
   return out
  expected=image(predicted)
  for index in range(4):
   expected[Address('state-pointers',index*8),8]=Address('state'+str(index))
   for off in (44,48):
    for j,b in enumerate((1).to_bytes(4,'little')):expected[Address('headers',80*index+off+j),1]=b
  assert image(self.memory)==expected,'complete defined memory image'
  return dict(alignment=self.alignment,flagsPresent=self.flagsPresent,stopPC=hex(self.pc),calls=self.stateCalls,steps=self.steps,totalPayloadBytes=1712,ordinaryStateBytesUninitialized=1280)

cases=[StateAllocation(a,f).run_tail()for a in (8,16)for f in (False,True)]
def encode(v):
 if isinstance(v,Address):return dict(domain=v.domain,offset=v.offset)
 raise TypeError(type(v))
report=dict(status='PASS conditional constructor state allocation',cases=cases,condition='Root/default zero-allocation contract; optional packet count58=0; malloc callbacks fresh/disjoint/successful; free(NULL) callbacks preserve live allocations',wholeConstructorProven=False,actualCallerOriginProven=False,mallocAndFreeCallbackBodiesProven=False,subsequentInitializersProven=False,vendorExecuted=False,runtimePredicateApproved=False)
args.output.write_text(json.dumps(report,indent=2,default=encode)+'\n');print('PASS4 conditional state-allocation cases; no malloc bytes inferred')
