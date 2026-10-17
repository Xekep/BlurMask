"""EF1 mode0 tensor/cache lifecycle: bounded static PE + ORTM data reads only."""
import hashlib
import itertools
import json
import runpy
import struct
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
out = Path(sys.argv[1])
binaries = {name: Binary(name) for name in ['MyScriptDocument.dll', 'MyScriptEngine.dll']}
digests = {'MyScriptDocument.dll': '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f', 'MyScriptEngine.dll': 'd31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'}
for name, binary in binaries.items():
    assert hashlib.sha256(binary.data).hexdigest() == digests[name]
regions = [
    ('MyScriptDocument.dll', 0x180033ed0, 0x180034110, '896d2fe8fa80a78849ea498bd412ef97d82ce1caba9e14b2189e98ede29ca3ea'),
    ('MyScriptDocument.dll', 0x180049180, 0x180049210, '75c973478a84e4aaf1980b4cf53420bd621daac202a95f89d0572c6358d4ad32'),
    ('MyScriptDocument.dll', 0x180049210, 0x1800492f0, 'd133184a4dd6248694fd6d63bfa64cd7ba4c3ff0876fbdf6c712faaad7d8bd94'),
    ('MyScriptDocument.dll', 0x1800492f0, 0x180049360, '2cec47f19007f193eaa546d416b176e9bbee9a8e1433c280268d3e5c39e0e5cb'),
    ('MyScriptDocument.dll', 0x180049360, 0x180049520, '0675131ab4d8b8dc22a63a13c99a78aec2718ae7598588cef01e3a3a43b08e3c'),
    ('MyScriptDocument.dll', 0x180049520, 0x180049630, '4afb6444422ac820b8b92f7a6e2cdaf49de81484b72f84e446ad44137ab230d4'),
    ('MyScriptDocument.dll', 0x180049630, 0x18004a1f0, 'e65e9cf06b42789cb9f9f20f9f98a4d9dc8ef55397169de319cead75e92b1e77'),
    ('MyScriptDocument.dll', 0x18004a420, 0x18004a4b0, 'dcfd374272fb3a9573e9c2106644221f8646d27c098af190f50f190806abc1e3'),
    ('MyScriptDocument.dll', 0x18004a710, 0x18004b9e0, 'c742ebf4305f3704a082e0ca6eea022a931428779c6fc669ce3c2f0b2f997e3e'),
    ('MyScriptDocument.dll', 0x18004b9e0, 0x18004bb10, '10ac277e0e103f72775946f0268c7d9e928d826b70b6baa3525fa9dd245cf71b'),
    ('MyScriptDocument.dll', 0x18004c930, 0x18004d540, '41772ed4fc2013fc188322d36791508f0c23b09f0888c8e72138aed0b2ed075a'),
    ('MyScriptDocument.dll', 0x18003bd40, 0x18003c0d0, '1c024eb35449188805605af08077203d40979a4b68f6f89828ae9d36a0eb1cbc'),
    ('MyScriptDocument.dll', 0x18003da20, 0x18003dbe0, '7c241b6479679efd9b69719990470c73d77912aba9b385585d9d07ee84e89900'),
    ('MyScriptEngine.dll', 0x180033c70, 0x180033d40, 'bcb953a559f9ada2bb2477139ce092b558acfa840ae19eee8201406d3a7efacb'),
    ('MyScriptEngine.dll', 0x180034010, 0x180034200, '9235bad7b4f743cc314589e40d41737222cd2e2e8d8af3f3a6f7e2f72d5ef32c'),
    ('MyScriptEngine.dll', 0x180037410, 0x1800374d0, '97b20cd952a24ae4714548e48041b3ec38d7a2787a01d355f37fdbcb0b96fbea'),
    ('MyScriptEngine.dll', 0x1801ba950, 0x1801ba955, '6293aa158a12190b181ae80bc02df18fd528804febcee5c257fd495783b897ef'),
    ('MyScriptEngine.dll', 0x1801c71c0, 0x1801c7240, '775f7588a02e86fc14bc685982e01985aec3bf5e604a6581284f5ae55be382f0'),
    ('MyScriptEngine.dll', 0x180187440, 0x1801879a0, '7b18d10991b194bc99613929630f157c61cd9b8ca7320fe6884c663f75301843'),
    ('MyScriptEngine.dll', 0x1801879a0, 0x180187fa0, 'e8c7881a477f7a479832f495ff68aeab2f287acfee83244748689bf7ef1974fc'),
    ('MyScriptEngine.dll', 0x180187fa0, 0x1801882b0, '20f4a53e1f9f58b478e2613b913b10915e90c3364126eb40c0ebce0b86e43c2d'),
]
md = Cs(CS_ARCH_X86, CS_MODE_64)
instructions = {}
for name, start, end, sha in regions:
    b = binaries[name]
    raw = b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest() == sha
    instructions.update(((name, i.address), (i.mnemonic, i.op_str))
                        for i in md.disasm(raw, start))
