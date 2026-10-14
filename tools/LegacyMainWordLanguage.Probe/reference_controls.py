"""Independent numerical and40-byte history-record oracle over an original model."""
import json
import random
import runpy
import struct
import sys
from pathlib import Path

destination = Path(sys.argv[1])
runpy.run_path(str(Path(__file__).resolve().parents[1] / 'LegacyWordScorer.Probe/reference_controls.py'))
data = json.loads(destination.read_text(encoding='utf-8'))
r = random.Random(0xFFEC0)
for c in data['cases']:
    residue = r.randbytes(40)
    output = bytearray(residue)
    limit = c['subtotal'] + r.choice([-1, 0, 1, 100])
    history = c['history']
    tag = history[0] >> 24 & 15 if history and not c['clear'] else 0
    prefix = 1 if c['flags'] and c['category'] and tag else 0
    next_tag = tag if c['suppress'] and prefix else 0 if c['suppress'] else c['category']
    struct.pack_into('<i', output, 0, c['subtotal'])
    struct.pack_into('<I', output, 4, len(history) + 1)
    output[8] = 1
    struct.pack_into('<I', output, 12, c['wordId'] * 7 % 5 | next_tag << 24)
    struct.pack_into('<I', output, 24, 0xFFFFFFFF)
    struct.pack_into('<I', output, 28, c['wordId'])
    c.update(residue=residue.hex(), lexicalLimit=limit, accepted=c['subtotal'] <= limit,
             expected=output.hex())
injections = []
for i in range(512):
    parent, residue = bytearray(r.randbytes(88)), r.randbytes(32)
    parent[5] = (parent[5] & ~2) | (2 if i & 1 else 0)
    parent[7] = (parent[7] & ~1) | (1 if i & 2 else 0)
    expected = bytearray(residue)
    expected[:8] = parent[:8]
    expected[8:20] = bytes(12)
    expected[20:22] = parent[36:38]
    expected[22:24] = bytes(2)
    injections.append(dict(parent=parent.hex(), residue=residue.hex(), expected=expected.hex(), accepted=bool(i & 1 and not i & 2)))
data['injections'] = injections
destination.write_text(json.dumps(data, separators=(',', ':')), encoding='utf-8')
print('Generated640 independent subtotal/full40 history outputs and512 cached-terminal query injections')
