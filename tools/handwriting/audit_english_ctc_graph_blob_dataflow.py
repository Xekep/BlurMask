"""Conditional symbolic graph matrix binding; never executes vendor code.

Headers/matrix blocks are declared initialized live disjoint objects. This
proves the selected binding walk, not their allocator or caller origin.
"""
from pathlib import Path
import argparse,runpy,sys,json,hashlib,struct
from capstone import Cs,CS_ARCH_X86,CS_MODE_64,CS_OP_REG,CS_OP_MEM,CS_OP_IMM
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);args=p.parse_args()
repo=Path(__file__).resolve().parents[2];old=sys.argv;sys.argv=[str(repo/'tools/handwriting/audit_english_ctc_matrix_blob_dataflow.py'),'--sdk',str(args.sdk),'--models',str(args.models),'--output',str(args.output.with_name(args.output.stem+'-helper.json'))]
try:up=runpy.run_path(sys.argv[0],run_name='__main__')
finally:sys.argv=old
Meta=up['Base'];Address=up['Address'];plus=up['plus'];signed=up['signed'];aliases=up['aliases'];read=up['read']
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True;instructions={}
for start,size,digest in [(0x180197de0,1833,'c11f88a8dfed20a46f12f038558d3cd7542c425499f7d77be4a1368917236257'),(0x180193630,761,'28569a9b8c3bf7ddc33c55bd301a8869ecb822d19c733428fd951917bf89dd44')]:
 code=read(start,size);assert hashlib.sha256(code).hexdigest()==digest;ii=list(md.disasm(code,start));assert sum(i.size for i in ii)==size;instructions.update({i.address:i for i in ii})
