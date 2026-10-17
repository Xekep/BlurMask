"""Static common-secondary frontier -> 037430 argument8 producer. No native execution."""
import hashlib
import itertools
import json
import math
import struct
import sys
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_64
ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv) != 2:
    raise SystemExit('report.json')
REGIONS = [('MyScriptDocument.dll',
  6442681872,
  6442689120,
  '180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'),
 ('MyScriptDocument.dll',
  6442699456,
  6442701280,
  'f51653c489d94baa7dbc4fdca884f33431c585e07ccdd8d41c717bcadcc2091d'),
 ('MyScriptDocument.dll',
  6442677296,
  6442678160,
  'a8ec00faa8c399da401ef220d771754232d4f1d684fb6c1d95a84c7a4330620d'),
 ('MyScriptEngine.dll',
  6444047648,
  6444049040,
  '12334b9c715761adf46501be5ab50481c6bb1292c287876e3b7081f80adb2415'),
 ('MyScriptEngine.dll',
  6444045152,
  6444045568,
  '10bb3c5a6d728b3ce9ed1cc54dd1bdc9254dc34183b4a801c97899e018901927'),
 ('MyScriptEngine.dll',
  6444045568,
  6444045600,
  'a02d36dd746a856349f9d8c8f8eff8575470a7e44da065e091c208f9b5da3896')]
