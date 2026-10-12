"""Independent scalar 210870 collector, then functional selection/stitch oracle."""
import ast
import copy
import json
import random
import struct
import sys
from pathlib import Path

namespace = dict(copy=copy, struct=struct)
for directory, functions, assignments in (
    ("LegacyWordContextAlternatives.Probe", {"compose"}, {"actions"}),
    ("LegacyWordRejection.Probe", {"i32", "get", "put", "compare", "select", "stitch"}, {"IMP"}),
):
    path = Path(__file__).resolve().parents[1] / directory / "reference_controls.py"
    tree = ast.parse(path.read_text(encoding="utf-8-sig"))
    definitions = [node for node in tree.body if isinstance(node, ast.FunctionDef) and node.name in functions
                   or isinstance(node, ast.Assign) and any(isinstance(x, ast.Name) and x.id in assignments for x in node.targets)]
    exec(compile(ast.Module(body=definitions, type_ignores=[]), str(path), "exec"), namespace)
get, put, i32, select, stitch = [namespace[key] for key in ("get", "put", "i32", "select", "stitch")]
IMP = namespace["IMP"]
rng = random.Random(0x210870)

def collect(input_rows, heap, headers, mode, residues, pc, limit, node):
    rows = copy.deepcopy(input_rows)
    output = []
    header_calls = []
    higher_calls = []
    counters = dict(traced=0, marker=0, unavailable=0, protected=0, invalidated=0, ordinary=0, anchored=0, fallback=0)
    def record(at, arc):
        if arc >= limit:
            higher_calls.append([at, arc])
        return rows[at, arc]["record"]
    def alternative(at, arc, index):
        return rows[at, arc]["alts"][index]
    def parent(at, span):
        return i32(at - (span if span < 128 else span-256) - 1)
    def fallback(start, group, parent_node, current_arc, original_index):
        raw = bytearray(residues[len(output)])
        struct.pack_into("<iiiiiHHHHHBB", raw, 0, IMP, IMP, start, node, parent_node,
                         group, current_arc, limit, 65535, limit, original_index, 0)
        raw[32] = 0
        output.append(raw)
        counters["fallback"] += 1
    for current_arc in range(limit):
        current = record(node, current_arc)
        if get(current, 16) >= IMP:
            break
        header_calls.append(current[4])
        if not headers[current[4]] & 32:
            continue
        replacements = [get(slot, 62, "H") for slot in heap if get(slot, 30, "H") == current_arc and get(slot, 62, "H") != 65535]
        if pc <= 1:
            continue
        stop = []
        for index in range(pc):
            stop.append([-1, -1, 65535, False, False])
            if get(alternative(node, current_arc, index)["data"], 0) >= IMP:
                counters["unavailable"] += 1
                continue
            current_node, arc, selected = node, current_arc, index
            last_node, last_arc, last_selected = node, current_arc, index
            flag, boundary = False, False
            while current_node >= 1 and not flag and not boundary:
                last_node, last_arc, last_selected = current_node, arc, selected
                data = alternative(current_node, arc, selected)["data"]
                current_node = parent(current_node, data[26])
                arc, selected = get(data, 24, "H"), data[27] & 127
                flag = bool(data[31] & 64)
                boundary = True
                counters["traced"] += 1
                if current_node >= 0 and arc < limit:
                    previous = alternative(current_node, arc, selected)
                    symbol = previous["raw"][-1] if previous["raw"] else None
                    marker = symbol in (0xfffb, 0xfff9) or mode and symbol is not None and (symbol+21) % 65536 <= 8
                    boundary = bool(previous["data"][31] & 64) or marker
                    counters["marker"] += int(marker)
            data = alternative(last_node, last_arc, last_selected)["data"]
            predecessor = parent(last_node, data[26])
            repair = flag
            counters["protected"] += int(flag)
            if not flag and predecessor >= 0 and get(data, 24, "H") != 65535:
                repair = bool(record(predecessor, get(data, 24, "H"))[5] & 4)
            stop[index] = [predecessor, last_node, last_arc, flag, repair]
        if not replacements:
            for index, (predecessor, last_node, last_arc, flag, repair) in enumerate(stop):
                if repair:
                    fallback(last_node, last_arc, predecessor, current_arc, index)
            continue
        for replacement in replacements:
            for index, (predecessor, last_node, last_arc, flag, repair) in enumerate(stop):
                if predecessor < 0 or not (repair or flag):
                    continue
                # Native intentionally uses index here, not last_selected.
                original = alternative(last_node, last_arc, index)
                if len(original["raw"]) <= 1:
                    put(original["data"], 0, IMP)
                    counters["invalidated"] += 1
                for target in range(pc):
                    previous_arc = get(original["data"], 24, "H") if flag else get(record(predecessor, replacement), 30, "H")
                    previous_cost = get(alternative(predecessor, previous_arc, target)["data"], 0)
                    if previous_cost >= IMP:
                        fallback(last_node, last_arc, predecessor, current_arc, index)
                        break
                    source = record(last_node, last_arc)
                    replacement_record = record(predecessor, replacement)
                    source_cost = get(source, 16)
                    cost = i32(get(replacement_record, 16) + source_cost)
                    if cost >= IMP:
                        break
                    source_previous = get(source, 30, "H")
                    if source_previous < limit:
                        correction = i32(previous_cost - get(record(predecessor, previous_arc), 16) + source_cost)
                        counters["ordinary"] += 1
                    else:
                        higher = record(predecessor, source_previous)
                        reference = record(predecessor, get(higher, 30, "H"))
                        offset = 0
                        anchor_node = get(reference, 56)
                        if anchor_node >= 0:
                            anchor = record(anchor_node, get(reference, 60, "H"))
                            ancestor = parent(anchor_node, anchor[32])
                            if ancestor >= 0:
                                offset = i32(get(record(ancestor, get(higher, 62, "H")), 16) - get(record(ancestor, get(anchor, 30, "H")), 16))
                        correction = i32(source_cost - get(reference, 16) - offset + previous_cost)
                        counters["anchored"] += 1
                    raw = bytearray(residues[len(output)])
                    struct.pack_into("<iiiiiHHHHHBB", raw, 0, cost, correction, last_node, node, predecessor,
                                     last_arc, current_arc, replacement, previous_arc, source_previous, index, target)
                    raw[32] = 0
                    output.append(raw)
    return rows, output, header_calls, higher_calls, counters

