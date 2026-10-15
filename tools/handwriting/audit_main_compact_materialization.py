"""Pinned read-only207CB0 reachability/publication audit; no vendor execution."""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('output.json')
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [
    (0x180207780, 0x180207920, '865a0ed642d5b834637d3df25d52faa6f5103a3e490df649aeb0d6781556f52d'),
    (0x180207920, 0x180207b00, '28766b57f5b8602c77ac60da577d3d5c606bf562f8255581a2b8e9f067b95742'),
    (0x180207cb0, 0x18020baa0, '92bf7b2b122a344594ef9228efcea73139d4c74079805125cc79874c8bfdfd84'),
    (0x1801a7c70, 0x1801a7cf0, '3104548e88f135a234f30605a8097dbd4cce0f44bf86c08e349ecbc41d677100'),
]
for start, end, expected in regions:
    assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == expected
pins = [
    (0x180207fa3, '80641006fd'),  # Clear beam byte6 bit1.
    (0x180208534, '42804c010602'), # Mark reachable beam byte6 bit1.
    (0x18020789d, '488d535f'),    # Output48 +17, not +1F: row kind.
    (0x1802078a1, '0fb60224073c0274553c017451'),
    (0x18020b697, '803c01000f84a0020000'), # Zero USED bitmap skips header.
    (0x18020965f, 'f30f59c7f30f2cc0'),     # float*255 then truncate.
    (0x18020b54b, 'f30f59c7f30f2cc8'),
    (0x18020b74e, 'f30f59c7f3440f2ce0'),
    (0x18005a3f0, '33c0c3'),      # Actual2B7.V58 has no expansion.
    (0x1801529a0, '488b5278488b024c8b5078'), # Frontend150 -> acoustic.V78.
    (0x1801c12d8, 'f30f1004c8'),  # Float candidate field0, not vector10[channel0].
]
for address, expected in pins:
    assert b.data[b.off(address):b.off(address)+len(expected)//2].hex() == expected, hex(address)
bindings = [
    (0x1802b4360 + 0x58, 0x18005a3f0),
    (0x1802de610 + 0xa0, 0x180151ff0),
    (0x1802de610 + 0x150, 0x1801529a0),
    (0x1802de610 + 0x160, 0x1801529e0),
    (0x1802de610 + 0x170, 0x180152a30),
    (0x1802de610 + 0x220, 0x180152a50),
    (0x1802e1ff0 + 0x30, 0x1801a75d0),
    (0x1802e1ff0 + 0xc0, 0x1801a7c70),
    (0x1802e4180 + 0x70, 0x1801c1200),
    (0x1802e4180 + 0x78, 0x1801c1260),
]
for slot, target in bindings:
    assert b.qwords(slot, 1)[0] == target, hex(slot)
report = dict(regions=[dict(start=hex(s), end=hex(e), sha256=h) for s,e,h in regions],
              exactPins=len(pins), runtimeBindings=len(bindings),
              scope='fresh DIRECT mode0 primary2 secondary1, complete lookahead suffix compact publication',
              excluded=['full207CB0', 'CASE expansion', 'older finalized prefix publication',
                        'associated/protocol alternatives', '21FFD0 text publication', 'acceptance confidence'])
Path(sys.argv[1]).write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
print(f'PASS {len(regions)} native range hashes, {len(pins)} exact byte pins, {len(bindings)} runtime bindings; no vendor execution.')
