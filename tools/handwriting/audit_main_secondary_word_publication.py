"""Static owning configured selection->221840/21FFD0->ordinary WORD join."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[
 (0x180227760,0x180227850,'a0e91b91980814fd888a9f90d21d22ad670a5becfabe039a9ad6f3c5ef877212'),
 (0x1802218d0,0x18022193a,'4d6d47fae46b4910b7a46deb151e2b05c106893146ebb656d6686d3fd046fadb'),
 ]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins=[
 (0x180227767,'baffff0000'), # mov edx, 0xffff
 (0x1802277ab,'4585ff'), # test r15d, r15d
 (0x1802277ae,'756d'), # jne 0x18022781d
 (0x1802277d1,'6689542428'), # mov word ptr [rsp + 0x28], dx
 (0x1802277dd,'e85ea0ffff'), # call 0x180221840
 (0x180227814,'66c7442428ffff'), # mov word ptr [rsp + 0x28], 0xffff
 (0x18022783d,'6689542428'), # mov word ptr [rsp + 0x28], dx
 (0x18022784c,'e8ef9fffff'), # call 0x180221840
 (0x18022192c,'0fb74567'), # movzx eax, word ptr [rbp + 0x67]
 (0x180221930,'6689442420'), # mov word ptr [rsp + 0x20], ax
 (0x180221935,'e896e6ffff'), # call 0x18021ffd0
 ]
for a,raw in pins:assert b.data[b.off(a):b.off(a)+len(raw)//2].hex()==raw,hex(a)
report=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in regions],pins=len(pins),
 noVendorExecution=True,covered='same actual configured matrix terminal ->shared source numeric/WORD kernels; initialFFFF, first-only width prepass',
 excludes=['subtype6 positive inferred path','accepted output loop','native rescue','273 alternative mass normalization','app acceptance'])
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS two caller/forwarding regions/{len(pins)} pins, initialFFFF not secondaryordinal and TOP.B0 first-only prepass.')
