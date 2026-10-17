"""Independent event attention and sigmoid audit over actual captured tensors."""
import base64
from decimal import Decimal, localcontext
import json
from pathlib import Path
import struct
import subprocess
import sys

import numpy as np

if len(sys.argv) not in (3, 4):
    raise SystemExit('prefix-capture.json rawMath2.res [same-platform-prefix-baseline.json]')
root = Path(__file__).resolve().parents[2]
capture = Path(sys.argv[1])
subprocess.run([sys.executable, str(root / 'tools/LegacyMathCompletedRecords.Probe/audit_output.py'),
                *sys.argv[1:]], check=True)
subprocess.run([sys.executable, str(root / 'tools/handwriting/audit_math2_attention_ancestry.py'),
                str(capture), str(capture) + '.ancestry.json'], check=True)
prefix = json.loads(capture.read_bytes())
actual = json.loads(Path(str(capture) + '.attention.json').read_bytes())
assert actual['Guards'] == 110


def tensor(value):
    assert value['Type'] == 1
    return np.frombuffer(base64.b64decode(value['Bytes'], validate=True), dtype='<f4').reshape(value['Shape'])


events = iter(actual['Events'])
symbols = checked = 0
for run in prefix['Runs']:
    if run['Kind'] != 'actual-logits/native-prepared-candidate-kernels':
        continue
    for step in run['Steps']:
        previous = tensor(step['Before'][3])
        for expected in step['CompletedScores']:
            event = next(events)
            assert (event['Width'], event['Delta'], event['Offset'], event['Rank'], event['Parent']) == (
                run['Width'], run['Delta'], step['PrefixBefore']['Offset'], expected['Rank'], expected['SourceBeam'])
            parent = expected['SourceBeam']
            attention = previous[:, parent, :].T.copy()
            assert event['Symbols'] == len(expected['Prefix']) == attention.shape[0]
            assert event['Segments'] == attention.shape[1]
            assert tensor(event['Attention']).tobytes() == attention.tobytes()
            sigmoid = tensor(event['Sigmoid'])
            assert sigmoid.shape == attention.shape
            # Independent high-precision exp, with tolerance for the explicitly
            # portable MathF.Exp adaptation. The existing sigmoid probe tests
            # separate F32 operation order over a wider prepared value corpus.
            for x, y in zip(attention.flat, sigmoid.flat):
                assert np.isfinite(x) and np.isfinite(y) and 0 <= y <= 1
                if x < -100:
                    expected_value = 0.0
                elif x > 100:
                    expected_value = 1.0
                else:
                    with localcontext() as context:
                        context.prec = 75
                        expected_value = float(1 / (1 + (-Decimal.from_float(float(x))).exp()))
                assert abs(float(y) - expected_value) <= 2e-7
                checked += 1
            symbols += attention.shape[0]
assert next(events, None) is None and len(actual['Events']) == 11
assert len(actual['Prepared']) == 4
for sample in actual['Prepared']:
    source = tensor(sample['Source'])
    expected = source[:, sample['Parent'], :].T
    assert tensor(sample['Selected']).tobytes() == expected.tobytes()
print(f'PASS11 event-owned attention snapshots/{symbols} symbols/{checked} actual sigmoid values;4 coordinate controls/110 guards; old captures unchanged')
