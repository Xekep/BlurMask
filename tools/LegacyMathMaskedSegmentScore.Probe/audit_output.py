"""Independent finite041CF0 prepared masked numeric oracle; standard library only.
The supplied matrix is compressed, not an actual EF4/request publication.
"""
import base64
from decimal import Decimal, localcontext
import json
from pathlib import Path
import struct
import sys

if len(sys.argv) != 2:
    raise SystemExit('capture.json')
capture = json.loads(Path(sys.argv[1]).read_bytes())
def f32(x):
    return struct.unpack('<f', struct.pack('<f', x))[0]
def bits(x):
    return struct.pack('<f', x)
floor = struct.unpack('<f', bytes.fromhex('ffe6db2e'))[0]
logs = {}
with localcontext() as ctx:
    ctx.prec = 75
    for value in [0.0, -0.0, 1e-12, 1e-10, struct.unpack('<f', struct.pack('<I', 0x2edbe700))[0],
                  .125, .5, struct.unpack('<f', struct.pack('<I', 0x3f7fffff))[0], 1.0]:
        value = f32(value)
        logs[bits(value)] = f32(float(Decimal.from_float(max(value, floor)).ln()))
no_deletion = all_deleted = mixed = skipped_denominator = remap_discriminators = 0
max_error = 0.0
assert len(capture['Rows']) == 4096
seen_alpha = set()
for row in capture['Rows']:
    mask = base64.b64decode(row['Mask'], validate=True)
    kept = [i for i, deleted in enumerate(mask) if not deleted]
    segments = row['Segments']
    assert len(row['Probabilities']) == len(kept) * segments
    probabilities = list(map(f32, row['Probabilities']))
    assignments = row['Assignments']
    remap = [sum(j < i for j in kept) for i in range(len(mask))]
    assert row['Remap'] == remap
    valid = [(segment, original) for segment, original in enumerate(assignments) if original != -1]
    contributing = [(segment, kept.index(original)) for segment, original in valid if original in kept]
    assert row['Assigned'] == len(valid) and row['Contributing'] == len(contributing)
    penalty = 0.0
    for segment, compressed in contributing:
        penalty = f32(penalty - logs[bits(probabilities[compressed * segments + segment])])
    penalty = f32(penalty / len(valid)) if valid else 0.0
    actual_penalty = f32(row['Penalty'])
    error = abs(actual_penalty - penalty)
    max_error = max(max_error, error)
    # Portable MathF.Log CPU implementations may differ in last bits. F32
    # rounding, denominator and separate final multiply/add are still checked.
    assert error <= 2e-6, (error, row)
    before, alpha = f32(row['Before']), f32(row['Alpha'])
    after = f32(f32(actual_penalty * alpha) + before)
    assert bits(after) == bits(row['After'])
    assert list(map(bits, row['Diagnostics'])) == [bits(before), bits(actual_penalty)]
    seen_alpha.add(bits(alpha))
    no_deletion += len(kept) == len(mask)
    all_deleted += not kept
    mixed += 0 < len(kept) < len(mask)
    skipped_denominator += bool(contributing) and len(contributing) < len(valid) and actual_penalty > 0
    remap_discriminators += any(original != compressed for (_, original), (_, compressed) in
        zip([(s, o) for s, o in valid if o in kept], contributing))
assert no_deletion == capture['NoDeletionControls'] and no_deletion > 500
assert all_deleted > 500 and mixed > 2000 and skipped_denominator > 1000 and remap_discriminators > 1000
assert len(seen_alpha) == 7 and capture['Guards'] == 23
print(f'PASS4096 Decimal75 masked scores/{no_deletion} unchanged fresh controls/'
      f'{all_deleted} all-deleted/{mixed} mixed; {skipped_denominator} denominator and '
      f'{remap_discriminators} compressed-row discriminators; max portable log error={max_error};23 guards')
