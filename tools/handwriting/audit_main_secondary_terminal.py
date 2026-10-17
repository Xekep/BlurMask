"""Static2257C0 multi-secondary identity exclusions, no vendor execution."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
regions=[
 (0x1802257c0,0x180225faa,'b5e69b49ba5912e614128c5012316d6fe030a3569bfc2e88d008e25cca7610e8'),
 (0x180225990,0x180225b66,'33b5e6b03bd28266c967c024248a84d1d78a50432f627750712c8c3aff5f9fad'),
 (0x180225eb0,0x180225faa,'8ec5ad5ac88cd7ab8373c08fda4b7cf69bf74b9f4b75fd32696ddf8e840b0740'),
 (0x1802274b0,0x1802274f3,'ed00c168408c02efd811285e23b02e76ee2ea70421bb2604b6c5487ce170691e')]
for a,z,h in regions:assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==h
pins=[
 (0x18022589b,'44394c2910'),           #outer88 cost gatesmatrix
 (0x180225990,'488b8710240000'),      #2410, count*ordinal indexing
 (0x1802259bc,'418b1cc4'),(0x1802259c3,'0f8da4020000'), #stopfirstIMP
 (0x1802259c9,'458b74c404'),          #secondary cost04
 (0x180225a10,'48833800'),           #onlyrootcontexts special
 (0x180225a16,'0fb74020'),(0x180225a1a,'66413bc0'),
 (0x180225a24,'6683c005'),(0x180225a28,'6683f801'),(0x180225a2c,'7679'),
 (0x180225a31,'44389c24d8000000'),   #callerFilter4
 (0x180225a90,'035c2424'),           #signed32 languagepenalty addition
 (0x180225aaf,'3b18'),(0x180225ab1,'0f8d86000000'), #strictwinner, tiesfirst
 (0x180225af1,'4c8b4cd108'),         #opaqueidentity08
 (0x180225af6,'4c3b08'),(0x180225af9,'7442'),
 (0x180225b25,'4088b424c8000000'),   #selectedsecondaryBYTE
 (0x180225ee8,'488b44d008'),(0x180225ef5,'4a8904ca'), #appendactual08 exclusion
 (0x180225efd,'418903'),
 (0x180225f38,'0fb744d11c'),         #compactfieldcomesfrom40.1C, not88.22
 (0x180225f71,'41010e'),             #actualcumulative2520 addition
 (0x1802274b4,'ba08000000'),(0x1802274d2,'420fb74468fe'),
 (0x1802274db,'448bc0'),(0x1802274ec,'41ffd1') #nativeexclusioncapacity=SourceEnd
]
for address,raw in pins:assert b.data[b.off(address):b.off(address)+len(raw)//2].hex()==raw,hex(address)
report=dict(regions=[dict(start=hex(a),end=hex(z),sha256=h) for a,z,h in regions],pins=len(pins),
 covered='multi-secondary no-rescue2257C0 and same-owned configuredCASE exclusion cursor',
 excludes=['2271F0 accepted-output counter','duplicate callback outcomes','rescuebody','configured compact publication','recognized text'],
 noVendorExecution=True)
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(f'PASS4 native regions/{len(pins)} pins, matrixidentity08/selected40.1C/source-rowcapacity; static reads only.')
