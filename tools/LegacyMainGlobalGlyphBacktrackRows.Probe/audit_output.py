"""Independent full72 global append-event audit over actual compact/graph input."""
import json,subprocess,sys,tempfile
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('AK capture.json')
root=Path(__file__).resolve().parents[2];capture=Path(sys.argv[2]);rows=json.loads(capture.read_bytes());assert len(rows)==8
with tempfile.TemporaryDirectory(prefix='global-backtrack-oracle-',dir=capture.parent) as directory:
 for index,row in enumerate(rows):
  assert len(row['controls'])==128
  path=Path(directory)/f'{index}.json';path.write_text(json.dumps(row),encoding='utf-8')
  subprocess.run([sys.executable,str(root/'tools/LegacyMainGlyphBacktrackRows.Probe/audit_output.py'),sys.argv[1],str(path)],check=True)
print('PASS eight global compact owners/1024 residue controls; full72 bytes independently reconstructed, no expected text.')
