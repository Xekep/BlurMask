"""Static-only signed LIVE gate, persistent pools and SAME fresh292 query-cache proof."""
import json
import subprocess
import sys
import tempfile
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
if len(sys.argv) != 2:
    raise SystemExit('output.json')
output = Path(sys.argv[1]).resolve()
with tempfile.TemporaryDirectory(prefix='global-live-static-', dir=output.parent) as directory:
    shared = Path(directory) / 'shared.json'
    subprocess.run([sys.executable, str(ROOT / 'tools/handwriting/audit_main_live_compact_publication.py'), str(shared)], check=True)
    report = json.loads(shared.read_bytes())
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary
b = Binary()
pins = [
    (0x180215da6, '8d5f01'),                 # EBX=current node+1.
    (0x180215dc0, '3bd8'),                   # compare against actual source node count.
    (0x180215dc2, '7d1f'),                   # signed >= skips final node, including count240/node239.
    (0x180215dc4, '488b4618'),               # actual knowledge owner.
    (0x180215dc8, '8b4854'),                 # native maximum distance54.
    (0x180215dcb, '038e90240000'),           # add actual persistent next publication2490, DWORD wrap.
    (0x180215dd1, '3bd9'),                   # compare current+1 vs wrapped distance+2490.
    (0x180215dd3, '7c0e'),                   # signed < skips, equality publishes.
    (0x180215dd5, '448bc7'),                 # ending node passed unmodified to207CB0.
    (0x180215dde, 'e8cd1effff'),
]
for address, expected in pins:
    assert b.data[b.off(address):b.off(address)+len(expected)//2].hex() == expected, hex(address)
# Test the proven arithmetic/order separately from any chosen classifier output.
def gates(count):
    cursor = 0
    found = []
    for node in range(count):
        if node+1 < count and 120+cursor <= node+1:
            found.append(node)
            cursor = node+1
    return found
assert gates(124) == gates(140) == gates(240) == [119]
assert gates(241) == [119,239]
report.update(globalGatePins=len(pins), inclusiveOneCycleMaximum=240,
    actualRuntimeScope='Explicit typed logical global source; SAME backend/map/slots, LIVE before V80, same outer290 retained for final append.',
    actualNaturalFixtures=[124,140,240], preparedNonNativeGateFixture='global4/node2; explicitly friend-only',
    excluded=report['excluded']+['more than one intermediate cycle', 'physical1FA300 ring/alias allocation',
                               'natural nonempty >120 compact/winner not established'])
output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f'PASS signed LIVE gate {len(pins)} additional pins; equality119 and inclusive240 boundary; same fresh292 cache/pool pins retained.')
