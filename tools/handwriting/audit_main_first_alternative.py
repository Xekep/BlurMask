"""Read-only first main alternative construction/reference ownership pins."""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/myscript/scripts'))
from inspect_text_features import Binary

if len(sys.argv) != 2:
    raise SystemExit('report.json')
b = Binary()
assert hashlib.sha256(b.data).hexdigest() == '18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions = [
    (0x180141540, 0x18014155b, 'a1ca2e904b2d119d9d1316f3196cee4ede6f947debabbd6927c2f23918dc19bc'),
    (0x18013bd40, 0x18013be80, '5103c015c3c9c0f02ed73c2a954c02f3c807b05bd24bba887f8cffb66cd74dfc'),
    (0x180227603, 0x180227686, '1b0caf7b46f6e0540b63fa214ea16c4b9623452a23dc5b1150fc343af97c9e98'),
    (0x180227750, 0x180227767, '575d82c95c4eb94620ef4ab46ea06621bfe2562619ef53c9717d724096e48262'),
    (0x18000c88c, 0x18000c8e0, '25df5032cf9dd101c4e840a62a75f0bac882305619400a90476ac0f35642b72f'),
    (0x180141ff6, 0x180142022, '54d477bf169ece58c2977ef0b010ae1ed1603e95060c47a2d0a8621fa71a8bf2'),
]
for start, end, sha in regions:
    assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest() == sha
pins = [
    (0x180141540, '33c0c74214ffff7f7f488942088942104889421848894220b001c3'),
    (0x18013bd60, '0fbf4a60ffc13b4a70'),
    (0x18013bd6b, 'b86766666641b828000000'),
    (0x18013bd89, '03c03bc8448d700a'),
    (0x18013bd95, '440f44f1'),
    (0x18013bdb2, '4889476844897770'),
    (0x18013bdba, '480fbf5760488b4768488d0c9266ffc266895760488d1cc8'),
    (0x18013bdd6, 'ba79020000'), (0x18013bded, '488903'),
    (0x18013be14, '41ffd0'),
    (0x18000c8cb, '48898798000000'),  #QWORD98 clears byte99.
    (0x18000c8d9, '488987a4000000'),  #QWORDA4 clears byteA8.
    (0x18000c8a8, '33c0'),           #Their actual stored value is zero.
    (0x180227271, '4c63aa10230000'),  #Actual controller2310 count.
    (0x18022727c, '44896d90'),        #Save as score divisor count.
    (0x180227634, '4585ff7522'),
    (0x18022763d, '4438a0990000007515'),
    (0x180227646, '4438a0a8000000750c'),
    (0x18022764f, '488bda89558848895778'),
    (0x180227683, '41ffd0'),
    (0x180227763, '49894520'),        #Selected native beam identity, notrow22.
    (0x18013b88a, '33ed'),
    (0x18013b971, '66896f6048896f68896f70'), #Fresh count0/pointer0/capacity0.
    (0x18013b980, 'c78780000000000080bf'), #Fresh TOP80=-1.
    (0x180141ff6, '498b47184989442420'),  #Node.next=old279Head18.
    (0x180142009, '4d896718'),             #Actual279Head18=new node.
]
for address, raw in pins:
    actual = b.data[b.off(address):b.off(address) + len(raw)//2].hex()
    assert actual == raw, (hex(address), actual, raw)
#Class279 descriptor metadata binds constructor141540; inspect class metadata
#rather than calling any vendor factory or deriving fake portable pointers.
assert b.qwords(0x1802dcc00, 1)[0] == 0x180141540
report = dict(regions=[dict(start=hex(s), end=hex(e), sha256=h) for s,e,h in regions],
              pins=len(pins), nativeCount='controller2310 load227271/store22727C',
              snapshot='post-success callbacks, one actual400 allocation, typed WORD/beam identities',
              noVendorExecution=True, excluded=['native partial allocation/failure order', 'duplicates/exclusions',
                                                'quote and21F320 normalization', 'multi-alternative result', 'acceptance'])
Path(sys.argv[1]).write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
print(f'PASS{len(regions)} native regions/{len(pins)} pins/279 constructor binding; no vendor execution.')
