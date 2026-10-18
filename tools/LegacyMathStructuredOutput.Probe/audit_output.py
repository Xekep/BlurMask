"""Independent raw AL + literal byte-frame EE4 + interval ownership oracle.

No C# helpers, vendor execution, model execution or expected transcription.
Optional third argument is only a same-platform full-capture comparison.
"""
import base64
import hashlib
import json
from pathlib import Path
import runpy
import struct
import sys
from reference_formatter import Formatter

if len(sys.argv) not in (3, 4):
    raise SystemExit('capture.json rawMath2.res [same-platform-other.json]')
root = Path(__file__).resolve().parents[2]
data = json.loads(Path(sys.argv[1]).read_bytes())
raw = Path(sys.argv[2]).read_bytes()
assert data['Runtime'] == '1.23.2'
assert data['Resource'] == hashlib.sha256(raw).hexdigest() == \
    '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
reader = runpy.run_path(str(root/'tools/export-ink-math-model.py'))
end = reader['header'](raw, 90)[3]
r = reader['Reader'](raw, 106, end)
assert r.read('H', 8) == [65535]*7+[299]
n = r.read('H')
for increment in (32, 64, 1):
    table = r.read('H', n)
    n = next(value+increment for value in reversed(table) if value != 65535)
spans = r.read('H', n+1)
entries = [r.read('II') for _ in range(spans[-1]+1)]
strings = r.bytes(entries[-1][0])
intervals = {label & 65535: strings[offset:entries[index+1][0]]
             for index, (offset, label) in enumerate(entries[:-1])}
labels = {value.decode('utf-8'): key for key, value in intervals.items()}
assert set(intervals) == set(range(299)) and struct.unpack_from('<H', raw, end)[0] == 297
reference = Formatter(labels)
dot, period, zero = (labels[text] for text in ('⋅', '.', '0'))

def decimal(words):
    result = words.copy()
    for index in range(1, len(result)-1):
        left, current, right = result[index-1:index+2]
        if current & 65535 == dot and not any((value >> 16) & 255 for value in (left, current, right)) and \
           zero <= left & 65535 <= zero+9 and zero <= right & 65535 <= zero+9:
            result[index] = (current & 0xffff0000) | period
    return result

records = alternatives = utf8_bytes = actual_structured = 0
full_strings = sum(5*len(intervals[label])+1 for label in range(1, 297))
peak = max(5*len(intervals[label]) for label in range(1, 297))
assert [case['Width'] for case in data['Cases']] == [1, 3, 16]
for case in data['Cases']:
    stats = case['Statistics']
    assert stats['Complete'] and not stats['Failed'] and stats['StartedRecords'] == case['Width']
    assert len(case['Rows']) == case['Width'] and case['Releases'] == [1]*case['Width']
    assert case['RawSteps'] == len(case['RawHashes']) > 0
    assert all(len(value) == 64 for value in case['RawHashes'])
    assert len({row['LibraryIdentity'] for row in case['Rows']}) == case['Width']
    assert [row['ScoreBits'] for row in case['Rows']] == case['OriginalScoreBits']
    envelope = case['FinalReservation']
    for before, runs, lengths in zip(case['Before'], case['BeforeRuns'], case['WorkspaceLengths'], strict=True):
        count = len(before); maximum = min(65535, count*count+2*count)
        primary, assignment, initial, deletion = lengths
        envelope += full_strings + maximum*(4+2+40+16+8) + 80*count + 4*maximum + len(runs)*16 + primary*2 + assignment*4 + initial + deletion
    assert stats['PayloadReservation'] == envelope+peak
    for before, runs, row in zip(case['Before'], case['BeforeRuns'], case['Rows'], strict=True):
        records += 1
        actual_structured += any((word & 65535) in {tag for _, tags in reference.rules for tag in tags} for word in before)
        projected = reference.evaluate(before)
        assert projected['valid'] and len(projected['mapping']) == len(before)
        expected = decimal(projected['units'])
        assert expected == row['Units'], (before, expected, row['Units'])
        assert projected['mapping'] == row['Map']
        expected_runs = [dict(PrimaryIndex=run['PrimaryIndex'] if run['PrimaryIndex'] == -1 else row['Map'][run['PrimaryIndex']],
                              EndSegmentExclusive=run['EndSegmentExclusive']) for run in runs]
        assert row['Runs'] == expected_runs
        assert len(row['UnitRows']) == len(expected)
        interned = {}
        for word, unit in zip(expected, row['UnitRows'], strict=True):
            encoded = intervals[word & 65535]
            if encoded not in interned:
                interned[encoded] = len(interned)+1
            assert unit == dict(RawUnit=word, StringIdentity=interned[encoded], ByteCount=len(encoded),
                ScalarBits=0, Flags=0, Metadata=4294967295, Utf8=base64.b64encode(encoded).decode())
            alternatives += 1; utf8_bytes += len(encoded)
    assert stats['WrittenRows'] == sum(len(row['UnitRows']) for row in case['Rows'])
    assert stats['UnitPrefix'] == [row['Units'] for row in case['Rows']]
    assert stats['RunPrefix'] == [row['Runs'] for row in case['Rows']]
    assert stats['AlternativeIdentityPrefix'] == [[unit['StringIdentity'] for unit in row['UnitRows']] for row in case['Rows']]
