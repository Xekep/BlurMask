"""Original finite-tree auxiliary290 oracle; no vendor execution or C# output."""
import argparse
import json
import random
import struct
from pathlib import Path

rng = random.Random(0x290239100)
MASK = (1 << 32) - 1


def wrap(value):
    return ((value + (1 << 31)) & MASK) - (1 << 31)


def select_kind(current, inherited):
    if inherited == 1:
        return 1
    if inherited == 2:
        return 1 if current == 1 else 2
    if inherited == 3 and (((current - 1) & 0xfc) or current == 3):
        return 3
    if inherited == 4 and ((current - 1) & 255) > 1:
        return 4
    return current


def oracle(nodes, tag, first, flags, count):
    output = []

    def walk(tag, parent, cost, length, inherited, bit, ancestors):
        if tag in ancestors:
            return
        ancestors = ancestors + [tag]
        for raw in nodes[tag & MASK]:
            row = bytearray(raw)
            symbol = struct.unpack_from('<H', row, 20)[0]
            path = parent + [symbol]
            row[23] = (row[23] & 0xf0) | inherited | (bit << 3)
            protocol = symbol >= 192 and ((symbol + 29) & 65535) >= 8
            follow = handled = False
            if protocol:
                if symbol in (0xfff8, 0xfff9) and first == count - 1:
                    continue
                if symbol == 0xfff6:
                    row[23] |= 8
                elif symbol in (0xfff7, 0xfff8, 0xfff9):
                    row[23] = (row[23] & 0xf9) | 1
                    path.pop()
                elif symbol == 0xfffb:
                    if row[23] & 7 != 1:
                        row[23] = (row[23] & 0xfa) | 2
                    path.pop()
                elif symbol == 0xfffc:
                    kind = row[23] & 7
                    if kind != 1 and ((kind - 2) & 0xfd):
                        row[23] = (row[23] & 0xfb) | 3
                    path.pop()
                follow = handled = True
            elif symbol == 184:
                path.pop()
                follow = handled = True
            elif row[23] & 0x10:
                if not flags & 0x20:
                    path.pop()
                handled = True
            if follow and len(ancestors) < 8:
                walk(struct.unpack_from('<Q', row)[0], path,
                     wrap(cost + struct.unpack_from('<i', row, 16)[0]),
                     (length + row[22]) & 255, row[23] & 7, (row[23] >> 3) & 1, ancestors)
            if handled and (row[5] & 2 or flags & 0x20):
                row[5] = ((row[5] & 0x79) | (2 if symbol != 184 and row[5] & 2 else 0)
                          | (4 if symbol == 184 else 0) | ((tag >> 40) & 0x80))
                struct.pack_into('<i', row, 16, wrap(cost + struct.unpack_from('<i', row, 16)[0]))
                row[22] = (length + row[22]) & 255
                kind = select_kind(row[23] & 7, inherited)
                row[23] = (row[23] & 0xf0) | kind | ((((row[23] >> 3) & 1) | bit) << 3)
                output.append(dict(record=row.hex(), path=path))

    walk(tag, [], 0, 0, 0, 0, [])
    return output


tests = []
for case in range(256):
    nodes = []

    def make_node(depth):
        index = len(nodes)
        nodes.append([])
        for ordinal in range(rng.randrange(1, 5)):
            if depth < 3 and ordinal < 2:
                target = make_node(depth + 1)
                symbol = rng.choice([0xfff6, 0xfff7, 0xfff8, 0xfff9, 0xfffa, 0xfffb, 0xfffc, 184])
            else:
                target = index
                symbol = rng.randrange(64)
            tag = target | (1 << 32) | (rng.randrange(256) << 40)
            row = bytearray(rng.randbytes(32))
            struct.pack_into('<QQiHBB', row, 0, tag, 0, rng.choice([2147483640, -2147483640, 19, 123]),
                             symbol, rng.randrange(256), rng.randrange(256))
            nodes[index].append(row)
        return index

    make_node(0)
    #Safe finite trees isolate exact transformation from native repeated-tag
    #cache effects. Flat pagination is tested independently in the C# harness.
    flags = [0, 0x20, 8, 0x28][case % 4]
    first = case % 5
    tag = (1 << 32) | ((case % 2) * 0x80 << 40)
    expected = oracle(nodes, tag, first, flags, 5)
    assert len(expected) < 512
    tests.append(dict(nodes=[[row.hex() for row in rows] for rows in nodes], tag=tag,
                      first=first, flags=flags, node_count=5, expected=expected))

parser = argparse.ArgumentParser()
parser.add_argument('output', type=Path)
options = parser.parse_args()
options.output.parent.mkdir(parents=True, exist_ok=True)
options.output.write_text(json.dumps(tests, separators=(',', ':')), encoding='utf-8')
print(f'Generated {len(tests)} original auxiliary290 trees')
