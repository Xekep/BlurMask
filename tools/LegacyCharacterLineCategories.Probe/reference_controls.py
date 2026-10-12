"""Own prepared source-category -> item -> C1A controls, with independent binary32 preparation."""
import importlib.util, argparse, json
from pathlib import Path
import numpy as np

other = Path(__file__).resolve().parents[1] / 'LegacyCharacterLineFit.Probe/reference_controls.py'
spec = importlib.util.spec_from_file_location('fit_reference', other)
fit = importlib.util.module_from_spec(spec); spec.loader.exec_module(fit)

def prepare(profiles, pair, previous):
    main, alternate = pair
    if main == -1: return [1., 0., 0, previous[3], previous[4], previous[5], previous[6]]
    def profile(index):
        offset, height = profiles[index]
        b = fit.f(-1 - fit.f(offset * 2))
        t = fit.f(fit.f(height * 2) + b)
        return t, b
    mt, mb = profile(main)
    at, ab = profile(alternate) if alternate != -1 else (previous[4], previous[6])
    return [1., 0., 2 if alternate != -1 else 1, mt, at, mb, ab]

def create():
    rng = np.random.default_rng(53511)
    output = []
    for k in range(256):
        profiles = [[fit.f(rng.uniform(-2, 2)), fit.f(rng.uniform(.5, 3))] for _ in range(8)]
        entries = [['symbol'+str(i), int(rng.integers(0, 8)), int(rng.integers(0, 8)) if i%3==0 else -1] for i in range(10)]
        entries += [['symbol0', int(rng.integers(0, 8)), -1], ['unknown-main', -1, 3]]
        alphabet = ['symbol'+str(i) for i in range(10)] + ['SYMBOL0', 'unknown-main']
        catalog = {e[0]: e[1:] for e in entries}
        items, inputs = [], []
        previous = [1.,0.,0,0.,0.,0.,0.]
        for i in range(12):
            label = i if i < 10 else 65535 if i == 10 else 11
            pair = catalog.get(alphabet[label], [-1, -1]) if label != 65535 else [-1, -1]
            candidate = prepare(profiles, pair, previous)
            previous = candidate
            x, scale, intercept = fit.f(i*25+10), 20., 70.
            # Known candidates share a true scale, unknowns remain native zero-profile observations.
            top, bottom = fit.f(intercept-scale*candidate[3]), fit.f(intercept-scale*candidate[5])
            if top > bottom: top, bottom = bottom, top
            b = [fit.f(x-5), fit.f(x+5), top, bottom]
            inputs.append(dict(label=label, bounds=b, candidate=candidate))
            items.append(dict(known=False, marker=False, clip=False, bounds=b, candidates=[candidate]))
        case = dict(profile=[2,20,fit.f(-.4),fit.f(.4),fit.f(1e-6),fit.f(200),3.],
            force=True, incoming=1e100, items=items)
        expected = fit.run(case)
        output.append(dict(profiles=profiles, entries=entries, alphabet=alphabet, inputs=inputs, expected=expected))
    return dict(format='own-category-item-fit-v1', cases=output)

if __name__ == '__main__':
    parser=argparse.ArgumentParser(); parser.add_argument('output',type=Path); args=parser.parse_args()
    data=create(); args.output.parent.mkdir(parents=True,exist_ok=True)
    args.output.write_text(json.dumps(data,separators=(',',':')),encoding='utf8')
    print(f"{len(data['cases'])} original catalog -> prepared-item -> NumPy IRLS controls")
