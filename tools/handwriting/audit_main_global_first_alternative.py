"""Static dependencies for the bounded actual global postcallback snapshot."""
import json
import subprocess
import sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('report.json')
root = Path(__file__).resolve().parents[2]
out = Path(sys.argv[1]).resolve()
reports = []
for name in ('global_result_ownership', 'word_publication', 'first_alternative', 'result_score'):
    report = out.with_name(out.stem+'-'+name+'.json')
    subprocess.run([sys.executable, str(root/'tools/handwriting'/('audit_main_'+name+'.py')), str(report)], check=True)
    data = json.loads(report.read_bytes())
    assert data['noVendorExecution'] is True
    reports.append(dict(audit=name, evidence=data))
out.write_text(json.dumps(dict(dependencies=reports,
    scope='Actual completed global callback tokens, typed publisher/beam identities, explicit400-byte constructor/score snapshots',
    excluded=['native whole allocation/failure ordering', 'quote normalization', 'multiple alternatives', 'document acceptance', 'recognition accuracy'],
    noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS actual global ownership/publication/first alternative/score native dependency pins; no complete result claim')
