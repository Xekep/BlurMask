"""Independent high precision sigmoid plus separate-F32-operation controls."""
import base64,decimal,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
r=json.loads(Path(sys.argv[1]).read_bytes());assert r['guards']==10 and len(r['cases'])==4
decimal.getcontext().prec=75
D=decimal.Decimal
def decode(text):
    raw=base64.b64decode(text,validate=True)
    assert len(raw)%4==0
    return struct.unpack('<'+'f'*(len(raw)//4),raw)
def f32(value):
    try:return struct.unpack('<f',struct.pack('<f',value))[0]
    except OverflowError:return float('inf')
count=0;fold_discriminators=0
for case in r['cases']:
    inputs,outputs=decode(case['input']),decode(case['output'])
    assert len(inputs)==len(outputs)==case['symbols']*case['segments']
    for x,y in zip(inputs,outputs):
        assert 0<=y<=1
        if x < -200: assert y==0;count+=1;continue
        if x > 200: assert y==1;count+=1;continue
        exponential=(-D.from_float(x)).exp()
        expected=f32(1/f32(f32(float(exponential))+1))
        exact=float(1/(1+exponential))
        assert abs(y-expected)<=2e-7,(x,y,expected)
        assert abs(y-exact)<=2e-7,(x,y,exact)
        if expected!=f32(exact):fold_discriminators+=1
        count+=1
assert count==1894 and fold_discriminators>50
print(f'PASS{count} prepared sigmoid values/10 guards;{fold_discriminators} separate-F32 versus folded controls; portable EXP tolerance2e-7, no vendor equivalence')
