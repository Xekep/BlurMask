"""Independent float32 operation oracle for prepared and actual field0 captures."""
import json,math,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
def f(x):
    try:return struct.unpack('<f',struct.pack('<f',x))[0]
    except OverflowError:return math.copysign(math.inf,x)
def frombits(x):return struct.unpack('<f',struct.pack('<I',x&0xffffffff))[0]
def bits(x):return struct.unpack('<I',struct.pack('<f',x))[0]
def oracle(costs,scale):
    if len(costs)==1:return [1.0]
    denominator=f(scale*8192);weights=[];total=0.0
    for cost in costs[:20]:
        exponent=f(-f(cost)/denominator)
        try:value=f(math.exp(exponent))
        except OverflowError:value=math.inf
        weights.append(value);total=f(total+value)
    result=[]
    for value in weights:
        if total==0 or math.isinf(value) and math.isinf(total):result.append(math.nan)
        else:result.append(f(value/total))
    return result+[0.0]*max(0,len(costs)-20)
capture=json.loads(Path(sys.argv[1]).read_bytes());rows=0;maximum=0
cases=capture['preparedNumericalControls']+[g for s in capture['sources'] for g in s['groups']]
for case in cases:
    expected=oracle(case['costs'],frombits(case['scaleBits']))
    assert len(expected)==len(case['weightBits'])
    for e,a in zip(expected,case['weightBits']):
        actual=frombits(a);rows+=1
        if math.isnan(e):assert math.isnan(actual);continue
        if math.isinf(e):assert actual==e;continue
        distance=abs(bits(e)-(a&0xffffffff));maximum=max(maximum,distance)
        assert distance<=2,(case.get('seed',case.get('group')),e,actual,distance)
assert len(capture['preparedNumericalControls'])==320
assert sum(len(s['groups']) for s in capture['sources'])==capture['groups']==92
assert sum(len(g['costs']) for s in capture['sources'] for g in s['groups'])==capture['actualRows']==908
assert capture['guards']==10
print(f'PASS independent1BE530 {len(cases)} cases/{rows} rows, maxFiniteULP={maximum}; singleton/top20/float32 arithmetic, exceptional values preserved; no vendor execution')
