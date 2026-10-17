"""Independent cold recording-loop replay; actual tensors, heap, EOS/fallback,
owner projections and exact counter boundary. No expected transcription, vendor
or ONNX Runtime execution, and no platform-wide capture SHA requirement.
"""
import ast
import base64
import copy
import hashlib
import json
import math
import struct
import sys
from pathlib import Path
import numpy as np

if len(sys.argv) != 3:
    raise SystemExit('capture.json rawMath2.res')
ROOT = Path(__file__).resolve().parents[2]
capture = json.loads(Path(sys.argv[1]).read_bytes())
assert capture['Runtime'] == '1.23.2'
assert capture['Resource'] == hashlib.sha256(Path(sys.argv[2]).read_bytes()).hexdigest() == \
    '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
f32 = lambda value: struct.unpack('<f', struct.pack('<f', value))[0]
FMAX = f32(3.4028234663852886e38)

# Reuse only independent pure coordinate/ordering functions, never a probe's
# entry point, fixed capture assertions or C# implementation.
scope = dict(np=np, math=math, struct=struct, base64=base64, verified_bytes=0, f32=f32, FMAX=FMAX)
for path, names in [
    ('LegacyMathTensorState.Probe', {'tensor', 'same', 'observable', 'verify_step', 'cost', 'sort48'}),
    ('LegacyMathPrefixState.Probe', {'argmax', 'next_generation', 'expand'}),
]:
    parsed = ast.parse((ROOT / 'tools' / path / 'audit_output.py').read_text())
    functions = [node for node in parsed.body if isinstance(node, ast.FunctionDef) and node.name in names]
    assert len(functions) == len(names)
    exec(compile(ast.Module(body=functions, type_ignores=[]), 'independent-existing-oracle', 'exec'), scope)
tensor, observable, verify_step, cost, sort48, argmax, next_generation, expand = [scope[name] for name in
    ['tensor', 'observable', 'verify_step', 'cost', 'sort48', 'argmax', 'next_generation', 'expand']]


def equal(actual, expected):
    if isinstance(expected, float):
        assert struct.pack('<f', actual) == struct.pack('<f', expected), (actual, expected)
    elif isinstance(expected, list):
        assert len(actual) == len(expected)
        for left, right in zip(actual, expected):
            equal(left, right)
    elif isinstance(expected, dict):
        assert actual.keys() == expected.keys(), (actual.keys(), expected.keys())
        for key in expected:
            equal(actual[key], expected[key])
    else:
        assert actual == expected, (actual, expected)


def common_frontier(secondary, logits, previous):
    mismatch = next((i for i in range(len(secondary[0]))
                     if any(struct.unpack('<h', struct.pack('<H', secondary[0][i]))[0] != row[i]
                            for row in secondary[1:])), None)
    value = mismatch if mismatch is not None else len(secondary[0]) - 1 if secondary[0] else previous
    maxima = []
    for row in logits:
        maxima.append(0 if math.isnan(float(row[0])) else max(
            (i for i, v in enumerate(row) if not math.isnan(float(v))), key=lambda i: (row[i], -i)))
    common = mismatch is None and len(set(maxima)) == 1
    return value + int(common), common, maxima


def fallback_ranks(rows, width, old):
    result = []
    for rank in range(width):
        value = cost(rows[rank])
        if value == FMAX:
            continue
        parent, = struct.unpack_from('<i', rows[rank], 40)
        assert 0 <= parent < len(old)
        if any(observable(value) for value in old[parent]):
            result.append(rank)
    return result


def record(row, rank, generation, frontier, identity, kind):
    parent, = struct.unpack_from('<i', row, 40)
    primary, secondary = generation['Primary'][parent], generation['Secondary'][parent]
    divisor = f32(f32(f32(len(primary) + 1) + 5.0) / 6.0)
    return dict(Score=f32(cost(row) / divisor), Frontier=frontier, Parent=parent,
                Primary=primary[:], PrimaryCapacity=len(primary), Secondary=secondary[:],
                SecondaryCapacity=len(secondary), LibraryIdentity=identity, LibraryQwords=[0] * 8,
                NestedTensorCount=0, Source=dict(Kind=kind, Rank=rank, Offset=generation['Offset'],
                    Raw48=base64.b64encode(row).decode(), Primary=primary[:], Secondary=secondary[:],
                    Ancestry=copy.deepcopy(generation['Ancestry'])))


