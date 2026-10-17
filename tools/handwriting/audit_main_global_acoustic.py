"""Pinned, static-only global group -> guide/FF-FF normalization bindings."""
import argparse, hashlib, json, struct, sys
from pathlib import Path
import capstone

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary, functions

p = argparse.ArgumentParser()
p.add_argument('--output', required=True)
args = p.parse_args()
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a, z = 0x1801b4e40, 0x1801b5cf0
raw = b.data[b.off(a):b.off(z)]
h = hashlib.sha256(raw).hexdigest()
assert h == '291c14713096888683b613ce2917c8a8f18a0f2fd83670bfaed4c060b5bbb0f5'
md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
ins = {i.address: (i.mnemonic, i.op_str) for i in md.disasm(raw, a)}
pins = {
    0x1801b4ef2: ('mov', 'r9, qword ptr [rcx + 0x78]'),
    0x1801b4f0a: ('movzx', 'r8d, ax'),
    0x1801b4f0e: ('mov', 'rax, qword ptr [rcx + 0x10]'),
    0x1801b4f25: ('mov', 'esi, dword ptr [rax + 0x10]'),
    0x1801b4f52: ('mov', 'r9, qword ptr [rax + 0x40]'),
    0x1801b4f5c: ('movss', 'xmm10, dword ptr [rax + 4]'),
    0x1801b4f62: ('movss', 'xmm15, dword ptr [rax]'),
    0x1801b4f85: ('mov', 'r9, qword ptr [rax + 0x80]'),
    0x1801b4f94: ('cmp', 'ax, cx'),
    0x1801b4fb5: ('mov', 'r8d, dword ptr [rax + 0x10]'),
    0x1801b4fb9: ('cmp', 'r8d, -1'),
    0x1801b4fd0: ('mov', 'r9, qword ptr [rax + 0x40]'),
    0x1801b4fdf: ('subss', 'xmm0, dword ptr [rbx + 4]'),
    0x1801b4fe8: ('maxss', 'xmm10, xmm1'),
    0x1801b4fed: ('subss', 'xmm1, dword ptr [rax + 4]'),
    0x1801b4ff2: ('movaps', 'xmm15, xmm10'),
    0x1801b4ff6: ('minss', 'xmm1, xmm0'),
    0x1801b4ffa: ('subss', 'xmm10, xmm1'),
    0x1801b5012: ('mov', 'r10, qword ptr [rax + 0x98]'),
    0x1801b5321: ('call', '0x1801b49a0'),
    0x1801b5512: ('cmp', 'byte ptr [r13 + 8], 0xff'),
    0x1801b5519: ('cmp', 'byte ptr [r13 + 9], 0xff'),
    0x1801b5520: ('movzx', 'eax, word ptr [r13 + 0xa]'),
    0x1801b5534: ('subss', 'xmm0, dword ptr [rbp - 0x48]'),
    0x1801b5540: ('mulss', 'xmm0, xmm1'),
    0x1801b5544: ('divss', 'xmm0, xmm10'),
    0x1801b5553: ('cvttss2si', 'edi, xmm0'),
    0x1801b555d: ('subss', 'xmm0, xmm15'),
    0x1801b5562: ('mulss', 'xmm0, xmm1'),
    0x1801b5566: ('divss', 'xmm0, xmm10'),
    0x1801b5578: ('cvttss2si', 'eax, xmm0'),
}
for address, expected in pins.items():
    assert ins[address] == expected, (hex(address), ins.get(address), expected)
f = functions()['1801b4e40']
assert 'fVar28 = fVar29 - fVar28;' in f
assert 'FUN_1801b49a0(param_1,plVar14,0,local_168 - 1)' in f
assert '((*(float *)(lVar15 + lVar10 * 8) - local_100) * fVar24) / fVar28' in f
ak = (ROOT / 'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(ak).hexdigest() == '7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert struct.unpack_from('<I', ak, 189742)[0] == 0x21d
assert struct.unpack_from('<HBBH', ak, 189748) == (0x110, 255, 255, 100)
assert struct.unpack_from('<3f', ak, 195746) == (0., 1., 1.)
report = dict(start=hex(a), endExclusive=hex(z), sha256=h,
              pins=[f'{address:x} {m} {o}' for address, (m, o) in pins.items()],
              source=dict(acousticOffset=189734, target=100, coordinateModes=[255, 255],
                          contextVariance=1, contextOffset=1),
              ownership='Global group primary/secondary are descriptor IDs; source10 resolves line IDs, source40 resolves assigned guides. Cross normalization uses union baseline/height; CA5F0 context uses PRIMARY height separately. Ranges sorted by original chronology, no manufactured seed memberships.',
              vendorExecution=False)
Path(args.output).write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print(f'PASS static{len(pins)} global normalization/guide/chronology pins + exact PE block/profile; no vendor execution')
