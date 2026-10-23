"""Conditional constructor-to-initializer sizing pass; static instructions only.

Composes the independently checked header/state allocation and scalar kind8
initializers. Stops before service49 workspace allocation. The actual external
caller, second (workspace-writing) pass and full constructor remain unproved.
"""
from pathlib import Path
import argparse, hashlib, json, runpy, struct, sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_64, CS_OP_REG

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('--sdk', type=Path, required=True)
p.add_argument('--models', type=Path, required=True)
p.add_argument('--output', type=Path, required=True)
args = p.parse_args()
repo = Path(__file__).resolve().parents[2]

def compose(name, suffix, models=True):
    old = sys.argv
    sys.argv = [str(repo/'tools/handwriting'/name), '--sdk', str(args.sdk), '--output',
                str(args.output.with_name(args.output.stem+suffix+'.json'))]
    if models:
        sys.argv += ['--models', str(args.models)]
    try:
        return runpy.run_path(sys.argv[0], run_name='__main__')
    finally:
        sys.argv = old

up = compose('audit_english_ctc_state_allocation.py', '-allocation')
callbacks = compose('audit_english_ctc_state_callbacks.py', '-callbacks', False)
scalar = compose('audit_english_ctc_state_dataflow.py', '-scalar')
Base = up['StateAllocation']
Address, plus, signed, read = (up[k] for k in ('Address', 'plus', 'signed', 'read'))
OtherAddress = scalar['Address']
code = read(0x180193a40, 1561)
assert hashlib.sha256(code).hexdigest() == 'f2318c6a4a049a17da1fa62c80dd2b14c198caf210befff953ab4b11ef6b5003'
md = Cs(CS_ARCH_X86, CS_MODE_64)
md.detail = True
instructions = {i.address: i for i in md.disasm(code, 0x180193a40)}
assert len(instructions) == 396 and sum(i.size for i in instructions.values()) == len(code)

def image(memory, domains):
    out = {}
    for (a, n), v in memory.items():
        if a.domain not in domains:
            continue
        if isinstance(v, Address) or v is None:
            out[a, n] = v
        else:
            for j, b in enumerate((v & ((1 << (8*n))-1)).to_bytes(n, 'little')):
                out[Address(a.domain, a.offset+j), 1] = b
    return out

