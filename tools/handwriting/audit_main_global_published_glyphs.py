"""Native dependency closure for actual published selection -> global glyphs.
No new native algorithm is claimed by the private old/new owner view.
"""
import json
import subprocess
import sys
import tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
if len(sys.argv)!=2:raise SystemExit('report.json')
out=Path(sys.argv[1]).resolve();reports=[]
with tempfile.TemporaryDirectory(prefix='published-glyph-static-',dir=out.parent) as temporary:
    for name in ('audit_main_global_final_publication.py','audit_main_global_word_utf8.py'):
        path=Path(temporary)/name.replace('.py','.json')
        subprocess.run([sys.executable,str(ROOT/'tools/handwriting'/name),str(path)],check=True)
        reports.append(dict(audit=name,evidence=json.loads(path.read_bytes())))
out.write_text(json.dumps(dict(dependencies=reports,
    ownership='Distinct actual old materialization/new published selection union; exact selection identity, same pass/pool/confidence/lease, no oldterminal construction.',
    scope='Positive published selection only; noWinner refused before glyph-pool creation; fresh global source callbacks/flush/WORD/raw UTF8 unchanged.',
    excluded=['new first273 snapshot/quote/control-import owner','accepted text/quality threshold','physical ring aliases'],
    vendorExecution=False),indent=2)+'\n',encoding='utf-8')
print('PASS finalpool ordering and existing global21FFD0/217DB0/218560/21FE30/rawUTF8 dependency pins; no vendor execution.')