PINS = [('MyScriptDocument.dll', 0x18003867c, 'xor', 'r13d, r13d'),
('MyScriptDocument.dll', 6442682079, 'mov', 'dword ptr [rbp - 0x40], r13d'),
 ('MyScriptDocument.dll', 6442682710, 'mov', 'byte ptr [rsp + 0x62], 1'),
 ('MyScriptDocument.dll', 6442682715, 'mov', 'qword ptr [rbp - 0x30], r13'),
 ('MyScriptDocument.dll', 6442683421, 'mov', 'dword ptr [rbp - 0x40], eax'),
 ('MyScriptDocument.dll', 6442683431, 'mov', 'qword ptr [rbp - 0x30], rax'),
 ('MyScriptDocument.dll', 6442683659, 'movsxd', 'r10, dword ptr [rbp - 0x40]'),
 ('MyScriptDocument.dll', 6442683663, 'mov', 'r9b, 1'),
 ('MyScriptDocument.dll', 6442683666, 'mov', 'r12, qword ptr [rbp - 0x30]'),
 ('MyScriptDocument.dll', 6442683670, 'mov', 'byte ptr [rsp + 0x62], r9b'),
 ('MyScriptDocument.dll', 6442683683, 'cmp', 'r10, rax'),
 ('MyScriptDocument.dll', 6442683686, 'jae', '0x180038d77'),
 ('MyScriptDocument.dll', 6442683691, 'mov', 'r12, r10'),
 ('MyScriptDocument.dll', 6442683699, 'movsx', 'ebx, word ptr [rax + r10*2]'),
 ('MyScriptDocument.dll', 6442683716, 'lea', 'r8, [rcx + 0x10]'),
 ('MyScriptDocument.dll', 6442683728, 'movzx', 'ecx, word ptr [rax + r10*2]'),
 ('MyScriptDocument.dll', 6442683733, 'cmp', 'ecx, ebx'),
 ('MyScriptDocument.dll', 6442683735, 'sete', 'r9b'),
 ('MyScriptDocument.dll', 6442683746, 'mov', 'byte ptr [rsp + 0x62], r9b'),
 ('MyScriptDocument.dll', 6442683759, 'inc', 'r10'),
 ('MyScriptDocument.dll', 6442683765, 'jne', '0x180038d20'),
 ('MyScriptDocument.dll', 6442683769, 'mov', 'qword ptr [rbp - 0x30], r12'),
 ('MyScriptDocument.dll', 6442683907, 'lea', 'rax, [rsp + 0x62]'),
 ('MyScriptDocument.dll', 6442683916, 'mov', 'qword ptr [rsp + 0x40], rax'),
 ('MyScriptDocument.dll', 6442683963, 'call', 'r10'),
 ('MyScriptDocument.dll', 6442683974, 'cmp', 'byte ptr [rsp + 0x62], 0'),
 ('MyScriptDocument.dll', 6442683981, 'inc', 'r12'),
 ('MyScriptDocument.dll', 6442683984, 'mov', 'qword ptr [rbp - 0x30], r12'),
 ('MyScriptDocument.dll', 6442685174, 'mov', 'rax, qword ptr [rbp - 0x30]'),
 ('MyScriptDocument.dll', 6442685193, 'mov', 'qword ptr [rsp + 0x38], rax'),
 ('MyScriptDocument.dll', 6442685221, 'call', '0x180037430'),
 ('MyScriptDocument.dll', 6442677477, 'mov', 'rax, qword ptr [rsp + 0xa8]'),
 ('MyScriptDocument.dll', 6442677500, 'mov', 'qword ptr [r14 + 0x28], rax'),
 ('MyScriptDocument.dll', 6442699511, 'mov', 'r13, qword ptr [rdx + 0x110]'),
 ('MyScriptDocument.dll', 6442699652, 'mov', 'eax, dword ptr [r14 + 0x60]'),
 ('MyScriptDocument.dll', 6442699665, 'mov', 'r8, qword ptr [r14 + 8]'),
 ('MyScriptDocument.dll', 6442699679, 'cmp', 'r13, qword ptr [r14 + 0x18]'),
 ('MyScriptDocument.dll', 6442699689, 'cmp', 'rbx, qword ptr [r14 + 0x20]'),
 ('MyScriptDocument.dll', 6442699699, 'cmp', 'r13, 1'),
 ('MyScriptDocument.dll', 6442699703, 'jbe', '0x18003cd90'),
 ('MyScriptDocument.dll', 6442699715, 'mov', 'rax, qword ptr [r14 + 0x68]'),
 ('MyScriptDocument.dll', 6442699719, 'lea', 'rdx, [r14 + 0x18]'),
 ('MyScriptDocument.dll', 6442699736, 'sub', 'r8, 1'),
 ('MyScriptDocument.dll', 6442699840, 'lea', 'r8, [r8*8]'),
 ('MyScriptDocument.dll', 6442699852, 'call', '0x1801ac820'),
 ('MyScriptDocument.dll', 6442699861, 'lea', 'rdx, [r14 + 0xc8]'),
 ('MyScriptDocument.dll', 6442699882, 'lea', 'r8, [r8*8]'),
 ('MyScriptDocument.dll', 6442699894, 'call', '0x1801ac820'),
 ('MyScriptDocument.dll', 6442699903, 'lea', 'rdx, [r14 + 0x78]'),
 ('MyScriptDocument.dll', 6442699921, 'lea', 'r8, [r8*8]'),
 ('MyScriptDocument.dll', 6442699933, 'call', '0x1801ac820'),
 ('MyScriptDocument.dll', 6442700057, 'cmp', 'byte ptr [r12], sil'),
 ('MyScriptDocument.dll', 6442700100, 'mov', 'r9, rsi'),
 ('MyScriptDocument.dll', 6442700110, 'mov', 'r8d, 1'),
 ('MyScriptDocument.dll', 6442700119, 'mov', 'r10, qword ptr [rax + 0x38]'),
 ('MyScriptDocument.dll', 6442700123, 'call', 'r10'),
 ('MyScriptDocument.dll', 6442700133, 'cmp', 'r15, rax'),
 ('MyScriptDocument.dll', 6442700138, 'test', 'rsi, rsi'),
 ('MyScriptDocument.dll', 6442700143, 'mov', 'byte ptr [r12], 1'),
 ('MyScriptDocument.dll', 6442700156, 'mov', 'byte ptr [r12], 0'),
 ('MyScriptDocument.dll', 6442700185, 'call', '0x18003c0d0'),
 ('MyScriptEngine.dll', 6444047683, 'cmp', 'qword ptr [rdx + 8], 2'),
 ('MyScriptEngine.dll', 6444047719, 'cmp', 'r9, qword ptr [rdx + rsi*8 + 0x10]'),
 ('MyScriptEngine.dll', 6444047782, 'mov', 'r11, qword ptr [rdx + r8*8 + 0x10]'),
 ('MyScriptEngine.dll', 6444047857, 'mov', 'qword ptr [rsp + r15*8 + 0x70], rbx'),
 ('MyScriptEngine.dll', 6444047862, 'mov', 'qword ptr [rsp + rsi*8 + 0x70], r13'),
 ('MyScriptEngine.dll', 6444047898, 'call', '0x180185500'),
 ('MyScriptEngine.dll', 6444047939, 'test', 'rbx, rbx'),
 ('MyScriptEngine.dll', 6444047982, 'mov', 'qword ptr [rsp + 0x38], rcx'),
 ('MyScriptEngine.dll', 6444047997, 'jmp', '0x180185f30'),
 ('MyScriptEngine.dll', 6444048099, 'comiss', 'xmm1, xmm6'),
 ('MyScriptEngine.dll', 6444048108, 'seta', 'al'),
 ('MyScriptEngine.dll', 6444045207, 'mov', 'edi, 4'),
 ('MyScriptEngine.dll', 6444045292, 'add', 'rdx, 0x70'),
 ('MyScriptEngine.dll', 6444045319, 'imul', 'rax, qword ptr [rdx]'),
 ('MyScriptEngine.dll', 6444045323, 'add', 'rax, qword ptr [rdx + 0x50]'),
 ('MyScriptEngine.dll', 6444045331, 'imul', 'rax, rdi'),
 ('MyScriptEngine.dll', 6444045335, 'add', 'r10, rax'),
 ('MyScriptEngine.dll', 6444045343, 'mov', 'rdx, qword ptr [rsi + 0x68]'),
 ('MyScriptEngine.dll', 6444045500, 'mov', 'eax, dword ptr [r10 + rdx]'),
 ('MyScriptEngine.dll', 6444045504, 'mov', 'dword ptr [r8 + 8], eax'),
 ('MyScriptEngine.dll', 6444045582, 'lea', 'r9, [rsp + 0x48]'),
 ('MyScriptEngine.dll', 6444045587, 'call', '0x180185360')]
