"""Independent uint32 -> int64 ->float32*MULSS ->CVTTSS2SI capacity oracle."""
import json
import random
import struct
import sys
from pathlib import Path

def single(value):
    return struct.unpack('<f', struct.pack('<f', value))[0]

values = [0, 1, 9, 10, 31, 99, 100, 109, 110, 124, 125, 156, 655349,
          655350, 655359, 655360, 0x7ffffffe, 0x7fffffff, 0x80000000,
          0xfffffffe, 0xffffffff]
rng = random.Random(0x20564a)
values += [rng.randrange(0x100000000) for _ in range(2048 - len(values))]
rows = [[value, max(10, int(single(single(value) * single(.1))))] for value in values]
Path(sys.argv[1]).write_text(json.dumps(rows, separators=(',', ':')) + '\n', encoding='utf-8')
print(f'generated {len(rows)} independent uint32/MULSS capacity controls')
