"""Independent prefix/argmax/heap replay over official-ORT captures.
No vendor or ONNX Runtime execution. Reuses the existing independent tensor
coordinate oracle, not runtime C# implementation or expected transcription.
"""
import ast
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
    raise SystemExit('capture.json rawMath2.res')
ROOT = Path(__file__).resolve().parents[2]
r = json.loads(Path(sys.argv[1]).read_bytes())
raw = Path(sys.argv[2]).read_bytes()
assert hashlib.sha256(raw).hexdigest() == r['Resource'] == '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
_, blobs = runpy.run_path(str(ROOT / 'tools/export-ink-math-model.py'))['parse'](raw)
assert [hashlib.sha256(x).hexdigest() for x in blobs] == r['Models']
assert r['Runtime'] == '1.23.2' and r['Guards'] == 118
budgets = json.loads(Path(sys.argv[1] + '.bounds.json').read_bytes())
assert len(budgets) >= 3
for budget in budgets:
    n, k = budget['OriginalInputFrames'], budget['EncoderMaximumPredictionCount']
    assert n == r['Frames'] and 1 <= n <= 6000 and k >= 0
    base = min(1000, 4 * n + 1)
    assert budget['ColdMaximumPosition'] == (min(base, k) if k else base)

# Extract only pure independent oracle functions. The old probe's command-line
# entry and fixed capture assertions do not run or get weakened.
old = ast.parse((ROOT / 'tools/LegacyMathTensorState.Probe/audit_output.py').read_text())
functions = {'tensor', 'same', 'observable', 'verify_step', 'cost', 'sort48'}
module = ast.Module(body=[node for node in old.body
                         if isinstance(node, ast.FunctionDef) and node.name in functions], type_ignores=[])
scope = dict(np=np, math=math, struct=struct, base64=base64, verified_bytes=0)
exec(compile(module, 'existing-independent-tensor-oracle', 'exec'), scope)
tensor, verify_step, cost, sort48 = [scope[name] for name in ('tensor', 'verify_step', 'cost', 'sort48')]
f32 = lambda value: struct.unpack('<f', struct.pack('<f', value))[0]
FMAX = f32(3.4028234663852886e38)


def argmax(values):
    eligible = [(float(value), i) for i, value in enumerate(values)
                if not math.isnan(float(value)) and float(value) > -FMAX]
    return -1 if not eligible else max(eligible, key=lambda item: (item[0], -item[1]))[1]


def next_generation(before, raw):
    count = len(raw) // 48
    width = len(before['Primary'])
    primary, secondary, ancestry = [], [], []
    for rank in range(width):
        parent = -1
        if rank < count:
            row = raw[rank * 48:(rank + 1) * 48]
            parent = struct.unpack_from('<i', row, 40)[0]
            assert 0 <= parent < before['ActiveCount']
            alternate, = struct.unpack_from('<Q', row, 32)
            label = struct.unpack_from('<Q', row, 8)[0] if alternate == (1 << 64) - 1 else alternate
            primary.append(before['Primary'][parent] + [label & 0xffff])
            secondary.append(before['Secondary'][parent] + [struct.unpack_from('<H', row, 16)[0]])
        else:
            primary.append([]); secondary.append([])
        ancestry.append(before['Ancestry'][rank] + [parent])
    return dict(Offset=before['Offset'] + 1, ActiveCount=count,
                Primary=primary, Secondary=secondary, Ancestry=ancestry)


def expand(costs, width, information, conditional):
    rows, capacity = [], 2 * width
    for beam, values in enumerate(costs):
        for symbol, value in enumerate(values):
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
            row = bytearray(48)
            struct.pack_into('<f', row, 0, value)
            struct.pack_into('<Q', row, 8, symbol)
            struct.pack_into('<qI', row, 16, information[beam], conditional[beam])
            struct.pack_into('<Q', row, 32, (1 << 64) - 1)
            struct.pack_into('<i', row, 40, beam)
            rows[hole] = bytes(row)
    assert len(rows) == capacity
    return rows


