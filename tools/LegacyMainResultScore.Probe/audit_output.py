"""Compare actual numerical captures with independently generated controls.
Only calculated NaN payloads and platform exp/expf rounding have allowances;
all nonchannel bytes and signed-zero/exact operation results stay exact.
"""
import json
import math
import struct
import sys
from pathlib import Path

if len(sys.argv) != 3:
    raise SystemExit('controls.json capture.json')
c = json.loads(Path(sys.argv[1]).read_bytes())
o = json.loads(Path(sys.argv[2]).read_bytes())
def val(bits): return struct.unpack('<f', struct.pack('<I', bits))[0]
def check(actual, expected, ulp=0):
    a, b = val(actual), val(expected)
    if math.isnan(b): assert math.isnan(a); return
    if actual == expected: return
    assert math.isfinite(a) and math.isfinite(b)
    assert not (actual ^ expected) & 0x80000000 and abs(actual-expected) <= ulp, (hex(actual), hex(expected), ulp)
def storage(actual, expected, count, scalar):
    a, b = bytes.fromhex(actual), bytes.fromhex(expected)
    assert len(a) == len(b)
    at = 0
    while at < len(a):
        row, field = divmod(at, 40)
        active = row < count and field in ((8, 16, 20) if scalar else (8, 12))
        if active:
            allowed = 2 if (field == 8 if scalar else field == 12) else 0
            check(struct.unpack_from('<I', a, at)[0], struct.unpack_from('<I', b, at)[0], allowed)
            at += 4
        else:
            assert a[at] == b[at], ('preserved allocator byte', at)
            at += 1
    return len(a)

assert len(c['scalar']) == len(o['scalar']) and len(c['normalization']) == len(o['normalization'])
total = 0
for reference, observed in zip(c['scalar'], o['scalar']):
    expected = reference['expected']
    assert observed['difference'] == expected[0]
    for key, bits, ulp in zip(['path', 'row14', 'log', 'mass'], expected[1:], [0, 0, 0, 2]): check(observed[key], bits, ulp)
    total += storage(observed['row'], reference['row'], 1, True)
for reference, observed in zip(c['normalization'], o['normalization']):
    check(observed['top'], reference['topBits'])
    if reference['count'] == 0:
        assert observed['first'] is None and observed['top'] == reference['oldTopBits']
    else: assert observed['first'] == reference['rawFirstBits']
    total += storage(observed['storage'], reference['storage'], reference['count'], False)
assert o['castOrderDisagreements'] == c['castOrderDisagreements'] > 0 and o['guards'] == 10
print(f'PASS independent{len(c["scalar"])} SUB-before-float/{len(c["normalization"])} normalizations/{total} complete row bytes; {c["castOrderDisagreements"]} cast-order distinctions, no source-owner/acceptance claim.')
