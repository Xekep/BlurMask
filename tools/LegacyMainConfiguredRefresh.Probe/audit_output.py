"""Independent compact-to-pool byte composition and finite-score selection.

Scores are the separately audited owning scoring-stage outputs, not an
independent neural-network implementation in this composition auditor.
"""
import json
import math
import struct
import sys
from pathlib import Path

d = json.loads(Path(sys.argv[1]).read_bytes())
u16 = lambda b, o: struct.unpack_from('<H', b, o)[0]
i32 = lambda b, o: struct.unpack_from('<i', b, o)[0]
u64 = lambda b, o: struct.unpack_from('<Q', b, o)[0]
f32 = lambda b, o: struct.unpack_from('<f', b, o)[0]
owners = visited = added = compared = higher = 0
for sample in d['inputs']:
    compact = bytes.fromhex(sample['compact'])
    assert len(compact) % 40 == 0
    for owner in sample['owners']:
        owners += 1
        higher += int(owner['preparedHigher'])
        before = bytes.fromhex(owner['before'])
        after = bytes.fromhex(owner['after'])
        assert len(before) % 40 == len(after) % 40 == 0
        rows = [bytearray(before[o:o+40]) for o in range(0, len(before), 40)]
        base = owner['base']; node = base & 0xffffff; arc = (base >> 24) & 65535
        first, end = sample['starts'][node:node+2]
        matches = [index for index in range(first, end)
                   if compact[index*40+32] & 0x20 and compact[index*40+32] & 7 == 0
                   and u64(compact, index*40+8) == base]
        assert matches == [score['compact'] for score in owner['scores']]
        assert len(matches) == owner['visited']
        visited += len(matches)
        for index, score in zip(matches, owner['scores']):
            part = compact[index*40:(index+1)*40]
            descriptor = index-first
            assert descriptor == score['descriptor']
            assert score['firstScoreBits'] == owner['sourceScoreBits']
            assert u16(part, 28) == score['symbol'], 'This actual fixture has ordinary unchanged scalar labels only'
            found = next((n for n, row in enumerate(rows) if i32(row, 0) == descriptor), None)
            if owner['beforePacked'] & 0x8000:
                found = None
            if found is None:
                found = len(rows); row = bytearray.fromhex(owner['residue'])
                struct.pack_into('<i', row, 0, descriptor)
                struct.pack_into('<H', row, 4, u16(row, 4) & 0xfff0)
                struct.pack_into('<I', row, 6, 0xffff0000)
                struct.pack_into('<f', row, 12, 1)
                struct.pack_into('<f', row, 16, -1)
                struct.pack_into('<i', row, 20, 0)
                row[24:32] = bytes(8); row[32:40] = bytes([255])*8
                rows.append(row); added += 1
            row = rows[found]
            struct.pack_into('<H', row, 4, u16(row, 4) & 0xfff8)
            struct.pack_into('<H', row, 8, score['symbol'])
            struct.pack_into('<I', row, 16, score['symbolScoreBits'])
            struct.pack_into('<i', row, 20, score['cost'])
            struct.pack_into('<f', row, 12, part[33]/255)
            struct.pack_into('<H', row, 4, (u16(row, 4) & 0xfff7) | (8 if part[32] & 0x40 else 0))
            request = bytes.fromhex(score['request'])
            assert len(request) == 16 and i32(request, 0) == node
            assert u16(request, 4) == arc and u16(request, 6) == 65535
            assert u16(request, 8) == score['symbol'] and request[11:] == bytes(5)
        assert b''.join(rows) == after
        assert owner['afterPacked'] == (len(rows) << 2) | (owner['beforePacked'] & 0x8000)
        assert len(bytes.fromhex(owner['source26'])) > 0 and len(bytes.fromhex(owner['source26'])) % 26 == 0
        compared += len(after)
        # An independent finite-score formulation: choose the earliest maximum
        # for each identity, then order by score and winning insertion ordinal.
        winners = {}
        identity = lambda row: (u16(row, 4) & 7, u16(row, 8), u16(row, 6) & 0xff00)
        for ordinal, row in enumerate(rows):
            value = f32(row, 12); assert math.isfinite(value)
            key = identity(row)
            if key not in winners or value > f32(rows[winners[key]], 12):
                winners[key] = ordinal
        order = sorted(winners.values(), key=lambda n: (-f32(rows[n], 12), n))
        selections = bytes.fromhex(owner['selection'])
        assert list(struct.unpack('<'+'i'*(len(selections)//4), selections)) == order
        assert owner['marker'] == order.index(winners[identity(rows[owner['inputMarker']])])
assert len(d['inputs']) == 2
assert owners == d['refreshed'] == d['selectionFailures'] == 2 and higher == 0
assert visited == d['visited'] and visited == 4 and added == d['added'] and added == 2
assert d['guards'] == 15
print(f'PASS independent {owners} actual owner compositions/{visited} compact rows/{added} added records/{compared} pool bytes; finite alternative order and marker. Scoring math audited separately.')
