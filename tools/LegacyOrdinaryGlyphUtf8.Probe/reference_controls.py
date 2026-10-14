"""Original no-marker identity/refusal fixtures; exact opcode proof is separate."""
import argparse
import json
import random
from pathlib import Path


def main():
    p = argparse.ArgumentParser()
    p.add_argument('output', type=Path)
    args = p.parse_args()
    samples = ['', '\ufeffabc', 'e\u0301', 'a\u0315\u0300', 'ЁЙё\r\n', 'لا',
               '\u1100\u1161\u11a8', '\ufffe\uffff', '\U0010ffff', '\U000f0001',
               '\U000f0010\U000f0018', '\u037e\u0387', '\U000f0000', '\U000f0000a\U000f0001']
    rng = random.Random(49_1_0)
    for _ in range(1024):
        points = []
        for _ in range(rng.randrange(1, 25)):
            cp = rng.randrange(0x110000)
            while 0xd800 <= cp <= 0xdfff:
                cp = rng.randrange(0x110000)
            points.append(cp)
        samples.append(''.join(chr(cp) for cp in points))
    cases = [{'bytes': text.encode('utf8').hex(), 'accepted': '\U000f0000' not in text} for text in samples]
    for raw in ['ff', '80', 'c0af', 'eda080', 'f0808080', 'f4908080', 'f888808080', 'fc8480808080', 'e2', 'e282']:
        cases.append({'bytes': raw, 'accepted': False})
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_bytes((json.dumps({'schema': 1, 'cases': cases}, separators=(',', ':'))+'\n').encode('ascii'))
    print(f'Wrote {len(cases)} original valid-Unicode identity/scope-refusal cases.')


if __name__ == '__main__':
    main()