pins = [
    ('MyScriptDocument.dll', 0x18004a743, 'mov', 'qword ptr [rdx + 0x130], 1'),
    ('MyScriptDocument.dll', 0x18004a7a5, 'mov', 'r9d, 0xb'),
    ('MyScriptDocument.dll', 0x18004a7de, 'test', 'r12, r12'),
    ('MyScriptDocument.dll', 0x18004a7fe, 'mov', 'qword ptr [rcx], rdi'),
    ('MyScriptDocument.dll', 0x18004a83b, 'mov', 'dword ptr [rsp + 0x28], 1'),
    ('MyScriptDocument.dll', 0x18004a848, 'call', '0x180049360'),
    ('MyScriptDocument.dll', 0x18004a882, 'mov', 'r9d, 0xb'),
    ('MyScriptDocument.dll', 0x18004a8b0, 'call', '0x180049360'),
    ('MyScriptDocument.dll', 0x18004a9f6, 'call', '0x180049360'),
    ('MyScriptDocument.dll', 0x18004aa69, 'call', '0x180049360'),
    ('MyScriptDocument.dll', 0x18004abd6, 'call', '0x180049360'),
    ('MyScriptDocument.dll', 0x18004ac1c, 'call', '0x180049360'),
    ('MyScriptDocument.dll', 0x18004adef, 'mov', 'r9d, 7'),
    ('MyScriptDocument.dll', 0x18004ae26, 'call', '0x180049360'),
    ('MyScriptDocument.dll', 0x18004b067, 'mov', 'r15, qword ptr [rsi + 0x18]'),
    ('MyScriptDocument.dll', 0x18004b07f, 'call', 'r8'),
    ('MyScriptDocument.dll', 0x18004b09e, 'call', 'r9'),
    ('MyScriptDocument.dll', 0x18004b0bc, 'call', 'r10'),
    ('MyScriptDocument.dll', 0x18004b0db, 'call', 'r9'),
    ('MyScriptDocument.dll', 0x18004b0e6, 'cmp', 'dword ptr [rbp + 0x90], 2'),
    ('MyScriptDocument.dll', 0x18004b108, 'cmp', 'qword ptr [rcx + rdx + 8], 4'),
    ('MyScriptDocument.dll', 0x18004b122, 'cmp', 'qword ptr [rcx + rdx + 8], 4'),
    ('MyScriptDocument.dll', 0x18004b141, 'cmp', 'qword ptr [rcx + rdx + 8], 3'),
    ('MyScriptDocument.dll', 0x18004b1a7, 'movups', 'xmm0, xmmword ptr [rax]'),
    ('MyScriptDocument.dll', 0x18004c956, 'mov', 'rsi, qword ptr [rdx + 0x130]'),
    ('MyScriptDocument.dll', 0x18004ca02, 'movsxd', 'rax, dword ptr [r14 + rax*8 + 0x28]'),
    ('MyScriptDocument.dll', 0x18004ca07, 'mov', 'qword ptr [rbx + rcx*8], rax'),
    ('MyScriptDocument.dll', 0x18004ca10, 'cmovb', 'rax, rcx'),
    ('MyScriptDocument.dll', 0x18004ca14, 'mov', 'qword ptr [rdx + rcx*8], rax'),
    ('MyScriptDocument.dll', 0x18004ca7f, 'call', '0x180049520'),
    ('MyScriptDocument.dll', 0x18004cc20, 'call', '0x1800492f0'),
    ('MyScriptDocument.dll', 0x18004cc5a, 'call', '0x1800492f0'),
    ('MyScriptDocument.dll', 0x18004cc7d, 'call', '0x180049520'),
    ('MyScriptDocument.dll', 0x18004cca0, 'call', '0x180049520'),
    ('MyScriptDocument.dll', 0x18004ccd9, 'call', '0x1800492f0'),
    ('MyScriptDocument.dll', 0x18004cd06, 'call', '0x1800492f0'),
    ('MyScriptDocument.dll', 0x18004cda0, 'mov', 'r8d, 1'),
    ('MyScriptDocument.dll', 0x18004cda6, 'call', '0x180049520'),
    ('MyScriptDocument.dll', 0x18004cdf1, 'mov', 'r9, qword ptr [rax + 0x68]'),
    ('MyScriptDocument.dll', 0x18004cdf5, 'call', 'r9'),
    ('MyScriptDocument.dll', 0x18004ce05, 'mov', 'r8d, 1'),
    ('MyScriptDocument.dll', 0x18004ce16, 'call', '0x180049520'),
    ('MyScriptDocument.dll', 0x18004cee9, 'mov', 'dword ptr [rax + rcx*4], 0xff7fffff'),
    ('MyScriptDocument.dll', 0x18004cffb, 'call', '0x180049520'),
    ('MyScriptDocument.dll', 0x18004d359, 'mov', 'rax, qword ptr [r15]'),
    ('MyScriptDocument.dll', 0x18004d35f, 'mov', 'qword ptr [r8], rax'),
    ('MyScriptDocument.dll', 0x18004d38c, 'movsxd', 'rax, dword ptr [r8 + 0x114]'),
    ('MyScriptDocument.dll', 0x18004d39a, 'movsxd', 'rax, dword ptr [r8 + 0x118]'),
    ('MyScriptDocument.dll', 0x18004d3a1, 'mov', 'r8d, 5'),
    ('MyScriptDocument.dll', 0x18004d3c9, 'div', 'rcx'),
    ('MyScriptDocument.dll', 0x18004d3f0, 'call', '0x1800492f0'),
    ('MyScriptDocument.dll', 0x18004d42a, 'call', '0x1800492f0'),
    ('MyScriptDocument.dll', 0x18004d43f, 'mov', 'r8d, 1'),
    ('MyScriptDocument.dll', 0x18004d44b, 'call', '0x180049520'),
    ('MyScriptDocument.dll', 0x18004d468, 'call', '0x180049520'),
    ('MyScriptDocument.dll', 0x18004d484, 'mov', 'r8d, 4'),
    ('MyScriptDocument.dll', 0x18004d49a, 'call', '0x1800492f0'),
    ('MyScriptDocument.dll', 0x18004d4c3, 'call', '0x1800492f0'),
    ('MyScriptDocument.dll', 0x18004d50c, 'mov', 'qword ptr [r15 + 0x130], r12'),
    ('MyScriptDocument.dll', 0x180049385, 'call', '0x1801ac460'),
    ('MyScriptDocument.dll', 0x18004940c, 'call', '0x180049210'),
    ('MyScriptDocument.dll', 0x18004949d, 'call', 'r9'),
    ('MyScriptDocument.dll', 0x180049557, 'jbe', '0x180049588'),
    ('MyScriptDocument.dll', 0x18004957d, 'mov', 'r10, qword ptr [rax + 0x78]'),
    ('MyScriptDocument.dll', 0x1800495b4, 'mov', 'r10, qword ptr [rax + 0x70]'),
    ('MyScriptDocument.dll', 0x1800495c8, 'jae', '0x180049606'),
    ('MyScriptDocument.dll', 0x1800495f5, 'mov', 'r10, qword ptr [rax + 0x78]'),
    ('MyScriptDocument.dll', 0x18003daa2, 'call', '0x18003d670'),
    ('MyScriptDocument.dll', 0x18003daaf, 'mov', 'rdx, qword ptr [rbx + 0xb8]'),
    ('MyScriptDocument.dll', 0x18003dab9, 'mov', 'r8, qword ptr [rbx + 0xd0]'),
    ('MyScriptEngine.dll', 0x180033c87, 'lea', 'rdi, [rip + 0x21e372]'),
    ('MyScriptEngine.dll', 0x180033ca2, 'mov', 'rax, qword ptr [rdi + 8]'),
    ('MyScriptEngine.dll', 0x180033ca6, 'call', 'rax'),
    ('MyScriptEngine.dll', 0x180034032, 'mov', 'qword ptr [rcx + 8], rax'),
    ('MyScriptEngine.dll', 0x180034059, 'mov', 'rdx, qword ptr [rax + 0x20]'),
    ('MyScriptEngine.dll', 0x18003405d, 'mov', 'qword ptr [r10], rdx'),
    ('MyScriptEngine.dll', 0x180037423, 'je', '0x180037439'),
    ('MyScriptEngine.dll', 0x180037439, 'mov', 'rax, qword ptr [rbx + 0x50]'),
    ('MyScriptEngine.dll', 0x18003743d, 'mov', 'rcx, r8'),
    ('MyScriptEngine.dll', 0x180037440, 'call', 'rax'),
    ('MyScriptEngine.dll', 0x1801ba950, 'jmp', '0x1801c71c0'),
    ('MyScriptEngine.dll', 0x1801c7210, 'mov', 'edx, 8'),
    ('MyScriptEngine.dll', 0x1801c7215, 'call', 'qword ptr [rip + 0x1409d]'),
]
pins += [
    ('MyScriptDocument.dll', 0x1800496d7, 'mov', 'dword ptr [rbx + 0xb0], esi'),
    ('MyScriptDocument.dll', 0x18004974c, 'mov', 'dword ptr [rbx + 0xb8], esi'),
    ('MyScriptDocument.dll', 0x180049788, 'mov', 'dword ptr [rbx + 0xbc], esi'),
    ('MyScriptDocument.dll', 0x180049847, 'mov', 'dword ptr [rbx + 0xc0], esi'),
    ('MyScriptDocument.dll', 0x180049880, 'mov', 'dword ptr [rbx + 0xc4], esi'),
    ('MyScriptDocument.dll', 0x1800498bc, 'mov', 'dword ptr [rbx + 0xc8], esi'),
    ('MyScriptDocument.dll', 0x1800498c4, 'mov', 'byte ptr [rbx + 0x41], 1'),
    ('MyScriptDocument.dll', 0x180049a93, 'mov', 'dword ptr [rbx + 0xd0], esi'),
    ('MyScriptDocument.dll', 0x1800499d1, 'mov', 'dword ptr [rbx + 0xd4], esi'),
    ('MyScriptDocument.dll', 0x180049b4d, 'mov', 'dword ptr [rbx + 0xdc], esi'),
    ('MyScriptDocument.dll', 0x180049b86, 'mov', 'dword ptr [rbx + 0xe0], esi'),
    ('MyScriptDocument.dll', 0x180049bc2, 'mov', 'dword ptr [rbx + 0xe4], esi'),
    ('MyScriptDocument.dll', 0x180049c5d, 'mov', 'dword ptr [rbx + 0xe8], esi'),
    ('MyScriptDocument.dll', 0x180049c96, 'mov', 'dword ptr [rbx + 0xec], esi'),
    ('MyScriptDocument.dll', 0x180049cd2, 'mov', 'dword ptr [rbx + 0xf0], esi'),
    ('MyScriptDocument.dll', 0x180049d8d, 'mov', 'dword ptr [rbx + 0xf4], esi'),
    ('MyScriptDocument.dll', 0x180049dc6, 'mov', 'dword ptr [rbx + 0xf8], esi'),
    ('MyScriptDocument.dll', 0x180049e3e, 'mov', 'dword ptr [rbx + 0x100], esi'),
    ('MyScriptDocument.dll', 0x180049e7a, 'mov', 'dword ptr [rbx + 0x104], esi'),
    ('MyScriptDocument.dll', 0x180049e82, 'mov', 'byte ptr [rbx + 0x42], r13b'),
    ('MyScriptDocument.dll', 0x180049eba, 'mov', 'dword ptr [rbx + 0x108], esi'),
    ('MyScriptDocument.dll', 0x180049efa, 'mov', 'dword ptr [rbx + 0x10c], esi'),
    ('MyScriptDocument.dll', 0x180049f02, 'mov', 'byte ptr [rbx + 0x44], r13b'),
    ('MyScriptDocument.dll', 0x180049f3a, 'mov', 'dword ptr [rbx + 0x110], esi'),
    ('MyScriptDocument.dll', 0x18004a014, 'mov', 'dword ptr [rbx + 0x114], esi'),
    ('MyScriptDocument.dll', 0x18004a007, 'mov', 'dword ptr [rbx + 0x118], esi'),
    ('MyScriptDocument.dll', 0x18004a1b8, 'mov', 'dword ptr [rbx + 0x11c], esi'),
    ('MyScriptDocument.dll', 0x18004a12f, 'mov', 'dword ptr [rbx + 0x124], esi'),
    ('MyScriptDocument.dll', 0x18004a137, 'mov', 'byte ptr [rbx + 0x40], r13b'),
    ('MyScriptDocument.dll', 0x18004a16f, 'mov', 'dword ptr [rbx + 0x128], esi'),
    ('MyScriptDocument.dll', 0x18004a1ab, 'mov', 'dword ptr [rbx + 0x12c], esi'),
    ('MyScriptDocument.dll', 0x18003dad5, 'mov', 'r10, qword ptr [rax + 0x30]'),
    ('MyScriptDocument.dll', 0x18003dad9, 'call', 'r10'),
    ('MyScriptDocument.dll', 0x18003dbaa, 'mov', 'qword ptr [rbx + 0x110], rsi'),
    ('MyScriptDocument.dll', 0x18003dbb1, 'mov', 'dword ptr [rbx + 0x120], edi'),
    ('MyScriptEngine.dll', 0x180033cb8, 'movups', 'xmmword ptr [rbx + 0x48], xmm0'),
    ('MyScriptEngine.dll', 0x180187539, 'cmp', 'rax, 3'),
    ('MyScriptEngine.dll', 0x180187552, 'cmp', 'rcx, 2'),
    ('MyScriptEngine.dll', 0x180187618, 'mov', 'rcx, qword ptr [r14 + 0x20]'),
    ('MyScriptEngine.dll', 0x180187628, 'mov', 'rax, qword ptr [rdi + 0x20]'),
    ('MyScriptEngine.dll', 0x180187631, 'add', 'rax, rcx'),
]
for name, address, mnemonic, operands in pins:
    assert instructions[name, address] == (mnemonic, operands), hex(address)

