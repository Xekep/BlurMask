"""Conditional complete194980 metadata/scratch passes; no SDK execution.

Fresh successful disjoint allocations, the pinned installed service49 and
default ROOT allocator remain conditions. Does not prove selected caller origin,
failure cleanup, matrix import, whole constructor or recognition readiness.
"""
from pathlib import Path
import argparse, hashlib, json, runpy, struct, sys

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True)
a=p.parse_args();tool=Path(__file__).with_name('audit_english_ctc_matrix_allocation.py')
old=sys.argv;sys.argv=[str(tool),'--sdk',str(a.sdk),'--models',str(a.models),'--output',str(a.output.with_name(a.output.stem+'-sizing.json'))]
try:up=runpy.run_path(str(tool),run_name='__main__')
finally:sys.argv=old
Base=up['MatrixAllocation'];Address=up['Address'];plus=up['plus'];read=up['read'];meta=up['meta']

# Extend the already pinned104CC0 positive-size arithmetic to the matrix sizes;
# the upstream certificate covered larger state workspaces only. Exhaust all
# residues, including alignments beyond the actual allocator's usual minimum.
alignment_controls=[]
for payload in (4576,5056):
    for alignment in (32,64):
        total=payload+8+alignment
        assert 0<total<2**64
        for residue in range(alignment):
            raw=0x100000+residue;start=raw+8;remainder=start%alignment
            if remainder:start+=alignment-remainder
            assert start==((raw+8+alignment-1)//alignment)*alignment
            assert raw<=start-8 and start+payload<=raw+total
            block=bytearray(total);at=start-8-raw
            block[at:at+8]=raw.to_bytes(8,'little')
            assert at+8<=start-raw and not any(block[start-raw:start-raw+payload])
        alignment_controls.append(dict(payloadBytes=payload,alignmentBytes=alignment,
            allocationBytes=total,pointerResidues=alignment,prefixOutsidePayload=True,zeroPayloadPreserved=True))
assert sum(x['pointerResidues']for x in alignment_controls)==192


class MatrixWorkspace(Base):
    def __init__(self,alignment,flags,extra):
        super().__init__(alignment,flags,extra)
        self.matrix_nonvolatile={r:self.getreg(r)for r in ('rbx','rbp','rsi','rdi','r12','r13','r14','r15')}
        self.matrix_return_sp=plus(self.getreg('rsp'),8)
        self.sizing=super().run_matrix()
        self.scratch_active=True
        self.scratch_bytes=self.sizing['matrixScratchBytes']
        self.scratch=None;self.scratch_calls=[]
        self.matrix_steps=0

    def load(self,address,size):
        if not getattr(self,'scratch_active',False):return super().load(address,size)
        if isinstance(address,int) and 0x1801953a8<=address and address+size<=0x1801953e0:
            assert size==4;return struct.unpack('<I',read(address,size))[0]
        return super().load(address,size)

    def store(self,address,size,value):
        if getattr(self,'scratch_active',False) and address==Address('graph',56):
            assert size==8 and value==Address('matrix-scratch')
            was=self.initial;self.initial=True
            try:up['up']['u']['up']['Header'].store(self,address,size,value)
            finally:self.initial=was
            return
        # No helper writes scratch payload. Reject every such write, including0.
        if getattr(self,'scratch_active',False):assert address.domain!='matrix-scratch'
        return super().store(address,size,value)

    def invoke(self,target,nxt):
        if not getattr(self,'scratch_active',False):return super().invoke(target,nxt)
        if target in (0x180037f50,0x180104cc0):
            assert self.getreg('rcx')==Address('environment')
            if target==0x180037f50:
                assert self.pc==0x180194bd1 and self.getreg('edx')==73 and not self.scratch_calls
                result=0x180229c00
                self.scratch_calls.append(dict(kind='lookup',service=73,assumedInstalledDescriptor='0x180229ca0'))
            else:
                assert self.pc==0x180194be1 and len(self.scratch_calls)==1
                assert self.getreg('rdx')==self.scratch_bytes and self.getreg('r8')==4*self.alignment
                assert self.scratch is None and 0<self.scratch_bytes<65536
                self.bounds['matrix-scratch']=self.scratch_bytes
                self.scratch=bytearray(self.scratch_bytes)
                result=Address('matrix-scratch')
                self.scratch_calls.append(dict(kind='aligned-zero-allocation',bytes=self.scratch_bytes,alignmentBytes=4*self.alignment))
            for r in ('rax','rcx','rdx','r8','r9','r10','r11'):self.setreg(r,None)
            self.setreg('rax',result);self.zf=self.last_cmp=None
            return nxt
        assert target!=0xABD, 'second pass must not allocate metadata again'
        return super().invoke(target,nxt)

    def run_scratch(self):
        assert self.load(Address('graph',56),8)==0
        self.run_until(0x1801946d2)
        assert self.getreg('al')==1 and self.getreg('rsp')==self.matrix_return_sp
        assert all(self.getreg(r)==v for r,v in self.matrix_nonvolatile.items())
        assert len(self.scratch_calls)==2 and self.scratch==bytearray(self.scratch_bytes)
        expected={};base=0
        for index,(width,mode)in enumerate([(16,False),(128,True),(256,False),(128,True),(256,False),(128,True)]):
            h=meta['Analyzer'](width,self.alignment,mode,True);h.run()
            for(address,n),value in h.memory.items():
                if address.domain!='state':continue
                if isinstance(value,meta['Address']):
                    assert value.domain=='workspace';value=Address('matrix-scratch',base+value.offset)
                expected[Address('matrix'+str(index),address.offset),n]=value
            base+=h.expected
        stride=((256+2*self.alignment-1)//(2*self.alignment))*(2*self.alignment)
        for offset in range(0,56,8):expected[Address('matrix6',offset),8]=Address('matrix-scratch',base)if offset==16 else 0
        expected[Address('matrix6',56),4]=stride
        base+=stride*2+self.alignment*4
        actual={k:v for k,v in self.memory.items()if k[0].domain.startswith('matrix')}
        assert base==self.scratch_bytes and actual==expected
        # Original data must still differ only in seven metadata pointers and
        # the new graph scratch pointer. Scratch is stored separately as bytes.
        original=set(self.bounds)-{x['domain']for x in self.allocations}-{'matrix-scratch'}
        before=self.before_matrix.copy()
        for layer in range(1,5):
            for offset,index in [(56,2*(layer-1)),(64,2*(layer-1)+1)]:
                if layer==4 and offset==64:continue
                for j in range(8):before.pop((Address('headers',80*layer+offset+j),1),None)
                before[Address('headers',80*layer+offset),8]=Address('matrix'+str(index))
        for j in range(8):before.pop((Address('graph',56+j),1),None)
        before[Address('graph',56),8]=Address('matrix-scratch')
        assert up['up']['u']['image'](self.memory,original)==before
        return dict(alignment=self.alignment,flagsPresent=self.flagsPresent,extraRow=self.extra,
            metadataAllocatedBytes=5440,definedMetadataBytes=5100,undefinedPaddingBytes=340,
            scratchBytes=self.scratch_bytes,scratchSha256=hashlib.sha256(self.scratch).hexdigest(),
            sizingSteps=self.sizing['steps'],scratchSteps=self.matrix_steps,returnedTrue=True,
            stopPC=hex(self.pc),nonvolatileRestored=True,originalObjectsPreserved=True,services=self.scratch_calls)


cases=[MatrixWorkspace(alignment,flags,extra).run_scratch()for alignment in(8,16)for flags in(False,True)for extra in(False,True)]
report=dict(status='PASS conditional complete194980 metadata and scratch passes',cases=cases,alignmentControls=alignment_controls,
    actualService49RegistrationProven=False,actualCtcEnvironmentOriginProven=False,
    wholeConstructorProven=False,matrixImportProven=False,runtimePredicateApproved=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS8 conditional194980 metadata/scratch passes; whole constructor and runtime readiness unresolved')
