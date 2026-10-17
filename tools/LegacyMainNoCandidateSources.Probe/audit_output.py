"""Independent bounded no-bit8 singleton source completion composition."""
import base64,hashlib,importlib.util,json,struct,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('capture.json')
root=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('primary_al',root/'tools/LegacyRussianGlyphSymbol.Probe/audit_output.py');al=importlib.util.module_from_spec(spec);spec.loader.exec_module(al)
resource=(root/'models/ru-RU/ru_RU-ak-cur.lite.res').read_bytes()
assert hashlib.sha256(resource).hexdigest()=='7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978'
alphabet=al.read_alphabet(resource);assert not set('\u2610\u2611\u2612')&set(alphabet.values())
data=json.loads(Path(sys.argv[1]).read_text());total=0;source_count=0
for case in data['inputs']:
    compact=base64.b64decode(case['compact']);assert len(compact)==case['count']*40
    assert all(not compact[i+32]&8 for i in range(0,len(compact),40))
    before=base64.b64decode(case['before']);after=base64.b64decode(case['after']);assert len(before)==len(after)==120
    expected=bytearray(before);struct.pack_into('<ff',expected,16,1.0,1.0)
    assert bytes(expected)==after and case['completed']
    #The singleton rank has no comparison, pruning, payload swap or279 remap.
    #Its native score finalizer writes exactly1 to both channels, independent
    #of the initial finite score. Everything else remains byte-identical.
    assert struct.unpack_from('<I',before,40)[0]==len(case['references'])
    source=base64.b64decode(case['sources26']);assert len(source)==case['sourceCount']*26
    for i in range(case['sourceCount']):
        a=struct.unpack_from('<iii',source,i*24);b=struct.unpack_from('<iii',source,i*24+12)
        assert a[0]==b[0]==0 and all(v>=0 for v in a[1:]+b[1:])
    total+=case['count'];source_count+=case['sourceCount']
assert len(data['inputs'])in(7,9)and data['guards']==len(data['inputs'])*5
print(f'PASS independent{len(data["inputs"])} actual singleton sources/{total} real active no-bit8 rows/{source_count} source references; whole120B finalization exact, RU checkbox labels absent')
