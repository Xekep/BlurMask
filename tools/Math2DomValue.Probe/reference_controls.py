"""Native-disassembly-derived oracle using Python integers and IEEE packing.

No SDK loading. Write controls BEFORE compiling/running the C# implementation.
"""
import json
import math
import random
import struct
import sys
from pathlib import Path

rng = random.Random(20261029)
numbers = []

def number(tag, storage, result, logged=False, heap=None):
    numbers.append(dict(tag=tag, storage=storage.hex(), heap=None if heap is None else heap.hex(),
                        value=result, logged=logged))

for tag in range(256):
    if tag not in (1, 2, 4, 5, 6, 7, 8, 9, 10):
        number(tag, bytes(16), 12345, True)
for tag, width, signed in ((4, 8, True), (9, 8, True), (7, 4, True), (8, 4, False), (6, 1, False)):
    bits = [0, 1, (1 << (width * 8)) - 1, 1 << (width * 8 - 1)]
    bits += [rng.getrandbits(width * 8) for _ in range(100)]
    for raw in bits:
        payload = raw.to_bytes(width, 'little')
        result = int(raw != 0) if tag == 6 else int.from_bytes(payload, 'little', signed=signed)
        number(tag, payload, result)
for tag, size in ((5, 8), (10, 4)):
    fmt = '<d' if size == 8 else '<f'
    values = [0., -0., .5, -.5, 1.5, -1.5, math.inf, -math.inf, math.nan,
              2147483648., -2147483648., 9223372036854775808., -9223372036854775808.]
    raw_values = [struct.pack(fmt, value) for value in values]
    raw_values += [rng.getrandbits(size * 8).to_bytes(size, 'little') for _ in range(500)]
    for payload in raw_values:
        value = struct.unpack(fmt, payload)[0]
        shifted = value - .5 if value < 0 else value + .5
        if size == 4:
            try:
                shifted = struct.unpack('<f', struct.pack('<f', shifted))[0]
            except OverflowError:
                shifted = math.copysign(math.inf, shifted)
        width = 64 if size == 8 else 32
        result = -(1 << (width - 1)) if not math.isfinite(shifted) or not -(1 << (width - 1)) <= shifted < (1 << (width - 1)) else int(shifted)
        number(tag, payload, result)
for text in [b'', b'0', b'-2', b'+3', b' 3', b'3 ', b'3x', b'3\0', b'-', b'18446744073709551616', b'9223372036854775808']:
    import re
    valid = re.fullmatch(rb'[+-]?[0-9]+', text)
    result = int(text) % (1 << 64) if valid else 12345
    if valid and result >= 1 << 63:
        result -= 1 << 64
    lane = bytearray(16)
    struct.pack_into('<I', lane, 8, len(text))
    number(1, lane, result, heap=text)
    if 0 < len(text) <= 15:
        inline = bytearray(16)
        inline[:len(text)] = text
        inline[15] = len(text) if len(text) < 15 else 0
        number(2, inline, result)

output = Path(sys.argv[1])
with output.open('x', encoding='utf-8') as stream:
    json.dump(dict(numbers=numbers), stream)
print(f'{len(numbers)} independent numeric reader fixtures')
