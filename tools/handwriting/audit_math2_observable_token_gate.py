"""Static EF3 observable-token classification, not a decoder implementation."""
import hashlib
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
b = Binary('MyScriptDocument.dll')
assert hashlib.sha256(b.data).hexdigest() == '17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f'
regions = [
    (0x18003eb00, 0x18003f050, '934af89ae8ec6ebc3c803ca6bd0974d6766d07939311ea133a10a970cd06dbb7'),
    (0x18003e770, 0x18003e840, '17e1b81755c6733547c9122f2618a28466bf0e9aa523396b9af4b1c4f5e8cc1b'),
    (0x18003e840, 0x18003eb00, 'b188ec6c00333f3366507d1b6f4960857827586417209198e20f82134333bdd6'),
    (0x180035d60, 0x180035e60, '77855b1a9947b85557b447ddadc730b7537fd3050d81e667cef6b9f215857a49'),
    (0x180035920, 0x1800359f0, '3752894d3a2c76bb9aac2105ee02372eab9a07d5b9f2bb4ed3d9bc2510ed133e'),
    (0x180038610, 0x18003a260, '180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'),
    (0x180045460, 0x180045b90, '8a9d69ac15350ad940b5871b5753d679ea833dafffe8eba010d8699cb47ff908'),
]
md = Cs(CS_ARCH_X86, CS_MODE_64)
instructions = {}
for start, end, sha in regions:
    raw = b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(raw).hexdigest() == sha
    instructions.update((i.address, (i.mnemonic, i.op_str)) for i in md.disasm(raw, start))
pins = {
    0x18003eb00: ('mov', 'qword ptr [rdx], 0xffffffffffffffff'),
    0x18003eb07: ('mov', 'qword ptr [rdx + 8], 0xffffffffffffffff'),
    0x18003eb4f: ('movzx', 'eax, word ptr [r8 + 0x12]'),
    0x18003eb57: ('mov', 'word ptr [rdx + 0xa], ax'),
    0x18003eb65: ('call', '0x18003e770'),
    0x18003eb7b: ('call', '0x18003e770'),
    0x18003ec59: ('call', '0x18003e840'),
    0x18003ed4c: ('mov', 'word ptr [rsi], cx'),
    0x18003ed4f: ('mov', 'word ptr [rsi + 4], cx'),
    0x18003edb2: ('mov', 'word ptr [rsi + 0xc], bx'),
    0x18003ee24: ('mov', 'word ptr [rsi + 6], bx'),
    0x18003ee96: ('mov', 'word ptr [rsi + 8], bx'),
    0x18003ef0c: ('mov', 'word ptr [rsi + 2], bx'),
    0x18003ef7a: ('mov', 'word ptr [rsi + 0xe], bx'),
    0x180035dec: ('mov', 'qword ptr [rbx + 0x60], rdi'),
    0x180035dfc: ('mov', 'rdi, qword ptr [rdi]'),
    0x180035e0e: ('mov', 'r8, rdi'),
    0x180035e11: ('lea', 'rdx, [rbx + 0x68]'),
    0x180035e18: ('mov', 'r9, qword ptr [rax + 8]'),
    0x180035e1c: ('call', 'r9'),
    0x180035e32: ('lea', 'r8, [rbx + 0x68]'),
    0x180035e39: ('mov', 'r9, qword ptr [rax + 0x10]'),
    0x180045953: ('mov', 'rdx, qword ptr [rdi + 0x220]'),
    0x18004595a: ('lea', 'r8, [rdi + 0x1d8]'),
    0x180045967: ('mov', 'r9, qword ptr [rax + 0x10]'),
    0x18004596b: ('call', 'r9'),
    0x1800392c6: ('movzx', 'r8d, bx'),
    0x1800392ca: ('lea', 'rdx, [r12 + 0x68]'),
    0x1800392d2: ('mov', 'r9, qword ptr [rax + 0x10]'),
    0x1800392d6: ('call', 'r9'),
    0x180039444: ('movzx', 'r8d, bx'),
    0x180039448: ('lea', 'rdx, [r15 + 0x68]'),
    0x18003944f: ('mov', 'r9, qword ptr [rax + 0x10]'),
    0x180039453: ('call', 'r9'),
}
# Pin the entire short gate, including its unsigned branches and sentinel skips.
gate = list(md.disasm(b.data[b.off(0x18003eff0):b.off(0x18003f04b)], 0x18003eff0))
expected_gate = [
    ('cmp', 'r8w, word ptr [rdx + 0xa]'), ('je', '0x18003f048'),
    ('cmp', 'word ptr [rdx], r8w'), ('ja', '0x18003f004'),
    ('cmp', 'r8w, word ptr [rdx + 4]'), ('jbe', '0x18003f045'),
    ('movzx', 'eax, word ptr [rdx + 6]'), ('mov', 'ecx, 0xffff'),
    ('cmp', 'ax, cx'), ('je', '0x18003f018'),
    ('cmp', 'r8w, ax'), ('je', '0x18003f045'),
    ('movzx', 'eax, word ptr [rdx + 8]'), ('cmp', 'ax, cx'),
    ('je', '0x18003f027'), ('cmp', 'r8w, ax'), ('je', '0x18003f045'),
    ('movzx', 'eax, word ptr [rdx + 0xc]'), ('cmp', 'ax, cx'),
    ('je', '0x18003f036'), ('cmp', 'r8w, ax'), ('je', '0x18003f045'),
    ('movzx', 'eax, word ptr [rdx + 0xe]'), ('cmp', 'ax, cx'),
    ('je', '0x18003f048'), ('cmp', 'r8w, ax'), ('jne', '0x18003f048'),
    ('mov', 'al, 1'), ('ret', ''), ('xor', 'al, al'), ('ret', ''),
]
assert [(i.mnemonic, i.op_str) for i in gate] == expected_gate
for address, expected in pins.items():
    assert instructions[address] == expected, (hex(address), instructions[address], expected)
