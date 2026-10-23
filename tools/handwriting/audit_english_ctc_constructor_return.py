"""Conditional selected1941B0 constructor through its real caller return.

Keeps original objects, call frames and distinct entry-register markers through
the composed header/state/matrix passes. Only the declared selected packet,
optional count0, alignment8/16 and successful fresh default allocations apply.
No SDK instruction executes. Actual packet/environment origin, other branches,
failure cleanup and subsequent weight import remain unproved.
"""
from pathlib import Path
import argparse, hashlib, json, runpy, sys

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True)
a=p.parse_args();tool=Path(__file__).with_name('audit_english_ctc_matrix_workspace.py')
old=sys.argv;sys.argv=[str(tool),'--sdk',str(a.sdk),'--models',str(a.models),'--output',str(a.output.with_name(a.output.stem+'-matrix.json'))]
try:up=runpy.run_path(str(tool),run_name='__main__')
finally:sys.argv=old
Base=up['MatrixWorkspace'];Address=up['Address'];plus=up['plus']
instructions=up['up']['up']['u']['up']['instructions']
registers=('rbx','rbp','rsi','rdi','r12','r13','r14','r15')


class SelectedConstructor(Base):
    def __init__(self,alignment,flags,extra,marker_set=0):
        assert marker_set in (0,1)
        self.marker_set=marker_set
        super().__init__(alignment,flags,extra)
        self.matrix_result=super().run_scratch()

    def setreg(self,name,value):
        # HeaderOrigin's first register initialization happens before executing
        # the constructor prologue. Supply distinct full64-bit caller values
        # then, not after their save sites. No header/stack state is reseeded.
        if not hasattr(self,'entry_markers'):
            assert name=='rsp' and value==Address('stack')
            self.entry_markers={r:((0xA112233445566700 if self.marker_set==0 else 0x17FFEEDDCCBBAA00)+i*17)for i,r in enumerate(registers)}
            for r,v in self.entry_markers.items():super().setreg(r,v)
        super().setreg(name,value)

    def finish(self):
        assert self.pc==0x1801946d2 and self.getreg('al')==1
        steps=0
        while self.pc!=0xB00:
            steps+=1;assert steps<32
            i=instructions[self.pc];m=i.mnemonic;o=i.operands;nxt=self.pc+i.size
            if m=='test':
                x,y=self.value(i,o[0]),self.value(i,o[1]);self.zf=(x&y)==0
            elif m=='setne':
                assert self.zf is not None;self.assign(i,o[0],int(not self.zf))
            elif m=='jmp':nxt=self.value(i,o[0])
            elif m=='mov':self.assign(i,o[0],self.value(i,o[1]))
            elif m=='add':self.assign(i,o[0],plus(self.value(i,o[0]),self.value(i,o[1])))
            elif m=='pop':
                self.assign(i,o[0],self.load(self.getreg('rsp'),8));self.setreg('rsp',plus(self.getreg('rsp'),8))
            elif m=='ret':
                nxt=self.load(self.getreg('rsp'),8);self.setreg('rsp',plus(self.getreg('rsp'),8))
            else:raise AssertionError(('unsupported constructor epilogue',hex(self.pc),m,i.op_str))
            self.pc=nxt
        assert self.getreg('al')==1 and self.getreg('rsp')==Address('stack',8)
        assert all(self.getreg(r)==v for r,v in self.entry_markers.items())
        return dict(alignment=self.alignment,flagsPresent=self.flagsPresent,extraRow=self.extra,
            markerSet=self.marker_set,returnedTrue=True,callerStackRestored=True,distinctEntryRegistersRestored=8,
            finalPC=hex(self.pc),epilogueSteps=steps,stateWorkspaceBytes=self.total,matrixScratchBytes=self.scratch_bytes)


cases=[SelectedConstructor(alignment,flags,extra,markers).finish()for markers in(0,1)for alignment in(8,16)for flags in(False,True)for extra in(False,True)]
report=dict(status='PASS conditional selected successful1941B0 constructor return',cases=cases,
    condition='Same declared selected packet and optional-count0 plus inherited fresh-disjoint allocation, default-table and service73-installed contracts',
    selectedSuccessfulConstructorPathProven=True,wholeConstructorProven=False,
    actualPacketOriginProven=False,actualCtcEnvironmentOriginProven=False,
    failureCleanupProven=False,matrixImportProven=False,runtimePredicateApproved=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS16 conditional selected constructor returns with distinct saved registers; caller origin/runtime readiness unresolved')
