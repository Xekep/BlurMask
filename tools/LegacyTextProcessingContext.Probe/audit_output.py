"""Independent input DOM tree/prefix and C22->2DA208 proof, no model labels."""
import base64
import hashlib
import json
import struct
import sys
from pathlib import Path

capture = Path(sys.argv[1])
data = json.loads(capture.read_bytes())
assert len(data['actual']) == 4 and len(data['library']) == 128
# Native02D8C0's unconditional DOM.V18 pre-reset sequence; its literal/address
# values and ordering are checked separately against the pinned PE.
import_names = 'input conf textResult charSegmentList charSegment charCandidate external internal wordSegmentList wordSegment wordCandidate char text textCandidate word lines line transforms range tag line freeze forceSelected origin ref selected id x y a h b gen lang x-height'.split()
initial_names = ['textField', 'input', 'tag']
imported_names = list(dict.fromkeys(initial_names + import_names))
successes = failures = guards = 0
for actual, samples in [(True, data['actual']), (False, data['library'])]:
    for sample in samples:
        counts = sample['pointCounts']
        # Full endpoint fields use the actual raw point count in their packed
        # canonical endpoint constructors; no original raw ID becomes tag0.
        source = base64.b64decode(sample['sourceRanges'])
        first, last = struct.unpack('<QQ', source)
        assert first == (3 | (0xff00 if counts[0] == 1 else 0))
        assert last == ((len(counts)-1) << 32) | ((counts[-1]-1) << 16) | 0xff05
        attempts = sample['attempts']
        assert [a['stage'] for a in attempts] == ['success'] + (
            [f'dom{i}' for i in range(4)] + [f'prepare{i}' for i in range(2)] + [f'cancel{i}' for i in range(4)] if actual else [])
        guards += 2
        for attempt in attempts:
            stage = attempt['stage']; success = stage == 'success'
            dom_fail = int(stage[3:]) if stage.startswith('dom') else None
            prepared_stage = int(stage[7:]) if stage.startswith('prepare') else int(stage[6:]) if stage.startswith('cancel') else None
            imported = success or (prepared_stage is not None and prepared_stage >= 1)
            ranges_copied = success or (prepared_stage is not None and prepared_stage >= 2)
            assert attempt['succeeded'] == success and attempt['faulted'] != success
            assert attempt['sourceTag30'] == (0 if imported else -1)
            assert attempt['state18'] == int(success)
            assert attempt['count210'] == int(ranges_copied)
            assert attempt['textD8'] == attempt['strings3E'] == 0
            assert base64.b64decode(attempt['uuid38']) == bytes(16)
            assert base64.b64decode(attempt['ranges208']) == (source if ranges_copied else b'')
            assert attempt['boundRequest'] == attempt['boundInput'] == imported
            names = initial_names[:max(0, dom_fail)] if dom_fail is not None else imported_names if imported else initial_names
            assert attempt['names'] == names, (stage, attempt['names'])
            # Node creation is separate from name interning. Failed node kind3
            # interns its name, but never obtains an identity or parent link.
            node_count = dom_fail if dom_fail is not None else 4
            expected = []
            for i in range(node_count):
                kind = [4, 3, 3, 0][i]
                expected.append(dict(id=i+1, kind=kind, name=['', 'textField', 'input', 'tag'][i],
                    nameId=i if i else 0, tag=4 if i == 3 else 0,
                    value=base64.b64encode(struct.pack('<q', 0) if i == 3 else b'').decode(),
                    parent=[0, 1, 2, 3][i], released=False,
                    attributes=[4] if i == 2 and node_count == 4 else [],
                    children=[i+2] if i < 2 and i+1 < node_count else []))
            assert attempt['nodes'] == expected, stage
            dom_trace = []
            for i in range(4):
                kind = [4, 3, 3, 0][i]
                name = ['', 'textField', 'input', 'tag'][i]
                if name: dom_trace.append(f'name:{i}:{name}')
                dom_trace.append(f'try:node{kind}')
                if i == dom_fail:
                    dom_trace.append(f'fail:node{kind}')
                    break
                dom_trace.append(f'new:{i+1}:{kind}:{name}')
                if i in (1, 2): dom_trace.append(f'append:{i}:{i+1}')
                if i == 3: dom_trace += ['value:4:4:AAAAAAAAAAA=', 'attr:3:4']
            assert attempt['domTrace'] == dom_trace, (stage, attempt['domTrace'])
            trace = []
            if dom_fail is None and prepared_stage != 0:
                trace = ['snapshot2DB:create', 'result:reset', 'result:bind']
                if success or prepared_stage >= 2: trace += ['result:ranges']
                if success or prepared_stage == 3: trace += ['snapshot2DB:read-empty-text']
                if success: trace += ['context:flags=1']
                trace += ['snapshot2DB:release']
            if not success: trace += ['context:fault']
            assert attempt['preparationTrace'] == trace, (stage, attempt['preparationTrace'])
            assert attempt['releasedBindings']
            successes += success; failures += not success
            # Before-entry cancellation, replay and after-dispose accesses.
            guards += 4 + (3 if success else 0)
assert data['successes'] == successes == 132
assert data['failures'] == failures == 40
assert data['guards'] == guards
stable = hashlib.sha256(json.dumps(data, sort_keys=True, separators=(',', ':')).encode()).hexdigest()
Path(str(capture) + '.stable.sha256').write_text(stable + '\n', encoding='ascii')
print(f'PASS independent 132 imports / 40 prefix failures / {guards} guards; all DOM nodes, range bytes, names and release order; SHA={stable}')
