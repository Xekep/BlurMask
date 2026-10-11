"""Independent prepared unknown-row group controls and actual proposal NN fixture composition."""
import importlib.util
import json
import math
from pathlib import Path
import sys
import numpy as np
f = np.float32

def module():
    spec = importlib.util.spec_from_file_location('pair_reference', Path(__file__).parents[1] / 'LegacyLinePairFeatures.Probe' / 'reference_controls.py')
    value = importlib.util.module_from_spec(spec); spec.loader.exec_module(value); return value

features = module()

def sort_desc(rows):
    if len(rows) < 2: return rows
    rows = rows.copy(); l, r = 0, len(rows)-1; pivot = rows[r//2][2]
    while l <= r:
        while rows[l][2] > pivot: l += 1
        while rows[r][2] < pivot: r -= 1
        if l <= r: rows[l], rows[r] = rows[r], rows[l]; l += 1; r -= 1
    return sort_desc(rows[:r+1])+rows[r+1:l]+sort_desc(rows[l:])

def select(stats, pairs, scale):
    membership = [-1]*len(stats); groups = []; costs = {}
    for a, b, confidence, label in pairs:
        confidence = f(confidence); other = f(f(1.0000001192092896)-confidence)
        logs = (f(math.log(float(confidence))), f(math.log(float(other))))
        costs[(a,b)] = logs[::-1] if label else logs
    pitch = float(scale)*1.4500000476837158
    def center(ids):
        weights = [max(s[6], f(1)) for s in [stats[i] for i in ids]]
        total = f(0)
        for w in weights: total = f(total+w)
        return sum(float(w)/float(total)*float(stats[i][5]) for i,w in zip(ids,weights))
    def near(a,b): return abs(center(a)-center(b)) < pitch
    def join(a,b):
        yes, no = f(0), f(0)
        for x in a:
            for y in b:
                c = costs.get(tuple(sorted([x,y])))
                if c: yes, no = f(yes+c[0]), f(no+c[1])
        if yes != 0 and no != 0 and (f(yes+no) >= -5 or abs(f(no-yes)) < 5.5): return near(a,b)
        return no < yes
    def box(ids):
        return [min(stats[i][j] for i in ids) if j < 2 else max(stats[i][j] for i in ids) for j in range(4)]
    def compatible(a,b):
        a.sort(); b.sort()
        lo = lambda g: g[math.floor(float(f(f(f(len(g))/f(20))+f(.5))))]
        hi = lambda g: g[math.floor(float(f(f(f(len(g))/f(20))*f(19))))]
        if lo(a) < hi(b) and lo(b) < hi(a): return True
        aa, bb = box(a), box(b)
        aw, ah, bw, bh = f(aa[2]-aa[0]), f(aa[3]-aa[1]), f(bb[2]-bb[0]), f(bb[3]-bb[1])
        with np.errstate(divide='ignore', invalid='ignore'):
            if f(aw/ah) < 5 or not(f(bw/bh) >= 5 and abs(f(ah-bh)) <= f(scale*f(.25))): return True
            ox = f(f(bw+aw)-f(max(aa[2],bb[2])-min(aa[0],bb[0])))
            if not(ox > 0 and max(f(ox/bw),f(ox/aw)) >= .75): return True
            oy = f(f(bh+ah)-f(max(aa[3],bb[3])-min(aa[1],bb[1])))
            return bool(max(f(oy/bh),f(oy/ah)) >= .75)
    for a,b,p,label in sort_desc(pairs):
        if label: continue
        ga,gb = membership[a],membership[b]
        if ga == gb and ga >= 0: continue
        if ga < 0 and gb < 0:
            if f(p) <= f(.99) and not near([a],[b]): continue
            groups.append([a,b]); membership[a] = membership[b] = len(groups)-1
        elif ga < 0:
            if join(groups[gb],[a]): groups[gb].append(a); membership[a] = gb
        elif gb < 0:
            if join(groups[ga],[b]): groups[ga].append(b); membership[b] = ga
        else:
            limit = f(scale*f(10))
            if f(stats[a][3]-stats[a][1]) > limit or f(stats[b][3]-stats[b][1]) > limit or not join(groups[ga],groups[gb]) or (f(p) < f(.98) and not near(groups[ga],groups[gb])) or not compatible(groups[ga],groups[gb]): continue
            groups[ga] += groups[gb]
            for i in groups[gb]: membership[i] = ga
            del groups[gb]
            membership = [g-1 if g >= gb else g for g in membership]
    return groups,membership

def main():
    output = Path(sys.argv[1]); output.parent.mkdir(parents=True,exist_ok=True)
    rng = np.random.default_rng(0x7c160); controls = []
    for index in range(384):
        n = 2+index % 11
        points = [rng.uniform(-100,100,(5,2)).astype(np.float32) for _ in range(n)]
        if index % 7 == 0:
            for s,p in enumerate(points): p[:,0] = np.linspace(0,100,5); p[:,1] = f(s % 3*50)
        stats = [features.statistics(p.tolist()) for p in points]
        pairs = []
        for a in range(n):
            for b in range(a+1,n):
                if rng.random() < .3: continue
                p = f([.7,.95,.9799,.9801,.9899,.9901,.999][int(rng.integers(0,7))])
                pairs.append([a,b,float(p),bool(rng.random()<.25)])
        scale = f([10,50,100][index % 3]); groups,membership = select(stats,pairs,scale)
        controls.append(dict(statistics=[s.tolist() for s in stats],pairs=pairs,scale=float(scale),groups=groups,membership=membership))
    if len(sys.argv) > 2:
        proposal = json.loads(Path(sys.argv[2]).read_text())
        for rows,item in enumerate(proposal['controls'][-2:],1):
            stats = [features.statistics(s) for s in item['strokes']]
            pairs = item['neural'][-1]; groups,membership = select(stats,pairs,f(item['scale']))
            controls.append(dict(statistics=[s.tolist() for s in stats],pairs=pairs,scale=item['scale'],groups=groups,membership=membership,strokes=item['strokes'],expectedRows=rows))
            print(f'Own MIR {rows} rows: selected groups={groups}; membership={membership}')
    output.write_text(json.dumps(controls,separators=(',',':')),encoding='utf8')
    print(f'Wrote {len(controls)} independent ordinary group controls.')

if __name__ == '__main__': main()
