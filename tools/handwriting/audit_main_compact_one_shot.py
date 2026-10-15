"""Pinned fresh2490 reset, selectedknowledge54 and2144F0 intermediate gate."""
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
    (0x180215dc0, 0x180215de3, '9457abb39c680f9580ee97a76c32f2136d89c2f02fd008f82f4eae20c08ee7c8'),
    (0x180151bf0, 0x180151c06, '12e35b40fbfe08af7c1692fdf343739817cba0197792e255a54ca38eb8b072d6'),
]
for start, end, expected in regions:
    assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == expected
pins = [
    (0x18020baaf, '488bda'), (0x18020baba, '33f6'),
    (0x18020bc64, '89b390240000'), # NEW2402490=0, ESI preserved zero.
    (0x180151bf4, 'c7405478000000'), # Selected leaf knowledge54=120.
    (0x180215dc0, '3bd87d1f'),    # node+1 >= GraphCount skips publication.
    (0x180215dc4, '488b46188b4854038e902400003bd97c0e'),
    (0x180215dde, 'e8cd1effff'), # Genuine207CB0 caller.
]
for address, expected in pins:
    assert b.data[b.off(address):b.off(address)+len(expected)//2].hex() == expected, hex(address)
assert b.qwords(0x1802de610+0x50, 1)[0] == 0x180151bf0
report = dict(regions=[dict(start=hex(s), end=hex(e), sha256=h) for s,e,h in regions],
              exactPins=len(pins), binding='222.V50=151BF0',
              condition='node+1<GraphCount && knowledge54+2490<=node+1 (signed)',
              proof='NEW2490=0 and54=120 mean GraphCount<=120 has no earlier207CB0',
              excluded=['interleaved publication for longer sources', 'reused2490 state', 'native physical ring allocation'])
Path(sys.argv[1]).write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
print(f'PASS {len(regions)} native range hashes, {len(pins)} exact pins and selected54 binding; no vendor execution.')
