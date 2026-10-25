"""Pure source-cohort corruption controls; no recognition or managed execution."""
from pathlib import Path
import argparse, importlib.util, json, shutil, tempfile

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('repo', type=Path)
p.add_argument('report', type=Path)
a = p.parse_args()
root = a.repo.resolve()
assert not a.report.exists()
helper = root / 'tools/LegacyMathAkFirstAlphabet.Probe/source_contract.py'
spec = importlib.util.spec_from_file_location('numeric_source_contract', helper)
contract = importlib.util.module_from_spec(spec)
spec.loader.exec_module(contract)
contract.authenticate(root)
cohort = contract.NORMALIZED_SOURCE_COHORT
controls = []
with tempfile.TemporaryDirectory(prefix='first276-source-cohort-') as tmp:
    candidate = Path(tmp)
    source = candidate / 'src/InkRecognition'
    source.mkdir(parents=True)
    for path in (root / 'src/InkRecognition').glob('LegacyMathAk*.cs'):
        shutil.copyfile(path, source / path.name)
    contract.authenticate(candidate)
    for key in ['connectedFilename', 'numericalFilename', 'modelStorageFilename', 'normalizationFilename']:
        path = source / cohort[key]
        original = path.read_bytes()
        for name, bad in [('append', original + b'\n// unknown body\n'),
                          ('truncate', original[:-1]), ('missing', None)]:
            try:
                if bad is None:
                    path.unlink()
                else:
                    path.write_bytes(bad)
                try:
                    contract.authenticate(candidate)
                except (AssertionError, FileNotFoundError):
                    controls.append(cohort[key] + ':' + name)
                else:
                    raise AssertionError('Unreviewed source cohort accepted')
            finally:
                path.write_bytes(original)
    contract.authenticate(candidate)
assert len(controls) == 12
a.report.write_text(json.dumps(dict(PASS=True, rejectedSourceMutations=controls,
    managedExecutions=0, nativeExecutions=0), indent=2) + '\n', encoding='utf-8')
print('PASS twelve normalization source-cohort corruptions; zero managed/native calls')
