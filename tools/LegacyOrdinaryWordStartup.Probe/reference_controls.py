"""Independent regular1fd240 query/category/full128 writer oracle on pinned RU bytes."""
import argparse
import json
import random
import struct
from pathlib import Path

root = Path(__file__).resolve().parents[2]
data = (root / 'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
r = random.Random(0x1FD240)
imp = 0x4000000
def u16(at): return struct.unpack_from('<H', data, at)[0]
def u32(at): return struct.unpack_from('<I', data, at)[0]
def wrap(value): return (value + 2**31) % 2**32 - 2**31
def write(buf, at, fmt, value): struct.pack_into('<' + fmt, buf, at, value)

count = u16(99 + 14)
at = 115
n = u16(at)
at += 2
for increment in (32, 64, 1):
    table = struct.unpack_from('<' + 'H' * n, data, at)
    at += n * 2
    n = next(value + increment for value in reversed(table) if value != 65535)
spans = struct.unpack_from('<' + 'H' * (n + 1), data, at)
at += (n + 1) * 2
entries = [struct.unpack_from('<II', data, at + i * 8) for i in range(spans[-1] + 1)]
at += len(entries) * 8
strings = data[at:at + entries[-1][0]]
at += entries[-1][0] + count * 2
flag_present = data[at]
at += 1
bits = data[at:at + (count + 7) // 8] if flag_present else bytes((count + 7) // 8)
labels = {strings[start:end].decode(): key & 65535
          for (start, key), (end, _) in zip(entries, entries[1:])}

at = 188226
def ids():
    global at
    n = data[at]
    at = (at + 4) & ~3
    values = struct.unpack_from('<' + 'i' * n, data, at)
    at += 4 * n
    return values
first_ids = ids()
first_map = struct.unpack_from('<' + 'b' * (count + 3), data, at)
at += count + 3
geometry_ids = ids()
start = struct.unpack_from('<' + 'b' * (count + 3), data, at)
at += count + 3
end = struct.unpack_from('<' + 'b' * (count + 3), data, at)
composites = []
for first, a, b in zip(first_map, start, end):
    for x, y in ((first, a), (first, b), (first_ids.index(0), a), (first_ids.index(0), b)):
        if x >= 0 and y >= 0 and (first_ids[x], y) not in composites:
            composites.append((first_ids[x], y))

available = [labels[text] for text in ('a', 'b', 'c', 'A', 'M', 'p', 'м', 'и', 'р', 'И', '0', '1', '2', '+', '-') if text in labels]
cases = []
for index in range(512):
    symbol = available[index % len(available)]
    node = index % 6
    source_id = 1 + index % 2
    tag = r.getrandbits(64) & ~(0xFF << 32) & ~0x20000000000
    tag |= source_id << 32
    if index % 2: tag |= 0x20000000000
    flags = r.randrange(256)
    length = r.randrange(256)
    before = (0, 100, -100, 2**31 - 1, -2**31)[index % 5]
    class_cost = (9, 10, 11, 19, 20, 21, 29, 30, 31, imp, imp + 1, -2**31, 2**31 - 1)[index % 13]
    geometry = [(j * 37 - 400) * (-1 if index % 3 == 0 else 1) for j in range(35)]
    source_kind = (0, 1, 2, 3, 6, 7, 8, 15)[index % 8]
    header = (r.getrandbits(32) & ~0x1E00) | source_kind << 9
    input_flags = (0x10 if index % 3 == 0 else 0) | (0x600 if index % 4 == 0 else 0)
    suppress_header = index % 4 == 0
    suppress_front = index % 7 == 0
    suppressed = index % 5 == 0
    gate = index % 6 != 0
    cache_qualified = class_cost < imp and (gate or length == 0)
    rows = []
    expected_calls = 0
    if cache_qualified:
        for variant in range(1 if input_flags else 2):
            mapping = start[symbol] if variant == 0 else end[symbol]
            if mapping < 0: break
            category = composites.index((first_ids[first_map[symbol]], mapping))
            expected_calls += 1
            if suppress_front and suppressed: continue
            if class_cost == imp or source_kind in (1, 2, 3) and class_cost > (10, 20, 30)[source_kind - 1]: continue
            cost = wrap(before + class_cost + geometry[mapping])
            if cost >= imp: continue
            record = bytearray(128)
            write(record, 0, 'Q', tag)
            record[5] = ((tag >> 40) & 0x7D) | (int(not suppress_header and bool(bits[symbol // 8] & (1 << (symbol % 8)))) << 7) | (2 if tag & 0x20000000000 else 0)
            record[6] = (tag >> 48) & 0xFC
            # context08 is checked against the exact pre-created symbolic owner by the probe.
            write(record, 16, 'i', cost)
            write(record, 20, 'i', wrap(cost - before))
            write(record, 24, 'I', node | (category & 63) << 16)
            # Singleton0 followed by increasing-span merged arcs.
            arc = node
            write(record, 28, 'H', arc)
            write(record, 24, 'I', node | (category & 63) << 16 | arc << 22)
            write(record, 30, 'H', 65535)
            record[32] = node
            record[33] = ((flags >> 5 & 3) + length) & 63
            write(record, 34, 'H', 65535)
            write(record, 36, 'H', symbol)
            record[39] = (flags & 7) << 1
            write(record, 40, 'Q', 2**64 - 1)
            write(record, 48, 'i', 0 if flags & 8 else -1)
            write(record, 52, 'i', 0)
            write(record, 56, 'i', node)
            write(record, 60, 'H', 65535)
            write(record, 62, 'H', 65535)
            write(record, 88, 'Q', 2**64 - 1)
            write(record, 104, 'i', -1)
            rows.append(record.hex())
    cases.append(dict(node=node, symbol=symbol, sourceId=source_id, tag=tag, flags=flags, length=length,
                      before=before, classCost=class_cost, geometry=geometry, header=header,
                      inputFlags=input_flags, suppressHeader=suppress_header, suppressFront=suppress_front,
                      suppressed=suppressed, gate=gate, calls=expected_calls, qualified=int(cache_qualified), rows=rows))
p = argparse.ArgumentParser()
p.add_argument('output', type=Path)
a = p.parse_args()
a.output.parent.mkdir(parents=True, exist_ok=True)
a.output.write_text(json.dumps(cases, separators=(',', ':')), encoding='utf-8')
print(f'Generated {len(cases)} independent source-row/category/128-byte startup controls')
