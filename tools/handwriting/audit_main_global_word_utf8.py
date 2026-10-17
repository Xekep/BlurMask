"""Native dependencies of the actual global plain WORD-to-UTF8 adapter."""
import json
import subprocess
import sys
from pathlib import Path

if len(sys.argv) != 2:
    raise SystemExit('report.json')
root = Path(__file__).resolve().parents[2]
out = Path(sys.argv[1]).resolve()
reports = []
for name in ('global_glyph_execution', 'word_utf8_projection'):
    path = out.with_name(out.stem+'-'+name+'.json')
    subprocess.run([sys.executable,str(root/'tools/handwriting'/('audit_main_'+name+'.py')),str(path)],check=True)
    reports.append(dict(audit=name,evidence=json.loads(path.read_bytes())))
out.write_text(json.dumps(dict(dependencies=reports,
    scope='same executed global callback, actual pool40/reference order, source keys and ordinary alphabet bytes',
    excluded=['control/alternate alphabet substitution','raw document endpoint conversion','final accepted text'],
    noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS actual global publication and native glyph-token/source-reference dependency pins; static only')