steps = 0
maximum_score_error = 0.0
completed_facts = 0
for run in r['Runs']:
    previous = run['Initial']
    generation = run['InitialGeneration']
    width = run['Width']
    assert generation == dict(Offset=0, ActiveCount=1,
        Primary=[[] for _ in range(width)], Secondary=[[] for _ in range(width)],
        Ancestry=[[-1] for _ in range(width)])
    assert tensor(previous[0]).tolist() == [[0]] and tensor(previous[1]).tolist() == [0]
    assert tensor(previous[3]).shape[-1] == 0
    priors = np.zeros(1, dtype='<f4')
    for step in run['Steps']:
        assert step['PrefixBefore'] == generation
        b = generation['ActiveCount']; logs = tensor(step['Logs']); costs = tensor(step['Costs'])
        assert np.array_equal(tensor(step['Prior']), priors)
        logits = tensor(step['Outputs'][0])[0]
        expected_logs, expected_costs = np.empty_like(logs), np.empty_like(costs)
        for beam in range(b):
            maximum = float(np.max(logits[beam])); total = 0.0
            for value in logits[beam]:
                total = f32(total + f32(math.exp(f32(float(value) - maximum))))
            normalizer = f32(f32(math.log(total)) + maximum)
            for symbol, value in enumerate(logits[beam]):
                log = f32(float(value) - normalizer)
                expected_logs[beam, symbol] = log
                expected_costs[beam, symbol] = f32(-log + float(priors[beam]))
        for actual, expected in [(logs, expected_logs), (costs, expected_costs)]:
            error = float(np.max(np.abs(actual.astype(np.float64) - expected.astype(np.float64))))
            maximum_score_error = max(maximum_score_error, error)
            assert error <= 0.000004
        information = [argmax(row) for row in logs]
        assert base64.b64decode(step['Information64']) == struct.pack('<' + 'q' * b, *information)
        assert base64.b64decode(step['Conditional32']) == b'\xff' * (4 * b)
        selected = base64.b64decode(step['Selected48'])
        for rank in range(len(selected) // 48):
            parent = struct.unpack_from('<i', selected, rank * 48 + 40)[0]
            assert struct.unpack_from('<qI', selected, rank * 48 + 16) == (information[parent], 0xffffffff)
            assert struct.unpack_from('<Q', selected, rank * 48 + 32)[0] == (1 << 64) - 1
        if not step['Diagnostic']:
            masked = costs.copy(); masked[:, 0] = np.float32(FMAX)
            rows = expand(masked, width, information, [0xffffffff] * b)
            assert b''.join(rows) == base64.b64decode(step['Heap'])
            sort48(rows, 0, len(rows) - 1)
            assert b''.join(rows) == base64.b64decode(step['Sorted'])
            if run['Delta'] < 1000:
                threshold = f32(cost(rows[0]) + run['Delta'])
                for rank in range(1, len(rows)):
                    if cost(rows[rank]) >= threshold:
                        row = bytearray(rows[rank]); struct.pack_into('<f', row, 0, FMAX); rows[rank] = bytes(row)
            facts = []
            for rank, row in enumerate(rows):
                if struct.unpack_from('<Q', row, 8)[0] != 1 or cost(row) == FMAX:
                    continue
                parent = struct.unpack_from('<i', row, 40)[0]
                prefix = generation['Primary'][parent]
                if rank < width and any(scope['observable'](value) for value in prefix):
                    #With default alpha1, powf(x,1) returns x in this domain.
                    divisor = f32(f32(f32(float(len(prefix) + 1)) + 5.0) / 6.0)
                    facts.append(dict(Rank=rank, SourceBeam=parent, CandidateCost=cost(row),
                                      CompletedScore=f32(cost(row) / divisor), Prefix=prefix))
                changed = bytearray(row); struct.pack_into('<f', changed, 0, FMAX); rows[rank] = bytes(changed)
            assert len(facts) == len(step['CompletedScores'])
            for expected, actual in zip(facts, step['CompletedScores']):
                for key in ('Rank', 'SourceBeam', 'Prefix'):
                    assert actual[key] == expected[key]
                for key in ('CandidateCost', 'CompletedScore'):
                    assert struct.pack('<f', actual[key]) == struct.pack('<f', expected[key])
            completed_facts += len(facts)
            assert b''.join(rows) == base64.b64decode(step['AfterEos'])
            sort48(rows, 0, len(rows) - 1)
            assert b''.join(rows) == base64.b64decode(step['AfterResort'])
            active = 1
            while active < width and cost(rows[active]) != FMAX:
                active += 1
            assert selected == b''.join(rows[:active])
        assert step['PrefixAfter'] == next_generation(generation, selected)
        numeric = dict(Before=step['Before'], Outputs=step['Outputs'], After=step['After'],
            BeforeBeams=b, AfterBeams=len(selected) // 48, Offset=generation['Offset'], Selected48=step['Selected48'])
        previous = verify_step(numeric, previous, True)
        priors = np.array([cost(selected[i:i + 48]) for i in range(0, len(selected), 48)], dtype='<f4')
        generation = step['PrefixAfter']; steps += 1
assert steps == 20
for failure in r['Failures']:
    assert failure['After'] == next_generation(failure['Before'], base64.b64decode(failure['Selected48']))
assert len(r['Failures']) == 2 and [x['Canceled'] for x in r['Failures']] == [False, True]
for row in r['PreparedPlanes']:
    assert row['After'] == next_generation(row['Before'], base64.b64decode(row['Selected48']))
for index, row in enumerate(r['PreparedArgmax']):
    values = np.frombuffer(base64.b64decode(row['ValuesF32']), dtype='<f4')
    assert len(values) == 1 + index % 297
    if index % 8 == 0:
        assert (values == -FMAX).all()
    elif index % 8 == 1:
        assert np.isnan(values).all()
    elif index % 8 == 2:
        assert np.isneginf(values).all()
    elif index % 8 == 3:
        assert (values == 0).all()
    elif index % 8 == 4:
        assert np.isposinf(values[-1])
    assert argmax(values) == row['Index']
assert len(r['PreparedPlanes']) == 4 and len(r['PreparedArgmax']) == 320
print(f'PASS{steps} actual decoder steps/{scope["verified_bytes"]} exact tensor/cache bytes; '
      f'actual D8 argmax+conditional, full heap/EOS, {completed_facts} completed-score facts; '
      f'prefix/ancestry/failure generations and320 prepared argmax; max portable error{maximum_score_error}; '
      f'{r["Guards"]} guards; no vendor execution')
