"""Independent original descriptor-policy oracle; descriptors are explicit inputs."""
import json
import random
import sys
from pathlib import Path

def reference(headers, force, extra, previous800, previousA00):
    categories = [h & 7680 for h in headers]
    allowed = not force or len(headers) == 2 and not extra
    bits = max((h // 4) % 8 for h in headers)
    if (extra or not allowed) and any(h & 32 for h in headers):
        bits += 8
    if any(h & 32 and c != 2048 for h, c in zip(headers, categories)):
        bits += 16
    if 2560 in categories:
        bits += 32
    if any(h & 128 for h in headers):
        bits += 64
    if any(h & 256 for h in headers):
        bits += 128
    last = lambda kind, old: max((i for i, c in enumerate(categories) if c == kind), default=old)
    return dict(flags=bits, last800=last(2048, previous800), lastA00=last(2560, previousA00),
                allowed=allowed)

def main(path):
    rng = random.Random(0x20D8F0)
    cases = []
    boundaries = [0, 4, 28, 32, 128, 256, 2048, 2080, 2560, 3072, 0xffffffff]
    for i in range(512):
        headers = [rng.choice(boundaries) if i % 2 else rng.getrandbits(32) for _ in range(1 + i % 7)]
        force, extra = bool(i & 1), bool(i & 2)
        previous800, previousA00 = rng.choice([-1, 0, 3, 256]), rng.choice([-1, 0, 5, 256])
        item = dict(headers=headers, force=force, extra=extra, previous800=previous800, previousA00=previousA00)
        item.update(reference(headers, force, extra, previous800, previousA00)); cases.append(item)
    output = Path(path); output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(cases, separators=(',', ':')), encoding='utf-8')
    print(f'Wrote {len(cases)} independent prepared descriptor controls')

if __name__ == '__main__':
    main(sys.argv[1])
