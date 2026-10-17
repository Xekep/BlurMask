"""Independent actual-logit/frontier/037430 semantic-record audit.
Reads captured official ORT results; never executes a vendor DLL or ORT.
"""
import base64
import copy
import hashlib
import json
import math
import struct
import subprocess
import sys
from pathlib import Path

if len(sys.argv) != 3:
    raise SystemExit('prefix-capture.json rawMath2.res')
ROOT = Path(__file__).resolve().parents[2]
capture = Path(sys.argv[1])
subprocess.run([sys.executable, str(ROOT / 'tools/LegacyMathPrefixState.Probe/audit_output.py'),
                str(capture), sys.argv[2]], check=True)
assert hashlib.sha256(capture.read_bytes()).hexdigest() == \
    'd350e2ca63956bc524936c40c60664c173130d4c1216c1850ed930c3ca5b3d08'
source = json.loads(capture.read_bytes())
actual = json.loads(Path(str(capture) + '.records.json').read_bytes())
assert actual['Guards'] == 327
f32 = lambda value: struct.unpack('<f', struct.pack('<f', value))[0]
FMAX = f32(3.4028234663852886e38)


def same(actual_value, expected, path=''):
    if isinstance(expected, float):
        assert struct.pack('<f', actual_value) == struct.pack('<f', expected), (path, actual_value, expected)
    elif isinstance(expected, dict):
        assert actual_value.keys() == expected.keys(), (path, actual_value.keys(), expected.keys())
        for key in expected:
            same(actual_value[key], expected[key], path + '/' + key)
    elif isinstance(expected, list):
        assert len(actual_value) == len(expected), path
        for index, item in enumerate(expected):
            same(actual_value[index], item, path + '/' + str(index))
    else:
        assert actual_value == expected, (path, actual_value, expected)


def tensor_argmax(values):
    # Independent first-element order statistic. NaN first has no ordered
    # strictly greater successor. Later NaNs never replace the current best.
    if math.isnan(values[0]):
        return 0
    ordered = [(v, -i, i) for i, v in enumerate(values) if not math.isnan(v)]
    return max(ordered)[2]


def frontier(secondary, logits, vocabulary, previous):
    mismatch = next((index for index in range(len(secondary[0]))
                     if any(struct.unpack('<h', struct.pack('<H', secondary[0][index]))[0] != row[index]
                            for row in secondary[1:])), None)
    value = mismatch if mismatch is not None else len(secondary[0]) - 1 if secondary[0] else previous
    maxima = [tensor_argmax(logits[i * vocabulary:(i + 1) * vocabulary])
              for i in range(len(secondary))]
    common = mismatch is None and len(set(maxima)) == 1
    return value + int(common), common, maxima


def empty_state():
    return dict(Count=0, Capacity=0, Failed=False, CreatedLibraries=0,
                ReleasedUnadoptedLibraries=0, Records=[])


def empty_record(identity):
    return dict(Score=0.0, Frontier=0, Parent=-1, Primary=[], PrimaryCapacity=0,
                Secondary=[], SecondaryCapacity=0, LibraryIdentity=identity, LibraryDisposed=False,
                LibraryObjectBytes=24, LibraryPoolBytes=72, LibraryInitializedQwords=[0] * 8,
                LibraryInitializedDword10=0, NestedTensorCount=0)


def normalized_cost(raw48, count):
    cost, = struct.unpack_from('<f', raw48)
    divisor = f32(f32(f32(count + 1) + 5.0) / 6.0)
    return f32(cost / divisor)


def completed_record(raw48, primary, secondary, common_frontier, identity):
    record = empty_record(identity)
    record.update(Score=normalized_cost(raw48, len(primary)), Frontier=common_frontier,
                  Parent=struct.unpack_from('<i', raw48, 40)[0], Primary=primary[:],
                  PrimaryCapacity=len(primary), Secondary=secondary[:], SecondaryCapacity=len(secondary))
    return record


