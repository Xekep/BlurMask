"""Compose pinned native raw-history, cold force and observable-gate proofs.

Vendor files are read only. This does not load a DLL, run inference or prove
managed allocation order. Runtime ownership and numerical reconstruction are
checked by LegacyMathRawAttentionHistory.Probe and its independent auditor.
"""
import hashlib
import json
from pathlib import Path
import subprocess
import sys

if len(sys.argv) != 2:
    raise SystemExit('report.json')

root = Path(__file__).resolve().parents[2]
out = Path(sys.argv[1]).resolve()
out.parent.mkdir(parents=True, exist_ok=True)
dependencies = {}
for name in ('attention_ancestry_static', 'force_policy', 'observable_token_gate'):
    script = root / f'tools/handwriting/audit_math2_{name}.py'
    report = out.with_name(out.stem + '-' + name + '.json')
    subprocess.run([sys.executable, str(script), str(report)], check=True)
    dependencies[name] = dict(script=str(script.relative_to(root)),
        scriptSha256=hashlib.sha256(script.read_bytes()).hexdigest(),
        report=json.loads(report.read_bytes()))

attention = dependencies['attention_ancestry_static']['report']
force = dependencies['force_policy']['report']
gate = dependencies['observable_token_gate']['report']
assert (attention['regions'], attention['instructionPins'], attention['bindings']) == (4, 32, 2)
assert (len(force['regions']), force['instructionPins'], force['bindings']) == (14, 94, 6)
assert force['actualFreshForce'] is True
assert gate['ef3Table'] == [2, 190, 261, 268, 276, 65535, 242, 296]
assert gate['observableIds'] == list(range(2, 262)) + [268, 276, 296]

report = dict(
    scope='Cold mode0 SAME-loop per-time raw attention and historical completed-source reconstruction',
    vendorExecution=False, ortExecution=False, runtimeChangesByAuditor=False,
    dependencies=dependencies,
    nativeProducer='03CAC0 -> 03D196/03D1AA -> Engine185A50(axis2,last) -> EEF.F8 per-time ownership',
    trace='036F20: ancestry[parent][oldPrefixCount], descending time; raw[time][:,ancestor,0]; ancestry[ancestor][time]',
    mask='Actual old-primary EF3 nonobservable positions become F32 -Max, bits FF7FFFFF',
    currentFrame='Current EOS/fallback frame is saved but excluded from that old-prefix matrix',
    adaptation='Bounded managed F32 copies and shared immutable prefix provenance; no ABI pointer/allocation-order equivalence',
    limits=[
        'Dependency regions overlap; their counts are not unique disjoint coverage',
        'Opt-in 16MiB retained-payload budget is portable policy, not a native property or RSS bound',
        'Actual model output is F32[S,B,1]; no general multi-time output or KV retention',
        'Only actual recorded same-loop sources after complete successful recording; no imported or diagnostic alternate histories',
        'No sigmoid, EF4 branch policy, completed-formula output or acceptance join',
    ])
out.write_text(json.dumps(report, indent=2) + '\n', encoding='utf8')
print('PASS raw-history composition: native per-time producer/backward trace, actual cold force and exact EF3 gate; static only')
