"""Static raw48 sorting/pruning/EOS handoff; no native decoder execution."""
import hashlib
import json
import math
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
    (0x180035f70, 0x1800360b0, '5214240e830e23d5b0a638098417002e7eb6d78fcd88e8a67728db00e86f3e04'),
    (0x180038610, 0x18003a260, '180a1bcc3d913b2a19469e34a2b8a184e5a083354ef8f59414bed3d913e2716a'),
    (0x180044ac0, 0x180044fb0, 'f7cbc430d27bd819ff2a0d4346def61ff3939f7a8c238cd7f5ede03c104e4265'),
    (0x180045460, 0x180045b90, '8a9d69ac15350ad940b5871b5753d679ea833dafffe8eba010d8699cb47ff908'),
    (0x1800359f0, 0x180035c50, '44a3ff6d114956fe746eb35b3d80f0e3867f6094fc8d8f0da18c353ef1ce6b48'),
    (0x180044180, 0x180044630, '1375d6401023ec114aff9e145fc11b501b0201451e633c839852e97014243997'),
    (0x180044630, 0x1800448b0, 'bf03c90bfc43d2c819d3ecfccc89cdb75f47910aa5a6b58bd2798978079a241e'),
]
md = Cs(CS_ARCH_X86, CS_MODE_64)
ins = {}
for lo, hi, sha in regions:
    raw = b.data[b.off(lo):b.off(hi)]
    assert hashlib.sha256(raw).hexdigest() == sha
    ins.update((i.address, (i.mnemonic, i.op_str)) for i in md.disasm(raw, lo))