binaries = {name: Binary(name) for name, *_ in REGIONS}
md = Cs(CS_ARCH_X86, CS_MODE_64)
instructions = {}
for name, start, end, expected in REGIONS:
    b = binaries[name]
    data = b.data[b.off(start):b.off(end)]
    assert hashlib.sha256(data).hexdigest() == expected, (name, hex(start))
    instructions.update({(name, i.address): (i.mnemonic, i.op_str)
                         for i in md.disasm(data, start)})
for name, address, mnemonic, operands in PINS:
    assert instructions[name, address] == (mnemonic, operands), hex(address)
assert binaries['MyScriptDocument.dll'].qwords(0x1801eda50, 1)[0] == 0x18003cac0
engine = binaries['MyScriptEngine.dll']
assert engine.qwords(0x18022a238, 1)[0] == 0x180185d20
# F32 jump table case7, not decompiler's unordered comparison prose.
assert struct.unpack_from('<I', engine.data, engine.off(0x180185fa4) + 6 * 4)[0] == 0x185ee3

# First-element initialization + COMISS/SETA, including first NaN poisoning.
def tensor_argmax(values):
    if not values:
        return -1
    best, index = values[0], 0
    for at, current in enumerate(values[1:], 1):
        above = not (math.isnan(current) or math.isnan(best)) and current > best
        if above:
            best, index = current, at
    return index
argmax_count = 0
for n in range(1, 6):
    for row in itertools.product([-math.inf, -3.0, 0.0, math.inf, math.nan], repeat=n):
        expected = (0 if math.isnan(row[0]) else
                    max((i for i, v in enumerate(row) if not math.isnan(v)),
                        key=lambda i: (row[i], -i)))
        assert tensor_argmax(row) == expected
        argmax_count += 1
assert tensor_argmax([float('nan'), 100.0]) == 0
assert tensor_argmax([-3.0, float('nan'), 100.0]) == 2

# Native scan keeps saved frontier on empty input and starts scanning at the
# separate imported-start scalar, NOT the previous saved frontier.
def machine(secondary, logits, saved=0, start=0):
    pos, frontier, common = start, saved, True
    while pos < len(secondary[0]):
        frontier = pos
        first = struct.unpack('<h', struct.pack('<H', secondary[0][pos]))[0]
        common = all(first == row[pos] for row in secondary[1:])
        pos += 1
        if not common:
            break
    if common and len(logits) > 1:
        common = len({tensor_argmax(row) for row in logits}) == 1
    return frontier + int(common), common

def specification(secondary, logits, saved=0):
    mismatch = next((i for i in range(len(secondary[0]))
                     if any(struct.unpack('<h', struct.pack('<H', secondary[0][i]))[0] != row[i]
                            for row in secondary[1:])), None)
    frontier = mismatch if mismatch is not None else (len(secondary[0]) - 1 if secondary[0] else saved)
    common = mismatch is None and len({tensor_argmax(row) for row in logits}) == 1
    return frontier + int(common), common
frontier_count = 0
for width in range(1, 4):
    for length in range(5):
        for mismatch in range(-1, length):
            rows = [[10 + i for i in range(length)] for _ in range(width)]
            if mismatch >= 0 and width > 1:
                rows[-1][mismatch] += 1
            for argmaxes in itertools.product(range(3), repeat=width):
                logits = [[float(i == index) for i in range(3)] for index in argmaxes]
                for saved in (0, 7):
                    assert machine(rows, logits, saved) == specification(rows, logits, saved)
                    frontier_count += 1
assert machine([[10, 11]], [[0.0, 1.0]]) == (2, True)
assert machine([[0xffff], [0xffff]], [[0.0], [0.0]]) == (0, False)
assert machine([[]], [[0.0]]) == (1, True)  # cold scan is empty, F30 preserves true
# Distinguish rank2 view of frame0 from last-frame score adapter.
first_frame = [[4.0, 0.0], [3.0, 1.0]]
last_frame = [[4.0, 0.0], [0.0, 9.0]]
assert machine([[], []], first_frame)[1]
assert not machine([[], []], last_frame)[1]
report = dict(regions=[dict(binary=n, start=hex(a), end=hex(z), sha256=h) for n,a,z,h in REGIONS],
    instructionPins=len(PINS), bindings=3, preparedArgmaxControls=argmax_count,
    preparedFrontierControls=frontier_count,
    actualArgument='driver local648 -> 037430 argument8 -> completed168+28; not cold constant0',
    scope='No imported prefix, no detection/constraints; same-Step raw logits rank2 view; no runtime implementation',
    frameView='retain data68; drop time dimension/stride/offset; copy dims18, strides78, offsetsC8; contiguous zero-offset view reads frame0',
    runtimeChanges=False, vendorExecution=False)
out = Path(sys.argv[1]).resolve()
out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print(f'PASS {len(REGIONS)} regions/{len(PINS)} pins/3 bindings; {argmax_count} argmax/{frontier_count} frontier controls; static only')
