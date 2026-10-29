"""Exercise exact restoration and refusal through the real source-contract CLI."""
import argparse
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile

HERE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location('bootstrap_preimage', HERE / 'bootstrap_preimage.py')
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('repo', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    rows = json.loads((HERE / 'bootstrap-successor-preimages.json').read_bytes())['Sources']
    positive = negative = 0
    with tempfile.TemporaryDirectory(prefix='novapen-bootstrap-source-') as temporary:
        root = Path(temporary)
        current = {}
        historical = {}
        for row in rows:
            name, target = row['Relative'], row['HistoricalLF']
            data = (args.repo / name).read_bytes().replace(b'\r\n', b'\n')
            old = module.restore(name, data, target)
            current[name], historical[name] = data, old
            for accepted in (data, data.replace(b'\n', b'\r\n'), old):
                assert module.restore(name, accepted, target) == old
                positive += 1
            for bad_name, bad_data, bad_target in (
                (name, b'!' + data[1:], target), (name, data[:-1], target),
                (name, data + b'!', target), (name, b'!' + old[1:], target),
                (name + '.unknown', data, target), (name, data, '0' * 64),
            ):
                try:
                    module.restore(bad_name, bad_data, bad_target)
                except ValueError:
                    negative += 1
                else:
                    raise AssertionError('Unknown source accepted')

        def check(sources, name, expected):
            for relative, data in sources.items():
                path = root / relative
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_bytes(data)
            result = subprocess.run(
                [sys.executable, '-I', '-B', str(HERE / 'source_contract.py'),
                 str(root), str(root / (name + '.json'))],
                capture_output=True, timeout=15,
            )
            assert (result.returncode == 0) == expected, (name, result.stderr.decode())

        check(current, 'current', True)
        check(historical, 'historical', True)
        positive += 2
        for index, name in enumerate(current):
            altered = dict(current)
            altered[name] = altered[name] + b'!'
            check(altered, 'mutation-' + str(index), False)
            negative += 1

        copied = root / 'bootstrap_preimage.py'
        copied.write_bytes((HERE / 'bootstrap_preimage.py').read_bytes())
        table = root / 'bootstrap-successor-preimages.json'
        table.write_bytes((HERE / table.name).read_bytes() + b' ')
        bad_spec = importlib.util.spec_from_file_location('tampered_bootstrap', copied)
        bad_module = importlib.util.module_from_spec(bad_spec)
        bad_spec.loader.exec_module(bad_module)
        row = rows[0]
        try:
            bad_module.restore(row['Relative'], current[row['Relative']], row['HistoricalLF'])
        except ValueError:
            negative += 1
        else:
            raise AssertionError('Modified inverse table accepted')

    with args.output.open('x', encoding='utf8') as output:
        json.dump(dict(Status='PASS', Positive=positive, Refusals=negative,
                       RuntimeExecuted=False, TableSHA256=module.TABLE_SHA), output, indent=2)
        output.write('\n')
    print(f'PASS {positive} source restoration controls, {negative} refusals; no recognition executed')


if __name__ == '__main__':
    main()