steps = iter(actual['Steps'])
record_count = 0
failure_source = None
for run in source['Runs']:
    if run['Kind'] != 'actual-logits/native-prepared-candidate-kernels':
        continue
    state = empty_state()
    saved_frontier = 0
    for step in run['Steps']:
        observed = next(steps)
        old = step['PrefixBefore']
        output = step['Outputs'][0]
        raw_logits = base64.b64decode(output['Bytes'])
        logits = struct.unpack('<' + 'f' * (len(raw_logits) // 4), raw_logits)
        assert output['Shape'] == [1, old['ActiveCount'], 297]
        value, common, maxima = frontier(old['Secondary'][:old['ActiveCount']], logits, 297, saved_frontier)
        saved_frontier = value
        same([observed['Width'], observed['Delta'], observed['Offset']],
             [run['Width'], run['Delta'], old['Offset']])
        same([observed['Frontier'], observed['Common'], observed['RawArgmax']], [value, common, maxima])
        same(observed['Before'], state)
        # Reconstruct pruning from complete pre-EOS sorted storage, not runtime
        # completed records or numerical score facts used as expected output.
        pruned = bytearray(base64.b64decode(step['Sorted']))
        delta = run['Delta']
        if delta < 1000.0:
            threshold = f32(struct.unpack_from('<f', pruned)[0] + delta)
            for rank in range(1, 2 * run['Width']):
                if struct.unpack_from('<f', pruned, rank * 48)[0] >= threshold:
                    struct.pack_into('<f', pruned, rank * 48, FMAX)
        expected_ranks = [fact['Rank'] for fact in step['CompletedScores']]
        assert [event['Rank'] for event in observed['Events']] == expected_ranks
        for event in observed['Events']:
            rank = event['Rank']
            raw48 = bytes(pruned[rank * 48:(rank + 1) * 48])
            assert base64.b64decode(event['Raw48']) == raw48
            parent, = struct.unpack_from('<i', raw48, 40)
            assert struct.unpack_from('<Q', raw48, 8)[0] == 1
            same(event['Primary'], old['Primary'][parent])
            same(event['Secondary'], old['Secondary'][parent])
            same(event['Ancestry'], old['Ancestry'])
            assert event['Frontier'] == value and event['Offset'] == old['Offset']
            state['Count'] += 1
            state['Capacity'] = state['Count']
            state['CreatedLibraries'] += 1
            state['Records'].append(completed_record(raw48, event['Primary'], event['Secondary'], value, state['Count']))
            same(event['After'], state)
            record_count += 1
            if run['Width'] == 3 and delta == 1000 and old['Offset'] == 2 and rank == expected_ranks[0]:
                failure_source = (raw48, event['Primary'], event['Secondary'], value)
        same(observed['After'], state)
assert next(steps, None) is None and record_count == 11

# Native success/failure boundaries are interpreted independently as semantic
# state transitions. No physical allocation, pointer bytes or uninitialized
# padding are used as expected results.
assert failure_source is not None
raw48, primary, secondary, value = failure_source
expected_failure_states = []
state = empty_state()


def observe(name, index=-1):
    expected_failure_states.append(dict(Stage=name, Index=index, State=copy.deepcopy(state)))


observe('BeforeVectorGrowth')
state['Capacity'] = 1
observe('AfterVectorGrowth')
observe('BeforeLibraryBase')
observe('BeforeLibraryPool')
state['CreatedLibraries'] = 1
observe('BeforeLibraryAdoption')
state['Count'] = 1
state['Records'] = [empty_record(1)]
observe('AfterCountIncrement')
record = state['Records'][0]
record['Parent'] = struct.unpack_from('<i', raw48, 40)[0]
record['Frontier'] = value
observe('AfterHeaderWrites')
record['Score'] = normalized_cost(raw48, len(primary))
observe('AfterScoreWrite')
record['PrimaryCapacity'] = len(primary)
observe('AfterPrimaryReserve')
for index, item in enumerate(primary):
    record['Primary'].append(item)
    observe('AfterPrimaryElement', index)
observe('BeforeSecondaryReserve')
record['SecondaryCapacity'] = len(secondary)
observe('AfterSecondaryReserve')
record['Secondary'] = secondary[:]
observe('AfterSecondaryCopy')
observe('ContentCleanup')
assert len(actual['Failures']) == 28
for failure in actual['Failures']:
    assert base64.b64decode(failure['Raw48']) == raw48
    same([failure['Primary'], failure['Secondary'], failure['Frontier']], [primary, secondary, value])
    stop = next(i for i, event in enumerate(expected_failure_states) if event['Stage'] == failure['Stage'])
    same(failure['Events'], expected_failure_states[:stop + 1])
    retained = copy.deepcopy(expected_failure_states[stop]['State'])
    retained['Failed'] = True
    if not retained['Count'] and (retained['CreatedLibraries'] or failure['Stage'] == 'BeforeLibraryPool'):
        retained['ReleasedUnadoptedLibraries'] = 1
    same(failure['After'], retained)

assert len(actual['PreparedFrontiers']) == 512
for control in actual['PreparedFrontiers']:
    bits = base64.b64decode(control['LogitsF32'])
    logits = struct.unpack('<' + 'f' * (len(bits) // 4), bits)
    expected = frontier(control['Secondary'], logits, control['Vocabulary'], control['Previous'])
    same([control['Frontier'], control['Common'], control['Argmax']], list(expected))
print(f'PASS {record_count} actual owned EOS records; 28 logical failure/cancel prefixes; '
      f'512 frontier controls/{actual["Guards"]} guards; old full prefix capture unchanged; '
      'no ABI pointers, completed fallback, EF4 output or search acceptance claimed')
