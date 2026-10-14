"""Independent visible-prefix/identity ranking model for nonempty1f42d0."""
import base64
import math
import random
import struct
import json
import sys
from pathlib import Path

rng = random.Random(0x1F42D0)
IMP = 0x4000000
def i32(value):
    return (value + 2**31) % 2**32 - 2**31
def f32(value):
    try:
        return struct.unpack('<f', struct.pack('<f', value))[0]
    except OverflowError:
        return math.copysign(math.inf, value)
def bits(value):
    return struct.unpack('<I', struct.pack('<f', value))[0]
def field(record, offset, fmt):
    return struct.unpack_from('<' + fmt, record, offset)[0]
def record_at(nodes, width, node, row, column):
    start = (width * row + column) * 40
    return nodes[node][start:start + 40]

cases = []
for case in range(512):
    width, height, count = rng.randrange(1, 9), rng.randrange(1, 6), rng.randrange(1, 7)
    if case == 0:
        width, height = 256, 1
    if case == 1:
        height = 0
    if case == 2:
        width = 0
    skip = 99
    penalty = rng.choice([0, 40960, -40960, 2**31 - 1, -2**31])
    cfg99, cfg_a8 = bool(case & 1), bool(case & 2)
    source_kind = rng.choice([0x135, 0x239, -1])
    reference = rng.choice([0, -1, 2**63 - 1, -2**63])
    nodes = []
    for node in range(count):
        records = []
        for row in range(height):
            for column in range(width):
                data = bytearray(rng.randbytes(40))
                cost = rng.choice([rng.randrange(-10000, 120000), -2**31, IMP-1, IMP, 2**31-1])
                if case == 0:
                    cost = column // 3
                struct.pack_into('<ii', data, 0, cost, rng.randrange(-2**31, 2**31))
                struct.pack_into('<H', data, 12, rng.choice([skip, 42, 17, skip]))
                span = rng.randrange(min(node, 7) + 1)
                packed = field(data, 14, 'H')
                struct.pack_into('<H', data, 14, (packed & ~14) | (span << 1))
                data[9], data[10] = rng.randrange(height), rng.randrange(width)
                data[32] = rng.choice([1, 0, 3, 17, 19, 33, 129])
                identity = row * width + column if case == 0 else rng.choice([0, 1, 2, 3, 2**63-1, 2**63, 2**64-1])
                struct.pack_into('<Q', data, 24, identity)
                records.append(data)
        nodes.append(b''.join(records))

    # Each row exposes its finite prefix. Compute one representative per
    # identity, then rank representatives, rather than rescan for each output.
    unique = {}
    for row in range(height):
        for column in range(width):
            terminal = record_at(nodes, width, count-1, row, column)
            cost = field(terminal, 0, 'i')
            if cost >= IMP:
                break
            trait_node, trait_row, trait_col = count-1, row, column
            trait = terminal
            while field(trait, 12, 'H') == skip:
                previous = trait_node - ((field(trait, 14, 'H') >> 1) & 7) - 1
                if previous == -1:
                    break
                trait_node, trait_row, trait_col = previous, trait[9], trait[10]
                trait = record_at(nodes, width, trait_node, trait_row, trait_col)
            if trait[32] & 0x13 == 1:
                cost = i32(cost + penalty)
            if cost >= IMP:
                continue
            identity = field(terminal, 24, 'Q')
            entry = (cost, row, column, identity, field(terminal, 4, 'i'), trait_node, trait_row, trait_col)
            if identity not in unique or entry[:3] < unique[identity][:3]:
                unique[identity] = entry
    selected = sorted(unique.values(), key=lambda entry: entry[:3])[:width]
    expected = []
    final_reference = reference
    for index, (cost, row, column, identity, acoustic, tn, tr, tc) in enumerate(selected):
        relative = cost
        if not cfg99 and not cfg_a8:
            final_reference = selected[0][0]
            relative = i32(cost - final_reference)
        normalized = f32(f32(relative) / f32(count * 8192))
        log = f32(f32(-5.0 if source_kind == 0x135 else -1.0) * f32(relative))
        log = f32(log * f32(0.00012207031))
        try:
            mass = f32(math.exp(log))
        except OverflowError:
            mass = math.inf
        acoustic_normal = f32(f32(acoustic) * f32(0.00012207031))
        expected.append(dict(row=row, column=column, cost=cost, acoustic=acoustic, identity=identity,
                             trait=[tn, tr, tc], relative=relative,
                             normalized=bits(normalized), log=bits(log), mass=bits(mass),
                             acousticNormalized=bits(acoustic_normal)))
    cases.append(dict(width=width, height=height, nodes=[base64.b64encode(x).decode() for x in nodes],
                      skip=skip, penalty=penalty, cfg99=cfg99, cfgA8=cfg_a8, kind=source_kind,
                      reference=reference, expected=expected, finalReference=final_reference))

glyph_cases = []
for case in range(256):
    width, height, count = rng.randrange(1, 5), rng.randrange(1, 5), rng.randrange(1, 18)
    nodes, paths, costs = [], {}, {}
    for node in range(count):
        data = bytearray()
        for row in range(height):
            for column in range(width):
                span = rng.randrange(min(7, node) + 1)
                parent = node - span - 1
                pr, pc = (rng.randrange(height), rng.randrange(width)) if parent >= 0 else (255, 255)
                record = bytearray(rng.randbytes(40))
                total = rng.randrange(IMP)
                struct.pack_into('<i', record, 0, total)
                record[9], record[10], record[32] = pr, pc, 0
                struct.pack_into('<H', record, 12, rng.randrange(1, 193))
                struct.pack_into('<H', record, 14, (rng.randrange(64) << 4) | span << 1 | rng.randrange(2))
                struct.pack_into('<H', record, 16, rng.randrange(256))
                struct.pack_into('<Q', record, 24, (node * height + row) * width + column + 1)
                # Causal ancestry, independently built before binary backtracking exists.
                entry = dict(node=node, row=row, column=column, record=record.hex(), previous=parent,
                             previousArc=field(record_at(nodes, width, parent, pr, pc), 16, 'H') if parent >= 0 else 65535)
                paths[node, row, column] = (paths[parent, pr, pc] if parent >= 0 else []) + [entry]
                costs[node, row, column] = total
                data += record
        nodes.append(data)
    row, column = min(((r, c) for r in range(height) for c in range(width)), key=lambda rc: (costs[count-1, *rc], *rc))
    glyph_cases.append(dict(width=width, height=height, nodes=[base64.b64encode(n).decode() for n in nodes],
                            expected=paths[count - 1, row, column]))

out = Path(sys.argv[1])
out.parent.mkdir(parents=True, exist_ok=True)
out.write_bytes((json.dumps(dict(cases=cases, glyphCases=glyph_cases), separators=(',', ':')) + '\n').encode())
print('Generated512 independent terminal-grid rankings, including256-column boundary, cost ties, zero identities and IEEE exp extremes')
