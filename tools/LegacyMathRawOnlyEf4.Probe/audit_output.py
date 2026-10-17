"""Independent actual raw attention -> byte1 empty-library-source EF4 oracle.

The source exclusion is the library pen-only factory, not native EEB selection.
No implementation helpers, vendor execution, model execution or expected text.
"""
import base64
from collections import Counter
from decimal import Decimal, localcontext
import hashlib
import itertools
import json
import math
from pathlib import Path
import struct
import sys

if len(sys.argv) != 3:
    raise SystemExit('capture.json rawMath2.res')
data = json.loads(Path(sys.argv[1]).read_bytes())
assert data['Runtime'] == '1.23.2'
assert data['Resource'] == hashlib.sha256(Path(sys.argv[2]).read_bytes()).hexdigest() == \
    '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
observable = set(range(2, 262)) | {268, 276, 296}
MASK = struct.unpack('<f', bytes.fromhex('ffff7fff'))[0]
ALPHA = struct.unpack('<f', bytes.fromhex('cdcccc3d'))[0]
FLOOR = struct.unpack('<f', bytes.fromhex('ffe6db2e'))[0]

def f32(value):
    try:
        return struct.unpack('<f', struct.pack('<f', value))[0]
    except OverflowError:
        return math.copysign(math.inf, value)

