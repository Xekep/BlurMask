"""Independent immutable-parent field overlay and anchor/history oracle."""
import base64
import json
import random
import struct
import sys

rng = random.Random(0x1f9080)


def b64(value):
    return base64.b64encode(value).decode()


cases = []
for _ in range(512):
    parent = bytearray(rng.randbytes(88))
    slot = bytearray(rng.randbytes(128))
    parent_arc = rng.randrange(65536)
    anchor_node = rng.randrange(64)
    anchor_arc = rng.choice([65535, 0, 1, 2])
    struct.pack_into('<i', parent, 0x38, anchor_node)
    struct.pack_into('<H', parent, 0x3c, anchor_arc)
    payload = [rng.randrange(2**32) for _ in range(5)]
    fields = dict(tag=rng.randrange(2**64), cost=rng.randrange(-2**31, 2**31),
                  incremental=rng.randrange(-2**31, 2**31), state=rng.randrange(65536),
                  payload=payload, aux08=rng.randrange(2**64), context28=rng.randrange(2**64),
                  small=rng.randrange(256), secondary=rng.randrange(256), aux5c=rng.randrange(2**64))
    scale = rng.choice([0, -1, 1, 0.25])
    suppressed = bool(rng.randrange(3) == 0)
    result = slot[:]
    result[:88] = parent
    overlays = [(0, '<Q', fields['tag']), (8, '<Q', fields['aux08']),
                (0x10, '<i', fields['cost']), (0x14, '<i', fields['incremental']),
                (0x1e, '<H', parent_arc), (0x28, '<Q', fields['context28']),
                (0x3e, '<H', fields['state']), (0x58, '<I', 2**32 - 1),
                (0x5c, '<Q', fields['aux5c'])]
    for offset, fmt, value in overlays:
        struct.pack_into(fmt, result, offset, value)
    result[0x20] = 255
    result[0x21] = (parent[0x21] & 192) | (fields['small'] & 63)
    result[0x27] = (parent[0x27] & 254) | (fields['secondary'] & 1 if scale else 0)
    for index, value in enumerate(payload):
        struct.pack_into('<I', result, 0x40 + index * 4, value)
    records = {}
    calls = []

    def lookup(node, arc):
        key = f'{node}:{arc}'
        calls.append(key)
        return records.setdefault(key, rng.randbytes(88))

    previous = -1
    context = 0
    if not suppressed:
        anchor = result[:88] if anchor_arc == 65535 else lookup(anchor_node, anchor_arc)
        previous = anchor_node - struct.unpack_from('<b', anchor, 0x20)[0] - 1
        if previous >= 0:
            previous_arc = struct.unpack_from('<H', anchor, 0x1e)[0]
            context = struct.unpack_from('<Q', lookup(previous, previous_arc), 0x28)[0]
    struct.pack_into('<i', result, 0x68, previous)
    struct.pack_into('<Q', result, 0x70, context)
    cases.append(dict(parent=b64(parent), slot=b64(slot), parentArc=parent_arc, fields=fields,
                      scale=scale, suppressed=suppressed, records={k: b64(v) for k, v in records.items()},
                      calls=calls, expected=b64(result)))

with open(sys.argv[1], 'w', encoding='utf8') as stream:
    json.dump(cases, stream, separators=(',', ':'))
print(f'Wrote {len(cases)} immutable-parent continuation controls')
