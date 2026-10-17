"""Static actual published ownership ->13BE80 copied2A import dependencies."""
import json
import subprocess
import sys
import tempfile
from pathlib import Path
root=Path(__file__).resolve().parents[2]
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]).resolve();reports=[]
with tempfile.TemporaryDirectory(prefix='published-import-static-',dir=out.parent) as temporary:
 for name in ('audit_main_global_published_cleanup.py','audit_dynamic_control_import.py'):
  report=Path(temporary)/name.replace('.py','.json')
  subprocess.run([sys.executable,str(root/'tools/handwriting'/name),str(report)],check=True)
  reports.append(dict(audit=name,evidence=json.loads(report.read_bytes())))
out.write_text(json.dumps(dict(dependencies=reports,
 ownership='Private actual old/published snapshot union, original selected identity, same session/pool/head/model and final source query lease.',
 scope='Existing13BE80/13B350 copied pool and120byte WORD/reference snapshots; private fresh control2A, empty alternate arrays, literal spare-byte content unmodeled.',
 excluded=['generic property/associated lifecycle','nonempty alternate or auxiliary vectors','complete native result transport','accepted text','accuracy'],
 noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS actual published ownership/quote/F4 and native copied dynamic2A import dependencies; static vendor reads only')