class GraphBinder(Meta):
    def __init__(self,alignment,blobPresent,scratchPresent):
        self.registers={family:None for family,_,_ in aliases.values()};self.memory={};self.writes=[];self.reads=[];self.zf=None;self.steps=0;self.pc=0x180197de0;self.last_cmp=None;self.initial=True;self.calls=[]
        self.alignment=alignment;self.blobPresent=blobPresent;self.scratchPresent=scratchPresent;self.bounds={'graph':88,'headers':400};self.workBounds={};self.roles=[]
        for layer in (1,2,3,4):
            if layer<4:
                roles=[('i'+str(layer),16 if layer==1 else 256,False,896),('r'+str(layer),128,True,896)]
            else:roles=[('out',256,False,64)]
            for name,width,mode,bound in roles:
                seed=Meta(width,alignment,mode,scratchPresent);seed.run();self.bounds[name]=bound;self.workBounds['work-'+name]=seed.expected;self.roles.append((name,width,mode,bound))
                for (address,size),value in seed.memory.items():
                    if address.domain!='state' or address.offset>=bound:continue
                    if isinstance(value,Address):assert value.domain=='workspace';value=Address('work-'+name,value.offset)
                    self.store(Address(name,address.offset),size,value)
        for index,kind,width in [(0,1,13),(1,8,127),(2,8,127),(3,8,127),(4,1,182)]:
            self.store(Address('headers',index*80),4,kind);self.store(Address('headers',index*80+4),4,width)
            if index:
                self.store(Address('headers',index*80+36),4,0);self.store(Address('headers',index*80+40),4,2);self.store(Address('headers',index*80+56),8,Address('i'+str(index)if index<4 else 'out'))
                if index<4:self.store(Address('headers',index*80+64),8,Address('r'+str(index)))
        self.store(Address('graph'),4,4);self.store(Address('graph',8),8,Address('headers'))
        align=lambda n:((n+alignment-1)//alignment)*alignment
        self.expected=0
        for name,width,mode,bound in self.roles:
            rows=182 if name=='out'else 127;stride=((width+2*alignment-1)//(2*alignment))*(2*alignment);one=rows*stride*2+align(rows)*8+align(1)*4
            self.expected+=(one if name=='out'else 2*(4*one+int(mode)*3*align(width)*4))
        self.seedMemory=self.memory.copy();self.initial=False;self.writes=[];self.reads=[]
        self.setreg('rsp',Address('stack'));self.setreg('rcx',Address('environment'));self.setreg('rdx',Address('graph'));self.setreg('r8',Address('blob')if blobPresent else 0);self.setreg('r9d',alignment);self.store(Address('stack'),8,0xB00)
    def address(self,i,o):
        m=o.mem;v=m.disp
        if m.base:
            name=i.reg_name(m.base);v=plus(v,i.address+i.size if name=='rip'else self.getreg(name))
        if m.index:
            index=self.getreg(i.reg_name(m.index))
            if isinstance(index,Address):assert m.scale==1;v=plus(v,index)
            else:assert isinstance(index,int);v=plus(v,index*m.scale)
        return v
    def store(self,address,size,value):
        assert isinstance(address,Address)
        if address.domain=='stack':assert -1024<=address.offset and address.offset+size<=128
        else:
            assert address.domain in self.bounds and 0<=address.offset and address.offset+size<=self.bounds[address.domain]
            assert self.initial or address.domain not in ('graph','headers'),('readonly header/graph write',hex(self.pc),address)
        if not self.initial and isinstance(value,Address):
            if value.domain=='blob':assert self.blobPresent and 0<=value.offset<self.expected
            if value.domain.startswith('work-'):assert self.scratchPresent and 0<=value.offset<self.workBounds[value.domain]
        for old,n in list(self.memory):
            if old.domain==address.domain and old.offset<address.offset+size and address.offset<old.offset+n:del self.memory[old,n]
        self.memory[address,size]=value
        if not self.initial and address.domain!='stack':self.writes.append(dict(pc=hex(self.pc),domain=address.domain,offset=address.offset,size=size,value=value))
    def load(self,address,size):
        if isinstance(address,int):
            assert size==4 and 0x18019850c<=address and address+4<=0x180198544
            return struct.unpack('<I',read(address,size))[0]
        assert isinstance(address,Address) and address.domain!='blob' and not address.domain.startswith('work-')
        return super().load(address,size)
    def run(self):
        while True:
            self.steps+=1;assert self.steps<8192
            i=instructions[self.pc];m=i.mnemonic;o=i.operands;nxt=self.pc+i.size
            if m in ('add','sub','and','xor','imul','test','idiv','inc','shl','call'):self.last_cmp=None
            if m=='mov':self.assign(i,o[0],self.value(i,o[1]))
            elif m=='lea':self.assign(i,o[0],self.address(i,o[1]))
            elif m in ('movzx','movsxd'):
                v=self.value(i,o[1]);self.assign(i,o[0],signed(v,o[1].size*8)if m=='movsxd'else v)
            elif m=='push':
                v=self.value(i,o[0]);self.setreg('rsp',plus(self.getreg('rsp'),-8));self.store(self.getreg('rsp'),8,v)
            elif m=='pop':
                v=self.load(self.getreg('rsp'),8);self.assign(i,o[0],v);self.setreg('rsp',plus(self.getreg('rsp'),8))
            elif m in ('add','sub','and','xor','imul'):
                if m=='xor'and o[0].type==o[1].type==CS_OP_REG and o[0].reg==o[1].reg:v=0
                else:
                    a=self.value(i,o[0]);b=self.value(i,o[1])
                    if m=='add':v=plus(a,b)
                    elif m=='sub':assert isinstance(b,int);v=plus(a,-b)
                    else:
                        assert isinstance(a,int)and isinstance(b,int),(hex(self.pc),a,b)
                        v=a&b if m=='and'else a^b if m=='xor'else a*b
                self.assign(i,o[0],v)
                self.zf=None if m=='imul'or isinstance(v,Address)else self.value(i,o[0])==0
            elif m=='cmp':
                a=self.value(i,o[0]);b=self.value(i,o[1]);assert isinstance(a,int) and isinstance(b,int);self.zf=a==b;self.last_cmp=(a,b,o[0].size*8)
            elif m in ('inc','shl'):
                a=self.value(i,o[0]);assert isinstance(a,int);v=a+1 if m=='inc'else a<<self.value(i,o[1]);self.assign(i,o[0],v);self.zf=self.value(i,o[0])==0
            elif m=='test':
                a=self.value(i,o[0]);b=self.value(i,o[1])
                if isinstance(a,Address):assert a==b;self.zf=False
                else:assert isinstance(a,int)and isinstance(b,int);self.zf=(a&b)==0
            elif m=='cdq':self.setreg('edx',0xffffffff if signed(self.getreg('eax'),32)<0 else 0)
            elif m=='cdqe':self.setreg('rax',signed(self.getreg('eax'),32))
            elif m=='idiv':
                numerator=signed((self.getreg('edx')<<32)|self.getreg('eax'),64);denominator=signed(self.value(i,o[0]),32);assert denominator!=0
                quotient=abs(numerator)//abs(denominator);quotient=-quotient if(numerator<0)!=(denominator<0)else quotient
                assert -(1<<31)<=quotient<1<<31
                self.setreg('eax',quotient);self.setreg('edx',numerator-quotient*denominator);self.zf=None
            elif m in ('cmove','cmovne','sete','setne'):
                assert self.zf is not None,('undefined flag',hex(self.pc))
                condition=self.zf if m in ('cmove','sete')else not self.zf
                if m.startswith('set'):self.assign(i,o[0],int(condition))
                elif condition:self.assign(i,o[0],self.value(i,o[1]))
            elif m=='nop':pass
            elif m in ('jl','jle','ja'):
                assert self.last_cmp is not None;aa,bb,bits=self.last_cmp
                condition=signed(aa,bits)<signed(bb,bits) if m=='jl' else signed(aa,bits)<=signed(bb,bits) if m=='jle' else (aa&((1<<bits)-1))>(bb&((1<<bits)-1))
                if condition:nxt=self.value(i,o[0])
            elif m in ('je','jne','jmp'):
                if m=='jmp':nxt=self.value(i,o[0])
                else:
                    assert self.zf is not None,('undefined branch',hex(self.pc))
                    if self.zf==(m=='je'):nxt=self.value(i,o[0])
            elif m=='call':
                target=self.value(i,o[0]);assert target==0x180193630
                sp=self.getreg('rsp');self.calls.append(dict(pc=hex(self.pc),metadata=self.getreg('rdx'),rows=self.getreg('r8d'),code=self.getreg('r9d'),alignment=self.load(plus(sp,0x20),4),flags=self.load(plus(sp,0x28),4),mode=self.load(plus(sp,0x30),1),blob=self.load(plus(sp,0x38),8)))
                self.setreg('rsp',plus(self.getreg('rsp'),-8));self.store(self.getreg('rsp'),8,nxt);nxt=target
            elif m=='ret':
                nxt=self.load(self.getreg('rsp'),8);self.setreg('rsp',plus(self.getreg('rsp'),8))
                if nxt==0xB00:break
            else:raise AssertionError(('unsupported',hex(self.pc),m,i.op_str))
            self.pc=nxt
        assert self.getreg('rsp')==Address('stack',8)
        assert self.getreg('rax')==self.expected
        assert self.expected==(2007776 if self.alignment==8 else 2041920)
        assert all(w['domain']not in ('graph','headers')for w in self.writes)
        expected_calls=[(name,offset,mode)for layer in (1,2,3)for name,mode in [('i'+str(layer),0),('r'+str(layer),1)]for offset in (0,448)]
        assert len(self.calls)==12
        for call,(name,offset,mode)in zip(self.calls,expected_calls,strict=True):
            assert call['metadata']==Address(name,offset)and(call['rows'],call['code'],call['alignment'],call['flags'],call['mode'])==(127,2,self.alignment,0,mode)

        predicted=self.seedMemory.copy();offset=0;alignment=self.alignment;align=lambda n:((n+alignment-1)//alignment)*alignment
        for name,width,mode,bound in self.roles:
            rows=182 if name=='out'else 127;stride=((width+2*alignment-1)//(2*alignment))*(2*alignment);data=rows*stride*2;one=data+align(rows)*8+align(1)*4
            for direction in range(1 if name=='out'else 2):
                for ordinal in range(1 if name=='out'else 4):
                    base=direction*448+ordinal*64
                    for field,at in [(0,offset),(8,offset+data),(48,offset+data+align(rows)*8)]:predicted[Address(name,base+field),8]=Address('blob',at)if self.blobPresent else 0
                    offset+=one
                if mode:
                    for ordinal in range(3):predicted[Address(name,direction*448+256+ordinal*64),8]=Address('blob',offset)if self.blobPresent else 0;offset+=align(width)*4
        actual={k:v for k,v in self.memory.items()if k[0].domain!='stack'};assert actual==predicted,('independent complete graph/matrix image',self.alignment,self.blobPresent,self.scratchPresent);assert offset==self.expected
        for call in self.calls:assert call['blob']==predicted[plus(call['metadata'],0),8]
        assert len(self.writes)==(201 if self.scratchPresent else 165)
        return dict(alignment=self.alignment,blobPresent=self.blobPresent,scratchPresent=self.scratchPresent,bytes=self.expected,steps=self.steps,writes=self.writes,calls=self.calls)
cases=[]
for alignment in (8,16):
 for blob in (False,True):
  for scratch in (False,True):cases.append(GraphBinder(alignment,blob,scratch).run())
def encode(o):
 if isinstance(o,Address):return dict(domain=o.domain,offset=o.offset)
 raise TypeError(type(o))
report=dict(status='PASS conditional complete selected graph matrix blob-binding address flow',condition='Declared initialized headers/matrix objects are live and disjoint; allocator/caller origins not proven',knownMetadataBytes=5100,readonlyHeaderKnownBytes=128,readonlyGraphKnownBytes=12,conditionalRoleOrderProven=True,cases=cases,vendorExecuted=False,wholeLoaderAliasProofComplete=False,runtimePredicateApproved=False)
args.output.write_text(json.dumps(report,indent=2,default=encode)+'\n');print('PASS',len(cases),'declared graph cases')
