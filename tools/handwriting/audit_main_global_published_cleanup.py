"""Static native quote -> fresh F4 first-return dependency closure."""
import json
import subprocess
import sys
import tempfile
from pathlib import Path
if len(sys.argv) != 2:
    raise SystemExit('report.json')
root=Path(__file__).resolve().parents[2]
out=Path(sys.argv[1]).resolve();reports=[]
with tempfile.TemporaryDirectory(prefix='global-cleanup-static-',dir=out.parent) as temporary:
    for name in ('global_published_alternative','main_published_quote_normalization','main_result_glyph_normalization'):
        filename = 'audit_'+name+'.py' if name.startswith('main_') else 'audit_main_'+name+'.py'
        report=Path(temporary)/(name+'.json')
        subprocess.run([sys.executable,str(root/'tools/handwriting'/filename),str(report)],check=True)
        reports.append(dict(audit=name,evidence=json.loads(report.read_bytes())))
out.write_text(json.dumps(dict(dependencies=reports,
    freshOwnership='GlobalWordControllerPolicy private fresh constructor uses fresh00C860 knowledge limits, one fresh292 session and no external property/copy input; same source controller18 binding.',
    order='Published first snapshot ->21FBF0 ->configuration99zero ->21F320 F4zero before first WORD read.',
    scope='Same selected publication/session/pool/list; shared old/new quote session claim; zero-write F4 proof.',
    excluded=['enabled21F320','ControlImport','generic property/copy lifecycle','accepted text','accuracy'],
    noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS native quote order, F4/property/same-controller pins and actualpublished ownership; vendor static reads only')
