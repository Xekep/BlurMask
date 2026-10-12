"""Independent sorted alternative tuples and field-overlay identity oracle."""
import base64
import json
import random
import struct
import sys
import ast
import heapq
from pathlib import Path

rng = random.Random(0x1f9830)


def b64(v):
    return base64.b64encode(v).decode()


def read(v, off, fmt):
    return struct.unpack_from('<' + fmt, v, off)[0]


def alternatives(count, rows):
    output = []
    for row in range(rows):
        costs = sorted(rng.randrange(-50, 51) for _ in range(count))
        valid = rng.randrange(1, count + 1)
        entries = []
        for index, cost in enumerate(costs):
            entry = bytearray(rng.randbytes(40))
            struct.pack_into('<i', entry, 0, cost if index < valid else 0x4000000)
            struct.pack_into('<Q', entry, 8, rng.randrange(1, count + 3))
            entries.append(bytes(entry))
        output.append(entries)
    return output


def merge(kind, count, pool, leader, duplicate):
    a, b = read(leader, 88, 'i'), read(duplicate, 88, 'i')
    leader, duplicate = bytearray(leader), bytearray(duplicate)
    if kind == 'secondary' and read(pool[b][0], 0, 'i') < read(pool[a][0], 0, 'i'):
        leader, duplicate, a, b = duplicate, leader, b, a
    if kind == 'primary':
        special = pool[a][0][31] & pool[b][0][31] & 32
        ac = read(leader, 16, 'i') if special else read(pool[a][0], 0, 'i')
        bc = read(duplicate, 16, 'i') if special else read(pool[b][0], 0, 'i')
        seed = (a, 0) if ac < bc else (b, 0)
    else:
        seed = (a, 0)
    # Use the standard library's two-stream head merge, not global sorting:
    # primary special heap-cost seed selection can leave a pool's raw cost
    # order nonmonotonic after prior merges. Keep its actual iterator order.
    leader_first = seed[0] == a
    def entries(row, start, priority):
        for index in range(start, count):
            cost = read(pool[row][index], 0, 'i')
            if cost >= 0x4000000:
                break
            yield (cost, priority, index, row)
    candidates = heapq.merge(entries(a, 1 if leader_first else 0, 1),
                             entries(b, 0 if leader_first else 1, 0))
    selected = [seed]
    identities = {read(pool[seed[0]][seed[1]], 8, 'Q')}
    for _, _, index, row in candidates:
        identity = read(pool[row][index], 8, 'Q')
        if identity not in identities:
            identities.add(identity)
            selected.append((row, index))
            if len(selected) == count:
                break
    for destination, (row, index) in reversed(list(enumerate(selected))):
        if row == a and index == destination:
            continue
        # Selection references are fixed, but native field reads are live in
        # reverse copy order. Internal duplicate IDs can make a prior write
        # replace a later source; preserve that behavior rather than imposing
        # an unproved pool-uniqueness invariant or snapshotting all sources.
        source = pool[row][index]
        output = bytearray(pool[a][destination])
        # Semantic fields copy; padding and higher provenance bits remain owned
        # by the destination alternative, including secondary bit5.
        for lo, hi in [(0, 24), (24, 28), (30, 31)]:
            output[lo:hi] = source[lo:hi]
        mask = 63 if kind == 'primary' else 31
        output[31] = (output[31] & (255 ^ mask)) | (source[31] & mask)
        if kind == 'secondary':
            output[32:36] = source[32:36]
        pool[a][destination] = bytes(output)
    if kind == 'secondary':
        best = pool[a][0]
        leader[16:24] = best[0:8]
        leader[38] = best[30]
        leader[30:32] = best[24:26]
        leader[32] = best[26]
        leader[33] = (leader[33] & 192) | (best[31] & 31)
        leader[62:64] = best[32:34]
    return bytes(leader), bytes(duplicate), len(selected)


cases = []
for kind in ('primary', 'secondary'):
    for _ in range(384):
        count = rng.randrange(2, 9)
        rows = 5
        pool = alternatives(count, rows)
        leader, duplicate = bytearray(rng.randbytes(128)), bytearray(rng.randbytes(128))
        a, b = rng.sample(range(rows), 2)
        struct.pack_into('<i', leader, 88, a)
        struct.pack_into('<i', duplicate, 88, b)
        struct.pack_into('<i', leader, 16, rng.randrange(-50, 51))
        struct.pack_into('<i', duplicate, 16, rng.randrange(-50, 51))
        # Force special primary heap-cost choice and equal-cost seed policy.
        if _ % 4 == 0:
            for row in (a, b):
                entry = bytearray(pool[row][0])
                entry[31] |= 32
                pool[row][0] = bytes(entry)
            struct.pack_into('<i', leader, 16, 0)
            struct.pack_into('<i', duplicate, 16, 0)
        initial = b''.join(entry for row in pool for entry in row)
        old_leader, old_duplicate = bytes(leader), bytes(duplicate)
        new_leader, new_duplicate, selected = merge(kind, count, pool, leader, duplicate)
        cases.append(dict(kind=kind, count=count, initial=b64(initial), leader=b64(old_leader),
                          duplicate=b64(old_duplicate), selected=selected,
                          expectedLeader=b64(new_leader), expectedDuplicate=b64(new_duplicate),
                          expectedStorage=b64(b''.join(entry for row in pool for entry in row))))

