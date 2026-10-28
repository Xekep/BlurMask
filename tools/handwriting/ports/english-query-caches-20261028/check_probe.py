"""Check frozen synthetic sequence and exact extracted helper; no model execution."""
from pathlib import Path
import hashlib
import json
import sys

root = Path(__file__).resolve().parent
source = (root / 'source/LegacyEnglishCaseGraphPeriodicQueryBudget.cs').read_bytes()
assert hashlib.sha256(source).hexdigest() == '2f00b0b27eab29f8a075066ed1dce3f19c11a5a8f35eb996ce729860fdb50030'
text = source.decode().replace('\r\n', '\n')
helper = (root / 'probe/Helper.cs').read_text()
extracted = 'namespace InkRecognition;\n' + text[text.index('internal sealed class LegacyEnglishPeriodicRequestMembershipIndex'):]
assert helper.rstrip('\n') == extracted.rstrip('\n')
def read(path):
    raw = Path(path).read_bytes()
    text = raw.decode('utf-16' if raw.startswith((b'\xff\xfe', b'\xfe\xff')) else 'utf-8-sig')
    return [json.loads(line) for line in text.splitlines() if line.strip()]
expected = read(root / 'probe/expected.jsonl')
actual = read(sys.argv[1])
assert len(expected) == len(actual) == 494
assert actual == expected
print('PASS 494 frozen synthetic membership/collision/interruption/guard rows; no actual source ownership')
