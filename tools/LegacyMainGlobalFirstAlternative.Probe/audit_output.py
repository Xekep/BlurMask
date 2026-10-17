"""Independent native first273 row bytes on actual global callback owners."""
import base64
import json
import struct
import sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('capture.json')
o = json.loads(Path(sys.argv[1]).read_bytes())
assert o['ownerCount'] == 8 and len(o['owners']) == 8
assert o['snapshots'] == 512 and o['guards'] == 120

def raw(x):
    return base64.b64decode(x, validate=True)

def f32(x):
    return struct.unpack('<f', struct.pack('<f', x))[0]

verified = 0
for index, owner in enumerate(o['owners']):
    assert owner['owner'] == index and owner['nodes'] > 0
    assert owner['winnerOrdinal'] >= 0 and owner['callbacks'] == 2
    assert owner['words'] == 1
    before = raw(owner['selectedBeam'])
    assert len(before) == 88 and len(owner['cases']) == 64
    row_cost = f32(f32(owner['rowCost14']) * f32(1/8192))
    for seed, case in enumerate(owner['cases']):
        assert case['seed'] == seed and case['reference'] == owner['total']
        residue = raw(case['residue'])
        assert residue == bytes((i*17+seed*29+7) & 255 for i in range(400))
        constructed = bytearray(residue)
        constructed[:40] = bytes(40)
        struct.pack_into('<I', constructed, 20, 0x7f7fffff)
        assert raw(case['constructed']) == constructed
        scored = bytearray(constructed)
        #The first result establishes reference=total. Native integer SUB
        #therefore yields positive0 and exp(0) yields mass1. A source-count
        #divisor does not change zero; row14 remains its own distinct channel.
        struct.pack_into('<f', scored, 8, 1.)
        struct.pack_into('<f', scored, 16, 0.)
        struct.pack_into('<f', scored, 20, row_cost)
        assert raw(case['scored']) == scored
        normalized = bytearray(scored)
        struct.pack_into('<f', normalized, 8, 1.)
        struct.pack_into('<f', normalized, 12, 1.)
        assert raw(case['normalized']) == normalized
        assert case['rawFirst08'] == 0x3f800000
        assert case['top80'] & 0xffffffff == 0xbf800000
        after = raw(case['publishedBeam'])
        assert len(after) == 88
        #207CB0 only mutates the reached bit and compact index in these rows.
        assert all(before[i] == after[i] for i in range(88) if i not in (6, 0x22, 0x23))
        verified += len(constructed)+len(scored)+len(normalized)
print(f'PASS independent8 actual global callback owners/512 allocations/{verified} bytes/120 guards; no text-accuracy claim')
