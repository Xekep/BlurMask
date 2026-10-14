"""Independent 1520e0/1c0f60/152520 scalar and owning-cache oracle."""
import argparse
import json
import random
from pathlib import Path

p = argparse.ArgumentParser()
p.add_argument("output", type=Path)
a = p.parse_args()
r = random.Random(0x152520)
cases = []
for _ in range(1024):
    count = r.randint(1, 192)
    rows = [[r.choice((-2147483648, -1, 0, 39999, 0x3FFFFFF, 0x4000000, 0x4000001, 2147483647))
             for _ in range(count + 3)] for _ in range(r.randrange(7))]
    union = [int(any(row[i] < 0x4000000 for row in rows)) for i in range(count + 3)]
    bits = None if r.randrange(3) == 0 else bytes(r.randrange(256) for _ in range((count + 7) // 8))
    path = [r.choice((r.randrange(count), count, count + 2, 0xFFFA, 0xFFFF)) for _ in range(r.randrange(9))]
    length = r.randrange(256)
    symbol = r.randrange(count + 3)
    accepted = bool(union[symbol])
    if length and path and bits is not None:
        accepted &= all(s >= count or bool(bits[s // 8] & (1 << (s % 8))) for s in path)
    cases.append(dict(count=count, rows=rows, union=union, bits=None if bits is None else bits.hex(),
                      path=path, length=length, symbol=symbol, accepted=accepted))

traces = []
values = (0x4000000, 39999, 40000, 40001, -1, 100, 0x4000001)
for _ in range(256):
    n, cap = r.randint(2, 6), r.randint(2, 5)
    next_group = n
    graph = []
    for node in range(n):
        arcs = [(node, 0)]
        for span in range(1, min(node, cap - 1) + 1):
            arcs.append((next_group, span))
            next_group += 1
        graph.append(arcs)
    pool, records, calls = [], {}, 0
    table = [128, 0, 1, 2, 0, 255]
    ops = []
    for step in range(24):
        node, scope, allow = r.randrange(n), r.randrange(cap + 1), bool(r.randrange(2))
        if step == 0: node, scope, allow = n - 1, cap, True
        if step == 1: node, scope, allow = n - 1, 0, False
        if step == 2: node, scope, allow = n - 1, cap, False
        if step == 3: node, scope, allow = n - 1, 0, True
        present = {g for g, _ in graph[node]}
        for entry in pool:
            if entry['scope'] == scope and entry['id'] not in present:
                entry['id'] = -1
        emitted, computed, reused = [], 0, 0
        for ordinal, (group, span) in enumerate(graph[node]):
            if span != scope: continue
            needed = not (allow and group in records) and all(e['id'] != group for e in pool)
            if needed:
                entry = next((e for e in pool if e['id'] == -1), None)
                if entry is None:
                    entry = dict(id=-1, scope=scope, costs=[])
                    pool.append(entry)
                calls += 1
                computed += 1
                entry.update(id=group, scope=scope,
                             costs=[values[(group * 13 + calls * 5 + s * 11) % 7] for s in range(6)])
                records[group] = entry
            else:
                reused += 1
                entry = records[group]
            emitted.append(dict(ordinal=ordinal, group=group, costs=entry['costs'][:]))
        if not allow:
            table = [int(any(row['costs'][s] < 0x4000000 for row in emitted)) for s in range(6)]
        ops.append(dict(node=node, scope=scope, allow=allow, table=table[:], computed=computed,
                        reused=reused, calls=calls, rows=emitted))
    traces.append(dict(n=n, cap=cap, ops=ops))
a.output.parent.mkdir(parents=True, exist_ok=True)
a.output.write_text(json.dumps(dict(cases=cases, traces=traces), separators=(',', ':')), encoding='utf-8')
print(f"Generated {len(cases)} qualification cases and {sum(len(t['ops']) for t in traces)} owning preparation passes")
