"""Independent DFS terminal-rank controls from a caller-supplied checked LK file.

Generated resource-derived controls belong in ignored private output. No native
engine or activation runs. This checks stored ordering/cost lookup, not ink.
"""
import hashlib
import json
import struct
import sys
from pathlib import Path

source = Path(sys.argv[1])
output = Path(sys.argv[2])
if source.stat().st_size > 16 * 1024 * 1024:
    raise ValueError("Caller resource exceeds16MiB")
data = source.read_bytes()
if hashlib.sha256(data).hexdigest() != "d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e":
    raise ValueError("Only the inspected LK asset offsets are verified")
cursor = 609980 + 20
count, maximum_length = struct.unpack_from("<II", data, cursor)
cursor += 8


def packed():
    global cursor
    version, n, bits = struct.unpack_from("<IIB", data, cursor)
    assert version == 0x1000000 and n == count and 1 <= bits <= 31
    cursor += 9
    size = (n * bits + 31) // 32 * 4
    raw = data[cursor:cursor + size]
    cursor += size
    return bits, [
        (int.from_bytes(raw[(i * bits) // 8:(i * bits) // 8 + 5], "little") >> ((i * bits) % 8)) & ((1 << bits) - 1)
        for i in range(n)
    ]


bits, labels = packed()
terminal_bit = 1 << (bits - 1)
cursor += 1  # Stored child offset width; child count array follows.
_, counts = packed()
children = []
next_child = 1
for c in counts:
    children.append((next_child, next_child + c))
    next_child += c
assert next_child == count
targets = [
    [94, 82, 94, 82], [94, 84, 83], [94, 84, 135],
    [98, 83, 84, 86, 90, 85], [92, 79, 85], [134, 79, 94],
    [85, 90, 101, 85],
]
states = {}
for target in targets:
    state = 0
    for label in target:
        start, end = children[state]
        state = next(child for child in range(start, end) if labels[child] & (terminal_bit - 1) == label)
    assert labels[state] & terminal_bit
    states[state] = target
word_id = 0
stack = [0]
found = []
word_costs = struct.unpack_from("<256i", data, 3502860)
while stack:
    state = stack.pop()
    if labels[state] & terminal_bit:
        if state in states:
            found.append({"labels": states[state], "wordId": word_id,
                          "storedWordCost": word_costs[data[3063030 + word_id]]})
        word_id += 1
    start, end = children[state]
    stack.extend(range(end - 1, start - 1, -1))
assert word_id == 439826 and len(found) == len(targets)
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps({"cases": found}, indent=2), encoding="utf-8")
print(f"Wrote {len(found)} independently enumerated terminal-rank/stored-cost controls to caller private output")
