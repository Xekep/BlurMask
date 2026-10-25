"""Compiled mixed probabilities and additive calibration regressions, not handwriting accuracy."""
from pathlib import Path
import argparse, hashlib, importlib.util, json, os, struct, subprocess, sys

p = argparse.ArgumentParser(description=__doc__)
p.add_argument('output', type=Path)
p.add_argument('--built-artifacts', type=Path, help='Reuse separately reviewed binaries for local harness validation')
a = p.parse_args()
root = Path(__file__).resolve().parents[2]
out = a.output.resolve()
out.mkdir(parents=True, exist_ok=False)
art = a.built_artifacts.resolve() if a.built_artifacts else out / 'build'
results = []
env = os.environ.copy()
env.update(DOTNET_PROCESSOR_COUNT='2', MSBUILDDISABLENODEREUSE='1', DOTNET_CLI_USE_MSBUILD_SERVER='0', PYTHONDONTWRITEBYTECODE='1')

def run(name, command, timeout):
    with (out / (name + '.log')).open('xb') as log:
        result = subprocess.run(command, cwd=root, env=env, stdin=subprocess.DEVNULL, stdout=log, stderr=subprocess.STDOUT, timeout=timeout, creationflags=subprocess.CREATE_NO_WINDOW if sys.platform == 'win32' else 0)
    results.append(dict(stage=name, exit=result.returncode))
    (out / 'results.json').write_text(json.dumps(results, indent=2) + '\n', encoding='utf-8')
    if result.returncode:
        raise SystemExit(name + ' failed: ' + str(out / (name + '.log')))

tool = root / 'tools/LegacyMixedClassProbabilities.Probe'
calibration = root / 'tools/LegacyMainConfidenceCalibration.Probe'
ak = root / 'models/ru-RU/ru_RU-ak-cur.lite.res'
run('fake-fixture-before-build', [sys.executable, '-I', '-B', str(tool / 'reference_controls.py'), str(out / 'fake-controls.json')], 45)
run('calibration-fixture-before-build', [sys.executable, '-I', '-B', str(calibration / 'reference_controls.py'), str(ak), str(out / 'calibration-controls.json')], 45)

names = ['LegacyMixedClassProbabilities.Probe', 'LegacyInk.Probe', 'LegacyAcousticEnsemble.Probe', 'LegacyMainConfidenceCalibration.Probe']
if not a.built_artifacts:
    for i, name in enumerate(names):
        command = ['dotnet', 'build', str(root / 'tools' / name / (name + '.csproj')), '-c', 'Release', '--artifacts-path', str(art), '--disable-build-servers', '-p:UseSharedCompilation=false', '-m:1', '-nodeReuse:false']
        if i:
            command.append('-p:BuildProjectReferences=false')
        run('build-' + name, command, 600)

library = art / 'bin/InkRecognition/release/InkRecognition.dll'
library_sha = hashlib.sha256(library.read_bytes()).hexdigest()
for name in names:
    assembly = 'ExperimentalGlobalText.Probe' if name == names[0] else name
    dll = art / 'bin' / name / 'release' / (assembly + '.dll')
    assert dll.with_name('InkRecognition.dll').read_bytes() == library.read_bytes(), 'Different library copies'
    args = [str(out / 'fake-controls.json'), str(out / 'capture.json')] if name == names[0] else [str(ak), str(out / 'calibration-controls.json')] if name == names[3] else []
    run('actual-' + name, ['dotnet', str(dll)] + args, 45)
run('independent-data-audit', [sys.executable, '-I', '-B', str(tool / 'audit_output.py'), str(out / 'fake-controls.json'), str(out / 'capture.json'), '--reviewed-library-sha256', library_sha, '--report', str(out / 'audit.json')], 45)

# Validate the dispatch-aware shared auditor using synthetic raw dots only.
# Resource bytes authenticate AL/header policy; no network weights are evaluated.
path = root / 'tools/handwriting/ru_main_mixed_probability.py'
spec = importlib.util.spec_from_file_location('checked_mixed_probability', path)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)
resource = ak.read_bytes()
special = struct.unpack_from('<H', resource, 105)[0]
assert special == 178
probability = module.for_russian_main(resource)
for raw_special, expected in [(0, 16384), (40000000, 32768), (-40000000, 0)]:
    raw = [0] * 180
    raw[special] = raw_special
    actual = probability(raw)
    assert actual[special] == expected and all(value == 185 for i, value in enumerate(actual) if i != special)

audit = json.loads((out / 'audit.json').read_bytes())
assert (audit['mixed'], audit['generic'], audit['sigmoid'], audit['corruptionsRejected']) == (111, 9, 13, 137)
controls = json.loads((out / 'calibration-controls.json').read_bytes())
assert len(controls['rows']) == 16548 and len(controls['endpointRows']) == 1379
(out / 'summary.json').write_text(json.dumps(dict(PASS=True, scope='Compiled probability/calibration regressions; explicit rounded-exp assumption; no recognition-quality claim', librarySha256=library_sha, reusedReviewedArtifacts=bool(a.built_artifacts), mixed=111, generic=9, sigmoid=13, negativeControls=137, calibrationRows=16548, additionalEndpointRows=1379, sharedReferenceFakeVectors=3), indent=2) + '\n', encoding='utf-8')
print('PASS compiled mixed/generic probabilities, independent corruptions, calibration endpoints and existing numeric regressions; no handwriting accuracy claim.')
