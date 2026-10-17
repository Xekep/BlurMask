"""Independent run-based041BA0 four-byte unit compaction oracle."""
from itertools import groupby
import json
from pathlib import Path
import sys
if len(sys.argv) != 2:
    raise SystemExit('capture.json')
capture = json.loads(Path(sys.argv[1]).read_bytes())
assert len(capture['Rows']) == 6143 and capture['Guards'] == 6
high_words = leading = trailing = repeated = deleted_assignments = 0
for row in capture['Rows']:
    units, marker = row['Input'], row['Marker']
    runs = [list(group) for _, group in groupby(range(len(units)), key=lambda i: (units[i] & 65535) == marker)]
    kept = []
    for run_number, indices in enumerate(runs):
        is_separator = (units[indices[0]] & 65535) == marker
        if not is_separator:
            kept.extend(indices)
        elif 0 < run_number < len(runs)-1:
            kept.append(indices[0])
    mapping = {original: new for new, original in enumerate(kept)}
    assert row['Output'] == [units[i] for i in kept]
    assert row['Map'] == [mapping.get(i, -1) for i in range(len(units))]
    assert row['Mapped'] == [mapping.get(i, -1) for i in row['Assignments']]
    high_words += sum(units[i] >> 16 != 0 for i in kept)
    leading += bool(units) and units[0] & 65535 == marker
    trailing += bool(units) and units[-1] & 65535 == marker
    repeated += any(len(run) > 1 and units[run[0]] & 65535 == marker for run in runs)
    deleted_assignments += sum(i >= 0 and i not in mapping for i in row['Assignments'])
assert high_words > 10000 and leading > 3000 and trailing > 3000 and repeated > 3000
assert deleted_assignments > 10000
print(f'PASS6143 independent run-based outputs, complete4-byte preservation and original-assignment remap; '
      f'{deleted_assignments} deleted assignments/{high_words} preserved high words;6 guards')
