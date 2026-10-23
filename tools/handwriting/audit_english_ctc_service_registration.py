"""Conditional module service73 registration and lookup; no SDK execution.

Follows004920's first registration, the actual default core thunk/body, then
037F50 lookup. Assumes the module entry is called with the initialized default
core/ROOT tables and fresh successful disjoint allocator callbacks. Does not
prove module/caller origin, other services, replacement or failure cleanup.
"""
from pathlib import Path
import argparse, hashlib, json, runpy, struct, sys
from capstone import CS_OP_REG, CS_OP_IMM, CS_OP_MEM

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk',type=Path,required=True);p.add_argument('--output',type=Path,required=True)
a=p.parse_args();tool=Path(__file__).with_name('audit_english_ctc_aligned_service_lookup.py')
old=sys.argv;sys.argv=[str(tool),'--sdk',str(a.sdk),'--output',str(a.output.with_name(a.output.stem+'-lookup.json'))]
try:up=runpy.run_path(str(tool),run_name='__main__')
finally:sys.argv=old
callback_tool=Path(__file__).with_name('audit_english_ctc_state_callbacks.py')
old=sys.argv;sys.argv=[str(callback_tool),'--sdk',str(a.sdk),'--output',str(a.output.with_name(a.output.stem+'-callbacks.json'))]
try:callbacks=runpy.run_path(str(callback_tool),run_name='__main__')
finally:sys.argv=old
read,qword,md=(up[k]for k in ('read','qword','md'))
instructions=dict(up['decoded']);bodies=[]
for start,size,digest in [
    (0x180004920,347,'9f0326f50c002cb823b306abe055463ae9cc9a0aa295d63b82de922e6556d262'),
    (0x180037d60,5,'c0d58e74c92c5c5a7c9f5c0d472cd03919268f956e78e774603f3d090f3a2f31'),
    (0x1800387a0,717,'796d320d783d3c7bd6a64a100edc123ab935e74f0e045c4e7cfe0a8b4bea92a9'),
]:
    b=read(start,size);assert hashlib.sha256(b).hexdigest()==digest
    ii=list(md.disasm(b,start));assert sum(i.size for i in ii)==size
    instructions.update({i.address:i for i in ii});bodies.append(dict(address=hex(start),bytes=size,instructions=len(ii),sha256=digest))
assert qword(0x180252e20)==0x180229ca0 and qword(0x1801db830)==0x180037d60
assert qword(0x180229cd0)==0 and qword(0x180229cc0)==0x180229c00
assert qword(0x1801db5a0)==0x180037340 and qword(0x1801db5b8)==0x180037410

aliases={}
for family,names in [('rax',['rax','eax','ax','al']),('rbx',['rbx','ebx','bx','bl']),('rcx',['rcx','ecx','cx','cl']),('rdx',['rdx','edx','dx','dl']),('rsp',['rsp','esp','sp','spl']),('rbp',['rbp','ebp','bp','bpl']),('rsi',['rsi','esi','si','sil']),('rdi',['rdi','edi','di','dil'])]+[(f'r{i}',[f'r{i}',f'r{i}d',f'r{i}w',f'r{i}b'])for i in range(8,16)]:
    for name,bits in zip(names,(64,32,16,8)):aliases[name]=family,bits


