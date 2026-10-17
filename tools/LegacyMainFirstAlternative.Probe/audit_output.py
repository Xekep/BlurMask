"""Original complete-allocation oracle for actual first alternative snapshots.
No runtime helper or expected transcription is used to calculate these bytes.
"""
import json
import struct
import sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('actual-capture.json')
o = json.loads(Path(sys.argv[1]).read_bytes())
assert o['nodes'] > 0 and o['winnerOrdinal'] >= 0 and len(bytes.fromhex(o['selectedBeam'])) == 88
before, after = bytes.fromhex(o['selectedBeam']), bytes.fromhex(o['publishedBeam'])
assert len(after) == 88
#207CB0 writes marking/index metadata, not a new cost/path/glyph owner.
for at in range(88):
    if at not in (6, 0x22, 0x23): assert before[at] == after[at]
assert len(o['cases']) == 64 and o['guards'] == 14
def f32(x): return struct.unpack('<f', struct.pack('<f', x))[0]
row14 = f32(f32(o['rowCost14']) * f32(1/8192))
count = 0
for c in o['cases']:
    residue = bytes.fromhex(c['residue'])
    assert len(residue) == 400 and c['reference'] == o['total']
    #Native13BD40 owns one array.141540 initializes only its first row.
    ctor = bytearray(residue)
    ctor[:40] = bytes(40)
    struct.pack_into('<I', ctor, 20, 0x7f7fffff)
    assert bytes.fromhex(c['constructed']) == ctor
    scored = bytearray(ctor)
    #Fresh first flags99/A8 establish reference equal to this total, hence
    #difference0, actual nonempty-source divisor, raw mass1 and path negative0?
    #SUB→CVTDQ2PS gives +0; DIV gives +0 independently of the exp log's -0.
    struct.pack_into('<f', scored, 8, 1.)
    struct.pack_into('<f', scored, 16, 0.)
    struct.pack_into('<f', scored, 20, row14)
    assert bytes.fromhex(c['scored']) == scored
    final = bytearray(scored)
    struct.pack_into('<f', final, 8, 1.)
    struct.pack_into('<f', final, 12, 1.)
    assert bytes.fromhex(c['normalized']) == final
    assert c['rawFirst08'] == 0x3f800000 and c['top80'] == 0xbf800000
    assert c['words'] >= 0
    count += len(ctor) + len(scored) + len(final)
print(f'PASS independent64 real-owner allocations/{count} bytes/14 guards; no native failure-order or text-accuracy claim.')
