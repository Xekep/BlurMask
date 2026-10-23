"""Symbolic quantized-matrix blob binding for declared helper inputs only.

Own Python analysis of pinned bytes, never SDK execution. Preserves shared
scratch views; complete caller/allocator/header-alias closure remains open.
"""
from pathlib import Path
import argparse,runpy,sys,json,hashlib,struct
from capstone import Cs,CS_ARCH_X86,CS_MODE_64,CS_OP_REG,CS_OP_MEM,CS_OP_IMM
p=argparse.ArgumentParser();p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);args=p.parse_args()
repo=Path(__file__).resolve().parents[2]
old=sys.argv;sys.argv=[str(repo/'tools/handwriting/audit_english_ctc_matrix_metadata_dataflow.py'),'--sdk',str(args.sdk),'--models',str(args.models),'--output',str(args.output.with_name(args.output.stem+'-initializers.json'))]
try:up=runpy.run_path(sys.argv[0],run_name='__main__')
finally:sys.argv=old
Base=up['Analyzer'];Address=up['Address'];plus=up['plus'];signed=up['signed'];aliases=up['aliases'];read=up['read']
code=read(0x180193630,761);assert hashlib.sha256(code).hexdigest()=='28569a9b8c3bf7ddc33c55bd301a8869ecb822d19c733428fd951917bf89dd44'
md=Cs(CS_ARCH_X86,CS_MODE_64);md.detail=True;decoded=list(md.disasm(code,0x180193630));assert len(decoded)==222 and sum(i.size for i in decoded)==761;instructions={i.address:i for i in decoded}
class Binder(Base):
    def __init__(self,width,alignment,mode,blobPresent,scratchPresent):
        seed=Base(width,alignment,mode,scratchPresent);seed.run()
        self.width=width;self.alignment=alignment;self.mode=mode;self.blobPresent=blobPresent;self.nonnull=scratchPresent;self.scratchBytes=seed.expected
        self.memory={k:v for k,v in seed.memory.items()if k[0].domain=='state' and k[0].offset<448}
        self.seedMemory=self.memory.copy();self.registers={family:None for family,_,_ in aliases.values()};self.writes=[];self.reads=[];self.zf=None;self.steps=0;self.pc=0x180193630
        align=lambda n:((n+alignment-1)//alignment)*alignment
        stride=((width+2*alignment-1)//(2*alignment))*(2*alignment)
        self.matrixDataBytes=127*stride*2;self.matrixBytes=self.matrixDataBytes+align(127)*8+align(1)*4;self.vectorBytes=align(width)*4;self.expected=4*self.matrixBytes+int(mode)*3*self.vectorBytes
        self.setreg('rsp',Address('stack'));self.setreg('rcx',Address('environment'));self.setreg('rdx',Address('state'));self.setreg('r8d',127);self.setreg('r9d',2)
        self.store(Address('stack'),8,0xB00);self.store(Address('stack',0x28),4,alignment);self.store(Address('stack',0x30),4,0);self.store(Address('stack',0x38),1,int(mode));self.store(Address('stack',0x40),8,Address('blob')if blobPresent else 0)
    def store(self,address,size,value):
        if address.domain=='state':assert 0<=address.offset and address.offset+size<=448
        if isinstance(value,Address):
            if value.domain=='blob':assert self.blobPresent and 0<=value.offset<self.expected
            if value.domain=='workspace':assert self.nonnull and 0<=value.offset<self.scratchBytes
        return super().store(address,size,value)
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
        assert all(w['domain']=='state' for w in self.writes)
        assert all(r[1]=='state' for r in self.reads)
        predicted=self.seedMemory.copy()
        for ordinal in range(4):
            base=ordinal*64;offset=ordinal*self.matrixBytes
            for field,at in [(0,offset),(8,offset+self.matrixDataBytes),(48,offset+self.matrixDataBytes+((127+self.alignment-1)//self.alignment)*self.alignment*8)]:
                predicted[Address('state',base+field),8]=Address('blob',at)if self.blobPresent else 0
        if self.mode:
            for ordinal in range(3):predicted[Address('state',256+ordinal*64),8]=Address('blob',4*self.matrixBytes+ordinal*self.vectorBytes)if self.blobPresent else 0
        actual={k:v for k,v in self.memory.items()if k[0].domain=='state'}
        assert actual==predicted,('independent complete defined-field projection',self.width,self.alignment,self.mode,self.blobPresent,self.nonnull)
        assert len(self.writes)==12+3*int(self.mode)+3*int(self.nonnull)
        return dict(width=self.width,alignment=self.alignment,mode=self.mode,blobPresent=self.blobPresent,scratchPresent=self.nonnull,blobBytes=self.expected,steps=self.steps,metadataStores=len(self.writes),writes=self.writes)
cases=[]
for width in (16,128,256):
 for alignment in (8,16):
  for mode in (False,True):
   for blob in (False,True):
    for scratch in (False,True):cases.append(Binder(width,alignment,mode,blob,scratch).run())
def encode(o):
 if isinstance(o,Address):return dict(domain=o.domain,offset=o.offset)
 raise TypeError(type(o))
# Arithmetic cross-check only: the selected serialized alignment is8. This
# does not prove native role ordering, caller arguments or header preservation.
resource=(args.models/'en-US/en_US-ak-cur.res').read_bytes()
prefix=struct.unpack_from('<13I',resource,1774299+104)
assert prefix==(0,0,0,0,0,1,1,1,3,2,0,8,2007776)
payloadAt=1774299+156
padding=(-payloadAt)&31
assert resource[payloadAt:payloadAt+padding]==bytes([255])*padding
def selected(width,mode):
 return next(c['blobBytes']for c in cases if c['width']==width and c['alignment']==8 and c['mode']==mode and c['blobPresent']and not c['scratchPresent'])
incoming=[2*selected(width,False)for width in (16,256,256)]
recurrent=[2*selected(128,True)]*3
output=182*256*2+184*8+32
assert sum(incoming)+sum(recurrent)+output==prefix[-1]
footprint=dict(serializedAlignment=8,payloadAt=payloadAt+padding,payloadBytes=prefix[-1],incomingLayerBytes=incoming,recurrentLayerBytes=recurrent,conditionalOutputBytes=output,roleOrderingProven=False,callerArgumentsProven=False)
report=dict(status='PASS conditional quantized matrix blob-binding address dataflow',body=dict(address='0x180193630',bytes=761,instructions=222,sha256=hashlib.sha256(code).hexdigest()),definedMetadataBytes=420,unwrittenPaddingBytes=28,condition='Declared helper rows127/width16,128,256/code2/flags0/alignment8,16; initialized live disjoint metadata448/blob/computed scratch; no whole-caller authority',cases=cases,conditionalSerializedFootprint=footprint,vendorExecuted=False,runtimePredicateApproved=False,wholeLoaderAliasProofComplete=False)
args.output.write_text(json.dumps(report,indent=2,default=encode)+'\n');print('PASS',len(cases),'declared cases; no blob or scratch dereference/write')