bindings = [
    (0x1801fcd70 + 0x20, 0x1801fcd50),
    (0x1801fcd50, 0x18003eb00), (0x1801fcd50 + 8, 0x18003eb10),
    (0x1801fcd50 + 0x10, 0x18003eff0), (0x1801fcd50 + 0x18, 0x18003f050),
    (0x1801ed940, 0x180035d60), (0x1801ed9e8, 0x180035920),
]
for address, expected in bindings:
    assert b.qwords(address, 1)[0] == expected, hex(address)
assert b.qwords(0x1801fcd70, 3) == (1, 0xffffffff00000ef3, 0xffffffff)
strings = {0x18020d57c: '\u2402', 0x18020d580: '\u2403',
           0x18020fb34: '\u22c5', 0x18020d314: '\u221a', 0x18020d550: '<vopbar>'}
for address, expected in strings.items():
    p = b.off(address)
    assert b.data[p:p + len(expected.encode()) + 1] == expected.encode() + b'\0'

# Reuse the bounded resource reader, never the decoder or ONNX Runtime.
reader = runpy.run_path(str(ROOT / 'tools/export-ink-math-model.py'))
raw = (ROOT / 'models/math2/math2-unwrapped.res').read_bytes()
assert hashlib.sha256(raw).hexdigest() == '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
_, _, _, end = reader['header'](raw, 90)
r = reader['Reader'](raw, 106, end)
fields = r.read('H', 8)
assert fields == [65535] * 7 + [299]
n = r.read('H')
for increment in (32, 64, 1):
    table = r.read('H', n)
    n = next(v + increment for v in reversed(table) if v != 65535)
spans = r.read('H', n + 1)
entries = [r.read('II') for _ in range(spans[-1] + 1)]
utf8 = r.bytes(entries[-1][0])
labels = {label & 65535: utf8[offset:entries[j + 1][0]].decode('utf8')
          for j, (offset, label) in enumerate(entries[:-1])}
assert set(labels) == set(range(299))
assert labels[297] == '\u2402' and labels[298] == '\u2403'
control = {297, 298}
whitespace = set(range(9, 14)) | {0x20, 0xa0, 0x2028, 0x2029}
scalar_labels = [i for i in range(299) if len(labels[i]) == 1
                 and ord(labels[i]) not in whitespace and i not in control]
# Explicit identifiers avoid treating control names or multi-scalar tags as scalar labels.
inverse = {v: k for k, v in labels.items()}
table = [min(scalar_labels), inverse['\u221a'], max(scalar_labels), inverse['<fracbar>'],
         inverse['<slantedfracbar>'], 65535, inverse['\u22c5'], inverse['<vopbar>']]
assert table == [2, 190, 261, 268, 276, 65535, 242, 296]
assert scalar_labels == list(range(2, 262))

def gate_value(symbol):
    return symbol != table[5] and (table[0] <= symbol <= table[2]
        or any(table[i] != 65535 and symbol == table[i] for i in (3, 4, 6, 7)))

observable = [i for i in range(65536) if gate_value(i)]
assert observable == list(range(2, 262)) + [268, 276, 296]
report = dict(regions=[dict(start=hex(a), end=hex(z), sha256=s) for a, z, s in regions],
    instructionPins=len(pins) + len(expected_gate), tableBindings=len(bindings),
    ef3Table=table, sourceAlphabetCount=299, decoderVocabularyCount=297,
    observableIds=observable, nonObservableDecoderIds=[i for i in range(297) if not gate_value(i)],
    rawAlphabetSha256=hashlib.sha256(raw[90:end]).hexdigest(),
    source='045460 -> EED220.V10(EED1D8) -> EEE035D60 -> EF3+8(EEE68,*packet)',
    caveats=['Static classification only; no beam implementation',
             'owner80 setup and C8 enable/alias map remain separate',
             'Gate true denotes observable-token classification, not complete grammar eligibility',
             'No vendor execution, ORT session or inference'], runtimeChange=False)
out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print(f'PASS {len(regions)} regions/{len(pins) + len(expected_gate)} pins/{len(bindings)} bindings; '
      f'actual EF3 table {table}, {len(observable)} observable IDs; static only')