def floats(encoded):
    raw = base64.b64decode(encoded)
    return list(struct.unpack('<' + 'f' * (len(raw) // 4), raw))

def sigmoid(value):
    try:
        exponential = f32(math.exp(f32(-value)))
    except OverflowError:
        exponential = math.inf
    return f32(1.0 / f32(exponential + 1.0))

records = deleted_total = suppressed_total = wrong_basis_cases = 0
max_penalty_error = max_final_error = 0.0
for actual in data['Actual']:
    frames = actual['Frames']
    raw = [floats(frame['Values']) for frame in frames]
    ends = actual['StrokeEnds']
    assert actual['InputFrames'] == ends[-1]
    raw_bytes = sum(len(values) * 4 for values in raw)
    retained = raw_bytes + len(ends) * 8
    workspace = 0
    for row in actual['Rows']:
        n, segments = len(row['Primary']), row['Segments']
        assert n == row['Offset'] and 1 <= n < len(frames)
        assert all(0 <= label < 297 for label in row['Primary'])
        assert any(label in observable for label in row['Primary'])
        ancestor = row['Ancestry'][row['Parent']][n]
        attention = [[0.0] * segments for _ in range(n)]
        for time in reversed(range(n)):
            assert frames[time]['Segments'] == segments
            beams = frames[time]['Beams']
            assert 0 <= ancestor < beams
            for segment in range(segments):
                attention[time][segment] = raw[time][segment * beams + ancestor] \
                    if row['Primary'][time] in observable else MASK
            ancestor = row['Ancestry'][ancestor][time]
        assert ancestor == -1
        activated = [[sigmoid(value) for value in values] for values in attention]
        visible = [index for index, label in enumerate(row['Primary']) if label in observable]
        original = [max(visible, key=lambda index: (activated[index][segment], -index))
                    for segment in range(segments)]
        # Independent run/stroke boundary form: retain a run's first position
        # unless it alone ends a stroke that it did not also begin.
        suppressed = {index for index, value in enumerate(original)
                      if (index == 0 or value != original[index - 1])
                      and (index + 1 == segments or index + 1 in ends)
                      and index != 0 and index not in ends}
        uses = Counter(value for index, value in enumerate(original) if index not in suppressed)
        retained_indices = [index for index, label in enumerate(row['Primary'])
                            if uses[index] or label not in observable]
        deleted = bytes(int(index not in retained_indices) for index in range(n))
        assignments = [-1 if index in suppressed else retained_indices.index(value)
                       for index, value in enumerate(original)]
        units = [0x01000000 | row['Primary'][index] for index in retained_indices]
        assert base64.b64decode(row['Initial']) == bytes(n)
        assert base64.b64decode(row['Deleted']) == deleted
        assert units == row['Units'] and assignments == row['Assignments']
        assert all((unit & 65535) != 65535 for unit in units)  # actual separator no-op
        runs = []
        end = 0
        for value, group in itertools.groupby(assignments):
            end += len(list(group))
            runs.append(dict(PrimaryIndex=value, EndSegmentExclusive=end))
        assert runs == row['Runs']
        raw_candidate = base64.b64decode(row['RawCandidate'])
        assert len(raw_candidate) == 48 and struct.unpack_from('<Q', raw_candidate, 8)[0] == 1
        assert struct.unpack_from('<i', raw_candidate, 40)[0] == row['Parent']
        cost = struct.unpack_from('<f', raw_candidate)[0]
        divisor = f32(f32(f32(n + 1) + 5.0) / 6.0)
        assert abs(row['Before'] - f32(cost / divisor)) <= 2e-7 * max(1.0, abs(row['Before']))
        penalty = 0.0
        contributions = 0
        wrong = 0.0
        with localcontext() as context:
            context.prec = 75
            for segment, index in enumerate(assignments):
                if index == -1:
                    continue
                contributions += 1
                # Deliberately original matrix with post-pruning assignment.
                probability = max(activated[index][segment], FLOOR)
                penalty = f32(penalty - f32(float(Decimal(probability).ln())))
                wrong_probability = max(activated[retained_indices[index]][segment], FLOOR)
                wrong = f32(wrong - f32(float(Decimal(wrong_probability).ln())))
        if contributions:
            penalty = f32(penalty / f32(contributions))
            wrong = f32(wrong / f32(contributions))
        max_penalty_error = max(max_penalty_error, abs(penalty - row['Penalty']))
        assert abs(penalty - row['Penalty']) <= 2e-5 * max(1.0, abs(penalty))
        after = f32(f32(penalty * ALPHA) + row['Before'])
        max_final_error = max(max_final_error, abs(after - row['After']))
        assert abs(after - row['After']) <= 3e-6 * max(1.0, abs(after))
        wrong_basis_cases += int(abs(wrong - penalty) > 1e-6)
        records += 1
        deleted_total += sum(deleted)
        suppressed_total += len(suppressed)
        retained += 4 * n * segments + 32 * n + 16 * segments
        workspace = max(workspace, 16 * n * segments + 64 * (n + segments + len(ends)))
    stats = actual['Statistics']
    assert stats['PlannedPayloadEnvelope'] == retained + workspace <= stats['Budget']
    assert stats['Complete'] and not stats['Failed']
    assert stats['SkippedEmptyRecords'] == 0  # no fabricated actual empty EOS
    assert stats['StartedRecords'] == stats['ScoredRecords'] == len(actual['Rows'])
    assert stats['LastStages'] == ['score'] * len(actual['Rows'])
    assert [row['After'] for row in actual['Rows']] == stats['StoredScores']
    for key, values in [('UnitCounts', [len(row['Units']) for row in actual['Rows']]),
                        ('AssignmentCounts', [len(row['Assignments']) for row in actual['Rows']]),
                        ('InitialMaskCounts', [len(base64.b64decode(row['Initial'])) for row in actual['Rows']]),
                        ('DeletionMaskCounts', [len(base64.b64decode(row['Deleted'])) for row in actual['Rows']]),
                        ('RunCounts', [len(row['Runs']) for row in actual['Rows']])]:
        assert stats[key] == values

phases = {'Claimed': 'unstarted', 'BeforeUnits': 'unstarted', 'AfterUnits': 'units50',
          'AfterAssignment': 'assignment', 'AfterInsertionMask': 'insertion-mask', 'AfterPruning': 'pruned50/runs68',
          'AfterScore': 'score', 'BeforeCompletion': 'score', 'budget-before-payload': 'unstarted',
          'budget-below-envelope': 'unstarted', 'dispose-before-completion': 'score'}
assert len(data['Failures']) == 19
for case in data['Failures']:
    stats = case['Statistics']
    assert stats['Failed'] and not stats['Complete']
    assert stats['LastStages'] == [phases[case['Phase']]]
    if case['Phase'] not in ('AfterScore', 'BeforeCompletion', 'budget-before-payload', 'budget-below-envelope', 'dispose-before-completion'):
        assert stats['StoredScores'] == case['Before']
    assert stats['ScoredRecords'] == int(case['Phase'] in ('AfterScore', 'BeforeCompletion', 'dispose-before-completion'))
    phase = case['Phase']
    before_units = phase in ('Claimed', 'BeforeUnits', 'budget-before-payload', 'budget-below-envelope', 'dispose-before-completion')
    assert stats['UnitCounts'] == [0 if before_units else case['PrimaryCount']]
    assert stats['AssignmentCounts'] == [0 if before_units or phase == 'AfterUnits' else case['Segments']]
    mask_exists = phase in ('AfterInsertionMask', 'AfterPruning', 'AfterScore', 'BeforeCompletion')
    assert stats['InitialMaskCounts'] == [case['PrimaryCount'] if mask_exists else 0]
    pruned = phase in ('AfterPruning', 'AfterScore', 'BeforeCompletion')
    assert stats['DeletionMaskCounts'] == [case['PrimaryCount'] if pruned else 0]
    assert (stats['RunCounts'][0] > 0) == pruned
assert len(data['Prepared']) == 256
for case in data['Prepared']:
    n, s, matrix = case['Symbols'], case['Segments'], case['OriginalMatrix']
    assert len(matrix) == n * s and case['RemappedAssignments'] == [0] * s
    values = []
    with localcontext() as context:
        context.prec = 75
        for start in (0, s):
            value = 0.0
            for segment in range(s):
                value = f32(value - f32(float(Decimal(matrix[start + segment]).ln())))
            values.append(f32(value / f32(s)))
    assert abs(values[0] - case['Penalty']) <= 2e-6
    assert abs(values[1] - case['WrongRetainedPenalty']) <= 2e-6
    assert abs(values[0] - values[1]) > 0.1
    assert abs(f32(f32(values[0] * ALPHA) + case['Before']) - case['After']) <= 2e-6
assert len(data['PreparedEmpty']) == 256
for case in data['PreparedEmpty']:
    assert case['BeforeBits'] == case['AfterBits']
    assert case['CaptureCalls'] == case['OutputUnits'] == 0
assert data['PreparedEmpty'][0]['BeforeBits'] & 0x7f800000 == 0x7f800000
assert len(data['Actual']) == 3 and data['Guards'] == 129
assert records > 0
print(f'PASS{records} actual Source-owned EF4 records; {deleted_total} original units deleted/'
      f'{suppressed_total} suppressed segments/{wrong_basis_cases} literal-index counterexamples; '
      f'19 failure prefixes/256 basis+256 empty prepared controls/129 guards; max penalty error={max_penalty_error}, final error={max_final_error}; no final output')
