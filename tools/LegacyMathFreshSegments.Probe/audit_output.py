"""Independent finite-probability segment winners and run boundaries."""
import base64
import itertools
import json
from decimal import Decimal, localcontext
import struct
from pathlib import Path
import subprocess
import sys

import numpy as np

if len(sys.argv) not in (3, 4):
    raise SystemExit('prefix-capture.json rawMath2.res [same-platform-prefix-baseline.json]')
root = Path(__file__).resolve().parents[2]
subprocess.run([sys.executable, str(root / 'tools/LegacyMathCompletedAttention.Probe/audit_output.py'),
                *sys.argv[1:]], check=True)
path = Path(sys.argv[1])
a = json.loads(Path(str(path) + '.segments.json').read_bytes())
attention = json.loads(Path(str(path) + '.attention.json').read_bytes())
prefix = json.loads(path.read_bytes())
assert a['Guards'] == 15 and len(a['Events']) == len(attention['Events']) == 11


def tensor(value):
    assert value['Type'] == 1
    return np.frombuffer(base64.b64decode(value['Bytes'], validate=True), dtype='<f4').reshape(value['Shape'])


def runs(assignments):
    end = 0
    result = []
    for index, group in itertools.groupby(assignments):
        end += len(list(group))
        result.append(dict(PrimaryIndex=int(index), EndSegmentExclusive=end))
    return result


def check(event, probabilities, indices):
    assert np.isfinite(probabilities).all() and (probabilities >= 0).all() and (probabilities <= 1).all()
    # NumPy's first-occurrence column argmax is independent of the scalar port.
    assigned = np.asarray(indices)[np.argmax(probabilities, axis=0)].tolist()
    assert event['Assignments'] == assigned
    assert event['Runs'] == runs(assigned)


old_prefixes = {}
for run in prefix['Runs']:
    if run['Kind'] == 'actual-logits/native-prepared-candidate-kernels':
        for step in run['Steps']:
            for event in step['CompletedScores']:
                old_prefixes[run['Width'], run['Delta'], step['PrefixBefore']['Offset'], event['Rank']] = event['Prefix']
for event, source in zip(a['Events'], attention['Events']):
    key = tuple(event[k] for k in ('Width', 'Delta', 'Offset', 'Rank'))
    assert key == tuple(source[k] for k in ('Width', 'Delta', 'Offset', 'Rank'))
    primary = old_prefixes[key]
    indices = [i for i, symbol in enumerate(primary) if 2 <= symbol <= 261 or symbol in (268, 276, 296)]
    assert event['ObservableIndices'] == indices
    check(event, tensor(source['Sigmoid'])[indices, :], indices)
assert len(a['Prepared']) == 512
for event in a['Prepared']:
    check(event, tensor(event['Probabilities']), event['Indices'])
print('PASS11 actual-attention prepared branch maps/512 independent finite/tie coordinate controls/15 guards; no full EF4 claim')

scores = json.loads(Path(str(path) + '.scores.json').read_bytes())
assert scores['Guards'] == 13 and len(scores['Events']) == 11 and len(scores['Prepared']) == 4096
f32 = lambda x: struct.unpack('<f', struct.pack('<f', x))[0]
bits = lambda x: struct.pack('<f', x)
floor = struct.unpack('<f', bytes.fromhex('ffe6db2e'))[0]
maximum_penalty_error = 0.0
separate_rounding = 0


def score_check(value, probabilities, assignments):
    global maximum_penalty_error, separate_rounding
    expected = 0.0
    count = 0
    for segment, symbol in enumerate(assignments):
        if symbol == -1:
            continue
        count += 1
        probability = max(float(probabilities[symbol, segment]), floor)
        with localcontext() as context:
            context.prec = 75
            logarithm = f32(float(Decimal.from_float(probability).ln()))
        expected = f32(expected - logarithm)
    expected = f32(expected / count) if count else 0.0
    error = abs(value['Penalty'] - expected)
    maximum_penalty_error = max(maximum_penalty_error, error)
    assert error <= 2e-5 * max(1.0, abs(expected)), (value, expected)
    # Exact separate F32 multiply then add, independent of the portable log.
    penalty, alpha, before = (f32(value[k]) for k in ('Penalty', 'Alpha', 'Before'))
    after = f32(f32(penalty * alpha) + before)
    assert bits(after) == bits(value['After'])
    separate_rounding += bits(after) != bits(f32(penalty * alpha + before))


base_scores = {}
for run in prefix['Runs']:
    if run['Kind'] == 'actual-logits/native-prepared-candidate-kernels':
        for step in run['Steps']:
            for event in step['CompletedScores']:
                base_scores[run['Width'], run['Delta'], step['PrefixBefore']['Offset'], event['Rank']] = event['CompletedScore']
for event, source, mapped in zip(scores['Events'], attention['Events'], a['Events']):
    key = tuple(event[k] for k in ('Width', 'Delta', 'Offset', 'Rank'))
    assert key == tuple(mapped[k] for k in ('Width', 'Delta', 'Offset', 'Rank'))
    assert bits(event['Value']['Before']) == bits(base_scores[key])
    assert bits(event['Value']['Alpha']) == bits(0.1)
    score_check(event['Value'], tensor(source['Sigmoid']), mapped['Assignments'])
alphas, values = set(), set()
for sample in scores['Prepared']:
    probabilities = tensor(sample['Probabilities'])
    alphas.add(bits(sample['Value']['Alpha']))
    values.update(bits(float(x)) for x in probabilities.flat)
    score_check(sample['Value'], probabilities, sample['Assignments'])
assert alphas == {bits(x) for x in (0, 0.1, 1, -0.5, 3.7)}
assert values == {bits(x) for x in (0, 1e-12, 1e-10, 1, 0.5, 0.9, 0.01)}
assert separate_rounding > 100
print(f'PASS11 actual prepared fresh scores/4096 Decimal-log controls/13 guards; {separate_rounding} separate-rounding discriminators; max portable penalty error={maximum_penalty_error}')
