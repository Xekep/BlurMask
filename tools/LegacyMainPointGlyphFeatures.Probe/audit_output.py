"""Independent integer callback replay; no models or native code execution."""
import hashlib
import json
import struct
import sys
from pathlib import Path


def trunc_div(n, d):
    return (abs(n) // abs(d)) * (-1 if (n < 0) != (d < 0) else 1)


def short(n):
    return (n + 32768) % 65536 - 32768


def audit(v):
    assert type(v['Cases']) is int and v['Cases'] == 65536
    context = [-1000 + 37 * i for i in range(28)]
    assert v['Context'] == context
    selected = {x['Y']: x for x in v['Controls']}
    assert set(selected) == {-32768, -12801, -12800, -401, -400, -1,
                             0, 1, 99, 100, 101, 12799, 12800, 32767}
    full, bases = hashlib.sha256(), hashlib.sha256()
    for y in range(-32768, 32768):
        b = [-1024, -1024, max(-1024, min(1024, short(trunc_div(256*y, 100)))),
             -1024, 0, 0, 0, 0, 0]
        if y > 0:
            b[4:] = [-1024, -1024, 1024, -1024,
                     short(trunc_div(512*(-y-100), 100)-1024)]
        elif y >= 100:
            b[4:] = [-1024, 1024, -1024, -1024, -1024]
        else:
            b[4:] = [1024, -1024, -1024,
                     short(trunc_div(512*y, 100)-1024), -1024]
        t, r = [0]*256, [0]*238
        t[6:36] = [1024]*30
        t[210:217] = r[0:7] = [1024, -768, 0, 0, -768, 1024, -1024]
        t[217:226] = r[7:16] = b
        r[16:80], r[80:208] = [-1024]*64, [1024]*128
        t[226:254] = r[208:236] = context
        t[-2] = r[-2] = 1024 if y & 1 else -1024
        t[-1] = r[-1] = 1024 if y & 2 else -1024
        full.update(struct.pack('<494h', *(t+r)))
        bases.update(struct.pack('<9h', *b))
        if y in selected:
            assert selected[y]['Trajectory'] == t and selected[y]['Raster'] == r
            assert selected[y]['Start'] == t[-2] and selected[y]['End'] == t[-1]
    assert v['FeatureSha256'] == full.hexdigest()
    assert v['BaselineSha256'] == bases.hexdigest()
    assert set(v['Guards']) == {'generic-before-point', 'generic-raster-point',
                              'wrong-context', 'over4096', 'cancel-exact-token',
                              'foreign-baseline-refused', 'nonpoint-declines'}
    assert len(v['Guards']) == 7


if __name__ == '__main__':
    audit(json.loads(Path(sys.argv[1]).read_text(encoding='utf-8-sig')))
    print('PASS: 65536 point positions, 131072 complete vectors, 7 guards; no NN')
