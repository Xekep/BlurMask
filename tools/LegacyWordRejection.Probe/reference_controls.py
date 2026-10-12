"""Independent 36-byte selection and mutable primary-row path oracle.

The path operation uses functional symbol lists; native pointer/context IDs
are assigned separately by the managed consumer. No vendor DLL is executed.
"""
import ast
import copy
import json
import random
import struct
import sys
from pathlib import Path

context_source = Path(__file__).resolve().parents[1] / "LegacyWordContextAlternatives.Probe/reference_controls.py"
tree = ast.parse(context_source.read_text(encoding="utf-8-sig"))
selected = [node for node in tree.body if isinstance(node, ast.FunctionDef) and node.name == "compose"
            or isinstance(node, ast.Assign) and any(isinstance(x, ast.Name) and x.id == "actions" for x in node.targets)]
namespace = {}
exec(compile(ast.Module(body=selected, type_ignores=[]), str(context_source), "exec"), namespace)
compose = namespace["compose"]

IMP = 0x4000000
rng = random.Random(0x2113f0)

def i32(value):
    return (value + 0x80000000) % 0x100000000 - 0x80000000

def get(data, offset, kind="i"):
    return struct.unpack_from("<" + kind, data, offset)[0]

def put(data, offset, value, kind="i"):
    struct.pack_into("<" + kind, data, offset, i32(value) if kind == "i" else value)

def compare(left, right):
    for offset, kind in ((8, "i"), (20, "H")):
        a, b = get(left, offset, kind), get(right, offset, kind)
        if a != b:
            return -1 if a < b else 1
    return i32(get(left, 0) - get(right, 0))

