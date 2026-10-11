"""Original set-based controls; no SDK, models or inferred script aliases."""
import json
import random
import sys
from pathlib import Path


def expected(count, sources, prepared):
    enabled = set()
    if not prepared:
        return 1, []
    for source in sources:
        for token in source["tokens"]:
            label = token % 65536
            if source["map"]:
                label = source["map"][label]
            if label in range(65507, 65515):
                enabled.update(range(count))
            elif label < count:
                enabled.add(label)
        if not source["completed"]:
            return 1, []
    return 2, [int(i in enabled) for i in range(count)]


def main(path):
    rng = random.Random(0x240206720)
    cases = []
    boundary = [0, 1, 7, 8, 65506, *range(65507, 65515), 65515, 65535, 65536, 0xFFFFFFFF]
    for i in range(384):
        count = [1, 8, 138, 192, 65535][i % 5]
        sources = []
        for j in range(i % 4):
            mapped = (i + j) % 2 == 0
            mapping = [rng.choice(boundary) % 65536 for _ in range(17)] if mapped else []
            tokens = [rng.randrange(17) + 65536 * rng.randrange(65536) for _ in range(19)] if mapped else [rng.choice(boundary) for _ in range(19)]
            sources.append(dict(tokens=tokens, map=mapping, completed=(i + j) % 11 != 0))
        prepared = i % 13 != 0
        outcome, mask = expected(count, sources, prepared)
        cases.append(dict(count=count, sources=sources, prepared=prepared, outcome=outcome, expected=mask))
    for token in boundary:
        for high in [0, 65536, 0x12340000]:
            sources = [dict(tokens=[(token + high) % 2**32], map=[], completed=True)]
            outcome, mask = expected(192, sources, True)
            cases.append(dict(count=192, sources=sources, prepared=True, outcome=outcome, expected=mask))
    output = Path(path)
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(cases, separators=(",", ":")), encoding="utf-8")
    print(f"Wrote {len(cases)} independent linguistic source controls to {output}")


if __name__ == "__main__":
    main(sys.argv[1])
