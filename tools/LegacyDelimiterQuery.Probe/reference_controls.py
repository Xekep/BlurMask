"""Original 290 finite-tree oracle. Never executes vendor code or consumes managed output."""
import argparse
import json
import random
import struct
from pathlib import Path

rng = random.Random(0x290237610)
MASK = (1 << 32) - 1

def wrap(value):
    return ((value + (1 << 31)) & MASK) - (1 << 31)

def carry(flags, kind, bit):
    current = flags & 7
    if kind == 1:
        current = 1
    elif kind == 2:
        current = 1 if current == 1 else 2
    elif kind == 3 and (((current - 1) & 0xfc) or current == 3):
        current = 3
    elif kind == 4 and ((current - 1) & 255) > 1:
        current = 4
    return (flags & 0xf0) | current | (bit << 3)

def make_row(target, symbol, cost=None, length=None, flags=None):
    row = bytearray(rng.randbytes(32))
    tag = target | (1 << 32) | (rng.randrange(4) << 40)
    struct.pack_into('<QQiHBB', row, 0, tag, 0,
                     rng.randrange(-20000, 100000) if cost is None else cost, symbol,
                     rng.randrange(256) if length is None else length,
                     rng.randrange(256) if flags is None else flags)
    return row

def oracle(nodes, source_tag, mode, flags24, first, span, zwj):
    result = []
    def visit(tag, path, cost, length, kind, bit, ancestors):
        if tag in ancestors:
            return
        ancestors = ancestors + [tag]
        for original in nodes[tag & MASK]:
            row = bytearray(original)
            symbol = struct.unpack_from('<H', row, 20)[0]
            if symbol == zwj:
                continue
            row[23] = carry(row[23], kind, bit)
            protocol = symbol >= 192 and ((symbol + 29) & 65535) >= 8
            #The actual RU alphabet lacks ZWNJ; ZWJ is its stored label184.
            if protocol:
                follow = True
                if symbol == 0xfff6:
                    row[23] |= 8
                elif symbol in (0xfff7, 0xfff9):
                    if first == span:
                        follow = False
                    else:
                        row[23] = (row[23] & 0xf9) | 1
                elif symbol == 0xfff8:
                    row[23] = (row[23] & 0xf9) | 1
                elif symbol == 0xfffb and (row[23] & 7) != 1:
                    row[23] = (row[23] & 0xfa) | 2
                elif symbol == 0xfffc:
                    k = row[23] & 7
                    if k != 1 and ((k - 2) & 0xfd):
                        row[23] = (row[23] & 0xfb) | 3
                if follow and len(ancestors) < 8:
                    visit(struct.unpack_from('<Q', row)[0], path + [symbol],
                          wrap(cost + struct.unpack_from('<i', row, 16)[0]), (length + row[22]) & 255,
                          row[23] & 7, (row[23] >> 3) & 1, ancestors)
            else:
                struct.pack_into('<i', row, 16, wrap(cost + struct.unpack_from('<i', row, 16)[0]))
                row[22] = (length + row[22]) & 255
                row[23] = carry(row[23], kind, bit)
                result.append((row, path + [symbol]))
    visit(source_tag, [], 0, 0, 0, 0, [])
    for row, path in result:
        prefix = None
        allow_extra = False
        if source_tag & (8 << 40) and (wrap(span) < wrap(first) or flags24 & 0x81 == 1):
            if (source_tag & MASK) == 0:
                row[23] = carry(row[23], (source_tag >> 44) & 7, (row[23] >> 3) & 1)
                kind = row[23] & 7
                prefix = {0: 0xfffa if mode in (3, 4) else 0xfffb,
                          1: 0xfff9, 2: 0xfffb, 3: 0xfffc, 4: 0xfffc}.get(kind)
                if kind == 0 and mode not in (3, 4):
                    row[23] = (row[23] & 0xf8) | 2
                allow_extra = True
            elif not source_tag & (4 << 40):
                prefix = 0xfffa
        extra = None
        if prefix is not None:
            if allow_extra and flags24 & 0x100:
                extra = [prefix]
            else:
                path.insert(0, prefix)
        yield dict(record=row.hex(), path=path, extra=extra)

tests = []
for case in range(256):
    mode = case % 5
    source_tag = (8 if case % 3 == 0 else 4 if case % 3 == 1 else 0) << 40
    nodes = []
    def node(depth):
        index = len(nodes)
        nodes.append([])
        width = 140 if case % 32 == 0 and depth == 0 else rng.randrange(1, 5)
        for ordinal in range(width):
            if depth < 3 and ordinal < 2:
                target = node(depth + 1)
                symbol = rng.choice([0xfff6, 0xfff7, 0xfff8, 0xfff9, 0xfffb, 0xfffc])
            else:
                target = index
                symbol = rng.choice([rng.randrange(64), 184])
            nodes[index].append(make_row(target, symbol))
        if depth == 2 and case % 16 == 0:
            cycle = make_row(0, 0xfffb)
            struct.pack_into('<Q', cycle, 0, source_tag)
            nodes[index].append(cycle)
        return index
    node(0)
    if case % 16 == 2:
        struct.pack_into('<i', nodes[0][0], 16, 2147483640)
    if case % 16 == 1:
        index = len(nodes)
        nodes[0].insert(0, make_row(index, 0xfffb, 2147483640, 250))
        for depth in range(9):
            target = len(nodes) + 1
            nodes.append([make_row(target, 0xfffb, 100, 250)] if depth < 8 else [make_row(target, 23)])
    flags24 = [0, 1, 8, 0x100, 0x101][case % 5]
    first, span = 5, (5 if case % 2 else 2)
    expected = list(oracle(nodes, source_tag, mode, flags24, first, span, 184))
    tests.append(dict(nodes=[[row.hex() for row in rows] for rows in nodes], tag=source_tag,
                      mode=mode, flags=flags24, first=first, span=span, expected=expected,
                      capacity=[1, 3, 17, 128][case % 4]))

#Original full outer-prefix branch matrix: all outer/current kinds, low-node
#zero/nonzero, header4/8, signed1C/20 limits, Flags100 and high/bit3 payload.
#A nonzero source node is a literal separate tree root, not a synthetic remap.
for outer_kind in range(8):
    for root_node in (0, 1):
        for header in (0, 4, 8, 12):
            for limits_index, (first, span, base_flags) in enumerate([
                (0, 0, 0), (1, 0, 0), (0xffffffff, 0, 0), (0, 0, 1)
            ]):
                case = len(tests)
                tag = root_node | (header << 40) | (outer_kind << 44)
                mode = case % 5
                flags24 = base_flags | (0x100 if limits_index % 2 else 0)
                rows = [make_row(root_node, 7 + kind, 17 * kind, 250, 0xa8 | kind) for kind in range(8)]
                nodes = [rows] if root_node == 0 else [[], rows]
                expected = list(oracle(nodes, tag, mode, flags24, first, span, 184))
                tests.append(dict(nodes=[[row.hex() for row in entries] for entries in nodes],
                                  tag=tag, mode=mode, flags=flags24, first=first, span=span,
                                  expected=expected, capacity=[1, 3, 17, 128][case % 4]))

parser = argparse.ArgumentParser()
parser.add_argument('output', type=Path)
options = parser.parse_args()
options.output.parent.mkdir(parents=True, exist_ok=True)
options.output.write_text(json.dumps(tests, separators=(',', ':')), encoding='utf-8')
print(f'Generated {len(tests)} independent290 traversal/prefix/pagination fixtures')