def select(storage, count, primary_count):
    rows = [bytearray(storage[index:index+36]) for index in range(0, len(storage), 36)]
    def partition(lo, hi):
        pivot = rows[lo+(hi-lo)//2].copy()
        left, right = lo, hi
        while True:
            while compare(rows[left], pivot) < 0:
                left += 1
                if left > hi:
                    raise ValueError("unsafe wrapped comparator")
            while compare(rows[right], pivot) > 0:
                right -= 1
                if right < lo:
                    raise ValueError("unsafe wrapped comparator")
            if right < left:
                break
            rows[left], rows[right] = rows[right], rows[left]
            left += 1
            right -= 1
            if left > right:
                break
        if lo < right:
            partition(lo, right)
        if left < hi:
            partition(left, hi)
    if count:
        partition(0, count-1)
    start = 0
    while start < count:
        end = start+1
        while end < count and rows[end][8:12] == rows[start][8:12] and rows[end][20:22] == rows[start][20:22]:
            end += 1
        retained = []
        for row in rows[start:end]:
            if len(retained) == primary_count:
                break
            key = get(row, 24, "H")
            if key not in retained:
                retained.append(key)
        for row in rows[start:end]:
            for key in retained:
                if key == 65535:
                    break
                if get(row, 24, "H") == key and get(row, 0) < IMP:
                    row[32] = 1
                    break
        start = end
    return b"".join(rows)

def candidate(start, end, previous_node, arc, previous_arc, source_alt, target_alt, cost, correction, group=0, distinct=0):
    row = bytearray(rng.randbytes(36))
    struct.pack_into("<iiiiiHHHHHBB", row, 0, cost, correction, start, end, previous_node,
                     group, arc, distinct, previous_arc, 0xCAFE, source_alt, target_alt)
    row[32] = 0
    return row

def stitch(rows, records, primary_count, arc_limit):
    owned = copy.deepcopy(rows)
    counters = dict(invalidated=0, linked=0, gaps=0, abandoned=0, repaired=0, appended=0, copied=0)
    calls = []
    def alt(point):
        node, arc, index = point
        return owned[node, arc]["alts"][index]
    def parent(node, span):
        return i32(node - (span if span < 128 else span-256) - 1)
    def chain(record):
        stop = get(record, 8)
        point = (get(record, 12), get(record, 22, "H"), record[30])
        reverse = []
        while stop <= point[0] and point[1] < arc_limit:
            reverse.append(point)
            data = alt(point)["data"]
            point = (parent(point[0], data[26]), get(data, 24, "H"), data[27] & 127)
        return reverse[::-1]
    for record in records:
        if not record[32]:
            continue
        for point in chain(record):
            target = alt(point)["data"]
            if not target[31] & 64:
                put(target, 0, IMP)
                counters["invalidated"] += 1
    for record in records:
        if not record[32]:
            continue
        path = chain(record)
        for index in range(len(path)):
            point = path[index]
            previous = path[index-1] if index else (get(record, 16), get(record, 26, "H"), record[31])
            target = alt(point)
            data = target["data"]
            if get(data, 0) < IMP:
                actual = (parent(point[0], data[26]), get(data, 24, "H"), data[27] & 127)
                if previous == actual:
                    data[31] |= 64
                    counters["linked"] += 1
                    continue
                next_index = point[2]+1
                while next_index < primary_count and get(alt((point[0], point[1], next_index))["data"], 0) < IMP:
                    next_index += 1
                if next_index == primary_count:
                    counters["abandoned"] += 1
                    break
                point = (point[0], point[1], next_index)
                path[index] = point
                target = alt(point)
                data = target["data"]
                counters["gaps"] += 1
            native_record = owned[point[0], point[1]]["record"]
            if get(data, 4) >= IMP:
                increment = get(alt((previous[0], previous[1], 0))["data"], 4)
                increment -= get(owned[previous[0], previous[1]]["record"], 20)
                increment += get(native_record, 20)
                put(data, 4, increment)
                data[30] = native_record[38]
                counters["repaired"] += 1
            data[31] |= 64
            data[26] = (point[0]-previous[0]-1) & 255
            data[31] = (data[31] & 224) | (native_record[33] & 31)
            put(data, 24, previous[1], "H")
            data[27] = (data[27] & 128) | (previous[2] & 127)
            if not index:
                alt(previous)["data"][31] |= 64
                put(data, 0, get(record, 4))
                data[31] = (data[31] & 223) | 128
            else:
                physical = (parent(point[0], native_record[32]), get(native_record, 30, "H"))
                cost = get(alt(previous)["data"], 0) - get(owned[physical]["record"], 16) + get(native_record, 16)
                put(data, 0, cost)
            previous_alt = alt(previous)
            if get(native_record, 24, "H") == point[0] or native_record[5] < 128:
                target["mapped"], mapped_calls = compose(previous_alt["mapped"], owned[point[0], point[1]]["suffix"], True)
                target["raw"], _ = compose(previous_alt["raw"], owned[point[0], point[1]]["suffix"], False)
                calls.extend(mapped_calls)
                counters["appended"] += 1
            else:
                target["mapped"] = previous_alt["mapped"].copy()
                target["raw"] = previous_alt["raw"].copy()
                counters["copied"] += 1
    return owned, calls, counters

comparisons = []
for index in range(1024):
    left, right = bytearray(rng.randbytes(36)), bytearray(rng.randbytes(36))
    if index % 3:
        right[8:12] = left[8:12]
        right[20:22] = left[20:22]
    if index % 16 == 0:
        put(left, 0, -2147483648)
        put(right, 0, 0)
        right[8:12] = left[8:12]
        right[20:22] = left[20:22]
    comparisons.append(dict(left=left.hex(), right=right.hex(), expected=compare(left, right)))

selections = []
unsafe_count = 0
for index in range(512):
    n = rng.randrange(0, 25)
    pc = rng.choice([0, 1, 2, 3, 20])
    storage = bytearray(rng.randbytes((n+2)*36))
    for slot in range(n):
        at = slot*36
        put(storage, at+8, rng.choice([-3, -1, 0, 1, 5]))
        put(storage, at+20, rng.randrange(3), "H")
        put(storage, at+24, rng.choice([0, 1, 2, 65535]), "H")
        put(storage, at, rng.choice([-2147483648, 0, 2147483647]) if index % 7 == 0 else rng.randrange(-20, IMP+3))
        storage[at+32] = rng.choice([0, 0, 7])
    try:
        expected = select(storage, n, pc).hex()
    except ValueError:
        expected = None
        unsafe_count += 1
    selections.append(dict(storage=storage.hex(), count=n, pc=pc, expected=expected))

writers = []
for index in range(256):
    residue = bytearray(rng.randbytes(36))
    prepared = candidate(rng.randrange(-100, 100), rng.randrange(-100, 100), rng.randrange(-100, 100),
                         rng.randrange(65536), rng.randrange(65536), rng.randrange(256), rng.randrange(256),
                         rng.randrange(-2147483648, 2147483648), rng.randrange(-2147483648, 2147483648),
                         rng.randrange(65536), rng.randrange(65536))
    prepared[33:] = residue[33:]
    limit = rng.randrange(65536)
    fallback = bytearray(residue)
    fallback[:24] = prepared[:24]
    struct.pack_into("<ii", fallback, 0, IMP, IMP)
    struct.pack_into("<HHH", fallback, 24, limit, 65535, limit)
    fallback[30] = prepared[30]
    fallback[31:33] = bytes(2)
    writers.append(dict(residue=residue.hex(), prepared=prepared.hex(), limit=limit, fallback=fallback.hex()))

mutations = []
totals = {}
tokens = [65, 66, 67, 1025, 1048, 0xfff7, 0xfff8, 0xfff9, 0xfffa, 0xfffb, 0xfffc, 0xfffd]
for test_index in range(384):
    pc = rng.choice([2, 3, 4])
    rows = {}
    for node in range(3):
        for arc in range(3):
            record = bytearray(rng.randbytes(128))
            put(record, 16, rng.randrange(-100000, 100000))
            put(record, 20, rng.choice([rng.randrange(-1000, 1000), 2147483647, -2147483648]))
            put(record, 24, node if rng.randrange(2) else 0xfffe, "H")
            put(record, 30, rng.randrange(3), "H")
            record[5] = rng.choice([0, 127, 128, 255])
            record[32] = 0
            put(record, 60, arc, "H")
            put(record, 88, 0)
            record[8:16] = bytes(8)
            suffix = [rng.choice(tokens) for _ in range(rng.randrange(4))]
            alts = []
            for index in range(pc):
                data = bytearray(rng.randbytes(40))
                put(data, 0, rng.choice([rng.randrange(-5000, 5000), IMP, IMP+1]))
                put(data, 4, rng.choice([rng.randrange(-5000, 5000), IMP, 2147483647]))
                put(data, 24, rng.randrange(3) if node else 65535, "H")
                data[26] = 0
                data[27] = (data[27] & 128) | rng.randrange(pc)
                data[31] = (data[31] & 191) | (64 if rng.randrange(3) == 0 else 0)
                data[8:24] = bytes(16)
                mapped = [rng.choice(tokens) for _ in range(rng.randrange(4))]
                raw = [rng.choice(tokens) for _ in range(rng.randrange(4))]
                alts.append(dict(data=data, mapped=mapped, raw=raw))
            rows[node, arc] = dict(record=record, suffix=suffix, alts=alts, secondary=bytearray(rng.randbytes(80)))
    records = []
    for candidate_index in range(rng.randrange(1, 5)):
        start = rng.choice([1, 2])
        record = candidate(start, 2, start-1, rng.randrange(3), rng.randrange(3), rng.randrange(pc), rng.randrange(pc),
                           rng.randrange(-2000, 2000), rng.choice([rng.randrange(-10000, 10000), 2147483647, -2147483648]),
                           candidate_index % 2, rng.randrange(3))
        record[32] = 1 if rng.randrange(5) else 0
        records.append(record)
    result, calls, counts = stitch(rows, records, pc, 3)
    for key, value in counts.items():
        totals[key] = totals.get(key, 0)+value
    serialized_rows = []
    for (node, arc), row in rows.items():
        expected = result[node, arc]
        serialized_rows.append(dict(node=node, arc=arc, slot=row["record"].hex(), suffix=row["suffix"],
                                    primary=b"".join(a["data"] for a in row["alts"]).hex(), secondary=row["secondary"].hex(),
                                    prefixes=[[a["mapped"], a["raw"]] for a in row["alts"]],
                                    expected=b"".join(a["data"] for a in expected["alts"]).hex(),
                                    outputs=[[a["mapped"], a["raw"]] for a in expected["alts"]]))
    mutations.append(dict(pc=pc, rows=serialized_rows, candidates=b"".join(records).hex(), calls=calls))

Path(sys.argv[1]).write_text(json.dumps(dict(comparisons=comparisons, selections=selections, writers=writers,
                                           mutations=mutations, totals=totals), separators=(",", ":")), encoding="utf-8")
print(f"generated 1024 comparisons / 512 selections ({unsafe_count} unsafe refusals) / 256 writers / 384 mutable row cases; {totals}")
