"""Independent NumPy coordinate replay over actual official-ORT captures.
No SDK/MLOrt execution; this script reads data and does no model inference.
Portable exp/log is checked with float32 scalar rounding and tolerance, not
asserted to match the SDK CPU dispatch. All cache/gather bytes must be exact.
"""
import base64
import hashlib
import json
import math
import runpy
import struct
import sys
from pathlib import Path
import numpy as np

if len(sys.argv) != 3:
    raise SystemExit('capture.json raw-math2.res')
r = json.loads(Path(sys.argv[1]).read_bytes())
root = Path(__file__).resolve().parents[2]
resource = Path(sys.argv[2]).read_bytes()
assert hashlib.sha256(resource).hexdigest() == r['Resource'] == '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
_, blobs = runpy.run_path(str(root / 'tools/export-ink-math-model.py'))['parse'](resource)
assert [hashlib.sha256(x).hexdigest() for x in blobs] == r['Models']
assert r['Runtime'] == '1.23.2' and r['Frames'] > 0 and r['Guards'] == 105
assert [x['PreparedForce'] for x in r['Runs']] == [False, True]

def tensor(t):
    assert t['Type'] in (1, 7)
    raw = base64.b64decode(t['Bytes'], validate=True)
    dims = t['Shape']
    assert all(isinstance(x, int) and 0 <= x <= 100000 for x in dims)
    a = np.frombuffer(raw, dtype='<f4' if t['Type'] == 1 else '<i8')
    assert a.size == math.prod(dims)
    if t['Type'] == 1:
        assert np.isfinite(a).all()
    return a.reshape(dims)

verified_bytes = 0

def same(expected, capture):
    global verified_bytes
    actual = tensor(capture)
    assert actual.shape == expected.shape
    assert actual.dtype == expected.dtype
    assert actual.tobytes() == expected.tobytes(), capture['Name']
    verified_bytes += actual.nbytes

def observable(symbol):
    return 2 <= symbol <= 261 or symbol in (268, 276, 296)

def verify_step(step, previous, force):
    before = step['Before']; output = step['Outputs']; after = step['After']
    assert len(before) == len(after) == 9 and len(output) == 4
    for actual, prior in zip(before, previous):
        assert actual == prior
    b = step['BeforeBeams']; n = step['AfterBeams']; t = step['Offset']
    x = [tensor(item) for item in before]; y = [tensor(item) for item in output]
    records = base64.b64decode(step['Selected48'], validate=True)
    assert len(records) == n * 48 and 1 <= n <= 16
    parents = np.array([struct.unpack_from('<i', records, i * 48 + 40)[0] for i in range(n)])
    labels = np.array([struct.unpack_from('<Q', records, i * 48 + 8)[0] for i in range(n)], dtype='<i8')
    assert ((0 <= parents) & (parents < b)).all() and ((0 <= labels) & (labels < 297)).all()
    z = np.array([i if i < b else 0 for i in range(n)])
    assert x[0].shape == (1, b) and x[1].tolist() == [t]
    assert y[0].shape == (1, b, 297)
    assert y[1].shape == (x[2].shape[0], b, 1)
    same(labels.reshape(1, n), after[0])
    same(np.array([t + 1], dtype='<i8'), after[1])
    same(np.take(x[2], z, axis=1), after[2])
    attention = np.take(np.concatenate((x[3], y[1]), axis=2), parents, axis=1)
    if force:
        for i, label in enumerate(labels):
            if not observable(label):
                attention[:, i, -1] = np.float32(-np.finfo(np.float32).max)
    same(attention, after[3])
    same(np.take(x[4], z, axis=0), after[4])
    for slot, result in [(5, y[2]), (6, y[3]), (7, x[7]), (8, x[8])]:
        assert result.shape[0] == 2 and result.shape[1] == b * 4 and result.shape[3] == 64
        expanded = result.reshape(2, b, 4, result.shape[2], 64)
        chosen = np.take(expanded, parents if slot < 7 else z, axis=1)
        same(chosen.reshape(2, n * 4, result.shape[2], 64), after[slot])
    return after

for run in r['Runs']:
    encoded = [tensor(x) for x in run['Encoder']]
    initial = run['Initial']; init = [tensor(x) for x in initial]
    assert len(encoded) == 5
    assert init[0].tolist() == [[0]] and init[1].tolist() == [0]
    assert init[3].shape == (encoded[2].shape[0], 1, 0)
    assert init[5].shape == init[6].shape == (2, 4, 0, 64)
    assert init[7].shape == init[8].shape == (2, 4, encoded[0].shape[0], 64)
    assert run['MaximumPredictionCount'] == int(encoded[4][0])
    same(encoded[2], initial[2])
    previous = initial
    assert len(run['Steps']) == 5
    assert [s['BeforeBeams'] for s in run['Steps']] == [1, 3, 3, 2, 2]
    assert [s['AfterBeams'] for s in run['Steps']] == [3, 3, 2, 2, 1]
    for step in run['Steps']:
        previous = verify_step(step, previous, run['PreparedForce'])
