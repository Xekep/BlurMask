"""Independent finite-probability segment winners and run boundaries."""
import base64
import itertools
import json
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