bindings = [
    ('MyScriptDocument.dll', 0x1801fd150 + 0x18, 0x18004a640),
    ('MyScriptDocument.dll', 0x1801fd150 + 0x20, 0x18004a710),
    ('MyScriptDocument.dll', 0x1801fd150 + 0x28, 0x18004b9e0),
    ('MyScriptDocument.dll', 0x1801fd150 + 0x30, 0x18004c930),
    ('MyScriptDocument.dll', 0x1801eda20 + 0x28, 0x18003bd40),
    ('MyScriptDocument.dll', 0x1801eda20 + 0x40, 0x18003da20),
    ('MyScriptEngine.dll', 0x180252030, 0x1801db660),
    ('MyScriptEngine.dll', 0x1801db660 + 0x20, 0x1801db5a0),
    ('MyScriptEngine.dll', 0x1801db5a0 + 0x18, 0x180037410),
    ('MyScriptEngine.dll', 0x180252000 + 8, 0x1801ba950),
    ('MyScriptEngine.dll', 0x18022a200 + 0x68, 0x180187440),
    ('MyScriptEngine.dll', 0x18022a200 + 0x70, 0x1801879a0),
    ('MyScriptEngine.dll', 0x18022a200 + 0x78, 0x180187fa0),
]
for name, address, expected in bindings:
    assert binaries[name].qwords(address, 1)[0] == expected, hex(address)
