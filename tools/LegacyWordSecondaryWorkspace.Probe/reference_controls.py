"""Independent persistent 800-byte incoming writer, including stale unavailable fields."""
import json
import random
import struct
import sys
from pathlib import Path

rng = random.Random(0x210019)
streams = []
unavailable = 0
for stream_index in range(256):
    count = rng.choice([2, 3, 20])
    state = bytearray(800)
    updates = []
    for iteration in range(4):
        source = bytearray(rng.randbytes(count*40))
        arc = rng.randrange(65536)
        cost_offset = rng.choice([rng.randrange(-10000,10000), -2147483648, 2147483647])
        increment_offset = rng.choice([rng.randrange(-10000,10000), -2147483648, 2147483647])
        for index in range(count):
            at = index*40
            cost = rng.choice([rng.randrange(-10000,10000), 0x4000000, 0x4000001, -2147483648, 2147483647])
            struct.pack_into("<i", source, at, cost)
            if cost >= 0x4000000:
                struct.pack_into("<i", state, at, 0x4000000)
                unavailable += 1
            else:
                incremental = struct.unpack_from("<i",source,at+4)[0]
                for offset, value in [(0,cost+cost_offset),(4,incremental+increment_offset)]:
                    struct.pack_into("<I", state, at+offset, value&0xffffffff)
                state[at+30] = 0
                struct.pack_into("<H",state,at+24,arc)
                state[at+27] = (state[at+27]&128) | index
                state[at+8:at+16] = source[at+8:at+16]
        updates.append(dict(source=source.hex(),arc=arc,costOffset=cost_offset,incrementOffset=increment_offset,expected=state.hex()))
    streams.append(dict(count=count,updates=updates))
Path(sys.argv[1]).write_text(json.dumps(dict(streams=streams,unavailable=unavailable),separators=(",",":")),encoding="utf-8")
print(f"generated {len(streams)} persistent workspace streams / 1024 updates / {unavailable} cost-only unavailable writes")
