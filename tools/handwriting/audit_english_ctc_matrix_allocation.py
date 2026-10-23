"""Conditional matrix-metadata allocation/sizing walk; no SDK execution.

Continues the original constructor objects after193A40. Stops before allocating
matrix scratch storage. Default ROOT malloc, fresh successful disjoint objects
and the earlier service-registration/caller conditions remain assumptions.
"""
from pathlib import Path
import argparse, hashlib, json, runpy, struct, sys
from capstone import CS_OP_REG

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk', type=Path, required=True)
p.add_argument('--models', type=Path, required=True)
p.add_argument('--output', type=Path, required=True)
a = p.parse_args()
tool = Path(__file__).with_name('audit_english_ctc_workspace_dispatch.py')
old = sys.argv
sys.argv = [str(tool), '--sdk', str(a.sdk), '--models', str(a.models),
            '--output', str(a.output.with_name(a.output.stem+'-workspace.json'))]
try: up = runpy.run_path(str(tool), run_name='__main__')
finally: sys.argv = old
Base, Address, plus, signed, read = (up[k] for k in ('Workspace', 'Address', 'plus', 'signed', 'read'))
md = up['u']['md']
instructions = {}
bodies = []
# Ghidra's complete function body has two ranges, not one .pdata interval.
# The omitted13 bytes are padding; both trailing switch tables are data.
for start, size, digest in [
    (0x180194980, 83, '1470cefa25eb140afff44244e87cb428b8a7cad8911d06059b1258106988b879'),
    (0x1801949e0, 2445, 'bdea7437c1079dcc1c8aa1f93fa940f68ef9913bbf320c24b7eddf7ba76dd85b'),
]:
    code = read(start, size)
    assert hashlib.sha256(code).hexdigest() == digest
    ii = list(md.disasm(code, start)); assert sum(i.size for i in ii) == size
    instructions.update({i.address: i for i in ii})
    bodies.append(dict(address=hex(start), bytes=size, instructions=len(ii), sha256=digest))
# The helpers' complete bodies were already independently interpreted by the
# matrix metadata certificate; use their actual instructions, not call summaries.
# Follow the composed modules by their explicit named bindings.
header = up['u']['up']['up']
graph = header['up']
helper = graph['up']
meta = helper['up']
assert meta['Analyzer'].__name__ == 'Analyzer'
instructions.update(meta['instructions'])


