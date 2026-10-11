"""Original interval-distance oracle; no vendor code or model execution."""
import json, random, struct, sys

def f32(x): return struct.unpack('<f', struct.pack('<f', x))[0]
def position(box): return f32(f32(f32(1-f32(.6))*box[0])+f32(f32(.6)*box[2]))

def expected(strokes, markers, height, reorder):
    normal, delayed = [], []
    maximum = -f32(3.4028234663852886e38)
    boxes = []
    for index, (stroke, start) in enumerate(markers):
        end = markers[index+1][1]+1 if index+1 < len(markers) and markers[index+1][0] == stroke else len(strokes[stroke])
        points = strokes[stroke][start:end]
        box = [min(p[0] for p in points), min(p[1] for p in points), max(p[0] for p in points), max(p[1] for p in points)]
        boxes.append(box)
        threshold = f32(maximum - f32(height*f32(.3)))
        if reorder and max(p[0] for p in strokes[stroke]) < threshold: delayed.append(index)
        else: normal.append(index)
        maximum = max(maximum, box[2])
    slots = {}
    for index in delayed:
        target = position(boxes[index])
        # Construct closed admissible intervals for slots. Overlaps collapse
        # to their rounded midpoint. Choose first slot at minimum distance.
        intervals = [(-float('inf'), boxes[normal[0]][0])]
        for left, right in zip(normal, normal[1:]):
            lo, hi = boxes[left][2], boxes[right][0]
            intervals.append((lo, hi) if lo < hi else (f32(f32(lo+hi)*.5),)*2)
        intervals.append((boxes[normal[-1]][2], float('inf')))
        distances = [f32(max(lo-target, target-hi, 0)) for lo, hi in intervals]
        slots[index] = distances.index(min(distances))
    order = list(normal)
    displacement = {}
    # Insert higher slots first. Reverse-coordinate order preserves increasing
    # coordinate order within the same insertion location; exact ties reverse.
    for index in sorted(delayed, key=lambda i: (-slots[i], -position(boxes[i]))):
        at = min(slots[index]+1, len(normal))
        order.insert(at,index); displacement[index]=slots[index]-at
    return [[i, i in delayed, slots.get(i,-1), displacement.get(i,0)] for i in order]

rng = random.Random(0x19f850)
cases = []
for n in range(768):
    strokes=[]; markers=[]
    for stroke in range(rng.randint(1,16)):
        x=rng.randint(-200,200); width=rng.randint(0,70); y=rng.randint(-30,130)
        points=[[x,y],[x+width,y+20],[x+width//2,y-10],[x+width,y]]
        strokes.append(points); markers.append([stroke,0])
        if rng.randrange(3)==0:markers.append([stroke,2])
    height=rng.choice([10,40,80,200]); reorder=n%5!=0
    cases.append(dict(strokes=strokes,markers=markers,height=height,reorder=reorder,expected=expected(strokes,markers,height,reorder)))
# Strict threshold, zero-width overlaps, exact weighted-position ties, late ink.
for strokes in [[[[0,0],[100,20]],[[76,0],[76,20]]], [[[0,0],[100,20]],[[75,0],[75,20]]],
                [[[0,0],[20,20]],[[100,0],[120,20]],[[5,0],[10,20]],[[5,0],[10,20]]]]:
    markers=[[i,0] for i in range(len(strokes))]
    cases.append(dict(strokes=strokes,markers=markers,height=80,reorder=True,expected=expected(strokes,markers,80,True)))
with open(sys.argv[1],'w',encoding='utf-8') as stream:json.dump(cases,stream,separators=(',',':'))
print('Wrote',len(cases),'independent ordinary seed controls')