pins = {
    0x1800455b9: ('mov', 'dword ptr [rdi + 0x94], 0x447a0000'),
    0x180044e09: ('lea', 'r9, [rip + 0x1d5860]'),
    0x180044e47: ('movss', 'dword ptr [rsi + 0x2c], xmm0'),
    0x180045806: ('movss', 'dword ptr [rdi + 0x94], xmm0'),
    0x180035f99: ('sub', 'eax, r8d'),
    0x180035f9f: ('cdq', ''),
    0x180035fa3: ('sub', 'eax, edx'),
    0x180035fa5: ('sar', 'eax, 1'),
    0x180035fa7: ('add', 'eax, r8d'),
    0x180035fb3: ('movss', 'xmm5, dword ptr [rsi + rcx*8]'),
    0x180035fcb: ('comiss', 'xmm5, dword ptr [rcx]'),
    0x180035fce: ('jbe', '0x180035fde'),
    0x180035fdc: ('ja', '0x180035fd0'),
    0x180035fed: ('comiss', 'xmm0, xmm5'),
    0x180035ff0: ('jbe', '0x180036014'),
    0x180036012: ('ja', '0x180036000'),
    0x180036032: ('movups', 'xmm0, xmmword ptr [rsi + rax*8]'),
    0x180036036: ('movups', 'xmm2, xmmword ptr [rsi + rcx*8]'),
    0x18003603a: ('movups', 'xmm3, xmmword ptr [rsi + rcx*8 + 0x10]'),
    0x18003603f: ('movups', 'xmm4, xmmword ptr [rsi + rcx*8 + 0x20]'),
    0x180036044: ('movups', 'xmmword ptr [rsi + rcx*8], xmm0'),
    0x18003604d: ('movups', 'xmmword ptr [rsi + rcx*8 + 0x10], xmm1'),
    0x180036057: ('movups', 'xmmword ptr [rsi + rcx*8 + 0x20], xmm0'),
    0x18003605c: ('movups', 'xmmword ptr [rsi + rax*8], xmm2'),
    0x180036060: ('movups', 'xmmword ptr [rsi + rax*8 + 0x10], xmm3'),
    0x180036065: ('movups', 'xmmword ptr [rsi + rax*8 + 0x20], xmm4'),
    0x18003607e: ('call', '0x180035f70'),
    0x18003608a: ('jmp', '0x180035f94'),
    0x1800391ea: ('lea', 'r9d, [r9*2 - 1]'),
    0x1800391f2: ('call', '0x180035f70'),
    0x180039200: ('movss', 'xmm1, dword ptr [rsi + 0x34]'),
    0x180039208: ('comiss', 'xmm7, xmm1'),
    0x180039215: ('jbe', '0x180039245'),
    0x180039217: ('addss', 'xmm1, dword ptr [r11]'),
    0x18003921c: ('mov', 'edx, 1'),
    0x180039232: ('comiss', 'xmm0, xmm1'),
    0x180039235: ('jb', '0x18003923d'),
    0x180039237: ('mov', 'dword ptr [rcx], 0x7f7fffff'),
    0x18003925e: ('movsxd', 'rax, dword ptr [rsi + 0x28]'),
    0x180039262: ('cmp', 'qword ptr [r15 + 8], rax'),
    0x180039271: ('ucomiss', 'xmm0, xmm6'),
    0x180039274: ('jp', '0x18003927c'),
    0x180039276: ('je', '0x18003934e'),
    0x18003927c: ('cmp', 'edx, ecx'),
    0x18003927e: ('jge', '0x180039347'),
    0x1800392d6: ('call', 'r9'),
    0x1800392db: ('jne', '0x1800392ee'),
    0x1800392dd: ('inc', 'edi'),
    0x1800392e2: ('jb', '0x1800392a2'),
    0x180039325: ('call', '0x180037430'),
    0x180039336: ('jmp', '0x180039347'),
    0x180039347: ('mov', 'dword ptr [r15], 0x7f7fffff'),
    0x180039377: ('lea', 'r9d, [r9*2 - 1]'),
    0x18003937f: ('call', '0x180035f70'),
    0x180039394: ('cmp', 'qword ptr [rbp - 0x38], rax'),
    0x180039398: ('jne', '0x1800394ec'),
    0x180039453: ('call', 'r9'),
    0x180039458: ('jne', '0x18003946f'),
    0x18003945f: ('jb', '0x180039420'),
    0x1800394a6: ('call', '0x180037430'),
    0x1800394ec: ('mov', 'r9d, 1'),
    0x180039502: ('cmp', 'ecx, r9d'),
    0x180039505: ('jle', '0x18003952b'),
    0x180039514: ('ucomiss', 'xmm0, xmm6'),
    0x180039517: ('jp', '0x18003951b'),
    0x180039519: ('je', '0x180039527'),
    0x18003951b: ('inc', 'r9d'),
    0x180039a9c: ('cmp', 'eax, 1'),
    0x180039a9f: ('jbe', '0x180039ab8'),
    0x180039aa6: ('imul', 'rdx, rcx, 0xa8'),
    0x180039ab3: ('call', '0x180036810'),
    0x180039bf5: ('cmp', 'eax, 1'),
    0x180039bf8: ('jbe', '0x180039c2b'),
    0x180039bff: ('imul', 'rdx, rcx, 0xa8'),
    0x180039c0c: ('call', '0x180036810'),
}
for address, expected in pins.items():
    assert ins[address] == expected, (hex(address), ins[address], expected)
p = b.off(0x180044e10 + 0x1d5860)
assert b.data[p:p + 15] == b'delta_cost_max\0'
# 1000 and FMAX are persistent XMM constants loaded before the loop.
assert ins[0x180038c9d] == ('movss', 'xmm7, dword ptr [rip + 0x1f1093]')
assert ins[0x180038ca5] == ('movss', 'xmm6, dword ptr [rip + 0x1f1097]')
assert struct.unpack_from('<I', b.data, b.off(0x180038ca5 + 0x1f1093))[0] == 0x447a0000
assert struct.unpack_from('<I', b.data, b.off(0x180038cad + 0x1f1097))[0] == 0x7f7fffff

def f32(value):
    return struct.unpack('<f', struct.pack('<f', value))[0]

def prepared_prune(costs, delta):
    """Only the pinned numeric stage; no heap, prefix, EOS or model execution."""
    values = [f32(x) for x in costs]
    delta = f32(delta)
    if delta < 1000.0:
        threshold = f32(values[0] + delta)
        for i in range(1, len(values)):
            if values[i] >= threshold:
                values[i] = f32(3.4028234663852886e38)
    return values

