"""Independent prefix-rewrite and immutable alternative-matrix block oracle."""
import base64
import json
import random
import struct
import sys

rng = random.Random(0x213410)


def b64(value):
    return base64.b64encode(value).decode()


cases = []
for _ in range(512):
    slot = bytearray(rng.randbytes(128))
    arc = rng.choice([0, 1, 5, 20, 65534, 65535])
    limit = rng.randrange(21)
    anchor = rng.choice([65535, 0, 1, 65534])
    struct.pack_into('<H', slot, 0x3c, anchor)
    primary_count = rng.randrange(5)
    secondary_count = rng.randrange(5)
    ordinal = rng.randrange(7)
    struct.pack_into('<i', slot, 0x58, ordinal)
    primary = rng.randbytes(7 * primary_count * 40)
    secondary = rng.randbytes(7 * secondary_count * 40)
    expected = slot[:88]
    struct.pack_into('<H', expected, 0x3c, arc if anchor == 65535 else anchor)
    expected_primary = (primary[ordinal * primary_count * 40:(ordinal + 1) * primary_count * 40]
                        if primary_count > 1 and arc < limit else b'')
    expected_secondary = (secondary[ordinal * secondary_count * 40:(ordinal + 1) * secondary_count * 40]
                          if secondary_count > 1 else b'')
    cases.append(dict(slot=b64(slot), arc=arc, limit=limit, primaryCount=primary_count,
                      secondaryCount=secondary_count, primary=b64(primary), secondary=b64(secondary),
                      expected=b64(expected), expectedPrimary=b64(expected_primary),
                      expectedSecondary=b64(expected_secondary)))

with open(sys.argv[1], 'w', encoding='utf8') as stream:
    json.dump(cases, stream, separators=(',', ':'))
print(f'Wrote {len(cases)} independent word-row materialization controls')
