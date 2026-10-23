"""Conditional first constructor phase: declared fresh allocation; no SDK execution."""
from pathlib import Path
import argparse,runpy,sys,json,hashlib
from capstone import Cs,CS_ARCH_X86,CS_MODE_64,CS_OP_REG
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);args=p.parse_args()
repo=Path(__file__).resolve().parents[2];old=sys.argv;sys.argv=[str(repo/'tools/handwriting/audit_english_ctc_graph_blob_dataflow.py'),'--sdk',str(args.sdk),'--models',str(args.models),'--output',str(args.output.with_name(args.output.stem+'-upstream.json'))]
try:up=runpy.run_path(sys.argv[0],run_name='__main__')
finally:sys.argv=old
Meta=up['Meta'];Address=up['Address'];plus=up['plus'];signed=up['signed'];aliases=up['aliases'];read=up['read']
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True
code=read(0x1801941b0,1352);assert hashlib.sha256(code).hexdigest()=='c9a84b82b5e81426fbce1cce534a4bdacf7a30e243b89a0fb467f77106859bd7'
instructions={i.address:i for i in md.disasm(code,0x1801941b0)};assert len(instructions)==381 and sum(i.size for i in instructions.values())==1352
class HeaderOrigin(Meta):
    address=up['GraphBinder'].address
    def __init__(self,alignment,flagsPresent):
        self.registers={family:None for family,_,_ in aliases.values()};self.memory={};self.writes=[];self.reads=[];self.zf=None;self.steps=0;self.pc=0x1801941b0;self.last_cmp=None;self.initial=True;self.calls=[]
        self.alignment=alignment;self.flagsPresent=flagsPresent
        self.bounds={'graph':88,'headers':400,'packet':96,'kinds':20,'widths':20,'flags':20,'codes':16,'environment':16}
        self.store(Address('packet',4),4,5)
        for off,name in [(8,'kinds'),(16,'flags'),(24,'widths'),(56,'codes')]:self.store(Address('packet',off),8,0 if name=='flags'and not flagsPresent else Address(name))
        for i,(kind,width)in enumerate(zip([1,8,8,8,1],[13,127,127,127,182])):
            self.store(Address('kinds',4*i),4,kind);self.store(Address('widths',4*i),4,width);self.store(Address('flags',4*i),4,0)
            if i<4:self.store(Address('codes',4*i),4,2)
        self.store(Address('environment',8),8,0x7001)
        self.seed=self.memory.copy();self.initial=False;self.writes=[]
        self.setreg('rsp',Address('stack'));self.setreg('rcx',Address('environment'));self.setreg('rdx',Address('graph'));self.setreg('r8',Address('packet'));self.setreg('r9',None)
        self.store(Address('stack'),8,0xB00);self.store(Address('stack',56),4,alignment);self.store(Address('stack',48),4,None);self.store(Address('stack',40),4,None)
    def store(self,address,size,value):
        assert isinstance(address,Address)
        if address.domain=='stack':assert -1024<=address.offset and address.offset+size<=128
        else:
            assert address.domain in self.bounds and 0<=address.offset and address.offset+size<=self.bounds[address.domain]
            assert self.initial or address.domain in ('graph','headers')
        for old,n in list(self.memory):
            if old.domain==address.domain and old.offset<address.offset+size and address.offset<old.offset+n:del self.memory[old,n]
        self.memory[address,size]=value
        if not self.initial and address.domain!='stack':self.writes.append(dict(pc=hex(self.pc),domain=address.domain,offset=address.offset,size=size,value=value))
    def load(self,address,size):
        if isinstance(address,int):assert size==8 and address in (0x7000,0x8018);return 0x8000 if address==0x7000 else 0xABC
        return super().load(address,size)
    def run(self):
        while self.pc!=0x1801944ec:
            self.steps+=1;assert self.steps<8192
            i=instructions[self.pc];m=i.mnemonic;o=i.operands;nxt=self.pc+i.size
            if m in ('add','sub','and','xor','imul','test','idiv','inc','dec','shl','call'):self.last_cmp=None
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
            elif m in ('inc','dec','shl'):
                a=self.value(i,o[0]);assert isinstance(a,int);v=a+1 if m=='inc'else a-1 if m=='dec'else a<<self.value(i,o[1]);self.assign(i,o[0],v);self.zf=self.value(i,o[0])==0
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
                assert self.value(i,o[0])==0xABC
                assert self.getreg('rcx')==Address('environment') and self.getreg('edx')==80 and self.getreg('r8')==5
                self.calls.append(dict(pc=hex(self.pc),unitBytes=80,count=5,returned=Address('headers')))
                for name in ('rcx','rdx','r8','r9','r10','r11'):self.setreg(name,None)
                self.setreg('rax',Address('headers'))
            elif m=='ret':
                nxt=self.load(self.getreg('rsp'),8);self.setreg('rsp',plus(self.getreg('rsp'),8))
                if nxt==0xB00:break
            else:raise AssertionError(('unsupported',hex(self.pc),m,i.op_str))
            self.pc=nxt
        assert len(self.calls)==1
        predicted=self.seed.copy()
        for off,size in [(0,4),(8,8),(16,8),(24,8),(32,8),(40,8),(48,4),(56,8),(72,8),(80,8)]:predicted[Address('graph',off),size]=0
        predicted[Address('graph'),4]=4;predicted[Address('graph',8),8]=Address('headers')
        align=lambda n:((n+self.alignment-1)//self.alignment)*self.alignment
        for i,(kind,width)in enumerate(zip([1,8,8,8,1],[13,127,127,127,182])):
            output=width*2 if kind==8 else width
            for field,v in [(0,kind),(4,width),(8,align(width+1)),(12,output),(16,align(output+1)),(36,0)]:predicted[Address('headers',80*i+field),4]=v
            if i:predicted[Address('headers',80*i+40),4]=2
        actual={k:v for k,v in self.memory.items()if k[0].domain!='stack'}
        assert actual==predicted,('independent header image',self.alignment,self.flagsPresent)
        return dict(alignment=self.alignment,flagsPresent=self.flagsPresent,steps=self.steps,calls=self.calls,writes=self.writes,stopPC=hex(self.pc))
cases=[HeaderOrigin(a,f).run()for a in (8,16)for f in (False,True)]
def encode(o):
 if isinstance(o,Address):return dict(domain=o.domain,offset=o.offset)
 raise TypeError(type(o))
report=dict(status='PASS conditional first header constructor phase',cases=cases,condition='Exact projected packet lanes; one successful fresh disjoint allocator callback; callback ABI clobbers; no assumed zero-fill',wholeConstructorProven=False,actualAllocatorProven=False,vendorExecuted=False,runtimePredicateApproved=False)
args.output.write_text(json.dumps(report,indent=2,default=encode)+'\n');print('PASS',len(cases),'conditional header constructor cases')
