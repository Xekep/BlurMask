"""Bounded symbolic address flow through four matrix-metadata initializers.

No SDK instruction runs on the CPU. Only declared width16/128/256, code2,
flags0 and alignment8/16 helper inputs are covered. Whole caller argument,
allocation separation, matrix import and retained-header proofs remain open.
"""
from pathlib import Path
from dataclasses import dataclass
import argparse,hashlib,json,struct,importlib.util
from capstone import Cs,CS_ARCH_X86,CS_MODE_64,CS_OP_REG,CS_OP_IMM,CS_OP_MEM

parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--sdk',type=Path,required=True)
parser.add_argument('--models',type=Path,required=True)
parser.add_argument('--output',type=Path,required=True)
options=parser.parse_args()

@dataclass(frozen=True)
class Address:
    domain:str
    offset:int=0

def plus(a,b):
    if isinstance(a,Address) and isinstance(b,int):return Address(a.domain,a.offset+b)
    if isinstance(b,Address) and isinstance(a,int):return Address(b.domain,b.offset+a)
    assert isinstance(a,int) and isinstance(b,int),(a,b)
    return a+b

def signed(v,bits):
    assert isinstance(v,int)
    v&=(1<<bits)-1
    return v-(1<<bits) if v>>(bits-1) else v

def module(name):
    path=Path(__file__).with_name(name)
    spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
    return m

binary=options.sdk/'MyScriptEngine.dll'
layout=module('audit_english_ctc_cpu_layout.py').audit(binary,options.models/'en-US/en_US-ak-cur.res')
raw=binary.read_bytes();pe=struct.unpack_from('<I',raw,60)[0];optional=pe+24;image=struct.unpack_from('<Q',raw,optional+24)[0];table=optional+struct.unpack_from('<H',raw,pe+20)[0]
sections=[struct.unpack_from('<IIII',raw,table+i*40+8)for i in range(struct.unpack_from('<H',raw,pe+6)[0])]
def read(address,size):
    for _,va,length,offset in sections:
        if va<=address-image and address-image+size<=va+length:return raw[offset+address-image-va:offset+address-image-va+size]
    raise AssertionError(address)
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True
instructions={};bodies=[]
for address,length,digest in [(6444094944, 222, '6940ad63c07767ae408dcdaf703e20a1f9af86af37037d8bb3035d29499bd3ab'), (6444101392, 339, '69d80da228dad559a4653e2bcd65fc8a6cef7a16fd44acd3b79458e156bdbcf2'), (6444101744, 461, '73012438c38c46bf059178cf0e198674fc0366f105d980468f22e95ddcdabb07'), (6444124512, 147, '98c48a6b32de5db8ff8a9bc0b29246a854d0000c2ce8f018cecd155666a63c96')]:
    code=read(address,length);assert hashlib.sha256(code).hexdigest()==digest
    ins=list(md.disasm(code,address));assert sum(i.size for i in ins)==length
    instructions.update({i.address:i for i in ins});bodies.append(dict(address=hex(address),bytes=length,instructions=len(ins),sha256=digest))

aliases={}
for family,names in [('rax',['rax','eax','ax','al','ah']),('rbx',['rbx','ebx','bx','bl','bh']),('rcx',['rcx','ecx','cx','cl','ch']),('rdx',['rdx','edx','dx','dl','dh']),('rsi',['rsi','esi','si','sil']),('rdi',['rdi','edi','di','dil']),('rbp',['rbp','ebp','bp','bpl']),('rsp',['rsp','esp','sp','spl'])]+[(f'r{i}',[f'r{i}',f'r{i}d',f'r{i}w',f'r{i}b'])for i in range(8,16)]:
    for index,name in enumerate(names):aliases[name]=(family,64 if index==0 else 32 if index==1 else 16 if index==2 else 8,8 if name in ('ah','bh','ch','dh') else 0)

