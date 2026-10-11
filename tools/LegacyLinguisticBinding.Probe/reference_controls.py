"""Original set-based integration oracle, using an independently specified alphabet."""
import json
import random
import sys
from pathlib import Path

# Alphabet: A,a,CyrA,cyra,p,cyrr,-,softHyphen,virtual. Header primitives absent.
def expected(sources, bypass, prepared, permission):
    outcome = 0 if bypass else 1 if not prepared or any(not s['completed'] for s in sources) else 2
    primary = set(range(9)) if outcome != 2 else set()
    if outcome == 2:
        for source in sources:
            for token in source['tokens']:
                label = token % 65536
                if source['map']:
                    label = source['map'][label]
                if 65507 <= label <= 65514:
                    primary.update(range(9))
                elif label < 9:
                    primary.add(label)
    if outcome != 0:
        primary.update([6, 7])
    allowed = set(range(9)) if not permission else {i for i in range(9) if permission[i // 8] & (1 << (i % 8))}
    return outcome, [int(i in primary and i in allowed and i != 8) for i in range(9)], [int(i in allowed and i != 8) for i in range(9)]

def main(path):
    rng = random.Random(0x22221D240)
    cases = []
    for i in range(256):
        sources = []
        for j in range(i % 4):
            mapped = j % 2 == 0
            mapping = [rng.choice([0, 3, 5, 8, 65506, 65507, 65514, 65515, 65535]) for _ in range(9)] if mapped else []
            tokens = [(rng.randrange(9) if mapped else rng.choice([0, 3, 5, 8, 65507, 65514, 65535])) + 65536 * rng.randrange(65536) for _ in range(7)]
            sources.append(dict(tokens=tokens, map=mapping, completed=(i + j) % 17 != 0))
        bypass, prepared = i % 3 == 0, i % 7 != 0
        permission = [] if i % 5 == 0 else [rng.randrange(256), rng.randrange(2)]
        outcome, primary, secondary = expected(sources, bypass, prepared, permission)
        cases.append(dict(sources=sources, bypass=bypass, prepared=prepared, permission=permission,
                          outcome=outcome, primary=primary, secondary=secondary))
    output = Path(path); output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(cases, separators=(',', ':')), encoding='utf-8')
    print(f'Wrote {len(cases)} independent prepared-binding controls')

if __name__ == '__main__':
    main(sys.argv[1])
