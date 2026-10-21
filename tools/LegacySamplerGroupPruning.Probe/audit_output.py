"""Independent binary32 separation and literal mutable arc-deletion oracle."""
import copy
import json
import pathlib
import struct
import sys

def f(value):
    return struct.unpack('<f', struct.pack('<f', value))[0]

def rounded(value):
    bits = struct.unpack('<I', struct.pack('<f', value))[0]
    magnitude = bits & 0x7fffffff
    if magnitude >= 0x4b000000:
        return value
    if magnitude < 0x3f800000:
        bits = (bits & 0x80000000) | (0x3f800000 if magnitude >= 0x3f000000 else 0)
    else:
        shift = 150 - (magnitude >> 23)
        bits = ((bits >> shift) + ((bits >> (shift - 1)) & 1)) << shift
    return struct.unpack('<f', struct.pack('<I', bits))[0]

data = json.loads(pathlib.Path(sys.argv[1]).read_bytes())
table = [f(x) for x in data['table']]
assert len(table) == 256 and len(data['cases']) == 72
total_removed = final_rejections = clip_disagreements = 0
for case in data['cases']:
    geometry = []
    for ranges in case['groups']:
        boxes = []
        for stroke, start, end in ranges:
            xs = [f(p[0]) for p in case['strokes'][stroke][start:end]]
            assert xs
            boxes.append((min(xs), max(xs)))
        costs = []
        for boundary in range(1, len(boxes)):
            left, right = boxes[:boundary], boxes[boundary:]
            gap = max(f(min(x[0] for x in right) - max(x[1] for x in left)),
                      f(min(x[0] for x in left) - max(x[1] for x in right)))
            x = f(f(f(gap / f(case['height'])) - f(case['gap'])) * f(-case['slope']))
            value = 0 if x > 4 else table[int(f(f(f(x + 4) * 255) + .5)) >> 3] if x >= -4 else x
            scaled = f(f(-value) * 8192)
            costs.append(int(rounded(scaled)) if len(boxes) < 9 else int(f(scaled + (.5 if scaled > 0 else -.5))))
        total = 0
        for cost in costs:
            total = total + cost if len(boxes) >= 9 or f(f(total) + f(cost)) < 0x4000000 else 0x4000000
        geometry.append(min(total, 0x4000000))
    def mutate(include_final):
        graph = copy.deepcopy(case['before'])
        labels = [list(range(len(node))) for node in graph]
        removed = clipped = 0
        for node in range(len(graph) if include_final else len(graph)-1):
            for ordinal in range(len(graph[node])-1,-1,-1):
                arc=graph[node][ordinal]
                if arc[1]==0 or geometry[arc[0]]<=case['threshold']:continue
                if min(geometry[arc[0]],case['laterCap'])<=case['threshold']:clipped+=1
                for other_node,arcs in enumerate(graph):
                    for other in arcs:
                        if other_node-other[1]-1==node:
                            other[5]=[p-(p>ordinal) for p in other[5] if p!=ordinal]
                        other[6]=[[offset,index-(index>ordinal)] if other_node+offset==node else [offset,index]
                                  for offset,index in other[6] if other_node+offset!=node or index!=ordinal]
                del graph[node][ordinal];del labels[node][ordinal];removed+=1
        return graph,labels,removed,clipped
    graph,labels,removed,clipped=mutate(True)
    local,local_labels,local_removed,_=mutate(False)
    final_rejections += sum(a[1]>0 and geometry[a[0]]>case['threshold'] for a in local[-1])
    assert not any(a[1]>0 and geometry[a[0]]>case['threshold'] for a in graph[-1])
    assert graph==case['after'] and labels==case['originalOrdinals']
    assert local==case['local'] and local_labels==case['localOrdinals']
    assert removed==case['removed'] and local_removed==case['localRemoved']
    clip_disagreements+=clipped
    total_removed += removed
assert total_removed > 0 and final_rejections > 0 and clip_disagreements > 0
print(f'PASS72 independent actual binary32/mutable-pruning cases, {total_removed} removals, '
      f'{final_rejections} legacy-local final rejections removed by complete FIRST, {clip_disagreements} raw-versus-clipped decisions; full provenance/adjacency preserved.')
