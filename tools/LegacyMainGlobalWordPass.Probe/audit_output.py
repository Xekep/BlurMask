"""Independent raw descriptor guard and raw LK audit of global logical rows.

The capture is an inference diagnostic, not a decoded/accepted text fixture.
The separately owned procedural C# oracle checks complete scheduling/storage;
this audit independently checks line ownership and raw resource LM arithmetic.
"""
import base64
import hashlib
import json
import struct
import subprocess
import sys
import tempfile
from pathlib import Path

if len(sys.argv) != 5:
    raise SystemExit('AK LK case-preparation-controls capture')
ak_path, lk_path, classes_path, capture_path = map(Path, sys.argv[1:])
assert hashlib.sha256(ak_path.read_bytes()).hexdigest() == '7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
assert hashlib.sha256(lk_path.read_bytes()).hexdigest() == 'd26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
captures = json.loads(capture_path.read_bytes())
assert len(captures) == 8
converted = []
node_count = groups = guards = allowed = language_rows = 0
for owner, capture in enumerate(captures):
    assert capture['owner'] == owner and capture['status'] == 'completed'
    descriptors = {row['ordinal']: base64.b64decode(row['raw']) for row in capture['descriptors']}
    assert len(descriptors) == capture['descriptorCount'] and all(len(d) == 88 for d in descriptors.values())
    arcs = capture['arcs']
    assert len(arcs) == capture['nodes'] == len(capture['rows'])
    allocation_descriptors = capture['groupDescriptors']
    live = {group for row in arcs for group in row}
    assert all(0 <= group < len(allocation_descriptors) for group in live)
    assert all(allocation_descriptors[group] in descriptors for group in live)
    # The actual owner pool evaluates each live allocation once; an ordinal is
    # never substituted for a raw document or physical source identity.
    assert len(live) == capture['acousticEvaluations']
    def line(node, arc):
        group = arcs[node][arc]
        return struct.unpack_from('<i', descriptors[allocation_descriptors[group]], 16)[0]
    for current, previous, arc, kind, actual in capture['markedGuards']:
        assert kind in (0, 2) and 0 <= previous < current < len(arcs)
        prior_line = line(previous, arc)
        next_line = line(previous + 1, 0)
        expected = prior_line != -1 and next_line != prior_line and next_line - prior_line == 1
        assert actual == expected, (owner, current, previous, arc, kind, prior_line, next_line)
        guards += 1
        allowed += actual
    nodes = []
    for index, row in enumerate(capture['rows']):
        assert row['node'] == index
        records = bytes.fromhex(row['records'])
        alternatives = bytes.fromhex(row['alternatives'])
        primary = capture['primaryLimit']
        assert primary <= row['retainedEnd'] <= row['sourceEnd'] <= len(records) // 88
        assert len(records) % 88 == 0 and len(alternatives) == primary * 80
        language_rows += row['sourceEnd'] - primary
        nodes.append(dict(node=index, records=base64.b64encode(records).decode(),
                          alternatives=base64.b64encode(alternatives).decode(),
                          retainedEnd=row['retainedEnd'], sourceEnd=row['sourceEnd']))
    converted.append(dict(primary=capture['primaryLimit'], nodes=nodes))
    node_count += len(arcs)
    groups += len(live)
assert (node_count, groups, guards, allowed) == (110, 631, 1180, 40)
assert language_rows > 0
# Reuse the independently decoded packed dictionary/LK arithmetic audit, with
# a temporary capture only. Resource and golden files are never output paths.
with tempfile.TemporaryDirectory(prefix='global-word-lk-', dir=capture_path.resolve().parent) as directory:
    transformed = Path(directory) / 'raw-lk-input.json'
    transformed.write_text(json.dumps(converted), encoding='utf-8')
    audit = Path(__file__).resolve().parents[1] / 'LegacyCaseMainPass.Probe' / 'audit_output.py'
    subprocess.run([sys.executable, str(audit), str(ak_path), str(lk_path), str(classes_path), str(transformed)], check=True)
print(f'PASS {len(captures)} actual global owners/{node_count} nodes/{groups} owned NN groups; '
      f'{guards} independent descriptor-line decisions/{allowed} allowed; raw LK/fulltag arithmetic. '
      'No later-history scheduler, publication or OCR quality claim.')
