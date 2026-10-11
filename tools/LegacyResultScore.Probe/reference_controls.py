"""Original independent float32 controls for the checked result scalar stage."""
import argparse, json, math, random, struct

def f32(x):
    try:
        return struct.unpack('<f',struct.pack('<f',x))[0]
    except OverflowError:
        return math.copysign(math.inf,x)
def bits(x): return struct.unpack('<I',struct.pack('<f',x))[0]
def evaluate(path,candidate,accounted,node,extra,multiplier,language):
    normalized=f32(f32(f32(path+candidate)-f32(accounted))/f32((node+1+extra)*8192))
    lang=f32(f32(language)*f32(1/8192))
    log=f32(-f32(multiplier)*normalized)
    mass=0.0 if log < -75 else f32(3.73e32) if log > 75 else f32(math.exp(log))
    return [bits(v) for v in [normalized,lang,log,mass]]

p=argparse.ArgumentParser();p.add_argument('output');args=p.parse_args()
r=random.Random(225);cases=[]
for index in range(768):
    path=r.randrange(-67000000,67000001);candidate=r.randrange(-67000000,67000001)
    accounted=r.choice([r.randrange(-134000000,134000001),r.randrange(-(1<<62),1<<62)])
    node=r.randrange(0,262142);extra=r.randrange(2);multiplier=r.choice([.3,1,5,100,.01]);language=r.randrange(-67000000,67000001)
    row=[path,candidate,accounted,node,extra,multiplier,language]
    cases.append(dict(input=row,expected=evaluate(*row)))
# Clamp equality is significant: exp(75) differs from the upper constant.
for path in [-614401,-614400,-614399,614399,614400,614401,0]:
    row=[path,0,0,0,0,1,0];cases.append(dict(input=row,expected=evaluate(*row)))
# Independent casts differ from integer subtraction before conversion.
for row in [[16777217,0,16777216,0,0,5,1],[16777219,0,16777218,0,0,5,-1],[-16777217,0,-16777216,0,0,1,8192]]:
    cases.append(dict(input=row,expected=evaluate(*row)))
with open(args.output,'w',encoding='utf8') as file: json.dump(cases,file)
print(f'Wrote {len(cases)} original controls.')