class Registration:
    ENV=0x700000000;STACK=0x800000000;CORE=0x900000000
    def __init__(self,depth):
        assert depth in range(4)
        self.depth=depth;self.reg={};self.memory={};self.bounds={self.ENV:256,self.STACK-2048:4096,self.CORE:16}
        self.allocated=[];self.writes=[];self.pc=0x180004920;self.zf=self.cf=self.comparison=None;self.steps=0
        for n,(family,_)in aliases.items():self.reg[family]=0xA000+len(self.reg)
        self.reg['xmm0']=0
        self.setreg('rsp',self.STACK);self.setreg('rcx',self.ENV)
        self.seed(self.ENV,8,0);self.seed(self.ENV+8,8,self.CORE|1);self.seed(self.CORE,8,0x1801db5a0);self.seed(self.CORE+8,8,0x1801db830)
        self.seed(self.STACK,8,0xB000);self.seed(self.ENV+152,8,0)
        # Four admitted pre-registration states: missing root, missing second
        # table, missing leaf, or an existing empty leaf. Sentinels are actual
        # immutable tables; malloc bytes stay unknown until explicitly written.
        prior=[]
        for index,size in enumerate((512,128,128)[:depth]):
            base=0xA00000000+index*0x10000;self.bounds[base]=size;prior.append(base)
            for off in range(0,size,8):self.seed(base+off,8,0x1801db7b0 if index==0 else 0x1801db730 if index==1 else 0)
        if depth:self.seed(self.ENV+152,8,prior[0])
        if depth>1:self.seed(prior[0],8,prior[1])
        if depth>2:self.seed(prior[1]+32,8,prior[2])
        self.initial_memory=self.memory.copy()

    def getreg(self,name):
        if name.startswith('xmm'):return self.reg[name]
        family,bits=aliases[name];return self.reg[family]&((1<<bits)-1)
    def setreg(self,name,value):
        if name.startswith('xmm'):self.reg[name]=value&((1<<128)-1);return
        family,bits=aliases[name];value&=(1<<bits)-1
        self.reg[family]=value if bits>=32 else (self.reg[family]&~((1<<bits)-1))|value
    def admitted(self,address,size):return any(base<=address and address+size<=base+n for base,n in self.bounds.items())
    def seed(self,address,size,value):
        assert self.admitted(address,size)
        for j in range(size):self.memory[address+j]=(value>>(8*j))&255
    def store(self,address,size,value):
        self.seed(address,size,value);self.writes.append((address,size,value&((1<<(size*8))-1)))
    def load(self,address,size):
        if self.admitted(address,size):
            assert all(address+j in self.memory for j in range(size)),('undefined memory read',hex(self.pc),hex(address),size)
            return int.from_bytes(bytes(self.memory[address+j]for j in range(size)),'little')
        # Only immutable PE bytes are readable; runtime global tables are
        # represented explicitly above rather than trusting their PE zeroes.
        assert 0x180000000<=address<0x180253000,(hex(address),size)
        return int.from_bytes(read(address,size),'little')
    def address(self,i,o):
        m=o.mem;v=m.disp
        if m.base:v+=i.address+i.size if i.reg_name(m.base)=='rip'else self.getreg(i.reg_name(m.base))
        if m.index:v+=self.getreg(i.reg_name(m.index))*m.scale
        return v
    def value(self,i,o):
        if o.type==CS_OP_REG:return self.getreg(i.reg_name(o.reg))
        if o.type==CS_OP_IMM:return o.imm
        assert o.type==CS_OP_MEM;return self.load(self.address(i,o),o.size)
    def assign(self,i,o,v):
        if o.type==CS_OP_REG:self.setreg(i.reg_name(o.reg),v)
        else:assert o.type==CS_OP_MEM;self.store(self.address(i,o),o.size,v)
    def allocate(self,target):
        assert self.getreg('rcx')==self.ENV and self.getreg('r8')==1
        size=self.getreg('rdx');zero=target==0x180037410
        assert target in (0x180037340,0x180037410) and size in (512,128,24)
        assert zero is (size==128 and self.pc==0x180038925)
        base=0xB00000000+len(self.allocated)*0x10000;assert base not in self.bounds
        self.bounds[base]=size
        if zero:
            for j in range(size):self.seed(base+j,1,0)
        self.allocated.append(dict(address=base,bytes=size,zeroInitialized=zero))
        for n in ('rcx','rdx','r8','r9','r10','r11'):self.setreg(n,0xDEADC0DE)
        self.setreg('rax',base);self.zf=self.cf=self.comparison=None
    def run_until(self,stop):
        while self.pc!=stop:
            self.steps+=1;assert self.steps<4096
            i=instructions[self.pc];m=i.mnemonic;o=i.operands;nxt=self.pc+i.size
            if m in ('mov','movups','movq'):self.assign(i,o[0],self.value(i,o[1]))
            elif m=='movlhps':
                low=(1<<64)-1;self.assign(i,o[0],(self.value(i,o[0])&low)|((self.value(i,o[1])&low)<<64))
            elif m=='lea':self.assign(i,o[0],self.address(i,o[1]))
            elif m=='movsxd':
                v=self.value(i,o[1]);self.assign(i,o[0],v-(1<<32)if v&(1<<31)else v)
            elif m in ('push','pop'):
                if m=='push':v=self.value(i,o[0]);self.setreg('rsp',self.getreg('rsp')-8);self.store(self.getreg('rsp'),8,v)
                else:v=self.load(self.getreg('rsp'),8);self.assign(i,o[0],v);self.setreg('rsp',self.getreg('rsp')+8)
            elif m in ('add','sub','and','xor','sbb','sar'):
                x,y=self.value(i,o[0]),self.value(i,o[1]);bits=o[0].size*8
                if m=='sar':x=x-(1<<bits)if x&(1<<(bits-1))else x;v=x>>y
                else:v=x+y if m=='add'else x-y if m=='sub'else x&y if m=='and'else x^y if m=='xor'else x-y-int(self.cf)
                self.assign(i,o[0],v);self.zf=self.value(i,o[0])==0;self.comparison=None
            elif m=='neg':
                v=self.value(i,o[0]);self.cf=v!=0;self.assign(i,o[0],-v);self.zf=v==0
            elif m in ('cmp','test'):
                x,y=self.value(i,o[0]),self.value(i,o[1]);self.zf=x==y if m=='cmp'else(x&y)==0
                self.comparison=(x,y)if m=='cmp'else None
            elif m=='ja':
                assert self.comparison is not None
                if self.comparison[0]>self.comparison[1]:nxt=self.value(i,o[0])
            elif m in ('je','jne','jmp'):
                assert m=='jmp'or self.zf is not None
                if m=='jmp'or self.zf==(m=='je'):nxt=self.value(i,o[0])
            elif m=='call':
                target=self.value(i,o[0])
                if target in (0x180037340,0x180037410):self.allocate(target)
                else:
                    assert self.pc==0x18000494d and target==0x180037d60
                    assert self.getreg('rcx')==self.ENV and self.getreg('rdx')==0x180229ca0
                    self.callback_sp=self.getreg('rsp')
                    self.callback_nonvolatile={r:self.getreg(r)for r in ('rbx','rbp','rsi','rdi','r12','r13','r14','r15')}
                    self.setreg('rsp',self.getreg('rsp')-8);self.store(self.getreg('rsp'),8,nxt);nxt=target
            elif m=='ret':nxt=self.load(self.getreg('rsp'),8);self.setreg('rsp',self.getreg('rsp')+8)
            elif m=='nop':pass
            else:raise AssertionError(('unsupported registration instruction',hex(self.pc),m,i.op_str))
            self.pc=nxt
    def run(self):
        self.run_until(0x180004950)
        assert self.getreg('al')==1 and len(self.allocated)==4-self.depth
        assert [x['bytes']for x in self.allocated]==[512,128,128,24][self.depth:]
        assert self.getreg('rsp')==self.callback_sp
        assert all(self.getreg(r)==v for r,v in self.callback_nonvolatile.items())
        root=self.load(self.ENV+152,8);second=self.load(root,8);leaf=self.load(second+32,8);entry=self.load(leaf+72,8)
        assert self.load(entry,8)==0x180229ca0 and self.load(entry+8,1)==0 and self.load(entry+10,2)==1
        # Slot73 is the only nonzero registry leaf. Default sentinels elsewhere
        # still point to their immutable empty tables.
        assert all(self.load(root+8*j,8)==(second if j==0 else 0x1801db7b0)for j in range(64))
        assert all(self.load(second+8*j,8)==(leaf if j==4 else 0x1801db730)for j in range(16))
        assert all(self.load(leaf+8*j,8)==(entry if j==9 else 0)for j in range(16))
        assert not any(entry+j in self.memory for j in (9,12,13,14,15,16,17,18,19,20,21,22,23))
        self.setreg('rcx',self.ENV);self.setreg('edx',73)
        self.setreg('rsp',self.getreg('rsp')-8);self.store(self.getreg('rsp'),8,0xB001);self.pc=0x180037f50
        self.run_until(0xB001)
        assert self.getreg('rax')==0x180229c00
        return dict(existingRegistryDepth=self.depth,allocations=self.allocated,
            registryIndices=[0,4,9],descriptor='0x180229ca0',lookupVtable='0x180229c00',steps=self.steps,
            entryDefinedBytes=11,entryUndefinedBytes=13)


cases=[Registration(depth).run()for depth in range(4)]
allocator_sizes=[]
for size in (24,128,512):
    # Same pinned positive-count1 malloc branch; no multiplication/CRT limit
    # overflow and no promise about uninitialized payload. The128-byte leaf
    # instead uses the independently checked positive calloc branch.
    assert 0<size<2**64-32 and size*1==size
    allocator_sizes.append(dict(unitBytes=size,count=1,mallocHeapFlags=0,callocHeapFlags=8 if size==128 else None))
report=dict(status='PASS conditional module service73 registration and lookup',bodies=bodies,cases=cases,
    allocatorSizes=allocator_sizes,allocatorAccountingCountersIncluded=False,
    condition='004920 module entry called on initialized unmodified default ROOT/core; four empty registry shapes with0..3 existing levels, all other slots sentinel/zero and service73 absent; fresh successful disjoint malloc/calloc callbacks',
    actualModuleEntryCallerProven=False,actualCtcEnvironmentOriginProven=False,
    allModuleServicesProven=False,failureCleanupProven=False,runtimePredicateApproved=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS4 conditional service73 registration/lookup walks; module caller origin unresolved')
