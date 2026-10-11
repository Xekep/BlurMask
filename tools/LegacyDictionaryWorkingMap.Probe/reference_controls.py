"""Original set-based working-map controls independent of the C# implementation."""
import json
import random
import sys
from pathlib import Path

def main(path):
    rng = random.Random(0x093F80)
    cases = []
    for i in range(384):
        count = [1, 8, 192][i % 3]
        original = [rng.choice([0, count - 1, count, 65535, 65531]) for _ in range(19)]
        aux = list(reversed(original))
        permission = [rng.randrange(256) for _ in range(count)]
        kind = rng.choice([0, 2048, 2560, 3072]) | rng.getrandbits(32) & ~7680
        needed = {0, 2} if kind & 7680 == 2048 else {0}
        allowed = {j for j, value in enumerate(permission) if needed.issubset({b for b in range(8) if value & (1 << b)})}
        working = [label if label >= count or label in allowed else 65535 for label in original]
        auxiliary = bool(i & 1); query = rng.randrange(256)
        selected = aux if auxiliary else original if query & 64 else working
        cases.append(dict(count=count, flags=kind, original=original, aux=aux, permission=permission,
                          auxiliary=auxiliary, query=query, expected=selected, working=working))
    output = Path(path); output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(cases, separators=(',', ':')), encoding='utf-8')
    print(f'Wrote {len(cases)} independent dictionary working-map controls')

if __name__ == '__main__':
    main(sys.argv[1])