changed = repaired = invalid = 0
families = set()
for control in data['Syntax']:
    expected = reference.evaluate(control['Before'])
    assert expected['units'] == control['After'], (control['Family'], control['Before'], expected, control)
    assert expected['mapping'] == control['Map'], (control['Family'], control['Before'], expected, control)
    assert expected['valid'] == control['Valid'], (control['Family'], control['Before'], expected, control)
    families.add(control['Family'])
    changed += control['Before'] != control['After']
    repaired += len(control['After']) > len(control['Before'])
    invalid += not control['Valid']
assert len(data['Syntax']) == 1614 and {f'canonical:{kind}' for kind in range(14)} <= families
assert changed > 0 and repaired > 0
# Hand-written native discriminators, independent of the procedural interpreter.
for control in data['Syntax']:
    source = [word & 65535 for word in control['Before']]
    if source == [labels['<sub>'], labels['A']]:
        assert control['After'] == control['Before']+[labels['</sub>']]
        assert control['Map'] == [0, 1] and control['Valid']
    if source == [labels['<fence>'], labels['A'], labels['</fence>']]:
        assert control['After'] == [control['Before'][1]] and control['Map'] == [65535, 0, 65535]
    if source == [labels['<frac>'], labels['<fracbar>']]:
        assert control['After'] == [(control['Before'][1] & 0xffff0000) | labels['-']]
        assert control['Map'] == [65535, 0]
    if source == [labels['<fence>'], labels['<sub>'], labels['A'], labels['</sub>'], labels['</fence>']]:
        assert control['After'] == control['Before'][1:-1] and control['Map'] == [65535, 0, 1, 2, 65535]
for control in data['Prepared']:
    assert control['Output'] == decimal(control['Input'])
assert len(data['Failures']) == 18 and data['Guards'] >= 176 and actual_structured > 0
for fault in data['Failures']:
    stats = fault['Statistics']
    assert stats['Failed'] and not stats['Complete'] and fault['Releases'] == [1]*16
    complete = data['Cases'][2]['Rows']
    pool_counts = [0]*16
    units_prefix = [[] for _ in range(16)]
    runs_prefix = [[] for _ in range(16)]
    identity_prefix = [[] for _ in range(16)]
    completed = 0 if fault['Stage'] == 'Claimed' else 16 if fault['Stage'] == 'BeforeCompletion' else 3
    for at in range(completed):
        row = complete[at]
        units_prefix[at], runs_prefix[at] = row['Units'], row['Runs']
        identity_prefix[at] = [unit['StringIdentity'] for unit in row['UnitRows']]
        pool_counts[at] = len({unit['Utf8'] for unit in row['UnitRows']})
    written = sum(len(complete[at]['UnitRows']) for at in range(completed))
    if completed == 3:
        if fault['Stage'] != 'BeforeFormatter':
            units_prefix[3] = complete[3]['Units']
        if fault['Stage'] not in ('BeforeFormatter', 'AfterUnits'):
            runs_prefix[3] = complete[3]['Runs']
        if fault['Stage'] in ('AfterIntern', 'AfterRow'):
            pool_counts[3] = 1
            identity_prefix[3] = [None]*len(complete[3]['UnitRows'])
        if fault['Stage'] == 'AfterRow':
            identity_prefix[3][0] = 1; written += 1
    assert fault['PoolCounts'] == pool_counts
    assert stats['WrittenRows'] == written
    assert stats['UnitPrefix'] == units_prefix and stats['RunPrefix'] == runs_prefix
    assert stats['AlternativeIdentityPrefix'] == identity_prefix
if len(sys.argv) == 4:
    assert Path(sys.argv[1]).read_bytes() == Path(sys.argv[3]).read_bytes()
print(f'PASS {records} actual owned records/{actual_structured} structural records/{alternatives} alternatives/{utf8_bytes} interval bytes; '
      f'{len(data["Syntax"])} byte-frame syntax controls ({changed} changed/{repaired} extended/{invalid} invalid); '
      f'18 failure prefixes/{data["Guards"]} guards; no accepted formula')
