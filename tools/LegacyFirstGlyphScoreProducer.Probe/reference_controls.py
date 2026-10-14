"""Audit captured real NN inputs using an independent mode0 scalar oracle."""
import json, math, struct, sys
from pathlib import Path
def f(v):return struct.unpack('<f',struct.pack('<f',v))[0]
def bits(v):return struct.unpack('<i',struct.pack('<f',v))[0]
d=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8'));count=0;float_slope_differences=0
for row in d['rows']:
    assert len(row['a'])==len(row['b'])==len(row['expected'])==180
    for symbol,actual in enumerate(row['expected']):
        try:rank=row['order'].index(symbol)
        except ValueError:rank=150
        expected=0
        if rank<20:
            a=f(1/(1+math.exp(-(row['a'][symbol]/32768)*.575)))
            b=f(1/(1+math.exp(-(row['b'][symbol]/32768)*.575)))
            expected=bits(f(f(b+a)/2))
            af=f(1/(1+math.exp(-(row['a'][symbol]/32768)*f(.575))))
            bf=f(1/(1+math.exp(-(row['b'][symbol]/32768)*f(.575))))
            float_slope_differences+=bits(f(f(bf+af)/2))!=expected
        assert actual==expected,(count,symbol,actual,expected)
        count+=1
assert d['groups']==72 and count==d['values']==12960
print(f'PASS independent actual mode0 scalar audit: {count} scores /72 real groups, {float_slope_differences} float-slope disagreements; rank20 cutoff and DOUBLE0.575 preserved.')
