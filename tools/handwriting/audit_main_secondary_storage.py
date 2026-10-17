"""Static2B3 count property and secondary2430->2410 ownership pins."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[
 (0x18000d800,0x18000d8e0,'d4d25d76e62d2f381aabb2865d54288509a8b15457b5125d8cfeb777a7663e11'),
 (0x18021492a,0x1802149ba,'c16ef5fb9eb885c7e39fbd5742a3fe8a3659f06130c4473a8783394334a621d6'),
 (0x180214bd0,0x180214c1a,'3b4dd2bb35bf3f8f7656a1ee5957d5c27060828c359f24e03b518a2099db6750'),
 (0x180213515,0x18021357b,'dddd1a748901f1c5fd91bb86c4dad249d3e1d5d6a2e680762acd0b11a84d9ca4'),
 (0x180215b00,0x180215b89,'16d0953f0c2c48dd0baf54e506d365eedb213a97b1d99c571dd994f60c3d1462')]
for start,end,sha in regions:assert hashlib.sha256(b.data[b.off(start):b.off(end)]).hexdigest()==sha
pins=[
 (0x18000d80a,'0fb74218'),             #knowledge18, FFFF resolvesdefault2
 (0x18000d831,'85c0'),(0x18000d835,'83f814'),
 (0x18000d83a,'66894318'),             #storeknownproperty
 (0x18000d882,'6644894b18'),           #nullresetFFFF
 (0x18000d887,'3bd1'),                #effectiveold/newcomparison BEFOREcallbacks
 (0x18000d88b,'488b5348'),(0x18000d8a3,'41b803002d01'),
 (0x18000d8af,'488b5360'),(0x18000d8c7,'41b803002d01'),
 (0x18021493a,'0fb77918'),             #2144 readsactualknowledge18
 (0x18021494e,'663bfa'),(0x180214951,'751f'), #configuredvalue bypassessource-deriveddefault
 (0x180214be5,'488b8610240000'),
 (0x180214bf0,'c704d100000004'),        #onlyfirstsecondary00and04initialized
 (0x180214c12,'c744d10400000004'),
 (0x180215279,'e82294ffff'),           #actual20E6A0 producer
 (0x180215345,'e8e644feff'),           #initial1F9830 merge
 (0x18021550b,'e800dfffff'),           #initial213410 materializer
 (0x180215679,'e832c8ffff'),           #continuation211EB0 producer
 (0x180215745,'e8e640feff'),           #continuation1F9830 merge
 (0x18021592b,'e8e0daffff'),(0x180215997,'e874d1ffff'),
 (0x180213515,'440fb78b04240000'),
 (0x180213529,'0faf4758'),             #sourcepoolordinal58
 (0x180213536,'488b8330240000'),(0x180213548,'488b8310240000'),
 (0x180215b32,'81f900000004'),(0x180215b38,'7d1c'), #stopatFIRSTIMP
 (0x180215b3a,'412bcc'),               #secondary00shiftbeforestored88.10
 (0x180215b69,'81f900000004'),(0x180215b71,'412bcc')]
for address,raw in pins:assert b.data[b.off(address):b.off(address)+len(raw)//2].hex()==raw,hex(address)
assert b.qwords(0x180274c60,3)==(0x4012d0003,0x18000d7d0,0x18000d800)
report=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in regions],pins=len(pins),
 property='0x012D0003/00D800 before controller construction, scopedvalue2',
 scope='actual CASE NN/query inference, secondary2430 producer/merge and2410 node storage',
 preconstruction='FFFF resolves2 for setter comparison, but2144 source-default mayresolve1; do not reinterpretlive setter asforcedrefresh',
 excluded=['livepropertymutation','configured207CB0/terminal/WORD bridge','freshdefaultcountchange','tracepoolstorage'],
 noVendorExecution=True)
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f'PASS5 native regions/{len(pins)} instruction pins/property binding; static PE reads only.')
