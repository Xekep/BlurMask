"""Own probabilities/calibration, separate double logistic and float32 sum."""
import json,math,random,struct,sys
from pathlib import Path
def f32(v):return struct.unpack('<f',struct.pack('<f',v))[0]
rng=random.Random(0xbcb);cases=[]
for case_index in range(640):
    width=rng.randrange(1,65);families=[]
    for slot in range(7):
        if slot and rng.random()<.4:families.append(None);continue
        families.append(dict(probabilities=[rng.randrange(32768)for _ in range(width)],
            centers=[f32(rng.uniform(-.5,1.5))for _ in range(width)],
            slopes=[f32(rng.choice([.575,.00001,1,10,30,100,1000,rng.uniform(.01,100)]))for _ in range(width)]))
    expected=[]
    for label in range(width):
        confidence=[]
        for family in families:
            if family is None:confidence.append(0.);continue
            exponent=-(family['probabilities'][label]/32768-family['centers'][label])*family['slopes'][label]
            confidence.append(0. if exponent>=math.log(3.4028234663852886e38)else f32(1/(1+math.exp(exponent))))
        total=f32(confidence[1]+confidence[0])
        for value in confidence[2:]:total=f32(total+value)
        expected.append(min(1.,f32(total/sum(f is not None for f in families))))
    cases.append(dict(families=families,expected=expected))
path=Path(sys.argv[1]);path.parent.mkdir(parents=True,exist_ok=True)
path.write_text(json.dumps(dict(cases=cases),separators=(',',':')),encoding='utf8');print(f'Wrote{len(cases)} own calibrated ensemble controls')
