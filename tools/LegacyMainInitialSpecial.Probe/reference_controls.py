"""Independent1FD240 primitive extra decision + complete128 writer oracle.

Explicit already-scored controls; these are not synthesized native source
queries or NN results. One output argument only.
"""
import json
import random
import struct
import sys
from pathlib import Path

rng = random.Random(0x1FD240)
labels = [187, 188, 65535, 65535]  #actual RU20C1C0 F0002/3/4/8 lookups
cases = []
for index in range(2048):
    end = rng.randrange(15)
    symbol = [187, 188, 134, 129, 65535][index % 5]
    values = [rng.getrandbits(64), rng.getrandbits(64), rng.randrange(-2**31, 2**31),
              rng.randrange(-2**31, 2**31), symbol, end, rng.randrange(1024), rng.randrange(65536),
              rng.randrange(64), 65535, end, rng.randrange(256), rng.randrange(2), rng.randrange(2),
              rng.randrange(2), rng.randrange(8), rng.randrange(256), rng.randrange(2), rng.randrange(2),
              rng.randrange(-2**31, 2**31), rng.randrange(-2**31, 2**31), end, 65535]
    base = rng.randrange(-2**31, 2**31)
    kind = rng.randrange(3)
    mode = bool(rng.randrange(2))
    flags = rng.randrange(256)
    accepted = kind == 0 and not mode and (flags & 7) > 1 and symbol != 65535 and symbol in labels
    residue = rng.randbytes(8)
    expected = bytearray(128)
    if accepted:
        f = values.copy()
        f[2] = (f[2] - base + 2**31) % 2**32 - 2**31
        f[11], f[17], f[18] = 0, 0, 1
        struct.pack_into('<QQii', expected, 0, *f[:4])
        expected[5] = ((f[0] >> 40) & 0x7d) | f[12] << 7 | (2 if f[0] & (1 << 41) and f[13] == 0 else 0)
        expected[6] = ((f[0] >> 48) & 0xfc) | (f[13] & 1)
        struct.pack_into('<IHH', expected, 24, f[5] | (f[8] & 63) << 16 | (f[6] & 1023) << 22, f[7], f[9])
        expected[32] = f[10] & 255
        expected[33] = (f[16] & 63) | (f[17] & 1) << 6 | (f[18] & 1) << 7
        struct.pack_into('<HHBBQii iHH', expected, 34, 65535, f[4], f[11], (f[15] & 7) << 1,
                         2**64 - 1, f[19], f[20], f[21], f[22], 65535)
        struct.pack_into('<Q', expected, 88, 2**64 - 1)
        struct.pack_into('<i', expected, 104, -1)
        expected[120:] = residue
    cases.append(dict(fields=values, base=base, kind=kind, mode=mode, flags=flags,
                      accepted=accepted, residue=residue.hex(), expected=expected.hex()))
Path(sys.argv[1]).write_text(json.dumps(cases), encoding='utf8')
print(f'Generated{len(cases)} primitive-extra complete128 controls, {sum(c["accepted"] for c in cases)} accepted.')
