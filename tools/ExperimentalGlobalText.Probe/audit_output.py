"""Check host admission captures; optional independent raw WORD/score evidence.

No expected labels are consulted. Raw evidence must come from the separate global
diagnostic, rather than from this host adapter. This checks admission, not accuracy.
"""
import argparse
import json
import math
import struct
import unicodedata
from pathlib import Path

p = argparse.ArgumentParser()
p.add_argument('capture', type=Path)
p.add_argument('--parity', type=Path)
p.add_argument('--raw-scores', type=Path)
p.add_argument('--raw-words', type=Path)
a = p.parse_args()
capture = json.loads(a.capture.read_bytes())
assert capture['guards'] == 11
rows = capture['samples']
assert rows and len({r['id'] for r in rows}) == len(rows)
for r in rows:
    score = struct.unpack('<f', struct.pack('<i', r['scoreBits']))[0]
    assert r['experimental'] is True
    assert math.isfinite(score)
    if r['text']:
        assert .5 <= score <= 1 and r['frames'] > 0
        assert not any(c.isspace() or unicodedata.category(c) in ('Cc', 'Cf', 'Co', 'Cs') for c in r['text'])
    else:
        assert r['scoreBits'] == 0 and r['frames'] == 0
if a.parity:
    assert a.capture.read_bytes() == a.parity.read_bytes()
if a.raw_scores or a.raw_words:
    assert a.raw_scores and a.raw_words
    scores = {r['id']: r for r in json.loads(a.raw_scores.read_bytes())['samples']}
    words = {r['id']: r for r in json.loads(a.raw_words.read_bytes())['samples']}
    assert set(scores) == set(words) == {r['id'] for r in rows}
    for r in rows:
        s, w = scores[r['id']], words[r['id']]
        text, bits = '', 0
        if s['supported'] and s['winner'] and s['complete'] and len(s['words']) == 1:
            sg = s['words'][0]['glyphs']
            wg = w['words'][0]['glyphs']
            assert len(sg) == len(wg)
            values = [struct.unpack('<f', struct.pack('<i', g['scoreBits']))[0] for g in sg]
            decoded = [bytes.fromhex(g['utf8']).decode('utf-8') for g in sg]
            if (1 <= len(sg) <= 128 and all(math.isfinite(v) and .5 <= v <= 1 for v in values)
                and all((g['flags'] & 7) == 0 for g in wg)
                and all(v and not any(c.isspace() or unicodedata.category(c) in ('Cc','Cf','Co','Cs') for c in v) for v in decoded)
                and len(''.join(decoded)) <= 1024):
                text = ''.join(decoded)
                bits = struct.unpack('<i', struct.pack('<f', min(values)))[0]
        assert (r['text'], r['scoreBits']) == (text, bits), r['id']
else:
    by_id = {r['id']: r for r in rows}
    assert len(rows) == 11 and by_id['MIR-ui']['text'] == 'МИР'
print(f'PASS {len(rows)} host admission rows, eleven guards; no targets read')