pruning_vectors = [
    ([0.0, 1.0], -1.0, [0.0, f32(3.4028234663852886e38)]),
    ([0.0, 0.0], 0.0, [0.0, f32(3.4028234663852886e38)]),
    ([0.0, 1.0], 1.0, [0.0, f32(3.4028234663852886e38)]),
    ([0.0, 1.0], 2.0, [0.0, 1.0]),
    ([0.0, 1.0], 1000.0, [0.0, 1.0]),
    ([0.0, 1.0], float('nan'), [0.0, 1.0]),
]
for costs, delta, expected in pruning_vectors:
    assert prepared_prune(costs, delta) == expected
assert math.isnan(prepared_prune([0.0, float('nan')], -1.0)[1])
assert prepared_prune([float('nan'), 1.0], -1.0)[1] == 1.0

# Prepared only: a two-record equal comparison stops both scans and swaps all48B.
# The two raw class IDs are real layout fields; no actual inference/prefix claim.
equal_records = []
for symbol in (1, 5):
    record = bytearray(struct.pack('<I', 0x7f7fffff) + bytes([symbol]) * 44)
    struct.pack_into('<Q', record, 8, symbol)
    struct.pack_into('<Q', record, 0x20, 0xffffffffffffffff)
    struct.pack_into('<i', record, 0x28, 0)
    equal_records.append(bytes(record))
prepared_before = b''.join(equal_records)
prepared_after = b''.join(reversed(equal_records))
assert len(prepared_after) == 96
assert struct.unpack_from('<Q', prepared_after, 8)[0] == 5
assert struct.unpack_from('<Q', prepared_after, 48 + 8)[0] == 1
out.parent.mkdir(parents=True, exist_ok=True)
dependencies = {}
for name in ('audit_math2_beam_width_producers', 'audit_math_candidate_ordering'):
    target = out.with_name(out.stem + '-' + name + '.json')
    subprocess.run([sys.executable, str(ROOT / 'tools/handwriting' / (name + '.py')), str(target)], check=True)
    dependencies[name] = json.loads(target.read_bytes())
assert dependencies['audit_math2_beam_width_producers']['models'][2]['config']['delta_cost_max'] == -1
report = dict(regions=[dict(start=hex(a), end=hex(z), sha256=s) for a, z, s in regions],
    instructionPins=len(pins) + 2, rawRecordBytes=48, completedRecordBytes=168,
    rawSortRange='inclusive0..2*beam-1, capacity not heap count',
    comparisons='Strict ordered scans; equality/unordered stop; full48B swap; unstable ties',
    coldInitializedDelta=1000, actualMetadataDelta=-1, actualDeltaBits='0xbf800000',
    prune='if delta<1000, F32 threshold=best+delta; ordinals1..2*beam-1 with ordered candidate>=threshold becomeFMAX; unordered preserved',
    eos='Raw EOS class +8 matching EEE28: cost not exactlyFMAX (unordered also enters), rank<beam and nonempty ANY observable prefix ->037430; every nonFMAX EOS candidate markedFMAX, then re-sort',
    activeCount='starts1; scans ordinals1..beam-1 until exact FMAX; NaN is not FMAX',
    preparedNumericVectors=len(pruning_vectors) + 2,
    preparedEqualFmaxWholeBytes=dict(before=prepared_before.hex(), after=prepared_after.hex()),
    width1='finite nonEOS runner-up pruned at delta-1; EOS masking can yield equalFMAX pair swapped on re-sort; activeCount remains1, not immediate termination',
    scope='Static handoff only; no runtime decoder, SDK execution, actual hypothesis trace or greedy equivalence',
    dependencies=dependencies)
out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print(f'PASS {len(regions)} regions/{len(pins) + 2} pins; raw48/capacity sort, actualdelta-1, '
      'EOS re-sort/ANY prefix/activeCount handoff; static only')
