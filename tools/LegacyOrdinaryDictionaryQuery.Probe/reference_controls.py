"""Independent literal query-field controls for ordinary initial/parent gates."""
import base64
import json
import random
import struct
import sys
from pathlib import Path

rng = random.Random(201270)
u32 = lambda n: n & 0xffffffff
signed = lambda n: struct.unpack('<i', struct.pack('<I', u32(n)))[0]
edges = [0, 1, 2, 127, 128, 255, 256, 0x7fffffff, 0x80000000, 0xffffffff]
cases = []
for index in range(4096):
    continuation = index >= 2048
    mode, limit = rng.randrange(256), rng.randrange(256)
    boundary, suppress, frontend = [bool(rng.getrandbits(1)) for _ in range(3)]
    distance = rng.choice([-0x80000000, -1, 0, 1, 127, 0x7fffffff])
    owner = rng.choice(edges) if index % 2 else rng.getrandbits(32)
    span = rng.choice(edges) if index % 3 else rng.getrandbits(32)
    flags = rng.choice([0, 1, 2, 3, 0x10, 0x11, 0xfff0, 0xffff])
    parent = bytearray(rng.randbytes(88))
    tag = rng.getrandbits(64)
    count = rng.randrange(256)
    state = rng.choice([0, 1, 0x3f, 0x7f, 0x80, 0xff])
    previous = rng.choice(edges) if index % 2 else rng.getrandbits(32)
    struct.pack_into('<Q', parent, 0, tag)
    parent[0x21], parent[0x26] = state, count
    struct.pack_into('<I', parent, 0x38, previous)
    before = [17, 113, 128, 0xabcdef01, 0x12345678, 0xa135, 0x6432, 0x61, 0x7c,
              0x123456789abcdef0, 0xfedcba9876543210]
    limited = (flags & 3) != 0 if frontend else mode != 0
    allowed = not limited or signed((count if continuation else 0) + span) <= limit
    if continuation:
        allowed &= state < 128 and (suppress or signed(owner - previous) <= distance)
    else:
        tag = 8 << 40
    after = before[:]
    if allowed:
        remaining = max(0, limit - count) if continuation else limit
        if not limited:
            remaining = 0
        after = [signed(tag), 0, before[2], owner, span, 4 if boundary and flags & 0x10 else 0,
                 flags, 0, remaining, before[9], 0]
    cases.append(dict(kind='continuation' if continuation else 'initial', policy=[mode, limit, boundary, suppress, distance],
                      owner=owner, span=span, flags=flags, frontend=frontend, parent=base64.b64encode(parent).decode(),
                      before=before, expected=after, tag=tag if allowed else None))
path = Path(sys.argv[1])
path.parent.mkdir(parents=True, exist_ok=True)
path.write_text(json.dumps(cases, separators=(',', ':')), encoding='utf-8')
print(f'Wrote {len(cases)} initial/continuation query controls')
