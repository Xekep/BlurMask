"""Audit each actual global publication with the independent packed-resource oracle."""
import json,subprocess,sys,tempfile
from pathlib import Path
if len(sys.argv)!=4:raise SystemExit('AK LK capture.json')
root=Path(__file__).resolve().parents[2];capture=Path(sys.argv[3]);rows=json.loads(capture.read_bytes())
assert len(rows)==8
with tempfile.TemporaryDirectory(prefix='global-compact-oracle-',dir=capture.parent) as directory:
 for index,row in enumerate(rows):
  path=Path(directory)/f'{index}.json';path.write_text(json.dumps(row),encoding='utf-8')
  subprocess.run([sys.executable,str(root/'tools/LegacyMainCompactMaterialization.Probe/audit_output.py'),sys.argv[1],sys.argv[2],str(path),'--fresh-case'],check=True)
print('PASS eight actual global compact publications; raw AK/LK, full compact allocator, rows and alternatives independently composed.')
