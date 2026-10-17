"""Static Math2 step-budget and temperature producers; no decoder execution."""
import hashlib
import json
import runpy
import struct
import subprocess
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
out = Path(sys.argv[1])
b = Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions = [
    (0x1800359f0, 0x180035c50, '44a3ff6d114956fe746eb35b3d80f0e3867f6094fc8d8f0da18c353ef1ce6b48'),
    (0x18003b300, 0x18003b540, '78a8116a27d4cf02342618d1da2ec2af0f4ed7e1c751acf897691cc3aea4865a'),
    (0x180044080, 0x180044180, '9d517a7a291fa99c60acb2e690ac1d65b7dd7604eafa6410c05aaafd59d5091a'),
    (0x180044180, 0x180044630, '1375d6401023ec114aff9e145fc11b501b0201451e633c839852e97014243997'),
    (0x180044630, 0x1800448b0, 'bf03c90bfc43d2c819d3ecfccc89cdb75f47910aa5a6b58bd2798978079a241e'),
    (0x180044ac0, 0x180044fb0, 'f7cbc430d27bd819ff2a0d4346def61ff3939f7a8c238cd7f5ede03c104e4265'),
    (0x180045460, 0x180045b90, '8a9d69ac15350ad940b5871b5753d679ea833dafffe8eba010d8699cb47ff908'),
    (0x18003a260, 0x18003b220, '43b6395c140ccc4975bcc1dde81378e5df1745dd5bdd1cbdb88a79007780f19d'),
    (0x180038610, 0x18003a260, '180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'),
    (0x18003c0d0, 0x18003c390, '82e9af90d2e5280e70f0ad15a6c83138ca7c333dcf6836cfb1e8b720b9d8c056'),
    (0x18004a4b0, 0x18004a640, 'c52f1d4a939e8a68ab7e7e0bb12e177c31a5bbde8e9ca0a03763e5f9d178d11a'),
    (0x18004a640, 0x18004a710, '717bda6fc50f7147fb2f5e636ac750163529ed93ba9efc1265bacaca552aef02'),
    (0x180049630, 0x18004a1f0, 'e65e9cf06b42789cb9f9f20f9f98a4d9dc8ef55397169de319cead75e92b1e77'),
]
md = Cs(CS_ARCH_X86, CS_MODE_64)
ins = {}
for start, end, sha in regions:
    raw = b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest() == sha
    ins.update((i.address, (i.mnemonic, i.op_str)) for i in md.disasm(raw, start))
