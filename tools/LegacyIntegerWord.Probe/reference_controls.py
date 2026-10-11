"""Independent original small dictionary integer terminal-ranking controls."""
import json
import random
import sys
from pathlib import Path

rng = random.Random(0x204290)
cases = []
for n in range(640):
    choices = [[label, rng.randrange(1001)] for label in [1, 2, 3]]
    if n % 4 == 0:
        choices += [[2, rng.randrange(1001)], [9, 0]]
    offset = rng.choice([0, .125, -.125])
    history = [] if n % 3 == 0 else [rng.randrange(2)]
    category = rng.randrange(13)
    clear, suppress, boundary, full = [bool(rng.randrange(2)) for _ in range(4)]
    additional, limit, beam = rng.randrange(-400, 401), rng.randrange(-1000, 11001), rng.randrange(1, 4)
    expected = []
    for label in [1, 2, 3]:
        word = label - 1
        acoustic = min(cost for code, cost in choices if code == label)
        cls = [0, 1, 0][word]
        class_cost = [30, 50][cls] if not history else [40, 80, 120, 160][history[0] * 2 + cls]
        category_cost = 0 if suppress else (-2 if clear else 1) if category == 3 else 2 if category in [5, 6] else 3 if category in [10, 11, 12] else 0
        lexical = [1000, 100, 300][word] - int(offset * 8192) + class_cost + category_cost + (8192 if boundary else 0)
        path = acoustic + additional
        total = path + lexical
        if total < limit if full else total <= limit:
            expected.append(dict(wordId=word, acoustic=acoustic, path=path, lexical=lexical, total=total))
    expected.sort(key=lambda e: (e['total'], e['wordId']))
    cases.append(dict(choices=choices, offset=offset, history=history, category=category, clear=clear,
                      suppress=suppress, boundary=boundary, full=full, additional=additional, limit=limit,
                      beam=beam, expected=expected[:beam]))
destination = Path(sys.argv[1]); destination.parent.mkdir(parents=True, exist_ok=True)
destination.write_text(json.dumps(dict(source='Own dictionary, prepared integer choices and independently composed terminal scores; no vendor weights.', cases=cases), separators=(',', ':')) + '\n', encoding='utf8')
print(f'Wrote {len(cases)} independent integer dictionary controls.')
