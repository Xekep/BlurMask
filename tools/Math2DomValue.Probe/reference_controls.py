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

def layout(digits, dp, negative):
    # Independent string-position oracle for 08AFD0's admitted first pass.
    # Work in decimal powers and insert a point rather than copying native loops.
    power = dp - 1
    scientific = (dp < 0 or dp > len(digits)) and not -3 <= power <= 5
    point = 1 if scientific else dp
    left = (digits[:point] + '0' * max(0, point - len(digits))) if point > 0 else '0'
    right = ('0' * max(0, -point) + digits[max(0, point):]) if point < len(digits) else ''
    text = ('-' if negative else '') + left + ('.' + right if right else '')
    return text + ('e' + str(power) if scientific else '')


# Human-readable boundary anchors guard the oracle itself, including its
# intentionally surprising native special-value spelling. No vendor execution.
for digits, dp, expected in [
    ('1', -3, '1e-4'), ('1', -2, '0.001'), ('1', 0, '0.1'),
    ('1', 6, '100000'), ('1', 7, '1e6'), ('1234567', 7, '1234567'),
    ('10000000000000002', 17, '10000000000000002'), ('1', 17, '1e16'),
    ('Infinity', 9999, 'I.nfinitye9998'), ('NaN', 9999, 'N.aNe9998')]:
    assert layout(digits, dp, False) == expected
layouts = []
for precision in (9, 17):
    for count in range(1, precision + 1):
        for dp in range(-323, 310):
            digits = ''.join(str(rng.randrange(1 if at == 0 else 0, 10)) for at in range(count))
            for negative in (False, True):
                layouts.append(dict(digits=digits, dp=dp, negative=negative, precision=precision,
                                    text=layout(digits, dp, negative)))
    for digits, dp in [('0', 1), ('NaN', 9999), ('Infinity', 9999)]:
        for negative in (False, True):
            layouts.append(dict(digits=digits, dp=dp, negative=negative, precision=precision,
                                text=layout(digits, dp, negative)))
assert max(len(row['text']) for row in layouts) <= 24
specials = []
for tag, size, exponent, fraction_bits in [(5, 8, 0x7ff, 52), (10, 4, 0xff, 23)]:
    for sign in (0, 1):
        for payload in [0, exponent << fraction_bits, (exponent << fraction_bits) | 1,
                        (exponent << fraction_bits) | (1 << (fraction_bits - 1)),
                        (1 << (size * 8 - 1)) - 1]:
            bits = payload | (sign << (size * 8 - 1))
            digits, dp = ('0', 1) if payload == 0 else ('Infinity' if payload == exponent << fraction_bits else 'NaN', 9999)
            specials.append(dict(tag=tag, storage=bits.to_bytes(size, 'little').hex(),
                                 text=layout(digits, dp, bool(sign))))

output = Path(sys.argv[1])
with output.open('x', encoding='utf-8') as stream:
    json.dump(dict(numbers=numbers, layouts=layouts, specials=specials), stream)
print(f'{len(numbers)} numeric reader, {len(layouts)} layout and {len(specials)} special-value fixtures')
