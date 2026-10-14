"""Original symbol-map controls, specified from lexical tokens and allocated residues."""
import json
import random
import sys
from pathlib import Path

if len(sys.argv) != 2 or Path(sys.argv[1]).suffix.lower() != '.json':
    raise SystemExit('Usage: reference_controls.py output.json')
rng = random.Random(92990)
acoustic = ['A', 'a', 'р', 'p', 'Ж', 'é', '\u200c', '\u200d']
controls = {'\0': 65533, ' ': 65531, '\n': 65529, '\u00a0': 65532,
            '\ufffd': 65535, '\U000f000c': 65530, '\U000f0000': 65526,
            '\U000f0005': 65514, '\U000f0026': 65507, '\U000f0009': 65535}
pool = acoustic + list(controls) + ['missing', 'virtual']
cases = []
for n in range(512):
    lexical = rng.sample(pool, 4 + n % (len(pool) - 3))
    virtual = lexical.index('virtual') if 'virtual' in lexical else -1
    residue = [rng.randrange(65536) for _ in lexical]
    primary, auxiliary = [], []
    for i, token in enumerate(lexical):
        if i == virtual:
            primary.append(65535)
            auxiliary.append(residue[i])
            continue
        mapped = controls.get(token, acoustic.index(token) if token in acoustic else 65535)
        primary.append(mapped)
        # Explicit native source60 initialization and its two promotion sets.
        auxiliary.append(mapped if mapped >= len(acoustic) or token in ('\u200c', '\u200d') else 65535)
    cases.append(dict(acoustic=acoustic, lexical=lexical, virtual=virtual, residue=residue,
                      primary=primary, auxiliary=auxiliary))
Path(sys.argv[1]).write_text(json.dumps(cases, ensure_ascii=False), encoding='utf8')
print('Generated512 independently specified initial source50/58/60 maps')
