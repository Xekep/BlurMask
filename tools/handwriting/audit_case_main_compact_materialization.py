"""Read-only fresh292 emptyV58 lifecycle and shared terminal producer pins."""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary, functions

if len(sys.argv) != 2:
    raise SystemExit('output.json')
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [
    (0x180231ed0, 0x180232060, 'b61cb18bc469784d92a0b86ed2daba96990218d81fd94cfa2cbde76fbd6a24e1'),
    (0x180233fc0, 0x180234840, 'dcf8c0b4d64a3660bf05016c91975c9cf984efa6e9305ff48f7f59cc6cda582f'),
]
for start, end, expected in regions:
    assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == expected
pins = [
    (0x18023203e, '48c786c08f0000ffffff7f'), # CursorMax/count0 afterV80.
    (0x18023409e, '48c783c08f0000ffffff7f'), # ChangedV58 query key.
    (0x180234118, '8b83c48f0000'),
    (0x180234125, '448babc08f0000'),
    (0x18023412c, '443be87c46'),          # Cached count <= cursor fetches.
    (0x18023413a, '488b5308498bcf488b024c8b485841ffd1'), # SAME inner+8.V58.
    (0x18005a3f0, '33c0c3'),             # Selected2B7 returns0.
]
for address, expected in pins:
    assert b.data[b.off(address):b.off(address)+len(expected)//2].hex() == expected, hex(address)
assert b.qwords(0x1802e85b0+0x58, 1)[0] == 0x180233fc0
assert b.qwords(0x1802e85b0+0x80, 1)[0] == 0x180232450
assert b.qwords(0x1802b4360+0x58, 1)[0] == 0x18005a3f0
f = functions()
assert 'FUN_180231ed0(param_1,param_2)' in f['180232450']
assert 'if (iVar9 == 0) break;' in f['180233fc0']
report = dict(regions=[dict(start=hex(s), end=hex(e), sha256=h) for s,e,h in regions],
              exactPins=len(pins), bindings=3,
              scope='fresh finalV80 then292V58 over actual selected2B7; count0 means fallback, not emulated case expansion',
              unresolved=['nonempty nestedV58 expansion','older finalized compact prefix','21FFD0 text/acceptance'])
Path(sys.argv[1]).write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
print(f'PASS {len(regions)} native range hashes, {len(pins)} exact pins, 3 bindings and2 reset/empty contracts; no vendor execution.')
