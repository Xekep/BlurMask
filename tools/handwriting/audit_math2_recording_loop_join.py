"""Compose the static prerequisites of the bounded cold recording loop.

Reads pinned PE/model bytes only. No SDK, ONNX Runtime or C# execution.
Dependency regions overlap; their counts are not unique coverage totals.
"""
import json
from pathlib import Path
import subprocess
import sys

if len(sys.argv) != 2:
    raise SystemExit('report.json')
ROOT = Path(__file__).resolve().parents[2]
out = Path(sys.argv[1]).resolve()
out.parent.mkdir(parents=True, exist_ok=True)

dependencies = [
    ('audit_math2_outer_driver.py', 1, 45, None),
    ('audit_math2_beam_width_producers.py', 9, 45, ('tableBindings', 13)),
    ('audit_math2_step_temperature.py', 13, 55, ('bindings', 1)),
    ('audit_math2_input_frame_bound.py', 9, 38, ('bindings', 6)),
]
reports = []
for name, regions, pins, binding in dependencies:
    target = out.with_name(out.stem + '-' + Path(name).stem + '.json')
    subprocess.run([sys.executable, str(ROOT / 'tools/handwriting' / name), str(target)], check=True)
    report = json.loads(target.read_bytes())
    value = report['regions']
    assert (len(value) if isinstance(value, list) else value) == regions
    assert report['instructionPins'] == pins
    if binding is not None:
        assert report[binding[0]] == binding[1]
    reports.append(dict(auditor=name, report=str(target), regions=regions, instructionPins=pins))
    if name == 'audit_math2_outer_driver.py':
        assert report['recordingSites'] == ['0x1800390b4', '0x180039325', '0x1800394a6']
    if name == 'audit_math2_beam_width_producers.py':
        decoder = report['models'][2]['config']
        assert decoder['delta_cost_max'] == -1 and decoder['seg_type'] == 'sigmoid'
        assert decoder['need_force_nonobs_positions'] is True
        assert all(not row['configHasBeamSize'] for row in report['models'])

recorder = out.with_name(out.stem + '-completed-recorder.json')
subprocess.run([sys.executable,
    str(ROOT / 'tools/handwriting/audit_math2_completed_recorder_join.py'), str(recorder)], check=True)
record_report = json.loads(recorder.read_bytes())
assert len(record_report['dependencies']) == 5 and not record_report['vendorExecution']

out.write_text(json.dumps(dict(dependencies=reports, completedRecorder=str(recorder),
    scope='Fresh authenticated cold bundle, ordinary EOS and last-position eligibility, '
          'same-Step source leases, ordered old-prefix records and tensor application through maximum',
    numericColdDefaults=dict(width=1, temperature=1, delta=-1),
    boundary='Stops BEFORE039A96 completed ordering, 036F20/EF4 and output',
    ieeeFallback='0393E4 JP enters parent checks for NaN; ordered FMAX equality alone skips',
    runtimeEvidence='Separate official-ORT probe and independent coordinate/record oracle',
    unobserved='Positive owned last-position-empty fallback is not observed in the eight actual ink fixtures',
    missing=['Native CPU transcendental implementation', 'native host progress callback binding',
             'completed168 sorting/truncation/destruction join', 'attention/EF4 and its caller-policy byte',
             'imported/configured/constraint/diagnostic decoder routes', 'formula transport/acceptance'],
    vendorExecution=False), indent=2) + '\n', encoding='utf8')
print('PASS four cold-loop source audits plus five recorder prerequisites; static only, no vendor execution')
