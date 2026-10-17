"""Independent actual-event provenance/data-element accounting.

Optional third argument compares against the previous same-platform actual
initial-sort capture. No platform SHA, model/vendor execution or expected text.
"""
import base64
import hashlib
import json
from pathlib import Path
import struct
import sys

if len(sys.argv) not in (3, 4):
    raise SystemExit('capture.json rawMath2.res [same-platform-initial-sort-baseline.json]')
capture = json.loads(Path(sys.argv[1]).read_bytes())
assert capture['Runtime'] == '1.23.2'
assert capture['Resource'] == hashlib.sha256(Path(sys.argv[2]).read_bytes()).hexdigest() == \
    '2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a'
baseline = json.loads(Path(sys.argv[3]).read_bytes()) if len(sys.argv) == 4 else None
total_events = avoided = shared_steps = shared_events = matrices = time_history = 0
assert [run['PreparedWidth'] for run in capture['Runs']] == [1, 3, 16]
for index, run in enumerate(capture['Runs']):
    event_count = retained_bytes = 0
    generation_ids = set()
    flattened = []
    assert len(run['Steps']) == run['Maximum']
    for offset, step in enumerate(run['Steps']):
        assert step['Offset'] == offset and step['GenerationId'] == offset + 1
        assert step['Width'] == run['PreparedWidth']
        assert 1 <= step['Active'] <= step['Width']
        assert len(step['Ancestry']) == len(step['Primary']) == len(step['Secondary']) == step['Width']
        assert all(len(row) == offset + 1 for row in step['Ancestry'])
        assert all(len(row) == offset for row in step['Primary'][:step['Active']])
        assert all(len(row) == offset for row in step['Secondary'][:step['Active']])
        flattened_ints = [value for row in step['Ancestry'] for value in row]
        raw_ancestry = struct.pack('<' + 'i' * len(flattened_ints), *flattened_ints)
        ancestry_hash = hashlib.sha256(raw_ancestry).hexdigest()
        assert step['AncestryHash'] == ancestry_hash
        shape = step['AttentionShape']
        assert len(shape) == 3 and shape[0] > 0 and shape[1] == step['Active'] and shape[2] > 0
        time_history += shape[0] * shape[1] * 4  #one last-frame slice per Step
        if len(step['Events']) > 1:
            shared_steps += 1
            shared_events += len(step['Events'])
        for event in step['Events']:
            assert event['GenerationId'] == step['GenerationId']
            generation_ids.add(event['GenerationId'])
            raw = base64.b64decode(event['Raw48'], validate=True)
            assert len(raw) == 48 and struct.unpack_from('<Q', raw, 8)[0] == 1
            parent, = struct.unpack_from('<i', raw, 40)
            assert parent == event['Parent'] and 0 <= parent < step['Active']
            assert event['Primary'] == step['Primary'][parent]
            assert event['Secondary'] == step['Secondary'][parent]
            assert event['Ancestry'] == step['Ancestry'] and event['AncestryHash'] == ancestry_hash
            expected_bytes = len(raw_ancestry) + len(event['Primary']) * 2 + len(event['Secondary']) * 2
            assert event['AvoidedRetainedDataBytes'] == expected_bytes
            assert event['PotentialAttentionMatrixBytes'] == shape[0] * offset * 4
            retained_bytes += expected_bytes
            matrices += event['PotentialAttentionMatrixBytes']
            flattened.append((offset, event))
            event_count += 1
    assert run['Events'] == event_count and run['RetainedGenerationIdentities'] == len(generation_ids)
    assert run['AvoidedRetainedDataBytes'] == retained_bytes and run['DetachedReadsAfterDispose'] is True
    if baseline is not None:
        prior = baseline['Cases'][index]
        assert prior['PreparedWidth'] == run['PreparedWidth'] and len(prior['Before']) == len(flattened)
        for old, (offset, event) in zip(prior['Before'], flattened):
            assert old['SourceOffset'] == offset and old['SourceRank'] == event['Rank']
            assert old['SourceRaw48'] == event['Raw48'] and old['Parent'] == event['Parent']
            assert old['SourcePrimary'] == event['Primary'] and old['SourceSecondary'] == event['Secondary']
            assert old['SourceAncestry'] == event['Ancestry']
    total_events += event_count
    avoided += retained_bytes
assert shared_steps > 1 and shared_events > shared_steps
assert capture['Guards'] == 72
print(f'PASS{len(capture["Runs"])} actual runs/{total_events} EOS events; {shared_events} events share '
      f'{shared_steps} multi-event Step generations; {avoided} duplicated provenance data-element bytes avoided; '
      f'future per-event attention={matrices}B vs once-Step last-frame history={time_history}B '
      '(measurement only, neither stored automatically);72 guards')
