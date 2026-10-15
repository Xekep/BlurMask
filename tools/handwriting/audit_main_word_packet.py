"""Static PE reads only: the21FE30 packet-to-WORD stores."""
import hashlib,json,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary
if len(sys.argv)!=2:raise SystemExit('report.json')
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x18021fe4c,0x18021fec0
h='3169ad28b74dd2a15d3a6dd36509cae181d4259e1eb2791c797adaa0d929579b'
assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins=[(0x18021fe6b,'894510'),(0x18021fe72,'894518'),(0x18021fe79,'89451c'),
 (0x18021fe81,'66894530'),(0x18021fe8a,'66894532'),(0x18021fe93,'884535'),
 (0x18021fe9b,'884536'),(0x18021fea2,'89453c'),
 (0x18021fea5,'410f10401c'),(0x18021feb4,'0f114544'),(0x18021febd,'884537'),
 (0x18021fec7,'418b403c'),(0x18021fecf,'418b4038'),(0x18021fed7,'410fb7402e')]
for addr,raw in pins:assert b.data[b.off(addr):b.off(addr)+len(raw)//2].hex()==raw
Path(sys.argv[1]).write_text(json.dumps(dict(start=hex(a),end=hex(z),sha256=h,
 pins=len(pins),scalarStores=9,vectorStoreBytes=16,
 boundaryHints='packet38/3C are218560 arguments; not TOP.AC/B0',
 scope='explicit packet fields only; no segment or accepted publication',noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS pinned21FE30 field-store block/14 pins; boundary hints remain separate from TOP fields')
