"""Independent backward036F20 ancestry versus carried EF1 attention.

Reads captures only; no SDK or model execution. The prepared controls test the
identity's assumptions, including a deliberate alternate-symbol counterexample.
"""
import base64
import json
import random
import sys
from pathlib import Path

import numpy as np


def observable(symbol):
    return 2 <= symbol <= 261 or symbol in (268, 276, 296)


MASK = np.float32(-np.finfo(np.float32).max)


def tensor(item):
    assert item['Type'] == 1
    values = np.frombuffer(base64.b64decode(item['Bytes'], validate=True), dtype='<f4')
    assert values.size == np.prod(item['Shape']) and np.isfinite(values).all()
    return values.reshape(item['Shape'])


def backward(primary, ancestry, raw_history, parent, segments):
    """Native uses time-indexed rank ancestry, not the final parent's path."""
    size = len(primary[parent])
    ancestor = ancestry[parent][size]
    result = np.empty((size, segments), dtype='<f4')
    for time in range(size - 1, -1, -1):
        assert 0 <= ancestor < raw_history[time].shape[1]
        result[time] = raw_history[time][:, ancestor, 0] if observable(primary[parent][time]) else MASK
        ancestor = ancestry[ancestor][time]
    assert ancestor == -1
    return result


def prepared():
    rng = random.Random(0x36F20)
    checks = nonobservable = permutations = shrinking = 0
    # Integer-valued, exactly representable F32 values make any wrong ancestor
    # visible. Vary source segments, history length, repeated parents and width.
    for trial in range(256):
        width = rng.randrange(1, 9)
        segments = rng.randrange(1, 18)
        primary = [[] for _ in range(width)]
        ancestry = [[-1] for _ in range(width)]
        active = 1
        carried = np.empty((segments, 1, 0), dtype='<f4')
        history = []
        for time in range(rng.randrange(2, 25)):
            for parent in range(active):
                expected = backward(primary, ancestry, history, parent, segments)
                assert expected.tobytes() == carried[:, parent, :].T.tobytes()
                checks += 1
            raw = np.arange(segments * active, dtype='<f4').reshape(segments, active, 1) + np.float32(10000 * time)
            count = rng.randrange(1, width + 1)
            parents = [rng.randrange(active) for _ in range(count)]
            labels = [rng.choice([0, 1, 2, 261, 262, 267, 268, 269, 276, 296]) for _ in range(count)]
            nonobservable += sum(not observable(x) for x in labels)
            permutations += int(parents != sorted(parents))
            shrinking += int(count < active)
            next_carried = np.take(np.concatenate((carried, raw), axis=2), parents, axis=1)
            for rank, label in enumerate(labels):
                if not observable(label):
                    next_carried[:, rank, -1] = MASK
            new_primary = [primary[p] + [label] for p, label in zip(parents, labels)]
            primary = new_primary + [[] for _ in range(width - count)]
            ancestry = [row + [parents[rank] if rank < count else -1] for rank, row in enumerate(ancestry)]
            history.append(raw.copy())
            carried, active = next_carried, count
        for parent in range(active):
            assert backward(primary, ancestry, history, parent, segments).tobytes() == carried[:, parent, :].T.tobytes()
            checks += 1
    # Primary can use raw48.alternate20 while EF1 masks raw48.symbol08.
    # If their observable status differs, the shortcut is NOT valid.
    raw = np.array([[[1.0]], [[2.0]]], dtype='<f4')
    mismatch = backward([[262]], [[-1, 0]], [raw], 0, 2)
    assert mismatch.tobytes() != raw[:, 0, :].T.tobytes()
    assert checks > 5000 and nonobservable > 1000 and permutations > 100 and shrinking > 100
    return dict(prefixes=checks, nonobservable=nonobservable, permutations=permutations, shrinking=shrinking,
                alternateObservableCounterexamples=1)


if __name__ == '__main__':
    if len(sys.argv) != 3:
        raise SystemExit('prefix-capture.json report.json')
    capture = json.loads(Path(sys.argv[1]).read_bytes())
    prefixes = completed = checked_bytes = 0
    kinds = {}
    for run in capture['Runs']:
        history = []
        for step in run['Steps']:
            old = step['PrefixBefore']
            previous = tensor(step['Before'][3])
            raw = tensor(step['Outputs'][1])
            assert previous.shape[2] == old['Offset'] == len(history)
            for parent in range(old['ActiveCount']):
                expected = backward(old['Primary'], old['Ancestry'], history, parent, previous.shape[0])
                assert expected.tobytes() == previous[:, parent, :].T.tobytes()
                prefixes += 1
                checked_bytes += expected.nbytes
            for event in step['CompletedScores']:
                parent = event['SourceBeam']
                assert event['Prefix'] == old['Primary'][parent]
                expected = backward(old['Primary'], old['Ancestry'], history, parent, previous.shape[0])
                assert expected.tobytes() == previous[:, parent, :].T.tobytes()
                completed += 1
            history.append(raw.copy())
        kinds[run['Kind']] = kinds.get(run['Kind'], 0) + 1
    assert prefixes == 34 and completed == 11
    report = dict(scope='Captured identity and prepared controls only; no runtime attention lease or EF4 output',
                  capturedPrefixes=prefixes, capturedEos=completed, checkedBytes=checked_bytes,
                  runs=kinds, prepared=prepared())
    Path(sys.argv[2]).write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
    print('PASS34 captured prefixes/11EOS and prepared ancestry identity; alternate-symbol limitation explicit', report['prepared'])