class MatrixAllocation(Base):
    def __init__(self, alignment, flags, extra):
        super().__init__(alignment, flags, extra)
        super().run_workspace()
        self.matrix_active = True
        self.allocations = []
        self.matrix_writes = []
        self.matrix_steps = 0
        self.before_matrix = up['u']['image'](self.memory, set(self.bounds))
        # Real callsite, retaining graph/header/state and stack identities.
        assert self.pc == 0x1801946c0 and self.getreg('al') == 1
        for pc, expected in [(0x1801946c0, ('test', 'al, al')),
                             (0x1801946c2, ('je', '0x1801946d9'))]:
            i = up['u']['up']['instructions'][pc]
            assert (i.mnemonic, i.op_str) == expected
        for pc in (0x1801946c4, 0x1801946c7, 0x1801946ca):
            i = up['u']['up']['instructions'][pc]
            assert i.mnemonic == 'mov'
            self.assign(i, i.operands[0], self.value(i, i.operands[1]))
        i = up['u']['up']['instructions'][0x1801946cd]
        assert i.mnemonic == 'call' and self.value(i, i.operands[0]) == 0x180194980
        assert self.getreg('rcx') == Address('environment') and self.getreg('rdx') == Address('graph')
        assert self.getreg('r8d') == alignment
        self.setreg('rsp', plus(self.getreg('rsp'), -8))
        self.store(self.getreg('rsp'), 8, i.address+i.size)
        self.pc = 0x180194980

    def load(self, address, size):
        if not getattr(self, 'matrix_active', False): return super().load(address, size)
        if isinstance(address, int):
            if address == 0x8000: assert size == 8; return 0xABD
            if 0x180195370 <= address and address+size <= 0x1801953a8:
                assert size == 4
                return struct.unpack('<I', read(address, size))[0]
            return super().load(address, size)
        return super().load(address, size)

    def store(self, address, size, value):
        if not getattr(self, 'matrix_active', False): return super().store(address, size, value)
        assert isinstance(address, Address)
        if address.domain != 'stack':
            assert address.domain.startswith('matrix') or (address.domain == 'headers' and
                address.offset in [i*80+offset for i in range(1,5) for offset in (56,64)] and size == 8)
            assert 0 <= address.offset and address.offset+size <= self.bounds[address.domain]
        was = self.initial; self.initial = True
        try: up['u']['up']['Header'].store(self, address, size, value)
        finally: self.initial = was
        if address.domain != 'stack': self.matrix_writes.append((address, size, value))

    def allocate(self, target):
        assert target == 0xABD and self.getreg('rcx') == Address('environment')
        index = len(self.allocations)
        assert index < 7 and self.getreg('r8') == 1
        size = 896 if index < 6 else 64
        assert self.getreg('edx') == size
        name = 'matrix'+str(index); assert name not in self.bounds
        self.bounds[name] = size
        self.allocations.append(dict(domain=name, bytes=size, zeroInitializationInferred=False))
        for r in ('rcx','rdx','r8','r9','r10','r11'): self.setreg(r, None)
        self.setreg('rax', Address(name)); self.zf = self.last_cmp = None

    def invoke(self, target, nxt):
        if target == 0xABD:
            self.allocate(target)
            return nxt
        assert target in (0x1801915e0,0x180192f10,0x180193070,0x180198960)
        self.setreg('rsp',plus(self.getreg('rsp'),-8))
        self.store(self.getreg('rsp'),8,nxt)
        return target

    def run_until(self, stop):
        while self.pc != stop:
            self.matrix_steps += 1; assert self.matrix_steps < 12000
            i = instructions[self.pc]; m = i.mnemonic; o = i.operands; nxt = self.pc+i.size
            if m in ('add','sub','and','xor','imul','inc','dec','test','idiv','shl','call'): self.last_cmp = None
            if m == 'mov': self.assign(i,o[0],self.value(i,o[1]))
            elif m == 'lea': self.assign(i,o[0],self.address(i,o[1]))
            elif m in ('movzx','movsxd'):
                v=self.value(i,o[1]); self.assign(i,o[0],signed(v,o[1].size*8) if m=='movsxd' else v)
            elif m in ('push','pop'):
                if m=='push':
                    v=self.value(i,o[0]); self.setreg('rsp',plus(self.getreg('rsp'),-8)); self.store(self.getreg('rsp'),8,v)
                else:
                    v=self.load(self.getreg('rsp'),8); self.assign(i,o[0],v); self.setreg('rsp',plus(self.getreg('rsp'),8))
            elif m in ('add','sub','and','xor','imul','shl'):
                if m=='xor' and o[0].type==o[1].type==CS_OP_REG and o[0].reg==o[1].reg: v=0
                else:
                    x=self.value(i,o[1]) if len(o)==3 else self.value(i,o[0]); y=self.value(i,o[2]) if len(o)==3 else self.value(i,o[1])
                    v=plus(x,y) if m=='add' else plus(x,-y) if m=='sub' else x&y if m=='and' else x*y if m=='imul' else x<<y if m=='shl' else x^y
                self.assign(i,o[0],v); self.zf=None if m=='imul' or isinstance(v,Address) else self.value(i,o[0])==0
            elif m in ('inc','dec'):
                self.assign(i,o[0],self.value(i,o[0])+(1 if m=='inc' else -1)); self.zf=self.value(i,o[0])==0
            elif m=='cmp':
                x,y=self.value(i,o[0]),self.value(i,o[1]); assert isinstance(x,int) and isinstance(y,int)
                self.last_cmp=x,y,o[0].size*8; self.zf=x==y
            elif m=='test':
                x,y=self.value(i,o[0]),self.value(i,o[1])
                if isinstance(x,Address): assert x==y; self.zf=False
                else: assert isinstance(x,int) and isinstance(y,int); self.zf=(x&y)==0
            elif m=='cdqe': self.setreg('rax',signed(self.getreg('eax'),32))
            elif m=='cdq': self.setreg('edx',0xffffffff if signed(self.getreg('eax'),32)<0 else 0)
            elif m=='idiv':
                x=signed((self.getreg('edx')<<32)|self.getreg('eax'),64); y=signed(self.value(i,o[0]),32); assert y
                q=abs(x)//abs(y); q=-q if (x<0)!=(y<0) else q; assert -(1<<31)<=q<1<<31
                self.setreg('eax',q); self.setreg('edx',x-q*y); self.zf=None
            elif m=='cmove':
                assert self.zf is not None
                if self.zf: self.assign(i,o[0],self.value(i,o[1]))
            elif m in ('jle','jl','ja'):
                assert self.last_cmp is not None; x,y,bits=self.last_cmp
                take=signed(x,bits)<=signed(y,bits) if m=='jle' else signed(x,bits)<signed(y,bits) if m=='jl' else (x&((1<<bits)-1))>(y&((1<<bits)-1))
                if take: nxt=self.value(i,o[0])
            elif m in ('je','jne','jmp'):
                assert m=='jmp' or self.zf is not None
                if m=='jmp' or self.zf==(m=='je'): nxt=self.value(i,o[0])
            elif m=='call':
                nxt=self.invoke(self.value(i,o[0]),nxt)
            elif m=='ret':
                nxt=self.load(self.getreg('rsp'),8); self.setreg('rsp',plus(self.getreg('rsp'),8))
            elif m=='nop': pass
            else: raise AssertionError(('unsupported matrix sizing',hex(self.pc),m,i.op_str))
            self.pc=nxt

    def run_matrix(self):
        self.run_until(0x180194b7d)
        assert len(self.allocations)==7
        expected={}; total=0
        for index,(width,mode) in enumerate([(16,False),(128,True),(256,False),(128,True),(256,False),(128,True)]):
            h=meta['Analyzer'](width,self.alignment,mode,False); h.run(); total+=h.expected
            for (address,n),value in h.memory.items():
                if address.domain=='state': expected[Address('matrix'+str(index),address.offset),n]=value
        stride=((256+2*self.alignment-1)//(2*self.alignment))*(2*self.alignment)
        for offset in range(0,56,8): expected[Address('matrix6',offset),8]=0
        expected[Address('matrix6',56),4]=stride
        total+=stride*2+self.alignment*4
        actual={k:v for k,v in self.memory.items() if k[0].domain.startswith('matrix')}
        assert actual==expected, ('metadata difference', [(str(k),v,expected.get(k)) for k,v in actual.items() if expected.get(k)!=v][:20], len(actual), len(expected))
        assert self.getreg('rbp')==total, ('scratch size', self.getreg('rbp'), total)
        assert sum(n for _,n in expected)==5100
        original=set(self.bounds)-{x['domain'] for x in self.allocations}
        before=self.before_matrix.copy()
        for layer in range(1,5):
            for offset,index in [(56,2*(layer-1)),(64,2*(layer-1)+1)]:
                if layer==4 and offset==64: continue
                for j in range(8): before.pop((Address('headers',80*layer+offset+j),1),None)
                before[Address('headers',80*layer+offset),8]=Address('matrix'+str(index))
        assert up['u']['image'](self.memory,original)==before
        return dict(alignment=self.alignment,flagsPresent=self.flagsPresent,extraRow=self.extra,
            allocations=self.allocations,definedMetadataBytes=5100,undefinedPaddingBytes=340,
            matrixScratchBytes=total,steps=self.matrix_steps,stopPC=hex(self.pc),originalObjectsPreserved=True)


cases=[MatrixAllocation(alignment,flags,extra).run_matrix() for alignment in (8,16) for flags in (False,True) for extra in (False,True)]
report=dict(status='PASS conditional194980 metadata allocation and sizing',bodies=bodies,cases=cases,
    condition='Inherited constructor conditions and fresh disjoint successful default ROOT malloc allocations',
    subsequentMatrixScratchPassProven=False,actualService49RegistrationProven=False,
    actualCtcEnvironmentOriginProven=False,wholeConstructorProven=False,runtimePredicateApproved=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS8 conditional matrix metadata allocation/sizing walks; full constructor remains unresolved')
