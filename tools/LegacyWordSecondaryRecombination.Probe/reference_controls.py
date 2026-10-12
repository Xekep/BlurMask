"""Independent two-stream scalar selection and literal reverse field writer."""
import json
import random
import struct
import sys
from pathlib import Path

rng = random.Random(0x1f87c0)
IMP = 0x4000000

def cost(row):
    return struct.unpack_from("<i", row)[0]

def key(row):
    return struct.unpack_from("<Q", row, 8)[0]

def merge(left, right, count, snapshot=False):
    output = bytearray(left)
    sources = [left, right]
    rows = [[data[index:index+40] for index in range(0, len(data), 40)] for data in sources]
    first = 1 if cost(rows[1][0]) <= cost(rows[0][0]) else 0
    chosen = [(first, 0)]
    contexts = [key(rows[first][0])]
    heads = [1-first, first]
    while len(chosen) < count:
        candidates = []
        for source in range(2):
            index = heads[source]
            if index < count and cost(rows[source][index]) < IMP:
                candidates.append((cost(rows[source][index]), -source, index))
        if not candidates:
            break
        _, neg_source, index = min(candidates)
        source = -neg_source
        heads[source] += 1
        identity = key(rows[source][index])
        if identity not in contexts:
            contexts.append(identity)
            chosen.append((source, index))
    for destination in range(len(chosen)-1, -1, -1):
        source, index = chosen[destination]
        source_data = sources[source] if source or snapshot else output
        begin = index*40
        at = destination*40
        # Five literal writes: raw context/state/residue never copied.
        for offset, size in [(0, 4), (4, 4), (30, 1), (24, 4), (8, 8)]:
            output[at+offset:at+offset+size] = source_data[begin+offset:begin+offset+size]
    return bytes(output), len(chosen)

cases = []
live_alias_differences = 0
for index in range(1024):
    count = rng.choice([2, 3, 4, 20])
    left, right = bytearray(rng.randbytes((count+2)*40)), bytearray(rng.randbytes((count+1)*40))
    for data in (left, right):
        costs = [rng.choice([rng.randrange(-50000, 50000), IMP, IMP+1, 2147483647, -2147483648]) for _ in range(count)]
        if index % 4:
            costs.sort()
        for slot, value in enumerate(costs):
            struct.pack_into("<iQ", data, slot*40, value, rng.getrandbits(64))
            struct.pack_into("<Q", data, slot*40+8, rng.randrange(8))
    if index == 0:
        count = 4
        left, right = bytearray(rng.randbytes(240)), bytearray(rng.randbytes(200))
        for slot, (value, identity) in enumerate(zip([0, 1, 2, 3], [0, 0, 1, 2])):
            struct.pack_into("<i", left, slot*40, value)
            struct.pack_into("<Q", left, slot*40+8, identity)
        for slot in range(4):
            struct.pack_into("<i", right, slot*40, 4+slot)
            struct.pack_into("<Q", right, slot*40+8, 3+slot)
    expected, written = merge(left, right, count)
    snapshot, _ = merge(left, right, count, True)
    live_alias_differences += expected != snapshot
    cases.append(dict(count=count, left=left.hex(), right=right.hex(), expected=expected.hex(), written=written,
                      aliasSensitive=expected != snapshot))
assert live_alias_differences > 0
Path(sys.argv[1]).write_text(json.dumps(dict(cases=cases, liveAliasDifferences=live_alias_differences), separators=(",", ":")), encoding="utf-8")
print(f"generated {len(cases)} complete secondary blocks, {live_alias_differences} live-alias differences from snapshot copying")