configurations = []
for _ in range(128):
    image = bytearray(rng.randbytes(0x260a))
    image[0x230c:0x2310] = struct.pack('<f', rng.choice([0, 0.25, 0.5, 1]))
    fields = {name: read(image, offset, fmt) for name, offset, fmt in (
        ('header', 0x2400, 'H'), ('primary', 0x2402, 'H'), ('secondary', 0x2404, 'H'),
        ('arcLimit', 0x230a, 'H'), ('expansion', 0x230c, 'f'), ('mode', 0x22f6, 'B'),
        ('source', 0x22e8, 'I'), ('trace', 0x2368, 'B'), ('suppress', 0x2609, 'B'), ('override', 0x2458, 'I'))}
    configurations.append(dict(image=b64(image), expected=fields))

# Reuse only the independently validated comparison/permutation/heap oracle
# definitions; do not execute the other generator's top-level write operation.
oracle_path = Path(__file__).resolve().parent.parent / 'LegacyWordOrdering.Probe' / 'reference_controls.py'
oracle_ast = ast.parse(oracle_path.read_text(encoding='utf8'))
names = {'sign', 'wrap', 'payload', 'compare', 'sorted_permutation', 'heap_add'}
definitions = [node for node in oracle_ast.body if isinstance(node, ast.FunctionDef) and node.name in names]
exec(compile(ast.Module(body=definitions, type_ignores=[]), str(oracle_path), 'exec'))
streams = []
for variant in (0, 4):
    for _ in range(128):
        n = rng.randrange(2, 16)
        cap = n + 1
        base = bytearray(rng.randbytes(128))
        base[4] = 0
        base[33] = 0
        base[64] = 0
        record_list = []
        primary = alternatives(2, n)
        secondary = alternatives(2, n)
        for index in range(n):
            record = base[:]
            struct.pack_into('<H', record, 36, index // 3)
            struct.pack_into('<i', record, 16, rng.randrange(-50, 51))
            struct.pack_into('<i', record, 88, index)
            if index % 2:
                struct.pack_into('<H', record, 60, 65535)
            record_list.append(bytes(record))
        initial = record_list + [rng.randbytes(128)]
        ordered = sorted_permutation(record_list, lambda a, b: compare(variant, [0], a, b))
        compact = ordered + initial[n:]
        original_primary = b''.join(entry for row in primary for entry in row)
        original_secondary = b''.join(entry for row in secondary for entry in row)
        unique = 1
        for index in range(1, n):
            leader = unique - 1
            if compare(variant, [0], compact[leader], compact[index]) == 0:
                compact[leader], compact[index], _ = merge('secondary', 2, secondary, compact[leader], compact[index])
                compact[leader], compact[index], _ = merge('primary', 2, primary, compact[leader], compact[index])
            else:
                compact[unique] = compact[index]
                unique += 1
        heap = compact[:]
        heap_count = 0
        for record in compact[:unique]:
            _, heap_count, _ = heap_add(heap, heap_count, record, cap)
        primary_bytes = b''.join(entry for row in primary for entry in row)
        secondary_bytes = b''.join(entry for row in secondary for entry in row)
        rows = []
        for index, record in enumerate(heap[:heap_count]):
            prefix = bytearray(record[:88])
            if read(prefix, 60, 'H') == 65535:
                struct.pack_into('<H', prefix, 60, index)
            ordinal = read(record, 88, 'i')
            rows.append(dict(record=b64(prefix), primary=b64(primary_bytes[ordinal * 80:(ordinal + 1) * 80]),
                             secondary=b64(secondary_bytes[ordinal * 80:(ordinal + 1) * 80])))
        streams.append(dict(continuation=variant == 4, storage=b64(b''.join(initial)), count=n,
                            primary=b64(original_primary), secondary=b64(original_secondary),
                            expectedSorted=b64(b''.join(ordered + initial[n:])), expectedCompacted=b64(b''.join(compact)),
                            expectedPrimary=b64(primary_bytes), expectedSecondary=b64(secondary_bytes),
                            unique=unique, heap=[b64(record) for record in heap[:heap_count]], rows=rows))

with open(sys.argv[1], 'w', encoding='utf8') as stream:
    json.dump(dict(merges=cases, configurations=configurations, streams=streams), stream, separators=(',', ':'))
print(f'Wrote {len(cases)} complete alternative merges/{len(configurations)} prepared controller views/'
      f'{len(streams)} complete default2/2 ordering->pool->row streams')