engine = binaries['MyScriptEngine.dll']
assert engine.qwords(0x180252000, 1)[0] == 1
# Pin the actual imported Windows allocator, not a decompiler-assigned CRT name.
import pefile
pe = pefile.PE(data=engine.data)
heap_alloc_iat = next(i.address for entry in pe.DIRECTORY_ENTRY_IMPORT
                      for i in entry.imports if i.name == b'HeapAlloc')
assert 0x1801c721b + 0x1409d == heap_alloc_iat

class FlatData:
    """Bounded ORTM field reader. No ORT/generated Python module is imported."""
    def __init__(self, data):
        self.data = data

    def read(self, fmt, position):
        size = struct.calcsize('<' + fmt)
        assert 0 <= position <= len(self.data) - size
        return struct.unpack_from('<' + fmt, self.data, position)[0]

    def field(self, table, index):
        vt = table - self.read('i', table)
        size = self.read('H', vt)
        obj = self.read('H', vt + 2)
        assert size >= 4 and size % 2 == 0 and obj >= 4
        assert 0 <= table <= len(self.data) - obj
        offset = self.read('H', vt + 4 + index * 2) if 4 + index * 2 < size else 0
        if offset == 0:
            return None
        assert 4 <= offset < obj
        return table + offset

    def scalar(self, table, index, fmt, default=0):
        p = self.field(table, index)
        return default if p is None else self.read(fmt, p)

    def indirect(self, position):
        assert position is not None
        target = position + self.read('I', position)
        assert 0 <= target < len(self.data)
        return target

    def string(self, position):
        start = self.indirect(position)
        size = self.read('I', start)
        assert size <= 1048576 and start + 4 + size < len(self.data)
        assert self.data[start + 4 + size] == 0
        return self.data[start + 4:start + 4 + size].decode('utf8')

    def vector(self, position, limit=65536):
        start = self.indirect(position)
        count = self.read('I', start)
        assert count <= limit and start + 4 + count * 4 <= len(self.data)
        return [start + 4 + i * 4 for i in range(count)]

    def graph(self):
        assert self.data[4:8] == b'ORTM'
        model = self.indirect(self.field(self.read('I', 0), 1))
        graph = self.indirect(self.field(model, 7))
        io = {name: [self.string(p) for p in self.vector(self.field(graph, slot), 256)]
              for name, slot in [('inputs', 5), ('outputs', 6)]}
        specs = {}
        for p in self.vector(self.field(graph, 1)):
            value = self.indirect(p)
            name = self.string(self.field(value, 0))
            if name not in io['inputs'] + io['outputs']:
                continue
            info = self.indirect(self.field(value, 2))
            assert self.scalar(info, 1, 'B') == 1  # TensorTypeAndShape union
            tensor = self.indirect(self.field(info, 2))
            shape = self.indirect(self.field(tensor, 1))
            dims = []
            for d in self.vector(self.field(shape, 0), 10):
                dimension = self.indirect(d)
                value = self.indirect(self.field(dimension, 0))
                kind = self.scalar(value, 0, 'B')
                assert kind in (1, 2)
                dims.append(self.scalar(value, 1, 'q') if kind == 1
                            else self.string(self.field(value, 2)))
            assert name not in specs
            specs[name] = dict(type=self.scalar(tensor, 0, 'i'), shape=dims)
        assert set(specs) == set(io['inputs'] + io['outputs'])
        return dict(io=io, specs=specs)

