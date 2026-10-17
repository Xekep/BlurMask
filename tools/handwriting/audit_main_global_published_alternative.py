"""Static dependency closure for postpublication global first273 snapshot."""
import json
import subprocess
import sys
import tempfile
from pathlib import Path
if len(sys.argv) != 2:
    raise SystemExit('report.json')
root = Path(__file__).resolve().parents[2]
out = Path(sys.argv[1]).resolve(); reports = []
with tempfile.TemporaryDirectory(prefix='published-score-static-',dir=out.parent) as temporary:
    for name in ('global_published_glyphs','global_first_alternative'):
        report = Path(temporary)/(name+'.json')
        subprocess.run([sys.executable,str(root/'tools/handwriting'/('audit_main_'+name+'.py')),str(report)],check=True)
        reports.append(dict(audit=name,evidence=json.loads(report.read_bytes())))
out.write_text(json.dumps(dict(dependencies=reports,
    ownership='Exact published selection, final compact pool/row22, post207CB0 row88, same completed callback session and native WORD head.',
    scope='Single fresh global postcallback first273 snapshot: explicit400 allocation, constructor/reference/integer-sub score/normalization.',
    excluded=['native partial allocation failure ordering','quote normalization','ControlImport','multiple alternatives','accepted text','accuracy'],
    noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS published207CB0-before-selection + callback ownership + native first273/score pins; no vendor execution')
