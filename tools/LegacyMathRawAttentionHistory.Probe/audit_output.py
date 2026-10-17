"""Independent raw-output/ancestry replay and retained-payload budget audit.
Never calls a C# reconstruction helper or executes a model/vendor library.
"""
import base64
import hashlib
import json
from pathlib import Path
import struct
import sys

import numpy as np

if len(sys.argv) != 3:
    raise SystemExit('capture.json rawMath2.res')
capture = json.loads(Path(sys.argv[1]).read_bytes())
assert capture['Runtime'] == '1.23.2'
assert capture['Resource'] == hashlib.sha256(Path(sys.argv[2]).read_bytes()).hexdigest() == \
    '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
MASK = np.float32(-np.finfo(np.float32).max)
BUDGET = 16 * 1024 * 1024


def observable(symbol):
    return 2 <= symbol <= 261 or symbol in (268, 276, 296)


def array(encoded):
    raw = base64.b64decode(encoded, validate=True)
    result = np.frombuffer(raw, dtype='<f4')
    assert np.isfinite(result).all()
    return result


def backward(primary, ancestry, parent, history):
    size = len(primary)
    segments = history[0].shape[0]
    ancestor = ancestry[parent][size]
    result = np.empty((size, segments), dtype='<f4')
    for time in range(size - 1, -1, -1):
        assert 0 <= ancestor < history[time].shape[1]
        result[time] = history[time][:, ancestor, 0] if observable(primary[time]) else MASK
        ancestor = ancestry[ancestor][time]
    assert ancestor == -1
    return result


def stats(value, frames, snapshots, raw_bytes, matrix_bytes, allocations, reserved,
          complete=True, failed=False, disposed=False):
    assert value['ReservedPayloadBytes'] == reserved
    assert value['RawPayloadBytes'] == raw_bytes and value['SnapshotPayloadBytes'] == matrix_bytes
    assert value['RawFrameCount'] == len(frames) and value['SnapshotCount'] == snapshots
    assert value['PayloadAllocationAttempts'] == allocations
    assert value['Complete'] is complete and value['Failed'] is failed and value['Disposed'] is disposed
    assert value['RawFrameHashes'] == [hashlib.sha256(frame.tobytes()).hexdigest() for frame in frames]
    assert reserved <= value['BudgetBytes']


assert [case['PreparedWidth'] for case in capture['Actual']] == [1, 3, 16]
total_matrices = total_bytes = 0
histories = []
for case in capture['Actual']:
    history = []
    for frame in case['Frames']:
        assert 1 <= frame['Segments'] <= 6000 and 1 <= frame['Beams'] <= case['PreparedWidth']
        raw = array(frame['Values'])
        assert raw.size == frame['Segments'] * frame['Beams']
        history.append(raw.reshape(frame['Segments'], frame['Beams'], 1))
    assert len({frame.shape[0] for frame in history}) == 1
    raw_bytes = sum(frame.nbytes for frame in history)
    before, after = case['Before'], case['After']
    assert before['BudgetBytes'] == after['BudgetBytes'] == BUDGET
    stats(before, history, 0, raw_bytes, 0, len(history), raw_bytes)
    expected_matrix_bytes = 0
    for matrix in case['Matrices']:
        assert matrix['Offset'] == matrix['Symbols'] == len(matrix['Primary'])
        assert 0 < matrix['Symbols'] < len(history)  #exclude current EOS frame
        assert matrix['Segments'] == history[0].shape[0]
        expected = backward(matrix['Primary'], matrix['Ancestry'], matrix['Parent'], history)
        assert array(matrix['Historical']).tobytes() == expected.tobytes()
        assert array(matrix['CarriedBeforeApply']).tobytes() == expected.tobytes()
        expected_matrix_bytes += expected.nbytes
    count = len(case['Matrices'])
    assert sorted(case['SortedSourceOrder']) == list(range(count))
    assert case['InitiallySorted'] is (case['PreparedWidth'] != 1)
    stats(after, history, count, raw_bytes, expected_matrix_bytes,
          len(history) + count, raw_bytes + expected_matrix_bytes)
    stats(case['Disposed'], [], 0, 0, 0, len(history) + count,
          raw_bytes + expected_matrix_bytes, disposed=True)
    total_matrices += count
    total_bytes += expected_matrix_bytes
    histories.append(history)

failures = capture['Failures']
assert [case['Kind'] for case in failures] == ['RawBudget'] * 3 + \
    ['SnapshotBudget', 'SnapshotCancel', 'RecordingFault', 'RecordingCancel']
