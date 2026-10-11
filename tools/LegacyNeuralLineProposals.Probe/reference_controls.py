"""Own float32 proposal references, native equal-key partition order and NumPy NN composition.

The neural oracle composes the independent existing feature and network readers,
then drops the self-pair BEFORE affine/BLSTM inference. No vendor DLL runs.
"""
import importlib.util
import json
import math
from pathlib import Path
import sys
import numpy as np
f = np.float32

def module(name, folder):
    spec = importlib.util.spec_from_file_location(name, Path(__file__).parents[1] / folder / 'reference_controls.py')
    value = importlib.util.module_from_spec(spec); spec.loader.exec_module(value); return value

features = module('pair_reference', 'LegacyLinePairFeatures.Probe')
network = module('line_reference', 'PreparedLineClassifier.Probe')

def partition_sort(ids, stats, channel):
    # Native middle pivot exchanges equal keys. Copy/sublist recursion is an
    # independent implementation; stable argsort would change equal-key context.
    if len(ids) < 2: return ids
    ids = ids.copy(); l, r = 0, len(ids)-1; pivot = stats[ids[r//2]][channel]
    while l <= r:
        while stats[ids[l]][channel] < pivot: l += 1
        while stats[ids[r]][channel] > pivot: r -= 1
        if l <= r: ids[l], ids[r] = ids[r], ids[l]; l += 1; r -= 1
    return partition_sort(ids[:r+1], stats, channel)+ids[r+1:l]+partition_sort(ids[l:], stats, channel)

def prepare(stats, active, nominal, overlap):
    left = min(s[0] for s, a in zip(stats, active) if a)
    right = max(s[2] for s, a in zip(stats, active) if a)
    extent = f(f(right-left)+f(.01)); width = min(extent, nominal)
    step = f(f(f(1)-overlap)*width)
    q = f(f(extent-f(width-step))/step); count = math.floor(float(q))
    tail = 0 if nominal <= extent else -1
    if tail >= 0 and overlap <= f(q-f(count)): tail = 1
    count = max(count, 1)
    windows = [[f(f(i)*step+left), f(f(f(i)*step+left)+width)] for i in range(count)]
    if tail == 0: windows[-1][1] = right
    if tail > 0: windows += [[f(right-width), right]]
    if len(windows) > 1: windows += [[left, right]]
    proposals = []
    for w, (lo, hi) in enumerate(windows):
        ids = [i for i, s in enumerate(stats) if active[i] and s[0] <= hi and lo <= s[2]]
        if not ids: continue
        if w != len(windows)-1 or len(windows) == 1:
            for order, channel in enumerate([5, 1, 3]):
                proposals.append(dict(window=w, ordering=order, ids=partition_sort(ids, stats, channel)))
        proposals.append(dict(window=w, ordering=3, ids=ids))
    return float(width), proposals

def predict(stats, proposals, scale, width, pair):
    result = {}
    for p in proposals:
        ids = p['ids']
        if len(ids) < 2: continue
        # Avoid even constructing the self row in this independent oracle.
        rows = np.array([features.pair(stats[a], stats[b], a, b, scale, width, []) for a, b in zip(ids, ids[1:])])
        net, affine, means, scales = pair[0 if p['ordering'] != 3 else 1]
        hidden = np.tanh(np.array([network.reference.affine(affine, x) for x in (rows-means)*scales])).astype(np.float32)
        scores, _ = network.reference.infer(net, hidden)
        for a, b, row in zip(ids, ids[1:], scores):
            key = tuple(sorted([a, b])); label = int(np.argmax(row)); confidence = float(row[label])
            if key not in result or result[key][0] < confidence: result[key] = (confidence, bool(label))
    return [[*key, *value] for key, value in result.items()]

def main():
    output = Path(sys.argv[1]); output.parent.mkdir(parents=True, exist_ok=True)
    own = [output.with_name(f'own-proposal-{h}.res') for h in [30, 50]]
    for h, path in zip([30, 50], own): network.synthetic(h, path)
    paths = [own]
    if len(sys.argv) == 4: paths += [[Path(sys.argv[2]), Path(sys.argv[3])]]
    nets = [[network.load(p) for p in pair] for pair in paths]
    rng = np.random.default_rng(0x73610); controls = []
    for index in range(128):
        n = 1+index % 9; strokes = []
        for s in range(n):
            points = np.array([[s*25+dx, (s % 3)*40+dy] for dx, dy in rng.uniform(0, 20, (4, 2))], dtype=np.float32)
            if index % 11 == 0: points[:, 1] = f(20)  # Native equal-key sort controls.
            strokes.append(points.tolist())
        active = [bool(index % 4 != 0 or s % 2 == 0) for s in range(n)]
        stats = [features.statistics(p) for p in strokes]
        nominal = f([40, 100, 500, 5][index % 4]); overlap = f([0, .25, .75][index % 3]); scale = f(20)
        width, proposals = prepare(stats, active, nominal, overlap)
        row = dict(strokes=strokes, active=active, nominalWidth=float(nominal), overlap=float(overlap), scale=float(scale), windowWidth=width, proposals=proposals)
        if index < 12: row['neural'] = [predict(stats, proposals, scale, f(width), pair) for pair in nets]
        controls.append(row)
    # Original own printed MIR physical strokes, plus a second translated row.
    # These exercise actual proposal ordering/context; they are not a corpus.
    mir = [[[0,90],[0,10],[25,60],[50,10],[50,90]],
           [[70,10],[70,90],[120,10],[120,90]],
           [[140,90],[140,10],[180,10],[190,20],[190,40],[180,50],[140,50]]]
    for rows in [1, 2]:
        strokes = [np.array(s, dtype=np.float32).__add__(np.array([0, 200*r], dtype=np.float32)).tolist() for r in range(rows) for s in mir]
        stats = [features.statistics(p) for p in strokes]; active = [True]*len(strokes)
        width, proposals = prepare(stats, active, f(1000), f(.75))
        predictions = [predict(stats, proposals, f(80), f(width), pair) for pair in nets]
        controls.append(dict(strokes=strokes, active=active, nominalWidth=1000, overlap=.75, scale=80, windowWidth=width,
            proposals=proposals, neural=predictions))
        if len(paths) > 1: print(f'Own printed MIR rows={rows}: actual pair evidence={predictions[-1]}')
    output.write_text(json.dumps(dict(models=[[str(p.resolve()) for p in pair] for pair in paths], controls=controls), separators=(',', ':')), encoding='utf8')
    print(f'Wrote {len(controls)} independent proposal cases with {len(paths)} 30/50 model pairs.')

if __name__ == '__main__': main()
