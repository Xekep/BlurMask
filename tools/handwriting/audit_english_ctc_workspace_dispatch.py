"""Conditional full193A40 workspace pass; symbolic instructions, no SDK execution.

The registry must already contain service49's pinned descriptor. Its actual
installation, selected caller origin,194980 and whole constructor stay open.
"""
from pathlib import Path
import argparse, json, runpy, sys, struct, hashlib
from capstone import CS_OP_REG

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True)
a=p.parse_args()
def compose(name,suffix,models=True):
 old=sys.argv;sys.argv=[str(Path(__file__).with_name(name)),'--sdk',str(a.sdk),'--output',str(a.output.with_name(a.output.stem+suffix+'.json'))]
 if models:sys.argv+=['--models',str(a.models)]
 try:return runpy.run_path(sys.argv[0],run_name='__main__')
 finally:sys.argv=old
u=compose('audit_english_ctc_initializer_dispatch.py','-sizing')
allocator=compose('audit_english_ctc_aligned_allocator.py','-allocator',False)
Base=u['Dispatch'];Address=u['Address'];OtherAddress=u['OtherAddress'];plus=u['plus'];signed=u['signed'];read=u['read'];instructions=u['instructions'];scalar=u['scalar']

class Workspace(Base):
 def __init__(self,alignment,flags,extra):
  super().__init__(alignment,flags,extra)
  self.entryNonvolatile={r:self.getreg(r)for r in ('rbx','rbp','rsi','rdi','r12','r13','r14','r15')}
  self.returnSP=plus(self.getreg('rsp'),8)
  self.sizing=super().run_dispatch()
  self.workactive=True;self.total=self.sizing['workspaceBytes'];self.bounds['workspace']=self.total
  self.workspace=None;self.payloadWrites=[];self.serviceCalls=[];self.recurrentCalls=[];self.secondSteps=0
 def load(self,address,size):
  if not getattr(self,'workactive',False):return super().load(address,size)
  if isinstance(address,int):
   if address==0x7000:assert size==8;return 0x8000
   if address==0x7008:assert size==8;return 0x1801db830
   if address in (0x1801db860,0x180229c08):assert size==8;return struct.unpack('<Q',read(address,8))[0]
   if 0x180194084<=address and address+size<=0x1801940ac:assert size==4;return struct.unpack('<I',read(address,4))[0]
  if isinstance(address,Address)and address.domain=='workspace':
   assert self.workspace is not None and 0<=address.offset and address.offset+size<=self.total
   return int.from_bytes(self.workspace[address.offset:address.offset+size],'little')
  return super().load(address,size)
 def store(self,address,size,value):
  if not getattr(self,'workactive',False):return super().store(address,size,value)
  if address.domain=='workspace':
   assert self.workspace is not None and size==4 and isinstance(value,int)and 0<=address.offset and address.offset+size<=self.total
   self.workspace[address.offset:address.offset+size]=(value&0xffffffff).to_bytes(4,'little')
   self.payloadWrites.append((address.offset,size,value&0xffffffff));return
  if address==Address('graph',24):
   assert size==8 and value==Address('workspace')
   old=self.initial;self.initial=True
   try:u['up']['Header'].store(self,address,size,value)
   finally:self.initial=old
   return
  return super().store(address,size,value)
 def call(self,target):
  if not getattr(self,'workactive',False):return super().call(target)
  assert self.getreg('rcx')==Address('environment')
  if self.pc==0x180193c8d:
   assert target==0x180037f50 and self.getreg('edx')==0x49
   result=0x180229c00;self.serviceCalls.append(dict(kind='lookup',service=73,assumedInstalledDescriptor='0x180229ca0'))
  elif self.pc==0x180193c9d:
   assert target==0x180104cc0 and self.getreg('rdx')==self.total and self.getreg('r8')==4*self.alignment
   assert self.workspace is None
   self.workspace=bytearray(self.total);result=Address('workspace')
   self.serviceCalls.append(dict(kind='aligned-zero-allocation',bytes=self.total,alignment=4*self.alignment))
  else:
   assert self.pc==0x180193dbf and target==0x180191470
   index=len(self.recurrentCalls)+1;assert index<=3
   assert self.getreg('rdx')==Address('state'+str(index))and self.getreg('r8d')==127 and self.getreg('r9d')==self.alignment
   sp=self.getreg('rsp');assert self.load(plus(sp,32),4)==0 and self.load(plus(sp,40),1)==int(self.extra)
   workspace=self.load(plus(sp,48),8);assert isinstance(workspace,Address)and workspace.domain=='workspace'
   helper=scalar['Analyzer'](self.alignment,self.extra,True)
   markers={r:0xA000+i for i,r in enumerate(('rbx','rbp','rsi','rdi','r12','r13','r14','r15'))}
   for r,v in markers.items():helper.setreg(r,v)
   evidence=helper.run();assert all(helper.getreg(r)==v for r,v in markers.items())
   for w in helper.writes:
    value=w['value']
    if isinstance(value,OtherAddress):
     assert value.domain=='workspace';value=plus(workspace,value.offset)
    destination=Address('state'+str(index),w['offset'])if w['domain']=='state'else plus(workspace,w['offset'])
    self.store(destination,w['size'],value)
   result=helper.getreg('rax');assert result*4==evidence['workspaceBytes']
   self.recurrentCalls.append(dict(state=index,byteStart=workspace.offset,bytes=result*4))
  for r in ('rax','rcx','rdx','r8','r9','r10','r11'):self.setreg(r,None)
  self.setreg('rax',result);self.zf=self.last_cmp=None
 def run_workspace(self):
  while self.pc!=0x1801946c0:
   self.secondSteps+=1;assert self.secondSteps<2048
   i=instructions[self.pc];m=i.mnemonic;o=i.operands;nxt=self.pc+i.size
   if m in ('add','sub','and','xor','imul','inc','test','idiv','shl','call'):self.last_cmp=None
   if m=='mov':self.assign(i,o[0],self.value(i,o[1]))
   elif m=='lea':self.assign(i,o[0],self.address(i,o[1]))
   elif m in ('movzx','movsxd'):
    v=self.value(i,o[1]);self.assign(i,o[0],signed(v,o[1].size*8)if m=='movsxd'else v)
   elif m in ('push','pop'):
    if m=='push':v=self.value(i,o[0]);self.setreg('rsp',plus(self.getreg('rsp'),-8));self.store(self.getreg('rsp'),8,v)
    else:v=self.load(self.getreg('rsp'),8);self.assign(i,o[0],v);self.setreg('rsp',plus(self.getreg('rsp'),8))
   elif m in ('add','sub','and','xor','imul','shl'):
    if m=='xor'and o[0].type==o[1].type==CS_OP_REG and o[0].reg==o[1].reg:v=0
    else:
     x=self.value(i,o[1])if len(o)==3 else self.value(i,o[0]);y=self.value(i,o[2])if len(o)==3 else self.value(i,o[1])
     if m=='and':assert isinstance(x,int)and isinstance(y,int);v=x&y
     else:v=plus(x,y)if m=='add'else plus(x,-y)if m=='sub'else x*y if m=='imul'else x<<y if m=='shl'else x^y
    self.assign(i,o[0],v);self.zf=None if m=='imul'or isinstance(v,Address)else self.value(i,o[0])==0
   elif m=='inc':self.assign(i,o[0],self.value(i,o[0])+1);self.zf=self.value(i,o[0])==0
   elif m=='cmp':
    x,y=self.value(i,o[0]),self.value(i,o[1]);assert isinstance(x,int)and isinstance(y,int)
    self.last_cmp=x,y,o[0].size*8;self.zf=x==y
   elif m=='test':
    x,y=self.value(i,o[0]),self.value(i,o[1])
    if isinstance(x,Address):assert x==y;self.zf=False
    else:assert isinstance(x,int)and isinstance(y,int);self.zf=(x&y)==0
   elif m=='cdqe':self.setreg('rax',signed(self.getreg('eax'),32))
   elif m=='cdq':self.setreg('edx',0xffffffff if signed(self.getreg('eax'),32)<0 else 0)
   elif m=='idiv':
    x=signed((self.getreg('edx')<<32)|self.getreg('eax'),64);y=signed(self.value(i,o[0]),32);assert y
    q=abs(x)//abs(y);q=-q if(x<0)!=(y<0)else q;assert -(1<<31)<=q<1<<31
    self.setreg('eax',q);self.setreg('edx',x-q*y);self.zf=None
   elif m in ('cmove','sete','setne'):
    assert self.zf is not None
    if m=='cmove':
     if self.zf:self.assign(i,o[0],self.value(i,o[1]))
    else:self.assign(i,o[0],int(self.zf==(m=='sete')))
   elif m in ('jle','jl','ja'):
    assert self.last_cmp is not None;x,y,bits=self.last_cmp
    take=signed(x,bits)<=signed(y,bits)if m=='jle'else signed(x,bits)<signed(y,bits)if m=='jl'else(x&((1<<bits)-1))>(y&((1<<bits)-1))
    if take:nxt=self.value(i,o[0])
   elif m in ('je','jne','jmp'):
    assert m=='jmp'or self.zf is not None
    if m=='jmp'or self.zf==(m=='je'):nxt=self.value(i,o[0])
   elif m=='call':self.call(self.value(i,o[0]))
   elif m=='ret':nxt=self.load(self.getreg('rsp'),8);self.setreg('rsp',plus(self.getreg('rsp'),8))
   elif m=='nop':pass
   else:raise AssertionError(('unsupported workspace pass',hex(self.pc),m,i.op_str))
   self.pc=nxt
  assert self.getreg('al')==1 and self.getreg('rsp')==self.returnSP
  assert all(self.getreg(r)==v for r,v in self.entryNonvolatile.items())
  assert len(self.serviceCalls)==2 and len(self.recurrentCalls)==3
  assert self.load(Address('graph',24),8)==Address('workspace')
  # Independent ordered payload-index algebra, including the two input bias1s.
  predicted=bytearray(self.total);stride=((14+self.alignment-1)//self.alignment)*self.alignment
  for offset in (13*4,(stride+13)*4):predicted[offset:offset+4]=struct.pack('<I',0x3f800000)
  certificate=next(c for c in scalar['algebra']['certificates']if c['alignmentFloatUnits']==self.alignment and c['extraRow']==self.extra)
  base=(2+int(self.extra))*stride*4
  expected_calls=[]
  metadata={}
  for off,n,value in [(0,8,Address('workspace')),(8,8,Address('workspace',stride*4)),
                       (16,8,Address('workspace',stride*8)if self.extra else 0),(24,4,13),(28,4,stride)]:
   metadata[Address('state0',off),n]=value
  for index in (1,2,3):
   expected_calls.append(dict(state=index,byteStart=base,bytes=certificate['workspaceBytes']))
   for s in certificate['orderedStores']:
    offset=base+s['byteOffset'];predicted[offset:offset+4]=struct.pack('<I',int(s['bits'],16))
   helper=scalar['Analyzer'](self.alignment,self.extra,True);helper.run()
   for (address,n),value in helper.memory.items():
    if address.domain!='state':continue
    if isinstance(value,OtherAddress):
     assert value.domain=='workspace';value=Address('workspace',base+value.offset)
    metadata[Address('state'+str(index),address.offset),n]=value
   base+=certificate['workspaceBytes']
  assert base==self.total and self.recurrentCalls==expected_calls and self.workspace==predicted
  domains={'state0','state1','state2','state3'}
  assert u['image'](self.memory,domains)==u['image'](metadata,domains)
  assert sum(n for address,n in metadata)==1280
  expected_readonly=self.readonly.copy()
  for j in range(8):del expected_readonly[Address('graph',24+j),1]
  expected_readonly[Address('graph',24),8]=Address('workspace')
  assert u['image'](self.memory,self.readonly_domains)==expected_readonly
  assert len(self.payloadWrites)==140
  return dict(alignment=self.alignment,flagsPresent=self.flagsPresent,extraRow=self.extra,
              stopPC=hex(self.pc),steps=self.secondSteps,workspaceBytes=self.total,
              payloadStoreOccurrences=len(self.payloadWrites),payloadSha256=hashlib.sha256(self.workspace).hexdigest(),
              stateMetadataBytesCompared=1280,readonlyObjectsPreserved=True,nonvolatileRestored=True,
              services=self.serviceCalls,recurrentCalls=self.recurrentCalls,returnedTrue=True)

cases=[Workspace(alignment,flags,extra).run_workspace()for alignment in (8,16)for flags in (False,True)for extra in (False,True)]
report=dict(status='PASS conditional193A40 workspace pass',cases=cases,
            condition='Inherited constructor sizing/allocation conditions plus actual registry service49 descriptor229CA0 and fresh successful ROOT/default aligned allocation',
            actualService49RegistrationProven=False,actualCtcEnvironmentOriginProven=False,
            subsequent194980Proven=False,wholeConstructorProven=False,runtimePredicateApproved=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('PASS8 conditional workspace passes; whole constructor remains unresolved')
