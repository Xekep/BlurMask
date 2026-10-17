"""Static native quote→conditionalF4→later numerical publication call order."""
import hashlib
import json
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(root/'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2: raise SystemExit('report.json')
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [(0x18022198e,0x1802219b2,'5f0b335ac1b48060718aeb39a7bd4caa44ce8503a1fbd70e72d01f1b683cdd46'),
           (0x180227c9f,0x180227cc4,'bce059fc92de794496be8d8ed6cdf673e6ef1ddea8e10551024a9c992a61dab3')]
for start,end,sha in regions:
    assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == sha
pins = [(0x18022198e,'e85de2ffff'), (0x180221993,'4c8b57184180ba99000000007543'),
        (0x1802219ad,'e86ed9ffff'), (0x18022784c,'e8ef9fffff'),
        (0x180227c9f,'f30f107808'), (0x180227cbd,'4c8b4060'), (0x180227cc1,'41ffd0'),
        (0x18021f344,'80b8f4000000000f8488080000')]
for at,raw in pins: assert b.data[b.off(at):b.off(at)+len(raw)//2].hex() == raw
assert b.qwords(0x1802dc740+0x60,1)[0] == 0x18013cba0
code = (root/'src/InkRecognition/LegacyFreshMainFirstCandidateCompletion.cs').read_text()
calls = ['LegacyOwnedMainFirstAlternativeSnapshot.Capture(', 'LegacyOwnedMainPublishedQuoteNormalization.Apply(',
         'normalization.Apply(', 'LegacyOwnedMainWordUtf8Projection.Project(']
positions = [code.index(c) for c in calls]
assert positions == sorted(positions)
report = dict(regions=[dict(start=hex(a),end=hex(z),sha256=s) for a,z,s in regions],pins=len(pins),
              binding='273.V60=13CBA0', managedCompositionOrder=calls,
              scope='Post-callback snapshot; pure numeric bytes precomputed, actual quote prefix retained on failure; raw UTF8 only',
              excluded=['native whole allocation/failure order','duplicate/exclusion/rescue','enabled21F320','accepted wrapper text'],
              noVendorExecution=True)
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print('PASS2 PE regions/8 instruction pins/273.V60 binding and bounded managed stage order; no full-wrapper claim.')
