"""Independent integer candidate checks on original tiny dictionary values."""
import argparse, json, random
from pathlib import Path

def generate():
    rng = random.Random(0x204290)
    cases = []
    for i in range(640):
        word = rng.randrange(41)
        cls = word * 7 % 5
        prior = rng.randrange(5)
        tag = rng.randrange(1, 13)
        history = [] if i % 3 == 0 else [(tag << 24) | prior]
        category = [0, 3, 5, 6, 10, 11, 12][i % 7]
        clear, suppress, boundary = bool(i & 1), bool(i & 2), bool(i & 4)
        wc = [100, 300, 800][word % 3]
        cc = ([11, 23, 47][cls % 3] if not history else
              [102, 501, 903][(prior * 5 + cls) % 3])
        if suppress: cat = 0
        elif history and not clear and category:
            cat = 2048 if tag == 1 and category == 3 else 0
        else:
            cat = {3: -2 if clear else 1, 5: 2, 6: 2, 10: 3, 11: 3, 12: 3}.get(category, 0)
        lexical = wc + cc + cat + (8192 if boundary else 0)
        path = rng.choice([0, rng.randrange(-30000, 30000), -(2**31), 2**31-1,
                           0x4000000 - lexical, 0x4000000 - lexical - 1])
        limit = rng.choice([0x4000000, 0, min(0x4000000, max(-(2**31), path + lexical)),
                            min(0x4000000, max(-(2**31), path + lexical - 1))])
        full = bool(i & 8)
        budget, total = limit - path, path + lexical
        fits = -(2**31) <= budget < 2**31 and -(2**31) <= total < 2**31
        accepted = (fits and lexical < 0x4000000 and lexical <= budget and
                    total < 0x4000000 and (total < limit if full else total <= limit))
        cases.append(dict(wordId=word, history=history, category=category, clear=clear,
                          suppress=suppress, boundary=boundary, path=path, limit=limit,
                          full=full, lexical=lexical, budget=budget, total=total,
                          fits=fits, accepted=accepted))
    return dict(schemaVersion=1, origin="original tiny dictionary, no vendor model", cases=cases)

if __name__ == '__main__':
    parser = argparse.ArgumentParser(); parser.add_argument('output'); args = parser.parse_args()
    target = Path(args.output); target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(json.dumps(generate(), separators=(',', ':')))
    print('Created 640 independent ordinary word candidate references.')
