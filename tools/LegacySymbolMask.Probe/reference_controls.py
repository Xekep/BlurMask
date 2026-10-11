"""Original set-based controls for the checked ordered native mask algebra.

No engine/model execution or vendor data; expectations are computed before
calling the independent C# implementation. Preserve one-pass order deliberately.
"""
import argparse
import json
import random
from pathlib import Path

COUNT = 27
VIRTUAL = set(range(7)) | {25}


def expected(case):
    p = {i for i, flag in enumerate(case['seed']) if flag} if case['outcome'] == 2 else set(range(COUNT))
    s = set(range(COUNT))
    if case['outcome'] == 2:
        for members, weight in case['groups']:
            if weight > 0 and p.intersection(members):
                p.update(members)
    if case['outcome'] != 0:
        p.update((7, 14))
    if case['restriction']:
        allowed = {i for i in range(COUNT) if case['restriction'][i // 8] & (1 << (i % 8))}
        p.intersection_update(allowed)
        s.intersection_update(allowed)
    if case['forms']:
        for i, members in enumerate(case['forms']):
            if i in s:
                valid = set(members) - {65535}
                s.update(valid)
                if i in p:
                    p.update(valid)
    for first, second in case['pairs']:
        if first in p or second in p:
            p.update((first, second))
    for symbols in (p, s):
        symbols.difference_update(VIRTUAL)
        if symbols.intersection(range(7, 13)):
            symbols.add(1)
        if 13 in symbols:
            symbols.add(2)
        symbols.update((3, 0, 4, 5, 6))
    result = {'expectedPrimary': [int(i in p) for i in range(COUNT)],
              'expectedSecondary': [int(i in s) for i in range(COUNT)]}
    count = (len(set(case['sourceIds'])) + case['templates']) % 256
    permitted = {i for i, (low, high) in enumerate(zip(case['lower'], case['upper']))
                 if (low == 0 or low <= count) and (high == 0 or count <= high)}
    result.update(expectedWorkingPrimary=[int(i in p and i in permitted) for i in range(COUNT)],
                  expectedWorkingSecondary=[int(i in s and i in permitted) for i in range(COUNT)])
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    rng = random.Random(0x1B8850)
    cases = []
    for index in range(384):
        outcome = index % 3
        groups = [[rng.sample(range(COUNT), rng.randrange(0, 9)), rng.choice([-1, 0, 0.25, 1])]
                  for _ in range(rng.randrange(0, 8))]
        case = dict(outcome=outcome, seed=[rng.randrange(2) for _ in range(COUNT)] if outcome == 2 else [],
                    groups=groups, restriction=[rng.randrange(256) for _ in range(4)] if index % 4 else [],
                    forms=[[rng.choice([65535, *range(COUNT)]) for _ in range(3)] for _ in range(COUNT)] if index % 5 else [],
                    pairs=[[rng.randrange(COUNT), rng.randrange(COUNT)] for _ in range(rng.randrange(8))],
                    sourceIds=[rng.randrange(-3, 8) for _ in range(rng.randrange(20))],
                    templates=rng.choice([0, 1, 2, 254, 255, 256, 257]),
                    lower=[rng.choice([0, 1, 2, 3, 254, 255]) for _ in range(COUNT)],
                    upper=[rng.choice([0, 1, 2, 3, 254, 255]) for _ in range(COUNT)])
        case.update(expected(case))
        cases.append(case)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps({'schema': 1, 'cases': cases}, separators=(',', ':')), encoding='utf8')
    print(f'Wrote {len(cases)} independent ordered set controls to {args.output}')


if __name__ == '__main__':
    main()
