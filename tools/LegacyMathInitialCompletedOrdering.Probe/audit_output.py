"""Independent036810 permutation plus actual owning-vector associations.

Only reads captures. No vendor/ORT execution, expected text or platform SHA.
"""
import ast
import base64
import copy
import hashlib
import json
from pathlib import Path
import struct
import sys

if len(sys.argv) != 3:
    raise SystemExit('capture.json rawMath2.res')
ROOT = Path(__file__).resolve().parents[2]
capture = json.loads(Path(sys.argv[1]).read_bytes())
assert capture['Runtime'] == '1.23.2'
assert capture['Resource'] == hashlib.sha256(Path(sys.argv[2]).read_bytes()).hexdigest() == \
    '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'

# Reuse the literal independent Python pointer/index transcription only, not
# the existing probe entry point or its platform/capture assertions.
tree = ast.parse((ROOT / 'tools/LegacyMathCandidateOrdering.Probe/audit_output.py').read_text())
scope = dict(struct=struct)
exec(compile(ast.Module(body=[node for node in tree.body if isinstance(node, ast.FunctionDef)
    and node.name == 'reorder'], type_ignores=[]), '<independent036810>', 'exec'), scope)
reorder = scope['reorder']
f32 = lambda value: struct.unpack('<f', struct.pack('<f', value))[0]


def permutation(rows):
    projected = []
    for ordinal, row in enumerate(rows):
        item = bytearray(168)
        struct.pack_into('<i', item, 0, row['ScoreBits'])
        struct.pack_into('<i', item, 8, ordinal)
        projected.append(bytes(item))
    if len(projected) > 1:
        reorder(projected, 0, len(projected) - 1)
    return [struct.unpack_from('<i', item, 8)[0] for item in projected]


def owner_moves(target):
    order = list(range(len(target)))
    result = []
    for at, wanted in enumerate(target):
        source = order.index(wanted)
        if source == at:
            continue
        order[at], order[source] = order[source], order[at]
        result.append(dict(TargetIndex=at, ActualOriginalOrdinals=order.copy()))
    assert order == target
    return result


def verify_record(row, ordinal):
    raw = base64.b64decode(row['SourceRaw48'], validate=True)
    assert len(raw) == 48
    raw_cost, = struct.unpack_from('<f', raw)
    current_symbol, = struct.unpack_from('<Q', raw, 8)
    assert current_symbol == 1  #actual ordinary EOS source, not fallback
    assert row['Primary'] == row['SourcePrimary'] and row['Secondary'] == row['SourceSecondary']
    assert len(row['Primary']) == len(row['Secondary']) == row['SourceOffset']
    assert row['PrimaryCapacity'] == len(row['Primary']) and row['SecondaryCapacity'] == len(row['Secondary'])
    score = f32(raw_cost / f32(f32(f32(len(row['Primary']) + 1) + 5.0) / 6.0))
    assert struct.unpack('<i', struct.pack('<f', score))[0] == row['ScoreBits']
    assert row['Parent'] == struct.unpack_from('<i', raw, 40)[0]
    assert row['LibraryId'] == ordinal + 1 and row['LibraryQwords'] == [0] * 8
    assert row['LibraryDword10'] == 0 and row['NestedTensorCount'] == 0
    assert 0 <= row['Frontier'] <= row['SourceOffset'] + 1 and row['SourceRank'] >= 0
    assert all(len(history) == row['SourceOffset'] + 1 for history in row['SourceAncestry'])


assert [case['PreparedWidth'] for case in capture['Cases']] == [1, 3, 16]
assert len(capture['Cases'][2]['Before']) > 20
record_count = 0
for case in capture['Cases']:
    rows = case['Before']
    assert case['MaximumPosition'] == case['DecoderCalls']
    assert len(rows) == case['VectorCapacity']
    for ordinal, row in enumerate(rows):
        verify_record(row, ordinal)
    expected = permutation(rows)
    assert case['OriginalOrdinals'] == expected
    assert case['After'] == [rows[index] for index in expected]
    assert case['Moves'] == owner_moves(expected)
    assert case['ReleaseCounts'] == [1] * len(rows)
    # Count is NOT reduced to width at the initial sort.
    if case['PreparedWidth'] > 1:
        assert len(rows) > case['PreparedWidth']
    record_count += len(rows)

stages = ['BeforeNumericOrdering', 'AfterNumericOrdering', 'BeforeOwnerMove',
          'AfterOwnerMove', 'BeforeCompletion']
assert [(case['Stage'], case['Cancel']) for case in capture['Failures']] == \
    [(stage, cancel) for stage in stages for cancel in [False, True]]
for case in capture['Failures']:
    rows = case['Before']
    for ordinal, row in enumerate(rows):
        verify_record(row, ordinal)
    expected = permutation(rows)
    moves = owner_moves(expected)
    assert moves
    if case['Stage'] == 'BeforeCompletion':
        order = expected
    elif case['Stage'] == 'AfterOwnerMove':
        order = moves[0]['ActualOriginalOrdinals']
    else:
        order = list(range(len(rows)))
    assert case['ActualOriginalOrdinals'] == order
    assert case['After'] == [rows[index] for index in order]
    assert sorted(order) == list(range(len(rows)))
    assert case['ReleaseCounts'] == [1] * len(rows)

bits = [0xff800000, 0xff7fffff, 0xbf800000, 0x80000000, 0, 0x3f800000,
        0x7f7fffff, 0x7f800000, 0x7fc00001]
counts = [0, 1, 2, 19, 20, 21, 22, 41, 42, 64, 255]
assert len(capture['Prepared']) == 512
verified_bytes = 0
for control, case in enumerate(capture['Prepared']):
    assert case['Control'] == control and case['Count'] == counts[control % len(counts)]
    initial = []
    for ordinal in range(case['Count']):
        row = bytearray((control * 17 + ordinal * 31 + at) & 255 for at in range(168))
        struct.pack_into('<I', row, 0, 0x3f800000 if control % 4 == 0 else bits[(ordinal * 7 + control) % len(bits)])
        struct.pack_into('<i', row, 8, ordinal)
        initial.append(bytes(row))
    assert base64.b64decode(case['Input'], validate=True) == b''.join(initial)
    ordered = copy.copy(initial)
    if len(ordered) > 1:
        reorder(ordered, 0, len(ordered) - 1)
    assert base64.b64decode(case['Output'], validate=True) == b''.join(ordered)
    if control % 4 == 0 and len(ordered) in (20, 21, 42):
        ids = [struct.unpack_from('<i', row, 8)[0] for row in ordered]
        assert ids == (list(range(20)) if len(ids) == 20 else
            list(range(20, -1, -1)) if len(ids) == 21 else list(range(21, 42)) + list(range(21)))
    verified_bytes += len(ordered) * 168
assert capture['Guards'] == 57
print(f'PASS3 actual owner vectors/{record_count} records with full Source/Library associations; '
      f'10 retained failure/cancel boundaries; 512 prepared byte sorts/{verified_bytes} exact bytes; '
      'no initial releases/truncation or postprocessing claim')
