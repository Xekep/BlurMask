"""Independent original213C00 full80 and callback/heap field projection; output argument only."""
import json
import random
import struct
import sys
from pathlib import Path

resource = (Path(__file__).resolve().parents[2] / 'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
p = 188222
version, base = struct.unpack_from('<HH', resource, p)
assert version == 0x201 and base == 192
p += 4
count = resource[p]
p = (p + 4) & ~3
first_ids = struct.unpack_from('<' + 'i' * count, resource, p)
p += 4 * count
first = struct.unpack_from('<' + 'b' * (base + 3), resource, p)
p += base + 3
count = resource[p]
p = (p + 4) & ~3
p += 4 * count
start = struct.unpack_from('<' + 'b' * (base + 3), resource, p)
p += base + 3
end = struct.unpack_from('<' + 'b' * (base + 3), resource, p)
zero = first_ids.index(0)
composites = []
for a, b, c in zip(first, start, end):
    for key in [(a, b), (a, c), (zero, b), (zero, c)]:
        if min(key) >= 0 and key not in composites:
            composites.append(key)
assert len(composites) == 35 and start[base] == 9

rng = random.Random(0x213c00)
SOFT, DASH, CELL = 183, 185, 0x1122334455667788
def u16(b, p): return struct.unpack_from('<H', b, p)[0]
def u64(b, p): return struct.unpack_from('<Q', b, p)[0]
def i32(b, p): return struct.unpack_from('<i', b, p)[0]
def w16(b, p, v): struct.pack_into('<H', b, p, v & 65535)
def w32(b, p, v): struct.pack_into('<I', b, p, v & 0xffffffff)
def w64(b, p, v): struct.pack_into('<Q', b, p, v & 0xffffffffffffffff)
def packet(current, span, previous, parent, header, residue):
    b = bytearray(residue)
    w32(b, 0, current); w32(b, 4, u16(parent, 24)); w32(b, 8, previous)
    w16(b, 14, struct.unpack_from('<I', parent, 24)[0] >> 22)
    w16(b, 16, u16(parent, 28)); b[18] = start[DASH] & 255
    b[19] = composites[parent[26] & 63][1]
    w32(b, 20, 3); w32(b, 24, 0); b[28] = 0; w32(b, 32, span)
    w16(b, 36, SOFT); w16(b, 38, u16(parent, 36)); w64(b, 40, CELL)
    b[56] = 0; w32(b, 64, header >> 9 & 15); w32(b, 68, header >> 9 & 15)
    b[72] = 0; b[73] = parent[7] & 1
    return b

packets = []
for index in range(1024):
    current, span = 16 + index % 128, index % 15
    previous = current - span - 1
    parent, residue = bytearray(rng.randbytes(88)), rng.randbytes(80)
    w32(parent, 24, rng.randrange(previous + 1) | rng.randrange(35) << 16 | rng.randrange(1024) << 22)
    header = rng.getrandbits(32)
    packets.append(dict(current=current, span=span, previous=previous, parent=parent.hex(),
                        residue=residue.hex(), header=header, expected=packet(current, span, previous, parent, header, residue).hex()))

streams = []
for index in range(256):
    current, span = 5 + index % 20, index % 3
    previous = current - span - 1
    parent = bytearray(rng.randbytes(88)); parent[4] = 1
    w32(parent, 16, rng.randrange(-1000, 1001)); w32(parent, 20, rng.randrange(-1000, 1001))
    w32(parent, 24, rng.randrange(previous + 1) | rng.randrange(35) << 16 | rng.randrange(1024) << 22)
    w32(parent, 56, 0); w16(parent, 60, 0)
    residue = rng.randbytes(80); header = [0xc0a, 0x80a, 0xa0a, 0x20a][index % 4]
    special = bool(index & 4); qualifies = index % 5 != 0; marker = bool(index & 8)
    eligible = header & 0x1e00 in (0x800, 0xc00, 0xe00, 0x1000) or header & 0x1e00 == 0xa00 and special
    arcs = [[0, 1, 2, 3, 7, 3], [3, 7, 11, 3], [], [3, 2, 1, 0, 15]][index % 4]
    capacity = index % 5
    initial = [rng.randbytes(128) for _ in range(capacity)]; storage = initial[:]; count = 0
    traces, accepted, gcalls, acalls, value = [], 0, 0, 0, 73
    b = bytearray(residue)
    def log(stage): traces.append([stage, b.hex(), value])
    if eligible and qualifies:
        b = packet(current, span, previous, parent, header, residue)
        log('F0'); b[57] = (b[57] + 3) & 255
        for arc in arcs:
            w16(b, 12, arc); value = i32(parent, 16); w32(b, 48, i32(parent, 20))
            log('118'); gcalls += 1
            value += (arc + 1) * 7; w32(b, 48, i32(b, 48) + (arc + 1) * 3); b[59] ^= 0x5a
            if arc % 4 == 0: continue
            log('140'); value += 11; b[60] = (b[60] + 1) & 255
            if arc % 4 == 1: continue
            log('128'); acalls += 1; value += 13; w32(b, 48, i32(b, 48) - 5); b[61] ^= 0xa5
            if arc % 4 == 2: continue
            if not capacity or count == capacity and i32(storage[0], 16) <= value: continue
            old = storage[:]
            if count < capacity:
                target = count; count += 1
                while target:
                    predecessor = (target - 1) // 2
                    if value <= i32(old[predecessor], 16): break
                    storage[target] = old[predecessor]; target = predecessor
            else:
                target = 0
                while target * 2 + 1 < count:
                    child = target * 2 + 1
                    if child + 1 < count and i32(old[child], 16) <= i32(old[child + 1], 16): child += 1
                    if i32(old[child], 16) <= value: break
                    storage[target] = old[child]; target = child
            out = bytearray(120) + storage[target][120:]
            out[:8] = parent[:8]; out[5] = (parent[5] & 0x7d) | (parent[5] >> 7) << 7
            out[6] = (parent[6] & 0xfc) | 1
            #Portable context identity08 is normalized, separately checked by path.
            w32(out, 16, value); w32(out, 20, i32(b, 48))
            w32(out, 24, current | (start[DASH] & 63) << 16 | (arc & 1023) << 22)
            w16(out, 28, arc); w16(out, 30, 17); out[32] = span
            w16(out, 34, 65535); w16(out, 36, DASH); out[38] = parent[38]
            w64(out, 40, -1); out[48:62] = parent[48:62]; w16(out, 62, 65535)
            w64(out, 88, -1); w32(out, 104, -1)
            storage[target] = bytes(out); accepted += 1
    streams.append(dict(current=current, span=span, previous=previous, parent=parent.hex(), residue=residue.hex(),
                        header=header, special=special, qualifies=qualifies, marker=marker, capacity=capacity,
                        initial=b''.join(initial).hex(), arcs=arcs, applied=eligible, qualified=eligible and qualifies,
                        accepted=accepted, geometry=gcalls, acoustic=acalls, traces=traces, cost=value,
                        frame=b.hex(), count=count, expected=b''.join(storage).hex()))

if len(sys.argv) != 2 or Path(sys.argv[1]).suffix.lower() != '.json': raise ValueError('output.json')
Path(sys.argv[1]).write_text(json.dumps(dict(packets=packets, streams=streams), separators=(',', ':')) + '\n')
print(f'Generated {len(packets)} full80 packets/{len(streams)} prepared callback/wholeheap streams')