assert len(capture['Runs']) == 10
ordinary_count = fallback_count = total_steps = 0
maximum_error = 0.0
for run in capture['Runs']:
    width, maximum = run['Width'], run['Maximum']
    assert 1 <= width <= 16 and run['InputFrames'] > 0 and run['EncoderMaximum'] >= 0
    bound = min(1000, run['InputFrames'] * 4 + 1)
    assert maximum == (min(bound, run['EncoderMaximum']) if run['EncoderMaximum'] else bound)
    assert len(run['Steps']) == run['DecoderCalls'] == maximum and run['Attempted'] == maximum + 1
    if run['ColdDefault']:
        assert width == 1 and run['Delta'] == -1
    generation = dict(Offset=0, ActiveCount=1, Primary=[[] for _ in range(width)],
                      Secondary=[[] for _ in range(width)], Ancestry=[[-1] for _ in range(width)])
    previous = run['Steps'][0]['Before']
    priors = np.zeros(1, dtype='<f4')
    recorded = []
    frontier = 0
    expected_iterations = []
    for offset, step in enumerate(run['Steps']):
        assert step['Position'] == offset + 1
        equal(step['PrefixBefore'], generation)
        equal(step['RecordsBefore'], recorded)
        batch = generation['ActiveCount']
        logits = tensor(step['Outputs'][0])[0]
        logs, costs = tensor(step['Logs']), tensor(step['Costs'])
        assert np.array_equal(tensor(step['Prior']), priors)
        for beam in range(batch):
            peak = float(np.max(logits[beam]))
            total = 0.0
            for value in logits[beam]:
                total = f32(total + f32(math.exp(f32(float(value) - peak))))
            normalizer = f32(f32(math.log(total)) + peak)
            expected_logs = np.array([f32(float(value) - normalizer) for value in logits[beam]], dtype='<f4')
            expected_costs = np.array([f32(-float(value) + float(priors[beam])) for value in expected_logs], dtype='<f4')
            for actual, expected in [(logs[beam], expected_logs), (costs[beam], expected_costs)]:
                error = float(np.max(np.abs(actual.astype(np.float64) - expected.astype(np.float64))))
                maximum_error = max(maximum_error, error)
                assert error <= 4e-6
        information = [argmax(row) for row in logs]
        assert base64.b64decode(step['Information64']) == struct.pack('<' + 'q' * batch, *information)
        assert base64.b64decode(step['Conditional32']) == b'\xff' * (4 * batch)
        frontier, common, maxima = common_frontier(generation['Secondary'][:batch], logits, frontier)
        equal([step['Frontier'], step['Common'], step['RawArgmax']], [frontier, common, maxima])
        masked = costs.copy(); masked[:, 0] = np.float32(FMAX)
        rows = expand(masked, width, information, [0xffffffff] * batch)
        assert b''.join(rows) == base64.b64decode(step['Heap'])
        sort48(rows, 0, len(rows) - 1)
        assert b''.join(rows) == base64.b64decode(step['Sorted'])
        if run['Delta'] < 1000:
            threshold = f32(cost(rows[0]) + run['Delta'])
            for rank in range(1, len(rows)):
                if cost(rows[rank]) >= threshold:
                    changed = bytearray(rows[rank]); struct.pack_into('<f', changed, 0, FMAX); rows[rank] = bytes(changed)
        eos = []
        for rank, row in enumerate(rows):
            if struct.unpack_from('<Q', row, 8)[0] != 1 or cost(row) == FMAX:
                continue
            parent, = struct.unpack_from('<i', row, 40)
            if rank < width and any(observable(value) for value in generation['Primary'][parent]):
                full = record(row, rank, generation, frontier, len(recorded) + 1, 'OrdinaryEos')
                recorded.append(full); ordinary_count += 1
                eos.append(dict(Rank=rank, SourceBeam=parent, CandidateCost=cost(row),
                                CompletedScore=full['Score'], Prefix=generation['Primary'][parent]))
            changed = bytearray(row); struct.pack_into('<f', changed, 0, FMAX); rows[rank] = bytes(changed)
        equal(step['CompletedScores'], eos)
        assert b''.join(rows) == base64.b64decode(step['AfterEos'])
        sort48(rows, 0, len(rows) - 1)
        assert b''.join(rows) == base64.b64decode(step['AfterResort'])
        if offset + 1 != maximum:
            outcome, fallback = 'NotLastPosition', []
        elif recorded:
            outcome, fallback = 'CompletedVectorNotEmpty', []
        else:
            outcome = 'LastPositionEmpty'
            fallback = fallback_ranks(rows, width, generation['Primary'][:batch])
            for rank in fallback:
                assert len(recorded) < width
                recorded.append(record(rows[rank], rank, generation, frontier, len(recorded) + 1, 'LastPositionFallback'))
                fallback_count += 1
        assert step['Fallback'] == outcome and step['FallbackRanks'] == fallback
        equal(step['RecordsAfter'], recorded)
        active = 1
        while active < width and cost(rows[active]) != FMAX:
            active += 1
        selected = b''.join(rows[:active])
        assert selected == base64.b64decode(step['Selected48'])
        equal(step['PrefixAfter'], next_generation(generation, selected))
        previous = verify_step(dict(Before=step['Before'], Outputs=step['Outputs'], After=step['After'],
            BeforeBeams=batch, AfterBeams=active, Offset=offset, Selected48=step['Selected48']), previous, True)
        expected_iterations.append(dict(Position=offset + 1, PreviousOffset=offset, PreviousBeams=batch,
            OrdinaryEosEvents=len(eos), Fallback=['NotLastPosition', 'CompletedVectorNotEmpty', 'LastPositionEmpty'].index(outcome),
            FallbackRanks=fallback, CompletedCount=len(recorded), Applied=True))
        priors = np.array([cost(row) for row in rows[:active]], dtype='<f4')
        generation = step['PrefixAfter']; total_steps += 1
    equal(run['Iterations'], expected_iterations)

