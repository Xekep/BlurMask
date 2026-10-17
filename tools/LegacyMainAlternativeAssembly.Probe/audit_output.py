"""Independent scalar/whole-storage audit of the actual shared-TOP capture."""
import json, struct, sys
from pathlib import Path

def f32(v): return struct.unpack('<f', struct.pack('<f', v))[0]
controls = json.loads(Path(sys.argv[1]).read_text())
capture = json.loads(Path(sys.argv[2]).read_text())
assert len(controls) == len(capture['comparisons']) == 1024
for source, got in zip(controls, capture['comparisons']):
    expected = source['expected']
    assert got['result'] == (expected if isinstance(expected, str) else str(expected).lower())
    assert bytes.fromhex(got['left4']) == bytes(source['ea'])
    assert bytes.fromhex(got['right4']) == bytes(source['eb'])
initial = bytearray.fromhex(capture['allocation'])
row = bytearray(initial[:40])
row[:8] = bytes(8)             #portableVT placeholder
struct.pack_into('<fff', row, 8, 1., 0., 0.)
struct.pack_into('<f', row, 20, f32(f32(capture['row14']) * f32(1/8192)))
row[24:40] = bytes(16)        #typed WORD/beam identity placeholders
expected = row + initial[40:]
assert bytes.fromhex(capture['firstStorage']) == expected
assert capture['sourceNodes'] > 0
for replay in capture['actualSameTopReplays']:
    assert replay['shared275'] and replay['words'] > 0
    assert bytes.fromhex(replay['storage']) == row + row + initial[80:]
final = row + row + initial[80:]
struct.pack_into('<f', final, 12, 1.)  #singleton finalrow0C
assert bytes.fromhex(capture['finalStorage']) == final
assert capture['guards'] == 23
print('PASS1024 independent comparison/scratch results and2400 actual constructor/duplicate/final allocator bytes; no text or acceptance oracle.')
