"""Pin ten resources and nine unchanged recordings; no model loading or inference."""
import hashlib,json,sys
from pathlib import Path
repo,output=map(Path,sys.argv[1:]);repo=repo.resolve()
assert not output.exists(), 'Fresh output required'
pins=json.loads((Path(__file__).parent/'resource-pins.json').read_bytes());files=[]
for name,pin in pins.items():
    path=repo/pin['relative'];data=path.read_bytes()
    assert len(data)==pin['bytes'] and hashlib.sha256(data).hexdigest()==pin['sha256'],path
    files.append(dict(name=name,path=str(path),bytes=pin['bytes'],sha256=pin['sha256']))
assert len(files)==19
output.write_text(json.dumps(dict(scope='Pinned ten resources and nine unchanged original recordings; DATA only',files=files),indent=2)+'\n',encoding='utf-8')
print('PASS nineteen original pins; no inference')
