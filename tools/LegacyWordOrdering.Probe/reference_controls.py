"""Independent tuple-field comparison and immutable permutation/heap oracles."""
import base64
import json
import random
import struct
import sys

rng = random.Random(0x1fc650)


def b64(value):
    return base64.b64encode(value).decode()


def read(value, offset, fmt):
    return struct.unpack_from('<' + fmt, value, offset)[0]


def sign(value):
    return (value > 0) - (value < 0)


def wrap(value):
    return (value + 2**31) % 2**32 - 2**31


def payload(a, b):
    n = min(a[64], b[64])
    if n:
        if a[4] != b[4]:
            return sign(a[4] - b[4])
        for index in range(n, 0, -1):
            difference = wrap(read(a, 64 + index * 4, 'i') - read(b, 64 + index * 4, 'i'))
            if difference:
                return difference
    if a[64] != b[64]:
        return sign(a[64] - b[64])
    return wrap(read(a, 80, 'i') - read(b, 80, 'i'))


def compare(variant, headers, a, b):
    reduced = variant % 2 == 1
    history = variant % 4 >= 2
    def initial_key(record):
        key = [read(record, 0, 'Q'), read(record, 36, 'H')]
        key += ([read(record, 26, 'H') & 63] if reduced else
                [read(record, 24, 'I'), read(record, 48, 'i'), read(record, 52, 'i'), read(record, 28, 'H')])
        if history:
            key += [read(record, 112, 'Q'), read(record, 104, 'i')]
        key += [record[33] >> 7]
        if reduced:
            key += [record[39] & 1]
        return tuple(key)
    if variant < 4:
        return sign((initial_key(a) > initial_key(b)) - (initial_key(a) < initial_key(b)))
    def continuation_key(record):
        key = [read(record, 40, 'Q'), record[33] & 63, read(record, 36, 'H'),
               read(record, 24, 'I'), read(record, 28, 'H'), record[33] >> 7]
        if reduced:
            key += [record[39] & 1]
        key += [read(record, 104, 'i') if history else read(record, 80, 'I')]
        return tuple(key)
    ka, kb = continuation_key(a), continuation_key(b)
    if ka != kb:
        return -1 if ka < kb else 1
    if not reduced and a[4] != b[4]:
        if headers[a[4]] & 0x1e00 == 0x1400:
            return -1
        if headers[b[4]] & 0x1e00 == 0x1400:
            return 1
    return payload(a, b) if history else 0