cases = []
totals = {}
tokens = [65, 66, 67, 0xffeb, 0xffec, 0xfff3, 0xfff4, 0xfff9, 0xfffb, 0xfffd]
for case_index in range(512):
    pc, limit = (20 if case_index % 64 == 60 else rng.choice([2, 3, 4])), 3
    node = 0 if case_index % 8 == 7 else 1 if case_index % 8 == 6 else 3
    rows = {}
    headers = [0x20, 0x21, 0, 0xff]
    for at in range(4):
        for arc in range(5):
            data = bytearray(rng.randbytes(128))
            data[4] = rng.randrange(4)
            data[5] = rng.choice([0, 4, 128, 132])
            data[32] = 0
            put(data, 16, rng.choice([rng.randrange(-5000, 30000), IMP]) if at == node else rng.randrange(-5000, 30000))
            put(data, 20, rng.randrange(-10000, 10000))
            put(data, 24, at if rng.randrange(2) else 0xffff, "H")
            put(data, 30, rng.randrange(3) if arc < limit else (4 if arc == 3 else 0), "H")
            if arc < limit and at > 0 and case_index % 3 == 0:
                put(data, 30, 3, "H")
            put(data, 56, rng.choice([-1, 2]) if arc >= limit else 0)
            put(data, 60, arc if arc < limit else 0, "H")
            put(data, 62, 1, "H")
            put(data, 88, 0)
            data[8:16] = bytes(8)
            suffix = [rng.choice(tokens) for _ in range(rng.randrange(4))]
            alts = []
            if arc < limit:
                for index in range(pc):
                    primary = bytearray(rng.randbytes(40))
                    put(primary, 0, rng.choice([rng.randrange(-10000, 10000), IMP]))
                    put(primary, 4, rng.choice([rng.randrange(-5000, 5000), IMP]))
                    put(primary, 24, rng.randrange(3) if at else 65535, "H")
                    primary[26] = rng.choice([0, 1]) if at > 1 else 0
                    if pc == 20 and at > 0 and arc > 0 and rng.randrange(3) == 0:
                        primary[26] = 255
                        put(primary, 24, arc-1, "H")
                    primary[27] = (primary[27] & 128) | rng.randrange(pc)
                    primary[31] = (primary[31] & 191) | (64 if rng.randrange(4) == 0 or case_index % 3 == 0 and at > 0 else 0)
                    primary[8:24] = bytes(16)
                    alts.append(dict(data=primary, mapped=[rng.choice(tokens) for _ in range(rng.randrange(4))],
                                     raw=[rng.choice(tokens) for _ in range(rng.randrange(4))]))
            rows[at, arc] = dict(record=data, suffix=suffix, alts=alts, secondary=bytearray(rng.randbytes(80)))
    heap = []
    for index in range(0 if pc == 20 else rng.randrange(0, 9)):
        slot = bytearray(rng.randbytes(128))
        put(slot, 30, rng.randrange(3), "H")
        put(slot, 62, rng.choice([0, 1, 2, 65535]), "H")
        heap.append(slot)
    residues = [rng.randbytes(36) for _ in range(256)]
    mode = bool(rng.randrange(2))
    collected, candidates, calls, higher_calls, counts = collect(rows, heap, headers, mode, residues, pc, limit, node)
    for key, value in counts.items():
        totals[key] = totals.get(key, 0)+value
    selected = select(b"".join(candidates), len(candidates), pc)
    selected_records = [bytearray(selected[index:index+36]) for index in range(0, len(selected), 36)]
    final_rows, map_calls, _ = stitch(collected, selected_records, pc, limit)
    serialized = []
    for (at, arc), row in rows.items():
        serialized.append(dict(node=at, arc=arc, slot=row["record"].hex(), suffix=row["suffix"],
                               primary=b"".join(alt["data"] for alt in row["alts"]).hex(), secondary=row["secondary"].hex(),
                               prefixes=[[alt["mapped"], alt["raw"]] for alt in row["alts"]],
                               collected=b"".join(alt["data"] for alt in collected[at, arc]["alts"]).hex(),
                               expected=b"".join(alt["data"] for alt in final_rows[at, arc]["alts"]).hex(),
                               outputs=[[alt["mapped"], alt["raw"]] for alt in final_rows[at, arc]["alts"]]))
    cases.append(dict(pc=pc, node=node, limit=limit, mode=mode, headers=headers, rows=serialized,
                      heap=b"".join(heap+[bytearray(rng.randbytes(128))]).hex(), count=len(heap),
                      residues=[raw.hex() for raw in residues], expected=b"".join(candidates).hex(), selected=selected.hex(),
                      headerCalls=calls, higherCalls=higher_calls, mapCalls=map_calls))
Path(sys.argv[1]).write_text(json.dumps(dict(cases=cases, totals=totals), separators=(",", ":")), encoding="utf-8")
print(f"generated {len(cases)} complete producer -> selection -> mutable row cases: {totals}")
