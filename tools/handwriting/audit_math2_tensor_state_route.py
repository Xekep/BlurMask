"""Compose frozen EF1 tensor, EEF score/mask static proofs; no DLL execution."""
import json
import subprocess
import sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('report.json')
out = Path(sys.argv[1])
root = Path(__file__).resolve().parents[2]
out.parent.mkdir(parents=True, exist_ok=True)
reports = {}
for name in ['audit_math2_tensor_lifecycle', 'audit_math2_score_channels', 'audit_math2_restriction_map']:
    target = out.with_name(out.stem + '-' + name + '.json')
    subprocess.run([sys.executable, str(root / 'tools/handwriting' / (name + '.py')), str(target)], check=True)
    reports[name] = json.loads(target.read_bytes())
assert reports['audit_math2_tensor_lifecycle']['instructionPins'] == 124
assert reports['audit_math2_tensor_lifecycle']['bindings'] == 13
assert len(reports['audit_math2_tensor_lifecycle']['models']) == 3
out.write_text(json.dumps(dict(staticDependencies=reports,
    managedAdaptation='Copy-based F32/I64 state; prepared policy and source sidecars; official ORT only',
    vendorExecution=False, ortExecution=False), indent=2) + '\n', encoding='utf8')
print('PASS composed EF1 lifecycle + EEF score-channel/BOS-mask static producers; no SDK/ORT execution')
