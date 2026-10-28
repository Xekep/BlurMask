"""Freeze independent Decimal controls; then optionally check Netlib output.

No production implementation is imported and no vendor library is executed.
"""
import decimal
import hashlib
import json
import math
import random
import struct
import sys
from pathlib import Path


def make(output):
    raw = set()

    def add(value):
        bits = struct.unpack('<Q', struct.pack('<d', value))[0]
        if bits & 0x7fffffffffffffff and bits >> 52 & 0x7ff != 0x7ff:
            raw.add(bits & 0x7fffffffffffffff)

    for exponent in range(2047):
        for fraction in (0, 1, (1 << 51) - 1, (1 << 52) - 1):
            add(struct.unpack('<d', struct.pack('<Q', exponent << 52 | fraction))[0])
    for power in range(-323, 309):
        value = float(f'1e{power}')
        for neighbor in (math.nextafter(value, 0), value, math.nextafter(value, math.inf)):
            add(neighbor)
    rng = random.Random(2026102809)
    for _ in range(5000):
        add(struct.unpack('<d', rng.getrandbits(64).to_bytes(8, 'little'))[0])
        add(struct.unpack('<f', rng.getrandbits(32).to_bytes(4, 'little'))[0])
    for exponent in range(255):
        for fraction in (0, 1, (1 << 23) - 1):
            add(struct.unpack('<f', struct.pack('<I', exponent << 23 | fraction))[0])
    # Exact decimal ties, alternating retained parity and carry through nines.
    for value in [123456788.5, 123456789.5, 999999999.5,
                  1234567890123456.25, 1234567890123456.75, .1, .3, 1/3]:
        for neighbor in (math.nextafter(value, 0), value, math.nextafter(value, math.inf)):
            add(neighbor)
    for integer in range(1, 2049):
        add(float(integer))
    for precision in (9, 17):
        for width in range(precision + 1, 21):
            divisor = 10 ** (width - precision)
            for leading in (10 ** (precision - 1), 10 ** (precision - 1) + 2,
                            10 ** (precision - 1) + 3, 10 ** precision - 1):
                for offset in (-1, 0, 1):
                    add(float(leading * divisor + divisor // 2 + offset))
    for power in range(66):
        for offset in (-3, -1, 0, 1, 3):
            add(float((1 << power) + offset))

    contexts = {p: decimal.Context(prec=p, rounding=decimal.ROUND_HALF_EVEN,
                                  Emin=-9999, Emax=9999) for p in (9, 17)}
    rows = []
    for magnitude in sorted(raw):
        for sign in (0, 1):
            bits = magnitude | sign << 63
            value = struct.unpack('<d', struct.pack('<Q', bits))[0]
            for precision, context in contexts.items():
                rounded = context.create_decimal_from_float(value).as_tuple()
                original = ''.join(map(str, rounded.digits))
                digits = original.rstrip('0')
                dp = len(original) + rounded.exponent
                scientific = (dp < 0 or dp > len(digits)) and not -3 <= dp - 1 <= 5
                point = 1 if scientific else dp
                padded = '0' * max(0, -point) + digits + '0' * max(0, point - len(digits))
                if point <= 0:
                    text = '0.' + padded
                elif point < len(digits):
                    text = padded[:point] + '.' + padded[point:]
                else:
                    text = padded
                if scientific:
                    text += 'e' + str(dp - 1)
                rows.append(dict(bits=f'{bits:016x}', precision=precision,
                                 digits=digits, dp=dp, negative=bool(sign),
                                 text=('-' if sign else '') + text))
    with output.open('x', encoding='utf-8') as stream:
        json.dump(dict(finite=rows), stream, separators=(',', ':'))
    with output.with_suffix('.tsv').open('x', encoding='ascii', newline='\n') as stream:
        for row in rows:
            stream.write(f"{row['bits']} {row['precision']}\n")
    print(f'{len(rows)} independent finite Decimal tuples')


def check(controls, reference):
    rows = json.loads(controls.read_text())['finite']
    lines = reference.read_text().splitlines()
    assert len(lines) == len(rows), (len(lines), len(rows))
    for row, line in zip(rows, lines):
        bits, precision, digits, dp, negative = line.split('\t')
        assert (bits, int(precision), digits, int(dp), bool(int(negative))) == (
            row['bits'], row['precision'], row['digits'], row['dp'], row['negative']), (row, line)
    print(f'PASS {len(rows)} Netlib/Decimal digit tuple comparisons; controls SHA256 '
          + hashlib.sha256(controls.read_bytes()).hexdigest())


if __name__ == '__main__':
    if len(sys.argv) == 2:
        make(Path(sys.argv[1]))
    elif len(sys.argv) == 4 and sys.argv[1] == '--check-reference':
        check(Path(sys.argv[2]), Path(sys.argv[3]))
    else:
        raise SystemExit('OUTPUT.json | --check-reference CONTROLS.json NETLIB.tsv')
