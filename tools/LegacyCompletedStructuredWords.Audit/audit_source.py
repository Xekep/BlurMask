import hashlib,json,sys
from pathlib import Path
D=Path(__file__).resolve().parent;root=Path(sys.argv[1]).resolve();pins=json.loads((D/'source-pins.json').read_text(encoding='utf-8'));sha=lambda b:hashlib.sha256(b).hexdigest()
for name,want in pins['lfSuccessors'].items():
 text=(root/'src/InkRecognition'/name).read_text(encoding='utf-8-sig').replace('\r\n','\n');assert sha(text.encode())==want,('unreviewed successor',name)
 if name in pins['exactOwnGetterReversals']:
  rule=pins['exactOwnGetterReversals'][name]
  for line in rule['removeExactly']:assert text.count(line)==1;(text:=text.replace(line,''))
  assert sha(text.encode())==rule['oldLfSha256'],('old exact getter-only preimage differs',name)
print('PASS7 exact successor LF source pins +2 exact own getter-only preimage reversals; no source mutation')
