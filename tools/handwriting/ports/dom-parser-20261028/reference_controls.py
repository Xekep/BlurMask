"""Independent arbitrary-precision/regex oracle. Never loads the SDK DLL."""
import json
import random
import re
import sys
from pathlib import Path

rng = random.Random(20261028)
parse = []
inputs = [b'', b' ', b'+', b'-', b'0', b'-0', b'123tail', b'123\x00456',
          b'  -19 ', b'\t\n\v\f\r+42', b'\xa042', b'++1', b'- 1',
          b'9223372036854775807', b'9223372036854775808', b'-9223372036854775808',
          b'-9223372036854775809', b'18446744073709551616', b'0x10', b'12.5']
for _ in range(200):
    inputs.append(rng.choice([b'', b' ', b'\t', b'\xa0']) + rng.choice([b'', b'+', b'-']) +
                  str(rng.randrange(10 ** rng.randrange(1, 100))).encode() + rng.choice([b'', b'x', b' ', b'\0']))
for data in inputs:
    for flags in range(8):
        pattern = (rb'[\x09-\x0d ]*' if flags & 1 else b'') + (rb'[0-9]+' if flags & 4 else rb'[+-]?[0-9]+')
        match = re.match(pattern, data)
        value = 12345
        if match:
            value = int(match[0]) % (1 << 64)
            if value >= 1 << 63:
                value -= 1 << 64
        parse.append(dict(hex=data.hex(), flags=flags, end=match.end() if match else -1, value=value))

formats = []
values = [0, 1, -1, 10, -10, (1 << 63) - 1, -(1 << 63)] + [rng.randrange(-(1 << 63), 1 << 63) for _ in range(200)]
for kind, value in [('int', x) for x in values] + [('bool', False), ('bool', True)]:
    text = (str(value) if kind == 'int' else str(value).lower()).encode()
    for capacity in sorted({0, len(text) - 1, len(text), len(text) + 1, len(text) + 2, 50}):
        data = bytearray([0xcc] * capacity)
        if capacity > len(text):
            data[:len(text) + 1] = text + b'\0'
        formats.append(dict(kind=kind, value=value if kind == 'int' else 0, boolean=bool(value),
                            capacity=capacity, length=len(text), hex=data.hex()))
Path(sys.argv[1]).write_text(json.dumps(dict(parse=parse, format=formats)), encoding='utf-8')
print(f'{len(parse)} parser and {len(formats)} formatter controls')
