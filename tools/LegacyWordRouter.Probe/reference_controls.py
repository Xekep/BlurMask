"""Independent functional record-permutation oracle for native bucket routing."""
import base64
import json
import random
import struct
import sys

rng = random.Random(0x1f8c90)


def b64(value):
    return base64.b64encode(value).decode()


def cost(record):
    return struct.unpack_from('<i', record, 0x10)[0]


def allocate(storage, count, value, write):
    if not storage or value > 0x3ffffff or (count == len(storage) and cost(storage[0]) <= value):
        return False, count, None
    old = storage[:]
    removed = None
    if count < len(storage):
        def path(node):
            parent = (node - 1) // 2
            return [node] + path(parent) if node and value > cost(old[parent]) else [node]
        movement = path(count)
        count += 1
        for child, parent in zip(movement, movement[1:]):
            storage[child] = old[parent]
    else:
        removed = old[0]
        def path(node):
            children = [child for child in (node * 2 + 1, node * 2 + 2) if child < count]
            if not children:
                return [node]
            child = max(children, key=lambda child: (cost(old[child]), child))
            return [node] + path(child) if cost(old[child]) > value else [node]
        movement = path(0)
        for parent, child in zip(movement, movement[1:]):
            storage[parent] = old[child]
    target = movement[-1]
    slot = bytearray(120) + storage[target][120:]
    struct.pack_into('<i', slot, 0x10, value)
    write(slot)
    storage[target] = bytes(slot)
    return True, count, removed


cases = []
for case in range(256):
    heap_count = rng.choice([2, 4, 5, 6, 8])
    capacities = [rng.randrange(5) for _ in range(heap_count)]
    storage = [[rng.randbytes(128) for _ in range(capacity)] for capacity in capacities]
    initial = [b64(b''.join(heap)) for heap in storage]
    counts = [0] * heap_count
    buckets = [rng.randrange(heap_count // 2) for _ in range(7)]
    override_state = rng.randrange(7)
    override = rng.choice([0xffffffff, (rng.randrange(0x1000000) << 8) | rng.randrange(heap_count)])
    steps = []
    for index in range(64):
        state = rng.randrange(7)
        secondary = rng.choice([0, 0, 1, 2, 255])
        use_override = bool(rng.randrange(2))
        value = rng.choice([rng.randrange(-30, 31), rng.randrange(-30, 31), -2**31, 0x4000000, 2**31 - 1])
        primary = buckets[state]
        target = primary
        promotion = False
        if secondary:
            target = heap_count // 2 + primary
            if capacities[target] and counts[target] == capacities[target]:
                promotion = value < cost(storage[target][0])
                target = target if promotion else primary
        elif use_override and override != 0xffffffff and state == override_state:
            target = override & 255
        promoted = False
        if promotion:
            removed = storage[target][0]
            promoted, counts[primary], _ = allocate(
                storage[primary], counts[primary], cost(removed), lambda slot: slot.__setitem__(slice(None), removed))

        def write(slot):
            struct.pack_into('<I', slot, 4, index)
            slot[0x27] = secondary & 1
            struct.pack_into('<I', slot, 0x50, 0xfedc0000 | state)

        accepted, counts[target], _ = allocate(storage[target], counts[target], value, write)
        steps.append(dict(state=state, secondary=secondary, override=use_override, cost=value,
                          accepted=accepted, target=target, promotion=promotion, promoted=promoted,
                          slots=[[b64(record) for record in heap[:count]] for heap, count in zip(storage, counts)]))
    cases.append(dict(capacities=capacities, initial=initial, buckets=buckets,
                      overrideState=override_state, overrideHeap=override, steps=steps))

with open(sys.argv[1], 'w', encoding='utf8') as stream:
    json.dump(cases, stream, separators=(',', ':'))
print(f'Wrote {len(cases)} independent router streams/{sum(len(c["steps"]) for c in cases)} routes')
