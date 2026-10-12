"""Original prepared-glyph line metric oracle using Python double hypot and float32 source data."""
import json, math, random, struct, sys, decimal
from pathlib import Path
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
rng=random.Random(0x60820);out=[]
decimal.getcontext().prec=200
for k in range(512):
 rows=[]
 for i in range(k%17):
  y=f(rng.uniform(-1000,1000));vx=f(rng.uniform(-200,200));vy=f(rng.uniform(-200,200))
  if k<64:vx,vy=rng.choice([(0,0),(3,4),(5,12),(-8,15),(1e-37,-2e-37),(3e37,4e37)])
  if k%3:b,h=f(rng.uniform(-2,2)),f(rng.uniform(0,1.5))
  else:b=h=None
  rows.append([y,f(vx),f(vy),b,h])
 sy=sh=0.
 for y,vx,vy,b,h in rows:
  # A float32 square fits exactly in binary64. Sum exact Decimal squares,
  # round that sum once to binary64, then apply correctly rounded sqrt.
  # This independent oracle has no native zero/exponent/rescaling branches.
  dx=decimal.Decimal.from_float(vx);dy=decimal.Decimal.from_float(vy)
  norm=math.sqrt(float(dx*dx+dy*dy))
  if b is None:sy=(sy+y)+norm;sh+=norm
  else:sy=(sy+b*norm)+y;sh+=h*norm
 out.append(dict(glyphs=rows,expected=None if not rows else[sy/len(rows),sh/len(rows),f(sh/len(rows))]))
for exponent in (-149,-126,-100,-55,-54,-53,-1,0,1,53,54,55,100,127):
 for sign in (-1,1):
  vx=f(sign*2.**exponent);vy=f(1.);dx=decimal.Decimal.from_float(vx);dy=decimal.Decimal.from_float(vy)
  norm=math.sqrt(float(dx*dx+dy*dy));out.append(dict(glyphs=[[0,vx,vy,None,None]],expected=[norm,norm,f(norm)]))
a=Path(sys.argv[1]);a.parent.mkdir(parents=True,exist_ok=True);a.write_text(json.dumps(out,separators=(',',':')),encoding='utf8');print(f'Generated {len(out)} independent prepared glyph-line cases')