pins = {
    0x180035b1e: ('lea', 'r8, [rdi + 0x50]'),
    0x18003b364: ('lea', 'rbp, [rbx + 0x10]'),
    0x18003b3eb: ('movups', 'xmm1, xmmword ptr [rsi + 0x10]'),
    0x18003b3ef: ('movups', 'xmmword ptr [rbp + 0x10], xmm1'),
    0x180044153: ('mov', 'dword ptr [rdi + 8], 0x3e8'),
    0x18004415a: ('mov', 'dword ptr [rdi + 0xc], 0x40800000'),
    0x180044168: ('mov', 'dword ptr [rdi + 0x68], 0x3f800000'),
    0x1800455a2: ('mov', 'dword ptr [rdi + 0xd0], 0x3f800000'),
    0x180044b01: ('mov', 'r12, qword ptr [rsi + 0x108]'),
    0x180044c19: ('lea', 'r9, [rip + 0x1d5a00]'),
    0x180044c4d: ('mov', 'r9, qword ptr [rax + 0x98]'),
    0x180044c57: ('movss', 'dword ptr [rsi + 0xc], xmm0'),
    0x180044e51: ('test', 'r12, r12'),
    0x180044e54: ('je', '0x180044f85'),
    0x180044e5e: ('lea', 'r9, [rip + 0x1d581b]'),
    0x180044e95: ('mov', 'r9, qword ptr [rax + 0x98]'),
    0x180044e9f: ('movss', 'dword ptr [rsi + 0x68], xmm0'),
    0x18003a496: ('movsxd', 'rsi, dword ptr [rdi + 0x10]'),
    0x18003a4a3: ('movss', 'xmm0, dword ptr [rdi + 0x14]'),
    0x18003a4aa: ('movss', 'xmm1, dword ptr [rip + 0x1ef88e]'),
    0x18003a4bc: ('comiss', 'xmm0, xmm1'),
    0x18003a4bf: ('mov', 'rdx, qword ptr [r15 + 8]'),
    0x18003a4c5: ('subss', 'xmm0, xmm1'),
    0x18003a4d1: ('cvttss2si', 'rax, xmm0'),
    0x18003a4d6: ('add', 'rcx, rax'),
    0x18003a4d9: ('imul', 'rcx, qword ptr [rdx + 0x10]'),
    0x18003a4de: ('inc', 'rcx'),
    0x18003a4e1: ('cmp', 'rsi, rcx'),
    0x18003a4e4: ('cmovb', 'rcx, rsi'),
    0x18003a4f0: ('test', 'rsi, rsi'),
    0x18003a4f5: ('cmp', 'rcx, rsi'),
    0x18003a4f8: ('cmovb', 'rsi, rcx'),
    0x18003a501: ('movsxd', 'rax, dword ptr [rdi + 0x40]'),
    0x18003a91b: ('mov', 'qword ptr [rsp + 0x20], rsi'),
    0x18003a920: ('call', '0x180038610'),
    0x180038cba: ('mov', 'rdx, qword ptr [rbp + 0x640]'),
    0x180038ccd: ('mov', 'rax, rbx'),
    0x180038cd0: ('inc', 'rbx'),
    0x180038ce2: ('cmp', 'rax, rdx'),
    0x180038ce5: ('jae', '0x1800399a5'),
    0x180038cfd: ('call', 'rax'),
    0x180038cff: ('test', 'al, al'),
    0x180038d01: ('je', '0x18003997a'),
    0x18003c104: ('movss', 'xmm6, dword ptr [rip + 0x1edb60]'),
    0x18003c10f: ('divss', 'xmm6, dword ptr [rdx + 0x20]'),
    0x18003c13f: ('movss', 'dword ptr [rsp + 0x30], xmm6'),
    0x18003c156: ('call', 'r10'),
    0x1800499d1: ('mov', 'dword ptr [rbx + 0xd4], esi'),
    0x180049a89: ('mov', 'dword ptr [rbx + 0x48], esi'),
    0x18004a5b0: ('movsxd', 'rax, dword ptr [rdx + 0xd4]'),
    0x18004a5f2: ('mov', 'qword ptr [rdx], rcx'),
    0x18004a6cd: ('mov', 'r8, qword ptr [rax + 0x10]'),
    0x18004a6d1: ('mov', 'rax, qword ptr [rsp + 0x60]'),
    0x18004a6d6: ('mov', 'qword ptr [rsp + 0x20], rax'),
    0x18004a6db: ('call', '0x18004a4b0'),
}
for address, expected in pins.items():
    assert ins[address] == expected, (hex(address), ins[address], expected)
for next_address, displacement, expected in [
    (0x180044c20, 0x1d5a00, b'max_len_factor\0'),
    (0x180044e65, 0x1d581b, b'model_temperature\0'),
]:
    p = b.off(next_address + displacement)
    assert b.data[p:p + len(expected)] == expected
assert b.qwords(0x1801fd150 + 0x18, 1)[0] == 0x18004a640
assert struct.unpack_from('<I', b.data, b.off(0x180229c6c))[0] == 0x3f800000
assert struct.unpack_from('<I', b.data, b.off(0x180229d40))[0] == 0x5f000000