resource = (ROOT / 'models/math2/math2-unwrapped.res').read_bytes()
assert hashlib.sha256(resource).hexdigest() == '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
_, models = runpy.run_path(str(ROOT / 'tools/export-ink-math-model.py'))['parse'](resource)
model_hashes = [
    '3c1a5b691e34385cdaf18f4bdbdb8c8f8cd2cb757f4faed081858f17ea69f286',
    '1393a3c67eafbee9f0bf80a6c3442119bf78b26901986587a2442b1c2d4a0c4a',
    '925a3302b612e634a226ddc907d6ac52c8e908b3d931040cd49b63f4875e4638',
]
assert [hashlib.sha256(x).hexdigest() for x in models] == model_hashes
shapes = [FlatData(model).graph() for model in models]
assert shapes[0]['io'] == dict(inputs=['src', 'downsampling', 'strk_ends'],
    outputs=['output', 'output_len', 'output_seg', 'counting_output', 'max_pred_counts'])
assert shapes[1]['io'] == dict(inputs=['memory', 'batch_size', 'memory_seg'],
    outputs=['cross_k', 'cross_v', 'proj_key_seg_in'])
assert shapes[2]['io'] == dict(inputs=['tgt', 'offset', 'memory_seg', 'prev_attn_seg',
    'proj_key_seg_in', 'state_in_k', 'state_in_v', 'cross_k', 'cross_v'],
    outputs=['reco_output', 'attn_seg', 'state_out_k', 'state_out_v'])
