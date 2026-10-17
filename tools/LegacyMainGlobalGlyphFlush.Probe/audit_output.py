"""Independent global flush packet and full72 path composition."""
import json,subprocess,sys,tempfile
from pathlib import Path
if len(sys.argv)!=3:raise SystemExit('AK capture.json')
root=Path(__file__).resolve().parents[2];capture=Path(sys.argv[2]);rows=json.loads(capture.read_bytes());assert len(rows)==8
with tempfile.TemporaryDirectory(prefix='global-flush-oracle-',dir=capture.parent) as directory:
 for index,row in enumerate(rows):
  assert len(row['controls'])==64
  path=Path(directory)/f'{index}.json';path.write_text(json.dumps(row),encoding='utf-8')
  subprocess.run([sys.executable,str(root/'tools/LegacyMainGlyphFlush.Probe/audit_output.py'),sys.argv[1],str(path),'--global'],check=True)
print('PASS eight global owners/512 flush controls and32768 numeric packet controls; no callback success or text acceptance inferred.')
