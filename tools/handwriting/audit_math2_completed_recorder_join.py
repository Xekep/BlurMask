"""Compose the static native prerequisites for the bounded owned EOS recorder.
Only pinned PE/model metadata are read; runtime execution belongs to the probe.
"""
import json
import subprocess
import sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('report.json')
ROOT = Path(__file__).resolve().parents[2]
out = Path(sys.argv[1]).resolve()
out.parent.mkdir(parents=True, exist_ok=True)
dependencies = [
    ('audit_math2_common_secondary_frontier.py', 6, 83, 3),
    ('audit_math2_completed_record_ownership.py', 8, 47, 3),
    ('audit_math2_completed_library_owner.py', 6, 46, 13),
    ('audit_math2_prefix_planes.py', 12, 116, 5),
    ('audit_math2_force_policy.py', 14, 94, 6),
]
reports = []
for name, regions, pins, bindings in dependencies:
    target = out.with_name(out.stem + '-' + Path(name).stem + '.json')
    subprocess.run([sys.executable, str(ROOT / 'tools/handwriting' / name), str(target)], check=True)
    report = json.loads(target.read_bytes())
    actual_regions = report['regions']
    assert (len(actual_regions) if isinstance(actual_regions, list) else actual_regions) == regions
    assert report['instructionPins'] == pins and report['bindings'] == bindings
    reports.append(dict(auditor=name, report=str(target), regions=regions, pins=pins, bindings=bindings))
out.write_text(json.dumps(dict(dependencies=reports,
    scope='same-Step ordinary EOS -> constructor-owned empty3E -> counted record -> old primary/secondary copy',
    representation='managed semantic owners; native pointer bytes, uninitialized padding and physical allocator order are not projected',
    missing=['imported prefix', 'constraint/diagnostic/optional model variants',
             'maximum-step nonEOS fallback lease', '036F20 attention reconstruction and EF4 outputs',
             'outer completed ranking/transport and accepted formula'],
    vendorExecution=False), indent=2) + '\n', encoding='utf8')
print('PASS five static native prerequisites; ordinary recorder ownership only, no vendor execution')
