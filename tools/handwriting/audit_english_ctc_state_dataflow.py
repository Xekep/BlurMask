"""Bounded static affine-address analysis of two pinned state initializers.

No instruction is run on the CPU. Native addresses are symbolic object-relative
values; unsupported dataflow and undefined flag use are rejected. Only the
declared actual width127/flags0 and supported alignments are admitted.
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
metadata=module('audit_english_ctc_state_metadata.py').audit(binary)
layout=module('audit_english_ctc_cpu_layout.py').audit(binary,options.models/'en-US/en_US-ak-cur.res')
raw=binary.read_bytes();pe=struct.unpack_from('<I',raw,60)[0];optional=pe+24;image=struct.unpack_from('<Q',raw,optional+24)[0];table=optional+struct.unpack_from('<H',raw,pe+20)[0]
sections=[struct.unpack_from('<IIII',raw,table+i*40+8)for i in range(struct.unpack_from('<H',raw,pe+6)[0])]
def read(address,size):
    for _,va,length,offset in sections:
        if va<=address-image and address-image+size<=va+length:return raw[offset+address-image-va:offset+address-image-va+size]
    raise AssertionError(address)
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True
instructions={}
for address,length,digest in [(0x180191470,356,'d1898c59dce50b403094fb4ffb1f034a3b2522ba81a7d6d86a8859c2045a1756'),(0x180192ae0,1070,'7bb47efde0a5858d4a3e90a722e7d280982d784916caafa1417789047a9ecac0')]:
    code=read(address,length);assert hashlib.sha256(code).hexdigest()==digest
    ins=list(md.disasm(code,address));assert sum(i.size for i in ins)==length
    instructions.update({i.address:i for i in ins})

aliases={}
for family,names in [('rax',['rax','eax','ax','al','ah']),('rbx',['rbx','ebx','bx','bl','bh']),('rcx',['rcx','ecx','cx','cl','ch']),('rdx',['rdx','edx','dx','dl','dh']),('rsi',['rsi','esi','si','sil']),('rdi',['rdi','edi','di','dil']),('rbp',['rbp','ebp','bp','bpl']),('rsp',['rsp','esp','sp','spl'])]+[(f'r{i}',[f'r{i}',f'r{i}d',f'r{i}w',f'r{i}b'])for i in range(8,16)]:
    for index,name in enumerate(names):aliases[name]=(family,64 if index==0 else 32 if index==1 else 16 if index==2 else 8,8 if name in ('ah','bh','ch','dh') else 0)

class Analyzer:
    def __init__(self,alignment,extra,nonnull):
        self.registers={family:None for family,_,_ in aliases.values()};self.memory={};self.writes=[];self.reads=[];self.zf=None;self.steps=0;self.pc=0x180191470
        self.nonnull=nonnull;self.expected=next(x['kind8WorkspaceBytes']for x in layout['conditionalKind8Workspace']if x['alignmentFloatUnits']==alignment and x['extraRow']==extra)
        self.setreg('rsp',Address('stack'));self.setreg('rdx',Address('state'));self.setreg('r8d',127);self.setreg('r9d',alignment)
        self.store(Address('stack'),8,0xB00)
        self.store(Address('stack',0x28),4,0);self.store(Address('stack',0x30),1,int(extra));self.store(Address('stack',0x38),8,Address('workspace')if nonnull else 0)
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
            index=self.getreg(i.reg_name(m.index));assert isinstance(index,int)
            v=plus(v,index*m.scale)
        return v
    def store(self,address,size,value):
        assert isinstance(address,Address),address
        if address.domain=='state':assert 0<=address.offset and address.offset+size<=416
        elif address.domain=='workspace':assert self.nonnull and size==4 and 0<=address.offset and address.offset+size<=self.expected
        else:assert address.domain=='stack' and -512<=address.offset<=128
        for old,n in list(self.memory):
            if old.domain==address.domain and old.offset<address.offset+size and address.offset<old.offset+n:del self.memory[old,n]
        self.memory[address,size]=value
        if address.domain!='stack':self.writes.append(dict(pc=hex(self.pc),domain=address.domain,offset=address.offset,size=size,value=value))
    def load(self,address,size):
        assert isinstance(address,Address)
        assert (address,size)in self.memory,('unwritten/partial read',hex(self.pc),address,size)
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
                assert self.value(i,o[0])==0x180192ae0
                self.setreg('rsp',plus(self.getreg('rsp'),-8));self.store(self.getreg('rsp'),8,nxt);nxt=0x180192ae0
            elif m=='ret':
                nxt=self.load(self.getreg('rsp'),8);self.setreg('rsp',plus(self.getreg('rsp'),8))
                if nxt==0xB00:break
            else:raise AssertionError(('unsupported',hex(self.pc),m,i.op_str))
            self.pc=nxt
        assert self.getreg('rsp')==Address('stack',8)
        assert self.getreg('rax')*4==self.expected
        payload=[x for x in self.writes if x['domain']=='workspace']
        assert len(payload)==(46 if self.nonnull else 0)
        if self.nonnull:assert len({x['pc']for x in payload})==24
        # Every symbolic store retained an object domain: no integer-as-pointer
        # or unknown pointer store could be classified by a guessed base.
        return dict(alignment=self.getreg('r9'),nonnull=self.nonnull,workspaceBytes=self.expected,steps=self.steps,
                    metadataStoreOccurrences=sum(x['domain']=='state'for x in self.writes),payloadStoreOccurrences=len(payload),
                    uniquePayloadStoreSites=len({x['pc']for x in payload}),writes=self.writes)

cases=[]
for alignment in (8,16):
    for extra in (False,True):
        for nonnull in (False,True):
            result=Analyzer(alignment,extra,nonnull).run();result['alignment']=alignment;result['extraRow']=extra;cases.append(result)
report=dict(status='PASS bounded static affine-address dataflow',cases=cases,vendorExecuted=False,
            condition='Entry metadata object416 bytes and computed workspace are live disjoint objects; this analyzes only191470/192AE0.',
            allocationSeparationProven=False,wholeLoaderAliasProofComplete=False,runtimePredicateApproved=False)
algebra=module('audit_english_ctc_payload_indices.py').audit(binary,options.models/'en-US/en_US-ak-cur.res')
for case in cases:
    if not case['nonnull']:continue
    expected=next(c for c in algebra['certificates']if c['alignmentFloatUnits']==case['alignment']and c['extraRow']==case['extraRow'])
    actual=[(s['pc'],s['offset'],s['size'],s['value'])for s in case['writes']if s['domain']=='workspace']
    predicted=[(s['site'],s['byteOffset'],4,int(s['bits'],16))for s in expected['orderedStores']]
    assert actual==predicted,('native order/address/value mapping',case['alignment'],case['extraRow'])
def encode(o):
    if isinstance(o,Address):return dict(domain=o.domain,offset=o.offset)
    raise TypeError(type(o))
options.output.write_text(json.dumps(report,indent=2,default=encode)+'\n',encoding='utf-8')
print(json.dumps(dict(status=report['status'],cases=len(cases),steps=[c['steps']for c in cases],payloadStores=[c['payloadStoreOccurrences']for c in cases],vendorExecuted=False)))
