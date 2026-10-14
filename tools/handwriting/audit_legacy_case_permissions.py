"""Pinned static234840 triplet permissions and actual TCM version lineage."""
import hashlib,json,struct,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
if len(sys.argv)!=2 or Path(sys.argv[1]).suffix.lower()!='.json':raise ValueError('output.json')
b=Binary();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
fs=functions();f=fs['180234840']
for term in ['+ 0x6a','+ 0x60) + 2','puVar5 = puVar5 + 3','uVar6 + param_3) | bVar1','uVar2 + param_3) | bVar1','&DAT_1802e86d0']:
 assert term in f,term
ctor=fs['180232090'];assert 'iVar1 == 0x200' in ctor and 'iVar1 == 0' in ctor and 'invalid TCM version' in ctor
assert b.data[b.off(0x1800f3360):b.off(0x1800f3360)+8].hex()=='0fb78250010000c3'
assert 'param_2 + 0x150,2,1' in fs['1800fcc10']
factory=fs['18020c9d0'];assert 'param_3[0x11] + 0x38' in factory and 'local_80 = CONCAT44' in factory
reader=fs['18010c5f0'];assert '*(longlong *)(param_2 + 0x88) = lVar5' in reader and 'local_24[0] & 1' in reader
lk=(ROOT/'models/ru-RU/ru_RU-lk-text.lite.res').read_bytes();assert hashlib.sha256(lk).hexdigest()=='d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e'
offset=2513123;size=struct.unpack_from('<I',lk,offset+4)[0]+8;version_offset=offset+size-0x2a4-0x2254-34
assert struct.unpack_from('<H',lk,version_offset)[0]==0x200
report={'phase':'234840 raw triplets in storage order, byte OR; calls inner68 afterward', 'version':'13B+88 LM.V38=0F3360 reads field150 populated by0FCC10', 'actualRussianVersion':512,'serializedOffset':version_offset,'unsupported':'stateful292 query and inner68 not restored by this phase'}
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8');print('PASS raw-triplet permission phase and actual RU TCM200 factory lineage; PE-only.')
