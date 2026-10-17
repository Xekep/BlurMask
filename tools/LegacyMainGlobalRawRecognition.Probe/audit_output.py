import json,subprocess,sys,tempfile
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('AK capture.json')
p=Path(sys.argv[2]);o=json.loads(p.read_bytes());assert len(o['owners'])==8 and o['entryGuards']==96
assert o['noWinner']==dict(nodes=124,compactRecords=0,hasWinner=False,groups=964)
with tempfile.TemporaryDirectory(prefix='global-raw-audit-',dir=p.parent) as d:
 for i,owner in enumerate(o['owners']):
  assert owner['owner']==i and owner['words']==4 and owner['emptyCallbacks']==4
  q=Path(d)/(str(i)+'.json');q.write_text(json.dumps(owner))
  subprocess.run([sys.executable,str(Path(__file__).with_name('audit_word.py')),sys.argv[1],str(q)],check=True)
print('PASS32 actual context invocations/rawAK full WORD/glyph/range bytes and explicit124-node noWinner; no expected text')