def sorted_permutation(values, cmp):
    # Immutable partition recursion retains the native midpoint pivot and swap
    # sequence; it deliberately does not assume comparator antisymmetry.
    def partition(state, lo, hi):
        pivot = state[(lo + hi) // 2]
        left, right = lo, hi
        while left <= right:
            while cmp(state[left], pivot) < 0:
                left += 1
                if left > hi:
                    raise ValueError('native left scan outside explicit partition')
            while cmp(state[right], pivot) > 0:
                right -= 1
                if right < lo:
                    raise ValueError('native right scan outside explicit partition')
            if right < left:
                break
            state = state[:left] + (state[right],) + state[left + 1:] if left == right else tuple(
                state[right] if i == left else state[left] if i == right else v for i, v in enumerate(state))
            left, right = left + 1, right - 1
        if lo < right:
            state = partition(state, lo, right)
        if left < hi:
            state = partition(state, left, hi)
        return state
    return list(partition(tuple(values), 0, len(values) - 1)) if len(values) > 1 else list(values)


def heap_add(storage, count, record, cap):
    value = read(record, 16, 'i')
    if cap == 0 or value > 0x3ffffff or (count == cap and read(storage[0], 16, 'i') <= value):
        return False, count, None
    old = storage[:]
    removed = None
    if count < cap:
        def path(node):
            parent = (node - 1) // 2
            return [node] + path(parent) if node and value > read(old[parent], 16, 'i') else [node]
        movement = path(count)
        count += 1
        for child, parent in zip(movement, movement[1:]):
            storage[child] = old[parent]
    else:
        removed = old[0]
        def path(node):
            children = [n for n in (node * 2 + 1, node * 2 + 2) if n < count]
            if not children:
                return [node]
            child = max(children, key=lambda n: (read(old[n], 16, 'i'), n))
            return [node] + path(child) if read(old[child], 16, 'i') > value else [node]
        movement = path(0)
        for parent, child in zip(movement, movement[1:]):
            storage[parent] = old[child]
    storage[movement[-1]] = record
    return True, count, removed


pair_cases = []
headers = [0x1400, 0x1400, 0x800, 0x1000, 0, 0x1600, 0x1400, 0x400]
for variant in range(8):
    for index in range(512):
        a = bytearray(rng.randbytes(128))
        a[4] = rng.randrange(8)
        a[64] = rng.randrange(5)
        b = a[:]
        field = rng.choice([0, 4, 16, 24, 26, 28, 33, 36, 39, 40, 48, 52, 64, 68, 76, 80, 104, 112, 120])
        b[field] ^= rng.randrange(1, 256)
        b[4] %= 8
        b[64] %= 5
        if index % 16 == 0:
            # All earlier continuation keys equal; two distinct special IDs.
            a[4], b[4] = 0, 1
        if index % 16 == 1:
            # Exact signed32 subtraction overflow in the terminal payload field.
            a[64] = b[64] = 0
            struct.pack_into('<i', a, 80, -2**31)
            struct.pack_into('<i', b, 80, 2**31 - 1)
        pair_cases.append(dict(variant=variant, headers=headers, left=b64(a), right=b64(b),
                               expected=compare(variant, headers, a, b)))

streams = []
for variant in range(8):
    for _ in range(32):
        count = 8 if _ == 1 else rng.randrange(1, 33)
        cap = count + rng.randrange(4)
        base = bytearray(rng.randbytes(128))
        base[4] = 0
        base[64] = rng.randrange(5)
        records = []
        for i in range(count):
            record = base[:]
            # Many equal native keys but varying costs and opaque ownership.
            record[36] = rng.randrange(4)
            struct.pack_into('<i', record, 16, rng.randrange(-500, 501))
            struct.pack_into('<I', record, 120, i)
            if i % 7 == 0:
                record[4] = rng.randrange(2)
            if _ == 1:
                # Equal costs retain the literal quicksort-selected leader;
                # shrinking rejects tail ties and keeps callback source order.
                record[4] = 0
                record[36] = i % 4
                struct.pack_into('<i', record, 16, 0)
            records.append(bytes(record))
        if variant in (6, 7) and _ == 0:
            # Real signed-subtraction wrap reaches the sorting algorithm, not
            # just scalar comparisons. Mathematical numeric sorting disagrees.
            values = [-2**31, 2**31 - 1, 0, 1, -1, -2**31, 2**31 - 1]
            count, cap = len(values), len(values) + 2
            records = []
            for index, value in enumerate(values):
                record = bytearray(128)
                struct.pack_into('<i', record, 80, value)
                struct.pack_into('<i', record, 16, 100 - index)
                struct.pack_into('<I', record, 120, index)
                records.append(bytes(record))
        storage = records + [rng.randbytes(128) for _ in range(cap - count)]
        cmp = lambda a, b: compare(variant, headers, a, b)
        try:
            ordered = sorted_permutation(records, cmp)
        except ValueError:
            continue
        compact = ordered + storage[count:]
        unique = 1
        for index in range(1, count):
            leader = unique - 1
            if cmp(compact[leader], compact[index]) == 0:
                if read(compact[index], 16, 'i') < read(compact[leader], 16, 'i'):
                    compact[leader], compact[index] = compact[index], compact[leader]
            else:
                compact[unique] = compact[index]
                unique += 1
        heap = compact[:]
        n = 0
        for record in compact[:unique]:
            _, n, _ = heap_add(heap, n, record, cap)
        newcap = 1 if _ == 1 else rng.randrange(n + 1)
        pruned = heap[:newcap]
        dropped = []
        for record in heap[newcap:n]:
            ok, _, removed = heap_add(pruned, newcap, record, newcap)
            dropped.append(removed if ok else record)
        assert sorted(read(r, 16, 'i') for r in pruned) == sorted(read(r, 16, 'i') for r in heap[:n])[:newcap]
        streams.append(dict(variant=variant, headers=headers, count=count, storage=b64(b''.join(storage)),
                            sorted=b64(b''.join(ordered + storage[count:])), compact=b64(b''.join(compact)),
                            unique=unique, heap=[b64(r) for r in heap[:n]], newCapacity=newcap,
                            pruned=[b64(r) for r in pruned], dropped=[b64(r) for r in dropped]))

with open(sys.argv[1], 'w', encoding='utf8') as stream:
    json.dump(dict(pairs=pair_cases, streams=streams), stream, separators=(',', ':'))
print(f'Wrote {len(pair_cases)} comparator pairs and {len(streams)} exact ordering/pruning streams')