for index, case in enumerate(failures[:3]):
    assert case['Budget'] == [0, 51, 52][index] == case['Statistics']['BudgetBytes']
    frames = [] if index < 2 else histories[1][:1]
    byte_count = sum(frame.nbytes for frame in frames)
    stats(case['Statistics'], frames, 0, byte_count, 0, len(frames), byte_count, complete=False, failed=True)
    assert case['DecoderCalls'] == len(frames) + 1

case = failures[3]
ordered = capture['Actual'][1]
first, second = [ordered['Matrices'][index] for index in ordered['SortedSourceOrder'][:2]]
matrix_bytes = first['Symbols'] * first['Segments'] * 4
raw_bytes = sum(frame.nbytes for frame in histories[1])
assert case['Budget'] == raw_bytes + matrix_bytes + second['Symbols'] * second['Segments'] * 4 - 1
stats(case['Statistics'], histories[1], 1, raw_bytes, matrix_bytes,
      len(histories[1]) + 1, raw_bytes + matrix_bytes, failed=True)
assert case['DecoderCalls'] == len(histories[1])
case = failures[4]
valid = capture['Actual'][0]['After']
changed = dict(valid, Failed=True)
assert case['Statistics'] == changed and case['DecoderCalls'] == len(histories[0])
for case in failures[5:]:
    assert case['SourceOffset'] == capture['Actual'][0]['Matrices'][0]['Offset']
    prefix = histories[0][:case['SourceOffset'] + 1]
    raw_bytes = sum(frame.nbytes for frame in prefix)
    stats(case['Statistics'], prefix, 0, raw_bytes, 0, len(prefix), raw_bytes, complete=False, failed=True)
    assert case['DecoderCalls'] == len(prefix)

repeats = shrinking = permutations = nonobservable = prepared_matrices = counterexamples = 0
assert len(capture['Prepared']) == 128
for index, case in enumerate(capture['Prepared']):
    assert case['Control'] == index and 1 <= case['Width'] <= 8 and 1 <= case['Segments'] <= 7
    width, segments = case['Width'], case['Segments']
    primary = [[] for _ in range(width)]
    ancestry = [[-1] for _ in range(width)]
    carried = np.empty((segments, 1, 0), dtype='<f4')
    active = 1
    history = []
    assert len(case['Frames']) == len(case['Plans']) == len(case['Beams'])
    for time, plan in enumerate(case['Plans']):
        assert case['Beams'][time] == active
        raw = array(case['Frames'][time]).reshape(segments, active, 1)
        assert raw.tobytes() == (np.arange(segments * active, dtype='<f4') + np.float32(time * 10000)).tobytes()
        parents, labels = plan['Parents'], plan['Labels']
        assert len(parents) == len(labels) and 1 <= len(parents) <= width
        assert all(0 <= parent < active for parent in parents)
        repeats += int(len(set(parents)) != len(parents))
        shrinking += int(len(parents) < active)
        permutations += int(parents != sorted(parents))
        nonobservable += sum(not observable(label) for label in labels)
        carried = np.take(np.concatenate((carried, raw), axis=2), parents, axis=1)
        for rank, label in enumerate(labels):
            if not observable(label):
                carried[:, rank, -1] = MASK
        primary = [primary[parent] + [label] for parent, label in zip(parents, labels)] + \
            [[] for _ in range(width - len(parents))]
        ancestry = [row + [parents[rank] if rank < len(parents) else -1] for rank, row in enumerate(ancestry)]
        active = len(parents)
        history.append(raw)
    assert primary == case['Primary'] and ancestry == case['Ancestry']
    assert len(case['Results']) == active
    for parent, encoded in enumerate(case['Results']):
        expected = backward(primary[parent], ancestry, parent, history)
        assert array(encoded).tobytes() == expected.tobytes() == carried[:, parent, :].T.tobytes()
        prepared_matrices += 1
    # A fixed current-parent shortcut loses time-varying ancestors.
    fixed = np.stack([frame[:, 0, 0] if observable(primary[0][time]) else np.full(segments, MASK)
                      for time, frame in enumerate(history)])
    counterexamples += int(fixed.tobytes() != carried[:, 0, :].T.tobytes())
assert repeats > 0 and shrinking > 0 and permutations > 0 and nonobservable > 0 and counterexamples > 0
assert capture['Guards'] == total_matrices + 62
print(f'PASS{total_matrices} actual historical matrices/{total_bytes} exact bytes from captured raw outputs '
      f'and ancestry + carried reference;7 failure/budget prefixes;128 prepared histories/{prepared_matrices} matrices; '
      f'repeat={repeats}/shrink={shrinking}/permutation={permutations}/nonobs={nonobservable}; '
      f'{capture["Guards"]} guards; no EF4/output claim')