class Dispatch(Base):
    def __init__(self, alignment, flags_present, extra):
        super().__init__(alignment, flags_present)
        super().run_tail()
        self.extra = extra
        # Constructor argument8 was not needed by the earlier bounded phases.
        # Bind that previously unknown external byte explicitly, retaining ALL
        # graph/state/register/stack identities from those phases. Then walk
        # the actual four argument moves and CALL, rather than reseeding a
        # plausible independent dispatcher invocation.
        assert not any(a.domain == 'stack' and a.offset < 65 and 64 < a.offset+n
                       for a, n in self.memory)
        self.store(Address('stack', 64), 1, int(extra))
        for pc in (0x1801946a9, 0x1801946b2, 0x1801946b5, 0x1801946b8):
            assert self.pc == pc
            i = up['instructions'][pc]
            assert i.mnemonic in ('mov', 'movzx')
            self.assign(i, i.operands[0], self.value(i, i.operands[1]))
            self.pc += i.size
        i = up['instructions'][self.pc]
        assert self.pc == 0x1801946bb and i.mnemonic == 'call' and self.value(i, i.operands[0]) == 0x180193a40
        assert self.getreg('rcx') == Address('environment') and self.getreg('rdx') == Address('graph')
        assert self.getreg('r8d') == alignment and self.getreg('r9d') == int(extra)
        self.setreg('rsp', plus(self.getreg('rsp'), -8))
        self.store(self.getreg('rsp'), 8, self.pc+i.size)
        self.readonly = image(self.memory, set(self.bounds))
        self.readonly_domains = set(self.bounds)-{'state0', 'state1', 'state2', 'state3'}
        self.pc = 0x180193a40
        self.steps = 0
        self.zf = self.last_cmp = None
        self.initializer_calls = []
        self.dispatch_write_start = len(self.writes)

    def load(self, address, size):
        if not hasattr(self, 'initializer_calls'):
            return super().load(address, size)
        if isinstance(address, int):
            assert size == 4 and 0x18019405c <= address and address+size <= 0x180194084
            return struct.unpack('<I', read(address, size))[0]
        assert isinstance(address, Address)
        if address.domain != 'stack':
            assert address.domain in self.bounds and 0 <= address.offset and address.offset+size <= self.bounds[address.domain]
        return super().load(address, size)

    def store(self, address, size, value):
        if not hasattr(self, 'initializer_calls'):
            return super().store(address, size, value)
        assert isinstance(address, Address)
        if address.domain != 'stack':
            assert address.domain in ('state0', 'state1', 'state2', 'state3'), ('readonly dispatch store', address)
            assert 0 <= address.offset and address.offset+size <= self.bounds[address.domain]
        # HeaderOrigin keeps overlap/bounds semantics; this pass adds only the
        # four newly allocated state domains, never arbitrary pointer writes.
        old = self.initial
        self.initial = True
        try:
            up['Header'].store(self, address, size, value)
        finally:
            self.initial = old
        if address.domain != 'stack':
            self.writes.append(dict(pc=hex(self.pc), domain=address.domain,
                                    offset=address.offset, size=size, value=value))

    def call(self, target):
        if not hasattr(self, 'initializer_calls'):
            return super().call(target)
        assert target == 0x180191470 and self.pc == 0x180193b18
        index = len(self.initializer_calls)+1
        assert index <= 3
        state = Address('state'+str(index))
        assert self.getreg('rcx') == Address('environment') and self.getreg('rdx') == state
        assert self.getreg('r8d') == 127 and self.getreg('r9d') == self.alignment
        sp = self.getreg('rsp')
        assert self.load(plus(sp, 0x20), 4) == 0
        assert self.load(plus(sp, 0x28), 1) == int(self.extra)
        assert self.load(plus(sp, 0x30), 8) == 0
        # Re-run the pinned helper's own affine analyzer, not SDK code. Typed
        # addresses are translated explicitly between its object domain and
        # the particular live state allocation supplied by this caller.
        helper = scalar['Analyzer'](self.alignment, self.extra, False)
        nonvolatile = ('rbx', 'rbp', 'rsi', 'rdi', 'r12', 'r13', 'r14', 'r15')
        markers = {r: 0xA000+i for i, r in enumerate(nonvolatile)}
        for r, marker in markers.items():
            helper.setreg(r, marker)
        result = helper.run()
        assert all(helper.getreg(r) == marker for r, marker in markers.items())
        assert not any(w['domain'] == 'workspace' for w in helper.writes)
        for w in helper.writes:
            assert w['domain'] == 'state'
            value = w['value']
            if isinstance(value, OtherAddress):
                raise AssertionError(('NULL-workspace pass emitted a pointer', value))
            self.store(Address(state.domain, w['offset']), w['size'], value)
        units = helper.getreg('rax')
        assert isinstance(units, int) and units*4 == result['workspaceBytes']
        self.initializer_calls.append(dict(state=state.domain, units=units,
                                          metadataStores=result['metadataStoreOccurrences'],
                                          nonvolatilePreserved=True))
        for r in ('rax', 'rcx', 'rdx', 'r8', 'r9', 'r10', 'r11'):
            self.setreg(r, None)
        self.setreg('rax', units)
        self.zf = self.last_cmp = None

    def run_dispatch(self):
        while self.pc != 0x180193c66:
            self.steps += 1
            assert self.steps < 2048
            i = instructions[self.pc]
            m, o, nxt = i.mnemonic, i.operands, self.pc+i.size
            if m in ('add', 'sub', 'and', 'xor', 'imul', 'inc', 'test', 'idiv', 'call'):
                self.last_cmp = None
            if m == 'mov': self.assign(i, o[0], self.value(i, o[1]))
            elif m == 'lea': self.assign(i, o[0], self.address(i, o[1]))
            elif m in ('movzx', 'movsxd'):
                v = self.value(i, o[1]); self.assign(i, o[0], signed(v, o[1].size*8) if m == 'movsxd' else v)
            elif m == 'push':
                v = self.value(i, o[0]); self.setreg('rsp', plus(self.getreg('rsp'), -8)); self.store(self.getreg('rsp'), 8, v)
            elif m in ('add', 'sub', 'xor', 'imul'):
                if m == 'xor' and o[0].type == o[1].type == CS_OP_REG and o[0].reg == o[1].reg: v = 0
                else:
                    a, b = self.value(i, o[0]), self.value(i, o[1])
                    v = plus(a, b) if m == 'add' else plus(a, -b) if m == 'sub' else a*b if m == 'imul' else a^b
                self.assign(i, o[0], v)
                self.zf = None if m == 'imul' or isinstance(v, Address) else self.value(i, o[0]) == 0
            elif m == 'inc': self.assign(i, o[0], self.value(i, o[0])+1); self.zf = self.value(i, o[0]) == 0
            elif m == 'cmp':
                a, b = self.value(i, o[0]), self.value(i, o[1])
                assert isinstance(a, int) and isinstance(b, int)
                self.last_cmp = (a, b, o[0].size*8); self.zf = a == b
            elif m == 'test':
                a, b = self.value(i, o[0]), self.value(i, o[1])
                if isinstance(a, Address): assert a == b; self.zf = False
                else: assert isinstance(a, int) and isinstance(b, int); self.zf = a&b == 0
            elif m == 'cdqe': self.setreg('rax', signed(self.getreg('eax'), 32))
            elif m == 'cdq': self.setreg('edx', 0xffffffff if signed(self.getreg('eax'), 32) < 0 else 0)
            elif m == 'idiv':
                a = signed((self.getreg('edx') << 32) | self.getreg('eax'), 64)
                b = signed(self.value(i, o[0]), 32); assert b
                q = abs(a)//abs(b); q = -q if (a < 0) != (b < 0) else q
                assert -(1 << 31) <= q < 1 << 31
                self.setreg('eax', q); self.setreg('edx', a-q*b); self.zf = None
            elif m in ('cmove', 'setne'):
                assert self.zf is not None
                if m == 'setne': self.assign(i, o[0], int(not self.zf))
                elif self.zf: self.assign(i, o[0], self.value(i, o[1]))
            elif m in ('jle', 'jge', 'ja'):
                assert self.last_cmp is not None
                a, b, bits = self.last_cmp
                take = signed(a, bits) <= signed(b, bits) if m == 'jle' else signed(a, bits) >= signed(b, bits) if m == 'jge' else (a&((1 << bits)-1)) > (b&((1 << bits)-1))
                if take: nxt = self.value(i, o[0])
            elif m in ('je', 'jne', 'jmp'):
                assert m == 'jmp' or self.zf is not None
                if m == 'jmp' or self.zf == (m == 'je'): nxt = self.value(i, o[0])
            elif m == 'call': self.call(self.value(i, o[0]))
            else: raise AssertionError(('unsupported sizing dispatch', hex(self.pc), m, i.op_str))
            self.pc = nxt
        align = lambda n: ((n+self.alignment-1)//self.alignment)*self.alignment
        expected_units = (2+int(self.extra))*align(14) + 3*next(c['kind8WorkspaceBytes']//4 for c in scalar['layout']['conditionalKind8Workspace'] if c['alignmentFloatUnits'] == self.alignment and c['extraRow'] == self.extra)
        assert self.getreg('r15') == expected_units
        assert len(self.initializer_calls) == 3
        assert image(self.memory, self.readonly_domains) == {k: v for k, v in self.readonly.items() if k[0].domain in self.readonly_domains}
        expected = {}
        for off, n, value in [(0, 8, 0), (8, 8, 0), (16, 8, 0), (24, 4, 13), (28, 4, align(14))]:
            expected[Address('state0', off), n] = value
        # Separately audited scalar certificates compare every defined byte;
        # untouched malloc bytes remain absent, not silently zero-filled.
        for index in (1, 2, 3):
            certificate = scalar['Analyzer'](self.alignment, self.extra, False)
            certificate.run()
            for (a, n), value in certificate.memory.items():
                if a.domain == 'state': expected[Address('state'+str(index), a.offset), n] = value
        domains = {'state0', 'state1', 'state2', 'state3'}
        assert image(self.memory, domains) == image(expected, domains)
        assert len(image(expected, domains)) == 1280
        return dict(alignment=self.alignment, flagsPresent=self.flagsPresent, extraRow=self.extra,
                    stopPC=hex(self.pc), steps=self.steps, initializerCalls=self.initializer_calls,
                    workspaceFloatUnits=expected_units, workspaceBytes=expected_units*4,
                    definedStateMetadataBytes=1280,
                    constructorArgumentHandoffProven=True,
                    workspaceAllocationReached=False, readonlyObjectsPreserved=True)

cases = [Dispatch(a, f, extra).run_dispatch() for a in (8, 16) for f in (False, True) for extra in (False, True)]
report = dict(status='PASS conditional initializer sizing dispatch', cases=cases,
              condition='Inherited projected packet, root/default zero-fill, successful fresh disjoint state allocations, optional packet count58=0; explicit external constructor argument8 byte; NULL existing graph workspace',
              wholeConstructorProven=False, actualCallerOriginProven=False,
              rootDefaultCallbackBodiesComposed=True,
              workspaceAllocationProven=False, secondPassProven=False,
              vendorExecuted=False, runtimePredicateApproved=False)
args.output.write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
print('PASS8 conditional constructor-to-initializer sizing cases; no SDK execution')
