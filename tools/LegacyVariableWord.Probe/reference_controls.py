"""Exhaustive original variable-partition controls, not a copy of the beam."""
import base64
import itertools
import json
import random
import struct
import sys
from pathlib import Path

words = [p for length in range(1, 5) for p in itertools.product(range(1, 4), repeat=length)]
prefixes = [()] + words
states = {word: i for i, word in enumerate(prefixes)}

def packed(values, width):
    bits = sum(v << (width * i) for i, v in enumerate(values))
    data = bits.to_bytes(((len(values) * width + 31) // 32) * 4, 'little')
    return struct.pack('<IIB', 0x01000000, len(values), width) + data

labels = [0] + [4 | w[-1] for w in words]
counts = [3 if len(p) < 4 else 0 for p in prefixes]
payload = struct.pack('<IIIII', 0x2b4, 0x01000000, 0, len(prefixes), 4)
payload += packed(labels, 3) + bytes([8]) + packed(counts, 2) + struct.pack('<I', len(words))
graph = struct.pack('<II', 0x0101fe90, len(payload)) + payload
rng = random.Random(0x1A5920)
cases = []
for number in range(512):
    n, cap = rng.randrange(1, 8), rng.randrange(1, 4)
    groups = []
    for first in range(n):
        for end in range(first, min(n, first + cap)):
            choices = [[label, rng.randrange(-20, 81)] for label in range(1, 4) if rng.randrange(4)]
            if number % 5 == 0:
                choices += [[2, rng.randrange(-20, 81)], [9, 0]]
            groups.append(dict(first=first, end=end, choices=choices))
    by_start = {i: [g for g in groups if g['first'] == i] for i in range(n)}
    terminal = {''.join(map(str, word)): rng.randrange(-50, 51) for word in words}
    limit = rng.randrange(-50, 351)
    best = {}
    def enumerate_partitions(first, word, cost):
        if first == n:
            if not word:
                return
            key = ''.join(map(str, word))
            total = cost + terminal[key]
            if total <= limit and (word not in best or cost < best[word][0]):
                best[word] = (cost, total)
            return
        if len(word) >= 4:
            return
        for group in by_start[first]:
            for label, value in group['choices']:
                if 1 <= label <= 3:
                    enumerate_partitions(group['end'] + 1, word + (label,), cost + value)
    enumerate_partitions(0, (), 0)
    expected = [dict(labels=list(word), acoustic=cost, total=total)
                for word, (cost, total) in sorted(best.items(), key=lambda e: (e[1][1], states[e[0]]))]
    cases.append(dict(n=n, cap=cap, groups=groups, terminal=terminal, limit=limit, expected=expected))
boundaries = []
for _ in range(512):
    counts = [rng.randrange(2, 65) for _ in range(rng.randrange(1, 6))]
    ranges = []
    for _ in range(rng.randrange(1, 8)):
        stroke = rng.randrange(len(counts)); start = rng.randrange(counts[stroke])
        end = rng.randrange(start + 1, counts[stroke] + 1)
        ranges.append([stroke, start, end])
    kind = rng.choice([0, 1, 2, 3, 255]); suppress_start, suppress_end = [bool(rng.randrange(2)) for _ in range(2)]
    if kind == 1:
        expected = [-1024 if suppress_start else 1024] * 2
    else:
        expected = [1024 if not suppress_start and ranges[0][1] == 0 else -1024,
                    1024 if not suppress_end and ranges[-1][2] == counts[ranges[-1][0]] else -1024]
    boundaries.append(dict(counts=counts, ranges=ranges, kind=kind, suppressStart=suppress_start,
                           suppressEnd=suppress_end, expected=expected))
target = Path(sys.argv[1]); target.parent.mkdir(parents=True, exist_ok=True)
target.write_text(json.dumps(dict(graph=base64.b64encode(graph).decode(), cases=cases, boundaries=boundaries), separators=(',', ':')) + '\n', encoding='utf8')
print(f'Wrote {len(cases)} exhaustive variable-partition controls; {sum(len(c["expected"]) for c in cases)} accepted words.')
