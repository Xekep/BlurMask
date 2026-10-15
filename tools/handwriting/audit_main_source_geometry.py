"""Read-only native geometry, source-kind and alphabet-flag ownership pins."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();e=Binary('MyScriptEngine.dll');f=functions()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
assert hashlib.sha256(e.data).hexdigest()=='d31abcd309cbb558bb80bf50fe1968921de515bc40fbeb0e886a5cf14dbc0a5c'
pins=[(0x1800127f0,0x180012890,'e9a061b5d9fda2e3599898997195d671f0c5d6b384142a08c7e203c4bb0c1ae5'),
 (0x18019c4e0,0x18019c970,'7b7d5f0baf79c5e40b61d128f2dbc688653e26de2311096295f9ec22d5405d97'),
 (0x180168a70,0x180168b40,'b018224c9893a95fb977797fa61bda236af5d0e909d90130bcaf200cc537db06'),
 (0x1801a79e0,0x1801a7a50,'73ad62dd6954d2a37c60c9cfb77fbd979f83b623471bf131bfb1a55fb9e2a879'),
 (0x1801a7d10,0x1801a7dc0,'f93afc7b64d1374dc9594461f7dc4a224e9655936d77849d54be9bb662d5f31d')]
for a,z,h in pins:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
assert hashlib.sha256(e.data[e.off(0x180052b50):e.off(0x180052b80)]).hexdigest()=='933bf72aa47b6d54a2a25ac5b79ce13db5c913432383073a2088788253565800'
assert b.qwords(0x1802e1ff0+0x98,1)[0]==0x1801a79e0 and b.qwords(0x1802e1ff0+0xd8,1)[0]==0x1801a7d10
assert e.qwords(0x1801de1b0+0x40,1)[0]==0x180052b50
assert '*(bool *)(lVar1 + 0xc1) = *(int *)(param_2 + 0x10) == 0x134' in f['180168a70']
assert 'iVar2 = *(int *)(*plVar6 + 4 + uVar5 * 0xc)' in f['1801a7d10']
assert 'iVar2 == 0' in f['1801a7d10']
compact=f['18019c4e0']
for field in ['puVar7 + 0x14','puVar7 + 0x16','puVar7 + 0x18','puVar7 + 0x1a']:
 assert field in compact
assert 'return *(undefined4 *)(lVar3 + 0x10)' in f['180152280']
out=Path(sys.argv[1]);out.parent.mkdir(parents=True,exist_ok=True)
out.write_text(json.dumps(dict(textPins=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in pins],engineFlagPin='052B50',
 bindings=3,scope='actual ordinary assigned descriptor; immutable point/range copy; original guide; source135 C1false; higher groups remain separate',
 noVendorExecution=True),indent=2)+'\n',encoding='utf-8')
print('PASS static5 text ranges/1 Engine flag range/3 source and alphabet slots; actualC1, compactbbox, line and topmark contracts')
