"""Independent raw AK/LK compact bytes, including genuine LIVE and prepared append."""
import json
import subprocess
import sys
import tempfile
from pathlib import Path
if len(sys.argv) != 4:
    raise SystemExit('AK LK capture.json')
root = Path(__file__).resolve().parents[2]
capture = Path(sys.argv[3]).resolve()
rows = json.loads(capture.read_bytes())
assert len(rows) == 4
assert [r['actualGraphNodeCount'] for r in rows] == [124, 140, 240, 4]
assert [r['nativeGate'] for r in rows] == [True, True, True, False]
assert [r['count'] for r in rows[:3]] == [0, 0, 0]
assert not any(r['hasTerminalWinner'] for r in rows[:3])
assert rows[3]['count'] > 0 and rows[3]['hasTerminalWinner']
with tempfile.TemporaryDirectory(prefix='global-live-', dir=capture.parent) as directory:
    for i, row in enumerate(rows):
        path = Path(directory) / f'{i}.json'
        path.write_text(json.dumps(row), encoding='utf-8')
        subprocess.run([sys.executable, str(root / 'tools/LegacyMainLiveCompactPublication.Probe/audit_output.py'),
                        sys.argv[1], sys.argv[2], str(path)], check=True)
print('PASS actual global124/140/240 empty native gates and separate prepared global4 nonempty LIVE/final append; no winner invented.')