assert shapes[1]['specs']['batch_size'] == dict(type=1, shape=['beam_size'])
assert shapes[2]['specs']['offset'] == dict(type=7, shape=[1])
assert shapes[2]['specs']['tgt'] == dict(type=7, shape=['label_size', 'beam_size'])
for key in ('state_in_k', 'state_in_v'):
    assert shapes[2]['specs'][key] == dict(type=1, shape=[2, 'beam_x_head', 'offset', 64])
assert shapes[2]['specs']['prev_attn_seg'] == dict(type=1,
    shape=['enc_mem_seg_len', 'beam_size', 'prev_label_size'])
assert shapes[2]['specs']['reco_output'] == dict(type=1, shape=['label_size', 'beam_size', 297])

# Prepared coordinate controls only: neither these dimensions nor parents are
# observed decoder outputs. Values encode all axes to expose mistaken gathers.
def product(values):
    value = 1
    for x in values:
        value *= x
    return value

def gather(flat, shape, axis, parents):
    before, after = product(shape[:axis]), product(shape[axis + 1:])
    return [flat[(outer * shape[axis] + parent) * after + inner]
            for outer in range(before) for parent in parents for inner in range(after)]

controls = []
for old_count, parents in [(1, [0, 0, 0]), (3, [2, 0, 2]), (3, [1, 1]), (2, [1, 0])]:
    new_count = len(parents)
    static_map = [i if i < old_count else 0 for i in range(new_count)]
    state_shape = [2, old_count, 4, 3, 2]
    state = list(range(product(state_shape)))
    result = gather(state, state_shape, 1, parents)
    independent = []
    for layer, parent, head, time, channel in itertools.product(range(2), parents,
            range(4), range(3), range(2)):
        independent.append(((((layer * old_count + parent) * 4 + head) * 3 + time) * 2) + channel)
    assert result == independent
    static = gather(state, state_shape, 1, static_map)
    if old_count > 1:
        assert result != static
    # Concatenate attention history before parent gather; replacing it drops T.
    previous = [100 * s + 10 * beam + t for s in range(2)
                for beam in range(old_count) for t in range(2)]
    latest = [1000 + 100 * s + 10 * beam for s in range(2) for beam in range(old_count)]
    concatenated = [x for s in range(2) for beam in range(old_count)
                    for x in previous[(s * old_count + beam) * 2:(s * old_count + beam) * 2 + 2]
                             + [latest[s * old_count + beam]]]
    attention = gather(concatenated, [2, old_count, 3], 1, parents)
    expected = [x for s in range(2) for parent in parents
                for x in [100 * s + 10 * parent, 100 * s + 10 * parent + 1,
                          1000 + 100 * s + 10 * parent]]
    assert attention == expected
    controls.append(dict(oldCount=old_count, parents=parents, staticMap=static_map,
                         stateElements=len(result), attention=attention))

