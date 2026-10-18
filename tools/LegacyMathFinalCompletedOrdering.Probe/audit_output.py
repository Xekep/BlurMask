"""Independent036810 permutation and035000 full-tail ownership observations.

Reads captures only; no SDK/ORT calls, expected formula or platform SHA.
"""
import ast
import base64
import hashlib
import json
from pathlib import Path
import struct
import sys

if len(sys.argv) != 3:
    raise SystemExit('capture.json rawMath2.res')
root = Path(__file__).resolve().parents[2]
data = json.loads(Path(sys.argv[1]).read_bytes())
assert data['Runtime'] == '1.23.2'
assert data['Resource'] == hashlib.sha256(Path(sys.argv[2]).read_bytes()).hexdigest()
tree = ast.parse((root / 'tools/LegacyMathCandidateOrdering.Probe/audit_output.py').read_text())
scope = dict(struct=struct)
exec(compile(ast.Module(body=[node for node in tree.body if isinstance(node, ast.FunctionDef)
    and node.name == 'reorder'], type_ignores=[]), '<independent036810>', 'exec'), scope)
reorder = scope['reorder']


def permutation(rows):
    projection = []
    for ordinal, row in enumerate(rows):
        item = bytearray(168)
        struct.pack_into('<i', item, 0, row['ScoreBits'])
        struct.pack_into('<i', item, 8, ordinal)
        projection.append(bytes(item))
    if len(projection) > 1:
        reorder(projection, 0, len(projection) - 1)
    return [struct.unpack_from('<i', row, 8)[0] for row in projection]


def moves(target):
    current = list(range(len(target)))
    result = []
    for index, wanted in enumerate(target):
        source = current.index(wanted)
        if source == index:
            continue
        current[index], current[source] = current[source], current[index]
        result.append(current.copy())
    assert current == target
    return result


assert [case['Width'] for case in data['Cases']] == [1, 3, 16]
records = released = 0
for case in data['Cases']:
    before, width = case['Before'], case['Width']
    count = len(before)
    order = permutation(before)
    assert case['Permutation'] == order
    assert case['Moves'] == moves(order)
    assert case['After'] == [before[prior] for prior in order[:width]]
    statistics = case['Statistics']
    assert statistics['OriginalCount'] == count
    assert statistics['CurrentCount'] == min(width, count)
    assert statistics['ReleasedOriginalOrdinals'] == order[width:]
    assert statistics['Complete'] and not statistics['Failed']
    assert statistics['PayloadReservation'] == case['Ef4Reservation'] + count * 353
    assert statistics['PayloadReservation'] <= 16 * 1024 * 1024
    expected = [0] * count
    logs = []
    for prior in order[width:]:
        expected[prior] = 1
        logs.append(expected.copy())
    assert case['Releases'] == logs
    assert case['BeforeDispose'] == expected
    assert case['AfterDispose'] == [1] * count
    for row in before:
        raw = base64.b64decode(row['Raw48'], validate=True)
        assert len(raw) == 48 and struct.unpack_from('<Q', raw, 8)[0] == 1
        assert row['SourceOffset'] == len(row['Primary'])
        assert row['SourceParent'] == struct.unpack_from('<i', raw, 40)[0]
        assert len(row['Assignments']) == row['Segments']
    records += count
    released += len(logs)

stages = ['BeforeNumericOrdering', 'AfterNumericOrdering', 'BeforeOwnerMove', 'AfterOwnerMove',
          'BeforeRelease', 'AfterRelease', 'BeforeCountCommit', 'AfterCountCommit', 'BeforeCompletion']
assert [(case['Stage'], case['Cancel']) for case in data['Failures']] == \
    [(stage, cancel) for stage in stages for cancel in [False, True]]
for case in data['Failures']:
    before = case['Before']
    order = permutation(before)
    changes = moves(order)
    assert changes
    stage = case['Stage']
    expected_order = (list(range(len(before))) if stage in stages[:3] else
                      changes[0] if stage == 'AfterOwnerMove' else
                      order[:16] if stage in ('AfterCountCommit', 'BeforeCompletion') else order)
    assert case['RetainedOrder'] == expected_order
    destroyed = (order[16:17] if stage == 'AfterRelease' else order[16:]
                 if stage in ('BeforeCountCommit', 'AfterCountCommit', 'BeforeCompletion') else [])
    assert case['Releases'] == [int(prior in destroyed) for prior in range(len(before))]
    stats = case['Statistics']
    assert stats['ReleasedOriginalOrdinals'] == destroyed
    assert stats['CurrentCount'] == len(expected_order)
    assert stats['Failed'] and not stats['Complete']
    assert case['AfterDispose'] == [1] * len(before)

assert len(data['Prepared']) == 256
prepared_bytes = 0
for case in data['Prepared']:
    raw = base64.b64decode(case['Input'], validate=True)
    assert len(raw) == case['Count'] * 168
    rows = [raw[at:at + 168] for at in range(0, len(raw), 168)]
    if len(rows) > 1:
        reorder(rows, 0, len(rows) - 1)
    assert base64.b64decode(case['Output'], validate=True) == b''.join(rows)
    prepared_bytes += len(raw)
assert data['Guards'] == 108
print(f'PASS{records} actual post-EF4 records/{released} ascending tail releases; '
      f'18 count/cleanup failure prefixes;256 prepared ties/NaN/range controls '
      f'({prepared_bytes} full bytes)/108 guards; no final formula/output')
