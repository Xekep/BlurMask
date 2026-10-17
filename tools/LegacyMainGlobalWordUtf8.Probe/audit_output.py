"""Independent raw-AK emission/reference/range oracle for global callbacks."""
import json
import subprocess
import sys
from pathlib import Path

if len(sys.argv) != 3:
    raise SystemExit('AK capture.json')
root = Path(__file__).resolve().parents[2]
capture = Path(sys.argv[2]).resolve()
owners = json.loads(capture.read_bytes())
assert len(owners) in (1, 8)
if len(owners) == 1:
    assert 0 <= owners[0]['recordedIndex'] < 9
    assert len(bytes.fromhex(owners[0]['trackSha256'])) == 32
    assert owners[0]['penUpMetadata'] == 'absent'
for i, owner in enumerate(owners):
    assert owner['owner'] == i
    path = capture.with_name(capture.stem+'-owner-'+str(i)+'.json')
    path.write_text(json.dumps(owner,separators=(',',':'))+'\n',encoding='utf-8')
    subprocess.run([sys.executable, str(root/'tools/LegacyMainWordUtf8.Probe/audit_output.py'), sys.argv[1], str(path)], check=True)
print(f'PASS independent{len(owners)} actual global owners/raw AK UTF8/reading order/pool40/associated source ranges; no expected labels')
