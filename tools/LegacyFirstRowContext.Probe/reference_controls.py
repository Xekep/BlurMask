"""Own float32/context raster reference; independent integer major-axis coverage."""
import json, math, pathlib, random, struct, sys
def f(x):
    return struct.unpack('<f', struct.pack('<f', x))[0]
def clip(x):
    return max(-1.0, min(1.0, x))
def reference(strokes, height, xr, yr, maximum, pointmean):
    allpoints = [p for stroke in strokes for p in stroke]
    minx, maxx = min(p[0] for p in allpoints), max(p[0] for p in allpoints)
    miny, maxy = min(p[1] for p in allpoints), max(p[1] for p in allpoints)
    rw, rh = float(maxx-minx), float(maxy-miny)
    scale = f(127 / f(max(rw, rh) + f(1e-6)))
    pixel = lambda value, origin: int(f(f(f(value-origin) * scale) + .5))
    covered = set(); arc = 0.0; ysum = 0.0; dirs = [0.0] * 4; physical = []
    for stroke in strokes:
        sa = 0.0
        if pointmean:
            ysum = f(ysum + stroke[0][1])
        for a, b in zip(stroke, stroke[1:]):
            dx, dy = float(b[0]-a[0]), float(b[1]-a[1])
            length = f(math.hypot(dx, dy)); arc = f(arc + length); sa = f(sa + length)
            contribution = b[1] if pointmean else f(f(f(a[1]+b[1]) * .5) * length)
            ysum = f(ysum + contribution)
            for i, delta in enumerate([-dy, dy, -dx, dx]):
                dirs[i] = f(dirs[i] + max(delta, 0))
            ax, ay, bx, by = pixel(a[0], minx), pixel(a[1], miny), pixel(b[0], minx), pixel(b[1], miny)
            if bx < ax:
                ax, bx, ay, by = bx, ax, by, ay
            ddx, ddy = bx-ax, abs(by-ay); sign = 1 if by >= ay else -1
            major, minor = max(ddx, ddy), min(ddx, ddy)
            for step in range(major+1):
                small = (2*minor*step+major)//(2*major) if major else 0
                x, y = (ax+step, ay+sign*small) if ddx >= ddy else (ax+small, ay+sign*step)
                covered.add((x, y))
        physical.append([float(min(p[1] for p in stroke)), float(max(p[0] for p in stroke)-min(p[0] for p in stroke)+1),
                         float(max(p[1] for p in stroke)-min(p[1] for p in stroke)+1), sa])
    if arc != 0:
        dirs = [f(value/arc) for value in dirs]
    if pointmean:
        my = f(ysum/len(allpoints))
    elif arc != 0:
        my = f(ysum/arc)
    else:
        my = f(sum(p[0][1] for p in strokes)/len(strokes))
    rx, ry = pixel(maxx, minx), pixel(maxy, miny)
    vr = sum((x, y) in covered and (x, y-1) not in covered for x in range(rx+1) for y in range(ry+1))
    hr = sum((x, y) in covered and (x-1, y) not in covered for x in range(rx+1) for y in range(ry+1))
    double, four = f(height+height), f(height*4)
    r = [1.0, clip(f(f(miny-yr)/four)), clip(f(f(my-yr)/four)), clip(f(f(f(rw+1)/double)-1)),
         clip(f(f(f(rh+1)/double)-1)), clip(f(f(arc/four)-1)), clip(f(f(f(vr/(rx+1))*.5)-1)),
         clip(f(f(f(hr/(ry+1))*.5)-1))] + dirs
    boundary = max(max(p[0] for p in stroke) if maximum else min(p[0] for p in stroke) for stroke in strokes)
    r.extend([clip(f(f(boundary-xr)/four)), clip(f((strokes[-1][-1][0]-strokes[0][0][0])/double)),
              clip(f((strokes[-1][-1][1]-strokes[0][0][1])/double))])
    r.extend([1.0 if len(strokes) == i else -1.0 for i in [1,2,3]])
    physical.extend([[0.0]*4]*3)
    for i, (top, w, h, a) in enumerate(physical[:3]):
        if i == 0 or top != 0:
            top = f(top-yr)
        r.extend([clip(f(top/four)), clip(f(f(w/double)-1)), clip(f(f(h/double)-1)), clip(f(f(a/four)-1))])
    return r
rnd = random.Random(0x226227); cases=[]
for i in range(512):
    unit=1 if i%4==0 else .125
    strokes=[[[f(rnd.randrange(-640,641)*unit),f(rnd.randrange(-640,641)*unit)] for _ in range(rnd.randrange(2,10))] for _ in range(rnd.randrange(1,4))]
    height=f(rnd.choice([3.125,40.25,128.5])); xr=f(rnd.randrange(-400,400)*.125);yr=f(rnd.randrange(-400,400)*.125)
    maximum,pointmean=bool(i&1),bool(i&2)
    cases.append(dict(strokes=strokes,height=height,xReference=xr,yReference=yr,maximumX=maximum,pointMeanY=pointmean,expected=reference(strokes,height,xr,yr,maximum,pointmean)))
path=pathlib.Path(sys.argv[1]);path.parent.mkdir(parents=True,exist_ok=True);path.write_text(json.dumps(cases,separators=(',',':')),encoding='utf8')
print('Wrote512 own fractional float-context geometry references, independent integer major-axis raster.')