#Prepared runtime gather controls expose static-map differences even when the
#actual static encoder payload is numerically identical across duplicated beams.
assert len(r['PreparedGathers']) == 5
for row in r['PreparedGathers']:
    same(np.take(tensor(row['Source']), row['Indices'], axis=row['Axis']), row['Output'])

f32 = lambda value: struct.unpack('<f', struct.pack('<f', value))[0]
maximum_score_error = 0.0

def cost(row):
    return struct.unpack_from('<f', row)[0]

def sort48(rows, lo, hi):
    while True:
        i, j = lo, hi
        pivot = cost(rows[lo + (hi - lo) // 2])
        while i <= j:
            while cost(rows[i]) < pivot:
                i += 1
            while pivot < cost(rows[j]):
                j -= 1
            if j < i:
                break
            rows[i], rows[j] = rows[j], rows[i]
            i += 1; j -= 1
        if lo < j:
            sort48(rows, lo, j)
        lo = i
        if i >= hi:
            return

def expand(costs, width):
    rows = []; capacity = width * 2
    for beam, row in enumerate(costs):
        for symbol, value in enumerate(row):
            value = float(value)
            if len(rows) == capacity:
                if value >= cost(rows[0]):
                    continue
                hole = 0
                while 2 * hole < len(rows) - 1:
                    child = 2 * hole + 1
                    if child < len(rows) - 1 and cost(rows[child + 1]) >= cost(rows[child]):
                        child += 1
                    if value >= cost(rows[child]):
                        break
                    rows[hole] = rows[child]; hole = child
            else:
                hole = len(rows); rows.append(bytes(48))
                while hole and not cost(rows[(hole - 1) // 2]) >= value:
                    rows[hole] = rows[(hole - 1) // 2]; hole = (hole - 1) // 2
            result = bytearray(48)
            struct.pack_into('<fQ', result, 0, value, 0)
            struct.pack_into('<Q', result, 8, symbol)
            struct.pack_into('<Q', result, 32, (1 << 64) - 1)
            struct.pack_into('<i', result, 40, beam)
            rows[hole] = bytes(result)
    assert len(rows) == capacity
    return rows

previous = r['PortableInitial']
priors = np.zeros(1, dtype='<f4')
assert len(r['PortableSteps']) == 4
for portable in r['PortableSteps']:
    step = portable['Step']; b = step['BeforeBeams']
    logits = tensor(step['Outputs'][0])[0]
    same(priors, portable['Prior'])
    expected_logs = np.empty((b, 297), dtype='<f4')
    expected_costs = np.empty_like(expected_logs)
    for beam in range(b):
        maximum = float(np.max(logits[beam])); total = 0.0
        for x in logits[beam]:
            total = f32(total + f32(math.exp(f32(float(x) - maximum))))
        normalizer = f32(f32(math.log(total)) + maximum)
        for i, x in enumerate(logits[beam]):
            log = f32(float(x) - normalizer)
            expected_logs[beam, i] = log
            expected_costs[beam, i] = f32(-log + float(priors[beam]))
    actual_logs = tensor(portable['Logs']); actual_costs = tensor(portable['Costs'])
    for a, e in [(actual_logs, expected_logs), (actual_costs, expected_costs)]:
        error = float(np.max(np.abs(a.astype(np.float64) - e.astype(np.float64))))
        maximum_score_error = max(maximum_score_error, error)
        assert error <= 0.000004
    #Feed exact captured portable scores to the independent literal heap,
    #then verify every byte of its record transport/ordering/EOS transitions.
    masked = actual_costs.copy(); masked[:, 0] = np.finfo(np.float32).max
    rows = expand(masked, 3)
    assert b''.join(rows) == base64.b64decode(portable['Heap'], validate=True)
    sort48(rows, 0, len(rows) - 1)
    assert b''.join(rows) == base64.b64decode(portable['Sorted'], validate=True)
    for i, row in enumerate(rows):
        if struct.unpack_from('<Q', row, 8)[0] == 1:
            changed = bytearray(row); struct.pack_into('<I', changed, 0, 0x7f7fffff); rows[i] = bytes(changed)
    assert b''.join(rows) == base64.b64decode(portable['AfterEos'], validate=True)
    sort48(rows, 0, len(rows) - 1)
    assert b''.join(rows) == base64.b64decode(portable['AfterResort'], validate=True)
    active = 1
    while active < 3 and cost(rows[active]) != np.finfo(np.float32).max:
        active += 1
    assert step['AfterBeams'] == active
    assert base64.b64decode(step['Selected48'], validate=True) == b''.join(rows[:active])
    previous = verify_step(step, previous, True)
    priors = np.array([cost(row) for row in rows[:active]], dtype='<f4')
print(f'PASS14 actual decoder steps/{verified_bytes} exact input/cache bytes +5 prepared gathers; '
      f'4 actual-logit portable-score/heap/EOS handoffs, max portable error{maximum_score_error}; '
      f'{r["Guards"]} lifecycle/ownership guards; no SDK/MLOrt execution')
