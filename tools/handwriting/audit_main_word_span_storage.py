"""Static275 interning,276 destruction and13D360 row/ordinal replacement."""
import hashlib,json,sys,re
from pathlib import Path
root=Path(__file__).resolve().parents[2];sys.path.insert(0,str(root/'research/myscript/scripts'))
from inspect_text_features import Binary,functions
b=Binary();f=functions();assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[(0x18013fe20,0x180140b90,'f1292c7108be8e41010d4f90f5065f496cbf0154e9128f6c9e733d0bd9e43776'),
 (0x18013f080,0x18013f100,'2a4266b28c10ccda84fd9f3d5c0d79683d186991645d773a620154e345b13b6d')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
assert b.qwords(0x1802dc850+0x20,1)[0]==0x1802dc828
assert b.qwords(0x1802dc8a0+0x20,1)[0]==0x1802dc900
for table,slot,target in[(0x1802dc828,0,0x18013fe20),(0x1802dc828,8,0x180140b90),(0x1802dc900,8,0x18013f080)]:assert b.qwords(table+slot,1)[0]==target
init=f['180002530'];assert 'puVar7 = &DAT_1802dc850'in init and 'puVar7 = &DAT_1802dc8a0'in init
assert 'param_1 + 0x18) + 0x18)'in init and 'param_1 + 0x18) + 0x20)'in init
intern=f['18013fe20'];pins=['==\n          param_4[1]','return uVar21 & 0xffff','*(undefined8 *)(lVar22 + lVar17) = *param_4','param_4[0xe]']
for p in pins:assert p in intern,p
destroy=f['18013f080'];assert 'param_2 + 0x20'in destroy and 'param_2 + 0x58'in destroy and 'param_2 + 0x68'in destroy
assert sorted(set(re.findall(r'FUN_(180[0-9a-f]+)\(',destroy)) - {'18013f080'})==['18013eb00','18013ee90']
for name in ['18013eb00','18013ee90']:assert 'param_1 + 0x18'not in f[name] #Auxiliary heap payloads, not274 records.
rank=f['18013d360'];assert 'puVar35[0xe] = puVar37[0xe]'in rank and 'lVar6 + 0x18) = lVar58'in rank
assert '*(undefined4 *)(lVar58 + 8) ='in rank and 'lVar58 + 8) * 0x14 + 4)'in rank
assert ' + 0x18) + 8))\n                         (param_1,lVar6,param_2)'in rank #Mandatory140B90 remains separate.
report=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h)for a,z,h in regions],bindings=5,
 scope='Plain13FE20 extraction and controlled portable275 replacement only; no ranking math/140B90 execution/219580 augmentation/fullV68',
 released='13F080 frees own flat/auxiliary vectors;274 pool is retained',vendorExecution=False)
Path(sys.argv[1]).write_bytes((json.dumps(report,indent=2)+'\n').encode())
print('PASS2 fullPE regions/5 descriptor+virtual bindings; head-only intern identity,120Bcopy,279ordinalpatch and mandatory140B90.')
