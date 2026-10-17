"""Independent global ordinary overlay/V140 and prepared positive217DB0 oracle.

Actual flushes have zero residuals. Positive residual assignments and callback
acknowledgements below are prepared controls, never a native winner claim.
"""
import base64, json, struct, sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('capture.json')
decode = lambda ref: (ref & 0xffffff, (ref >> 24) & 65535)
pack = lambda n, a: n | (a << 24)
b64 = base64.b64decode

def normalize(raw):
    values = [tuple(r) for r in raw]
    stack = [(0, len(values)-1)]
    while stack:
        lo, hi = stack.pop(); pivot = values[(lo+hi)//2][:2]; i, j = lo, hi
        while i <= j:
            while values[i][:2] < pivot: i += 1
            while values[j][:2] > pivot: j -= 1
            if i <= j: values[i], values[j] = values[j], values[i]; i += 1; j -= 1
        if i < hi: stack.append((i, hi))
        if lo < j: stack.append((lo, j))
    position = 1
    while position < len(values):
        first, second = values[position-1:position+1]
        if first[0] == second[0] and second[1] == first[2]-1:
            values[position-1] = (first[0], first[1], second[2]); del values[position]
        else: position += 1
    return [list(r) for r in values]

data = json.loads(Path(sys.argv[1]).read_bytes()); assert len(data) == 8
queries = higher = positive = endpoint_bytes = 0
for ordinal, c in enumerate(data):
    assert c['owner'] == ordinal and c['preparedCallbackAcks'] == 2
    graph, groups = c['graph'], c['groups']
    extras = [[] for _ in graph]
    def merge(inputs, cache):
        raw = []; capacity = 0; starts = []; ends = []
        for reference in inputs:
            n, a = decode(reference); arc = graph[n][a]; owned = groups[arc['group']]
            required = len(raw)+len(owned)
            if required > capacity: capacity = max(required, int(capacity*1.5))
            raw.extend(owned); starts.append(arc['start']); ends.append(arc['end'])
        merged = normalize(raw); last, _ = decode(inputs[-1])
        for i, (old, cap, original) in enumerate(cache[last]):
            if old == merged: return pack(last, len(graph[last])+i)
        matching = next((i for i, ranges in enumerate(groups) if ranges == merged), None)
        if matching is not None:
            for node, arcs in enumerate(graph):
                for index, arc in enumerate(arcs):
                    if (arc['group'], arc['start'], arc['end']) == (matching, min(starts), max(ends)):
                        return pack(node, index)
        reference = pack(last, len(graph[last])+len(cache[last]))
        cache[last].append((merged, capacity, inputs[-1])); return reference
    def stored(reference, cache):
        n, a = decode(reference)
        return cache[n][a-len(graph[n])] if a >= len(graph[n]) else (groups[graph[n][a]['group']], None, reference)
    def base_path(reference):
        node, arc = decode(reference); first = graph[node][arc]['start']; last = graph[node][arc]['end']
        first_node = first >> 7
        for ordinal, initial in enumerate(graph[first_node]):
            if initial['span'] != 0 or initial['start'] != first: continue
            stack = [(first_node, ordinal, [])]
            while stack:
                n, a, prefix = stack.pop(); current = graph[n][a]
                assert current['span'] == 0
                if current['end'] > last: continue
                path = prefix + [(n, a)]
                if current['end'] == last:
                    return [r for pn, pa in path for r in groups[graph[pn][pa]['group']]]
                for successor in reversed(current['next']): stack.append((n+1, successor, path))
        raise AssertionError(('missing real source path', reference))
    factor, ticks, counts, keys, ending = c['factor'], c['map'], c['counts'], c['keys'], c['endingKeys']
    metadata = []; offset = 0
    for count in counts:
        row = []
        for point in range(count):
            tick = ticks[offset+point] if ticks else offset+point
            whole, remainder = divmod(tick, factor); index = whole+(remainder != 0)
            row.append(dict(key=keys[index], end=ending[index], index=index,
                residual=(factor-remainder)&255 if remainder else 0))
        metadata.append(row); offset += count
    def project(ranges):
        rows = [dict(first=metadata[s][a], last=metadata[s][z-1], tail=z == counts[s]) for s, a, z in ranges]
        order = lambda r: tuple(r['first']['key'])+(-r['first']['residual'],)+tuple(r['last']['end'])+(-r['last']['residual'],)
        #Independent stable bubble order instead of the runtime LINQ comparer.
        for stop in range(len(rows)-1, 0, -1):
            for i in range(stop):
                if order(rows[i]) > order(rows[i+1]): rows[i], rows[i+1] = rows[i+1], rows[i]
        for i in range(len(rows)-1, 0, -1):
            prev, cur = rows[i-1], rows[i]; p, f = prev['last'], cur['first']
            if (f['key'] == p['key'] and f['residual'] == p['residual']) or (prev['tail'] and p['index']+1 < len(keys) and f['key'] == keys[p['index']+1]):
                prev['last'] = cur['last']; prev['tail'] = cur['tail']; del rows[i]
        references = b''; residual = b''
        for row in rows:
            f, e = row['first'], row['last']
            references += struct.pack('<iiiiii', *f['key'], *(e['end'] if row['tail'] else e['key']))
            residual += bytes([f['residual'], e['residual']])
        return references, residual
    assert len(c['actualFlushes']) == 2
    for flush in c['actualFlushes']:
        assert flush['remaining'] == flush['additions'] == 0 and b64(flush['before']) == b64(flush['after'])
    for q in c['queries']:
        if q['input']: assert merge(q['input'], extras) == q['target']
        physical, capacity, original = stored(q['target'], extras)
        n, a = decode(q['target']); is_higher = a >= len(graph[n])
        assert q['higher'] == is_higher and q['ranges'] == physical and q['base'] == original and q['capacity'] == capacity
        bn, ba = decode(original); assert q['group'] == graph[bn][ba]['group']
        expected_refs, expected_res = project(physical if is_higher else base_path(q['target']))
        assert b64(q['references24']) == expected_refs and b64(q['residuals2']) == expected_res
        queries += 1; higher += is_higher; endpoint_bytes += sum(v != 0 for v in expected_res)
    for case in c['preparedAssociations']:
        before = b64(case['before']); expected = bytearray(before)
        #One explicitly prepared ordinary candidate and same-source residuals:
        #native nearest-row selection is exactdistance0, with no top-mark shift.
        assert len(before) == 72 and before[60] == 0 and struct.unpack_from('<i', before, 64)[0] == 0
        source = pack(struct.unpack_from('<i', before)[0], struct.unpack_from('<H', before, 4)[0])
        assert all(r == source for r in case['input'])
        key = struct.unpack_from('<Q', before, 48)[0]; cache = [[] for _ in graph]
        merged = merge(case['input']+[key], cache)
        struct.pack_into('<i', expected, 64, len(case['input'])); struct.pack_into('<Q', expected, 48, merged); expected[68] = 1
        assert b64(case['after']) == expected and case['storage'] == case['input']+[key] and case['remaining'] == 0
        physical, _, original = stored(merged, cache); assert case['base'] == original
        n, a = decode(merged); expected_refs, expected_res = project(physical if a >= len(graph[n]) else base_path(merged))
        assert b64(case['references24']) == expected_refs and b64(case['residuals2']) == expected_res
        positive += 1
assert queries > 1000 and higher > 0 and positive == 64 and endpoint_bytes > 0
print(f'PASS independent8 globalowners/{queries} source queries/{higher} higher/{endpoint_bytes} nonzero endpoint bytes/768 prepared merges/64 prepared positive217DB0 cases; actual16 flushes residual0, acknowledgements prepared')
