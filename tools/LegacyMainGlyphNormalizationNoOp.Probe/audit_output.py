"""Independent zero-write result-stage oracle; no runtime helper imports."""
import json
import sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('actual-capture.json')
o = json.loads(Path(sys.argv[1]).read_bytes())
assert len(o['inputs']) == 5 and o['guards'] == 41
byte_count = 0
for case in o['inputs']:
    assert case['sourceNodes'] > 0 and case['property'] == 0x012d0017 and case['f4'] == 0
    assert not case['executed'] and case['foreignAppendRefused']
    before, after = bytes.fromhex(case['scoresBefore']), bytes.fromhex(case['scoresAfter'])
    assert len(before) == 400 and before == after
    byte_count += len(before)
    assert len(case['poolBefore']) == len(case['poolAfter'])
    for a, b in zip(case['poolBefore'], case['poolAfter']):
        assert a['key'] == b['key'] and a['count'] == b['count']
        first, second = bytes.fromhex(a['storage']), bytes.fromhex(b['storage'])
        assert first == second
        byte_count += len(first)
print(f'PASS independent5 actual completed pools/{byte_count} unchanged bytes/41 guards; fresh F4 no-op only.')
