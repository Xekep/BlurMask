"""Pinned read-only scalar ABI evidence for ordinary glyph result callbacks."""
import argparse, hashlib, json
from pathlib import Path
from audit_legacy_character_properties import PE

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--dll', type=Path, default=Path('research/myscript/sdk/build/x64/MyScriptText.dll'))
parser.add_argument('--output', type=Path, required=True)
args = parser.parse_args()
raw = args.dll.read_bytes()
sha = hashlib.sha256(raw).hexdigest()
if sha != '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f':
    raise ValueError('Unknown native evidence fingerprint')
pe = PE(raw)
bindings = {
    0x1802de610 + 0x158: 0x1801529c0,
    0x1802de610 + 0x1b8: 0x180152f50,
    0x1802de610 + 0x1c0: 0x180152f90,
    0x1802e4180 + 0x80: 0x1801c1300,
    0x1802e4180 + 0x88: 0x1801c13a0,
    0x1802e4180 + 0x90: 0x1801c1520,
}
for address, target in bindings.items():
    if pe.read(address, 'Q')[0] != target: raise ValueError(f'Changed callback binding: {address:x}')
checks = {
    0x1801529c0: '488b5278488b024c8b909000000049ffe2',
    0x180152f90: '4c8bda4d8bc8488b52784d8bc3488b024c8b908800000049ffe2',
    0x1801c1382: 'f30f100488',  # real XMM0 float return, vector[channel*4]
    0x1801c13ea: 'f30f2cc0',    # CVTTSS2SI scaled fallback, not rounded conversion
    0x1801c14ef: '480fbe570a',  # signed byte geometry index
    0x1801c14f8: '030491',      # native signed32 ADD wraps
    0x1801c1559: 'f30f1001',    # first vector[0], separate from candidate24 field0
}
for address, expected in checks.items():
    actual = bytes(pe.read(address, f'{len(expected)//2}B')).hex()
    if actual != expected: raise ValueError(f'Changed scalar ABI evidence: {address:x}: {actual}')
if pe.read(0x1801c13ea + 0x12c066, 'f')[0] != 40000.0:
    # MULSS instruction starts13e2, next instruction13ea, RIP displacement12c066.
    raise ValueError('Changed fallback multiplier')
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text(json.dumps(dict(schemaVersion=1, dllSha256=sha,
    bindings={f'{k:x}': f'{v:x}' for k,v in bindings.items()},
    fields={'10': 'candidate24.vector10[actual config88] rawfloat',
            '14': 'integer1c13a0 result, stored as integer bits',
            'owner1c': 'first candidate24.vector10[0] rawfloat'},
    warning='TOP273 mass8, candidate24 normalized field0 and score vectors are distinct owners/channels.',
    upstream='Actual graph68 resolves the group before these bounded callbacks;1bcbd0 produces confidence vectors.',
    noVendorExecution=True), indent=2), encoding='utf-8')
print('PASS 6 pinned frontend/backend bindings and rawfloat/signed geometry/wrap/CVTTSS2SI ABI; DLL was not executed.')