class FlatData:
    """Bounded data-only ORTM field access, with no generated/runtime imports."""
    def __init__(self, data):
        self.data = data

    def read(self, fmt, position):
        size = struct.calcsize('<' + fmt)
        assert 0 <= position <= len(self.data) - size
        return struct.unpack_from('<' + fmt, self.data, position)[0]

    def field(self, table, index):
        vt = table - self.read('i', table)
        size = self.read('H', vt)
        object_size = self.read('H', vt + 2)
        assert size >= 4 and size % 2 == 0 and object_size >= 4
        assert table + object_size <= len(self.data)
        offset = self.read('H', vt + 4 + index * 2) if 4 + index * 2 < size else 0
        if not offset:
            return None
        assert 4 <= offset <= object_size - 4
        return table + offset

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

    def graph_names(self):
        assert self.data[4:8] == b'ORTM'
        model = self.indirect(self.field(self.read('I', 0), 1))
        graph = self.indirect(self.field(model, 7))
        result = {}
        for name, index in [('inputs', 5), ('outputs', 6)]:
            vector = self.indirect(self.field(graph, index))
            count = self.read('I', vector)
            assert 0 < count <= 256
            result[name] = [self.string(vector + 4 + i * 4) for i in range(count)]
        return result

raw = (ROOT / 'models/math2/math2-unwrapped.res').read_bytes()
assert hashlib.sha256(raw).hexdigest() == '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
_, models = runpy.run_path(str(ROOT / 'tools/export-ink-math-model.py'))['parse'](raw)
graph_names = [FlatData(model).graph_names() for model in models]
assert graph_names[0] == dict(inputs=['src', 'downsampling', 'strk_ends'],
    outputs=['output', 'output_len', 'output_seg', 'counting_output', 'max_pred_counts'])
assert all('max_pred_len' not in graph['outputs'] for graph in graph_names)
# Compose the already pinned data-only metadata proof, preserving its bounds/hashes.
out.parent.mkdir(parents=True, exist_ok=True)
dependency = out.with_name(out.stem + '-width-producers.json')
subprocess.run([sys.executable, str(ROOT / 'tools/handwriting/audit_math2_beam_width_producers.py'),
                str(dependency)], check=True)
width = json.loads(dependency.read_bytes())
assert width['models'][2]['config']['max_len_factor'] == 4
assert all('model_temperature' not in item['config'] for item in width['models'])
controls = [dict(inputDimension=n, factor=4, baseCap=min(1000, 4 * n + 1))
            for n in (0, 1, 2, 249, 250, 300)]
assert [c['baseCap'] for c in controls] == [1, 5, 9, 997, 1000, 1000]
report = dict(regions=[dict(start=hex(a), end=hex(z), sha256=s) for a, z, s in regions],
    instructionPins=len(pins), bindings=1, graphNames=graph_names,
    coldDefaults=dict(EED70=1000, EED74Bits='0x40800000', EEDD0Bits='0x3f800000'),
    initializedTemperature=1, actualFourthModelPresent=False,
    actualMaxPredCountsOutputIndex=4, actualMaxPredLenOutputIndex=-1,
    actualCap='min(1000,4*N+1); additionally min with nonzero runtime encoder max_pred_counts K',
    expressionControls=controls,
    fractionalCounterexample=dict(factor=4.5, inputDimension=10,
        truncateFactorThenMultiply=41, multiplyFloatThenTruncate=46),
    temperature='F32 DIVSS(1.0f, EEF20) passed to Engine54.V48',
    inheritance='Clone/raw packets can retain caps/factors; warm EED220 may retain prior child fields despite refreshed EED scalar defaults',
    caveats=['K is computed by encoder inference, not a static resource constant',
             'N is native input tensor first dimension, not raw stroke or point count',
             'Optional max_pred_len and prefix/LM routes remain distinct',
             'Nonfinite/out-of-range conversion behavior is pinned, not ported as a scalar API',
             'No first-EOS/greedy equivalence or completed-hypothesis implementation'],
    metadataProof=width, runtimeChanges=False, vendorExecution=False, ortExecution=False)
out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print(f'PASS {len(regions)} regions/{len(pins)} pins; actual encoder output4 max_pred_counts, '
      'fresh temperature1 and truncation-before-multiply; static only')