assert len(capture['Failures']) == 6
reference = capture['Runs'][0]
for failure in capture['Failures']:
    assert failure['Position'] == failure['DecoderCalls'] == 3
    third = reference['Steps'][2]
    equal(failure['Before'], third['PrefixBefore'])
    after = third['PrefixAfter'] if failure['Stage'] == 'AfterApply' else third['PrefixBefore']
    equal(failure['After'], after)
    equal(failure['Records'], third['RecordsBefore'] if failure['Stage'] == 'BeforeRecording' else third['RecordsAfter'])
    count = 2 if failure['Stage'] == 'BeforeRecording' else 3
    expected = copy.deepcopy(reference['Iterations'][:count])
    if failure['Stage'] == 'AfterRecording':
        expected[-1]['Applied'] = False
    equal(failure['Iterations'], expected)

assert len(capture['Prepared']) == 1024
for control in capture['Prepared']:
    raw = base64.b64decode(control['Sorted48'])
    rows = [raw[index:index + 48] for index in range(0, len(raw), 48)]
    assert len(rows) == control['Width'] * 2
    assert fallback_ranks(rows, control['Width'], control['OldPrimary']) == control['EligibleRanks']
expected_guards = ['prepare-entry-cancel', 'prepared-width-zero', 'prepared-temperature-zero',
    'prepared-delta-nan', 'ordinary-phase-entry-cancel', 'ordinary-phase-replay',
    'ordinary-phase-foreign-candidate', 'fallback-foreign-phase', 'fallback-foreign-candidate',
    'fallback-foreign-vector', 'fallback-unissued-phase', 'fallback-entry-cancel',
    'fallback-repeat-prepare', 'fallback-empty-read-entry-cancel', 'ordered-phase-selection-identity',
    'unregistered-fallback-batch', 'forged-fallback-static-factory',
    'unregistered-fallback-content-callback', 'stale-fallback-phase', 'stale-fallback-pool',
    'stale-phase-replay', 'unordered-row-foreign-parent', 'eligible-row-foreign-parent', 'prepared-width-shape',
    'prepared-empty-prefix-table', 'prepared-entry-cancel']
assert capture['NamedGuards'] == expected_guards
assert capture['Guards'] >= 100
print(f'PASS {len(capture["Runs"])} actual recording runs/{total_steps} NN steps/'
      f'{scope["verified_bytes"]} exact tensor/cache bytes; {ordinary_count} EOS/{fallback_count} actual fallback records; '
      f'1024 prepared eligibility/6 outer failure controls/{capture["Guards"]} guards; '
      f'portable score maxError={maximum_error}; pre-ordering/pre-EF4, no accepted formula')