class Analyzer:
    def __init__(self,width,alignment,mode,nonnull):
        self.registers={family:None for family,_,_ in aliases.values()};self.memory={};self.writes=[];self.reads=[];self.zf=None;self.steps=0;self.pc=0x1801915e0
        self.width=width;self.alignment=alignment;self.mode=mode
        self.nonnull=nonnull;self.expected=2*(alignment*4+((width+2*alignment-1)//(2*alignment)*2*alignment)*2)
        self.setreg('rsp',Address('stack'));self.setreg('rcx',Address('environment'));self.setreg('rdx',Address('state'));self.setreg('r8d',width);self.setreg('r9d',2)
        self.store(Address('stack'),8,0xB00)
        self.store(Address('stack',0x28),4,alignment);self.store(Address('stack',0x30),4,0);self.store(Address('stack',0x38),1,int(mode));self.store(Address('stack',0x40),8,Address('workspace')if nonnull else 0)
    def getreg(self,name):
        family,bits,shift=aliases[name];v=self.registers[family]
        if v is None:return None
        if bits==64:return v
        assert isinstance(v,int),(name,v)
        return (v>>shift)&((1<<bits)-1)
    def setreg(self,name,v):
        family,bits,shift=aliases[name]
        if bits==64:self.registers[family]=v;return
        if v is None:self.registers[family]=None;return
        assert isinstance(v,int),(name,v)
        v&=(1<<bits)-1
        if bits==32:self.registers[family]=v;return
        prior=self.registers[family];assert isinstance(prior,int),(name,prior)
        mask=((1<<bits)-1)<<shift;self.registers[family]=(prior&~mask)|(v<<shift)
    def address(self,i,operand):
        m=operand.mem;v=m.disp
        if m.base:v=plus(v,self.getreg(i.reg_name(m.base)))
        if m.index:
            index=self.getreg(i.reg_name(m.index))
            if isinstance(index,Address):assert m.scale==1;v=plus(v,index)
            else:assert isinstance(index,int);v=plus(v,index*m.scale)
        return v
    def store(self,address,size,value):
        assert isinstance(address,Address),address
        if address.domain=='state':assert 0<=address.offset and address.offset+size<=896
        elif address.domain=='workspace':assert self.nonnull and size==4 and 0<=address.offset and address.offset+size<=self.expected
        else:assert address.domain=='stack' and -512<=address.offset<=128
        for old,n in list(self.memory):
            if old.domain==address.domain and old.offset<address.offset+size and address.offset<old.offset+n:del self.memory[old,n]
        if isinstance(value,Address) and value.domain=='workspace':assert self.nonnull and 0<=value.offset<self.expected
        self.memory[address,size]=value
        if address.domain!='stack':self.writes.append(dict(pc=hex(self.pc),domain=address.domain,offset=address.offset,size=size,value=value))
    def load(self,address,size):
        assert isinstance(address,Address)
        if (address,size)not in self.memory:
            containing=[(a,n,v)for (a,n),v in self.memory.items()if a.domain==address.domain and a.offset<=address.offset and address.offset+size<=a.offset+n and isinstance(v,int)]
            assert len(containing)==1,('unwritten/partial read',hex(self.pc),address,size)
            a,n,v=containing[0];value=(v>>(8*(address.offset-a.offset)))&((1<<(8*size))-1)
            return value

        if address.domain!='stack':self.reads.append((hex(self.pc),address.domain,address.offset,size))
        return self.memory[address,size]
    def value(self,i,o):
        if o.type==CS_OP_REG:return self.getreg(i.reg_name(o.reg))
        if o.type==CS_OP_IMM:return o.imm
        assert o.type==CS_OP_MEM
        return self.load(self.address(i,o),o.size)
    def assign(self,i,o,v):
        if o.type==CS_OP_REG:self.setreg(i.reg_name(o.reg),v)
        else:assert o.type==CS_OP_MEM;self.store(self.address(i,o),o.size,v)
    def run(self):
        while True:
            self.steps+=1;assert self.steps<2000
            i=instructions[self.pc];m=i.mnemonic;o=i.operands;nxt=self.pc+i.size
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
                a=self.value(i,o[0]);b=self.value(i,o[1]);assert isinstance(a,int) and isinstance(b,int);self.zf=a==b
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
            elif m in ('je','jne','jmp'):
                if m=='jmp':nxt=self.value(i,o[0])
                else:
                    assert self.zf is not None,('undefined branch',hex(self.pc))
                    if self.zf==(m=='je'):nxt=self.value(i,o[0])
            elif m=='call':
                target=self.value(i,o[0]);assert target in (0x180192f10,0x180193070,0x180198960)
                self.setreg('rsp',plus(self.getreg('rsp'),-8));self.store(self.getreg('rsp'),8,nxt);nxt=target
            elif m=='ret':
                nxt=self.load(self.getreg('rsp'),8);self.setreg('rsp',plus(self.getreg('rsp'),8))
                if nxt==0xB00:break
            else:raise AssertionError(('unsupported',hex(self.pc),m,i.op_str))
            self.pc=nxt
        assert self.getreg('rsp')==Address('stack',8)
        assert self.getreg('rax')==self.expected
        assert not any(x['domain']=='workspace' for x in self.writes)
        width,alignment,mode=self.width,self.alignment,self.mode
        expected={}
        for substate in (0,448):
            for ordinal in range(7):
                base=substate+64*ordinal
                for offset in range(0,56,8):expected[Address('state',base+offset),8]=0
                stride=((width+2*alignment-1)//(2*alignment))*(2*alignment) if ordinal<4 else ((width+alignment-1)//alignment)*alignment if mode else 0
                expected[Address('state',base+56),4]=stride
                if ordinal<4 and self.nonnull:expected[Address('state',base+16),8]=Address('workspace',0 if substate==0 else self.expected//2)
        actual={key:value for key,value in self.memory.items()if key[0].domain=='state'}
        assert actual==expected,('independent complete defined-field image',width,alignment,mode,self.nonnull)
        expected_stores=4*(7*(7+1))+8*((7+1)+1)+int(mode)*2*(3*(7+1))
        assert expected_stores==(344 if mode else 296) and len(self.writes)==expected_stores
        return dict(scratchBytes=self.expected,steps=self.steps,metadataStores=len(self.writes),writes=self.writes)

cases=[]
for width in (16,128,256):
 for alignment in (8,16):
  for mode in (False,True):
   for nonnull in (False,True):
    instance=Analyzer(width,alignment,mode,nonnull);result=instance.run();result.update(width=width,alignment=alignment,mode=mode,nonnull=nonnull);cases.append(result)
def encode(o):
 if isinstance(o,Address):return dict(domain=o.domain,offset=o.offset)
 raise TypeError(type(o))
report=dict(status='PASS conditional matrix-metadata initializer address flow',bodies=bodies,definedMetadataBytes=840,unwrittenPaddingBytes=56,condition='Declared helper inputs; live disjoint metadata896 and computed scratch objects; caller argument flow and allocation origin are not established',cases=cases,vendorExecuted=False,allocationSeparationProven=False,wholeLoaderAliasProofComplete=False,runtimePredicateApproved=False)
options.output.write_text(json.dumps(report,indent=2,default=encode)+'\n',encoding='utf-8')
print(json.dumps(dict(status=report['status'],cases=len(cases),steps=[x['steps']for x in cases])))