report = dict(regions=[dict(binary=n, start=hex(a), end=hex(z), sha256=h)
                      for n, a, z, h in regions], instructionPins=len(pins),
    bindings=len(bindings), models=shapes, modelSha256=model_hashes,
    preparedCoordinateControls=controls,
    initialScalars='Default global allocator only: no-prefix tgt0, offset0, F32 batch_size[1]=0',
    selectedState='candidate I32+28 parent map; reshape4->5, gather axis1, reshape5->4',
    staticState='identity-or-zero map; memory_seg axis1, proj_key_seg_in axis0, crossK/V expanded rank5 axis1',
    attention='Engine54.V68 concatenates dim2 before parent gather; nonobservable last column gets FF7FFFFF',
    failure='Native mutates tuples/beam count on failure; no transactional rollback claim',
    gaps=['No executable EF1 callback/OrtRuntime join',
          'Custom allocator and arbitrary prefix/optional model variants excluded from zero-fill proof',
          'Engine tensor gather/cat/resize implementations are bound and fingerprinted, not ported',
          'No model inference or numerical encoder outputs captured',
          'CPU exp/log dispatch and full beam/completion controller remain separate'],
    runtimeChanges=False, vendorExecution=False, ortExecution=False)
out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print(f'PASS {len(regions)} regions/{len(pins)} pins/{len(bindings)} bindings; '
      f'{sum(len(x["specs"]) for x in shapes)} actual IO shapes; '
      f'{len(controls)} prepared parent/attention controls; static only')
