"""Independent raw-AL / identity-map / decimal / SAME-library interval oracle.

No implementation helper, NN execution, private corpus or expected formula.
Optional third argument compares a same-platform Managed/Native capture only.
"""
import base64
import hashlib
import json
from pathlib import Path
import runpy
import struct
import sys

if len(sys.argv) not in (3, 4):
    raise SystemExit('capture.json rawMath2.res [same-platform-other.json]')
root = Path(__file__).resolve().parents[2]
data = json.loads(Path(sys.argv[1]).read_bytes())
resource = Path(sys.argv[2]).read_bytes()
assert data['Runtime'] == '1.23.2'
assert data['Resource'] == hashlib.sha256(resource).hexdigest() == \
    '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
reader = runpy.run_path(str(root/'tools/export-ink-math-model.py'))
_, _, _, end = reader['header'](resource, 90)
r = reader['Reader'](resource, 106, end)
assert r.read('H', 8) == [65535]*7 + [299]
n = r.read('H')
for increment in (32, 64, 1):
    table = r.read('H', n)
    n = next(value+increment for value in reversed(table) if value != 65535)
spans = r.read('H', n+1)
entries = [r.read('II') for _ in range(spans[-1]+1)]
intervals = r.bytes(entries[-1][0])
labels = {label & 65535: intervals[offset:entries[index+1][0]]
          for index, (offset, label) in enumerate(entries[:-1])}
inverse = {value.decode('utf-8'): key for key, value in labels.items()}
assert set(labels) == set(range(299)) and struct.unpack_from('<H', resource, end)[0] == 297
dot, period, zero = (inverse[text] for text in ('⋅', '.', '0'))
assert [inverse[str(digit)] for digit in range(10)] == list(range(zero, zero+10))

def decimal(words):
    result = words.copy()
    for index in range(1, len(result)-1):
        left, current, right = result[index-1:index+2]
        if current & 65535 == dot and not any((unit >> 16) & 255 for unit in (left, current, right)) and \
           zero <= left & 65535 <= zero+9 and zero <= right & 65535 <= zero+9:
            result[index] = (current & 0xffff0000) | period
    return result

records = units = byte_count = 0
assert [case['Width'] for case in data['Cases']] == [1, 3]
for case in data['Cases']:
    assert len(case['Rows']) == case['Width']
    assert [row['ScoreBits'] for row in case['Rows']] == case['OriginalScoreBits']
    assert case['Releases'] == [1]*case['Width']
    stats = case['Statistics']
    assert stats['Complete'] and not stats['Failed'] and stats['StartedRecords'] == case['Width']
    assert 0 < stats['PayloadReservation'] <= 16*1024*1024
    envelope = case['FinalReservation']
    peak = 0
    for before, runs, lengths in zip(case['Before'], case['BeforeRuns'], case['WorkspaceLengths'], strict=True):
        ids = {word & 65535 for word in before}
        if dot in ids:
            ids.add(period)
        envelope += sum(5*len(labels[label])+1 for label in ids)
        if before:
            peak = max(peak, max(5*len(labels[word & 65535]) for word in before))
        primary, assignments, initial, deleted = lengths
        envelope += len(before)*(4+2+40+16+8) + len(runs)*16 + primary*2 + assignments*4 + initial + deleted
    assert stats['PayloadReservation'] == envelope + peak
    assert case['Stages'][0] == 'Claimed:-1' and case['Stages'][-1] == f'BeforeCompletion:{case["Width"]}'
    for before, runs, row in zip(case['Before'], case['BeforeRuns'], case['Rows'], strict=True):
        records += 1
        assert all(0 < unit & 65535 < 297 and ((unit >> 16) & 255) == 0 for unit in before)
        expected = decimal(before)
        assert row['Units'] == expected and row['Map'] == list(range(len(before)))
        assert row['Runs'] == runs
        assert len(row['UnitRows']) == len(expected)
        interned = {}
        for word, unit in zip(expected, row['UnitRows'], strict=True):
            raw = labels[word & 65535]
            units += 1; byte_count += len(raw)
            if raw not in interned:
                interned[raw] = len(interned)+1
            assert unit == dict(RawUnit=word, StringIdentity=interned[raw], ByteCount=len(raw),
                ScalarBits=0, Flags=0, Metadata=4294967295, Utf8=base64.b64encode(raw).decode())
        # Scalar lane is raw native zero, deliberately not confidence.
        assert stats['WrittenRows'] == sum(len(item['Units']) for item in case['Rows'])
    assert stats['UnitPrefix'] == [row['Units'] for row in case['Rows']]
    assert stats['RunPrefix'] == [row['Runs'] for row in case['Rows']]
    assert stats['AlternativeIdentityPrefix'] == [[unit['StringIdentity'] for unit in row['UnitRows']] for row in case['Rows']]
assert len(data['Prepared']) == 8
for control in data['Prepared']:
    assert control['Output'] == decimal(control['Input'])
    assert (control['Output'] != control['Input']) == (control['Flags'] == 0)
    assert [unit & 0xffff0000 for unit in control['Input']] == [unit & 0xffff0000 for unit in control['Output']]
assert len(data['Failures']) == 18 and data['Guards'] >= 117
for fault in data['Failures']:
    stats = fault['Statistics']
    assert stats['Failed'] and not stats['Complete'] and fault['Releases'] == [1]
    assert stats['StartedRecords'] in (0, 1)
    if fault['Stage'] in ('Claimed', 'BeforeFormatter'):
        assert stats['UnitPrefix'] == stats['RunPrefix'] == [[]]
    else:
        assert stats['UnitPrefix'] == [data['Cases'][0]['Before'][0]]
        assert stats['RunPrefix'] == ([[]] if fault['Stage'] == 'AfterUnits' else [data['Cases'][0]['BeforeRuns'][0]])
    count = fault['PoolCounts'][0]
    if fault['Stage'] in ('Claimed', 'BeforeFormatter', 'AfterUnits', 'AfterRunMap', 'AfterDecimal', 'BeforeRows'):
        assert count == stats['WrittenRows'] == 0
        assert stats['AlternativeIdentityPrefix'] == [[]]
    elif fault['Stage'] == 'AfterIntern':
        assert count == 1 and stats['WrittenRows'] == 0
        assert stats['AlternativeIdentityPrefix'] == [[None, None]]
    elif fault['Stage'] == 'AfterRow':
        assert count == stats['WrittenRows'] == 1
        assert stats['AlternativeIdentityPrefix'] == [[1, None]]
    else:
        assert fault['Stage'] == 'BeforeCompletion' and count == stats['WrittenRows'] == 2
        assert stats['AlternativeIdentityPrefix'] == [[1, 2]]
if len(sys.argv) == 4:
    assert Path(sys.argv[1]).read_bytes() == Path(sys.argv[3]).read_bytes()
print(f'PASS {records} actual records/{units} separate unit alternatives/{byte_count} UTF8 bytes; '
      f'8 prepared decimal controls/18 fault prefixes/{data["Guards"]} guards; no formula concatenation')
