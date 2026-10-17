"""Static207CB0 configured2410 marking and unconditional beam fall-through."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/myscript/scripts'))
from inspect_text_features import Binary
b=Binary()
assert hashlib.sha256(b.data).hexdigest()=='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
a,z=0x180208340,0x1802084bb
digest='928e30f9a084822b4f6c4326d38954c397005e2c9ea992db9859924ca27a73cc'
assert hashlib.sha256(b.data[b.off(a):b.off(z)]).hexdigest()==digest
pins=[
 (0x180207fde,'8064d11b7f'), #clear2410 mark, no allocator normalization
 (0x18020835a,'42813ce800000004'),(0x180208362,'0f8d4f010000'), #firstIMP ends enumeration
 (0x180208377,'66413bdf'),(0x1802083a7,'66453bcf'),
 (0x1802083ab,'722e'), #primary ordinal bypasses already-marked test
 (0x1802083c4,'807cc81b00'),(0x1802083c9,'0f8cc2000000'),
 (0x1802083fa,'804cd11b80'),
 (0x180208421,'0fbe44ca1a'),(0x18020842a,'413bbe90240000'), #signed span /2490 boundary
 (0x180208439,'440fb74cca18'),(0x18020844f,'7237'),
 (0x180208456,'66443be0'),(0x18020845c,'450fb7cc'),
 (0x18020847c,'440fb764d120'), #replacement20 /22 persisted between links
 (0x180208482,'440fb66cd122'),
 (0x180208491,'8b55d8'),(0x1802084af,'3bd0'),(0x1802084b1,'0f8c89feffff'),
 (0x1802084b7,'4c8b6da0'),(0x1802084bb,'4d8b96e0230000') #fall-through, no count1-only jump
]
for address,raw in pins:
 assert b.data[b.off(address):b.off(address)+len(raw)//2].hex()==raw,hex(address)
report=dict(region=dict(start=hex(a),end=hex(z),sha256=digest),pins=len(pins),noVendorExecution=True,
 covered='known configured2 CASE source snapshots ->2410 and88 reachability marking',
 excludes=['compact40 append','2410.1C writeback','configured backtracking','273/WORD assembly'])
Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n',encoding='utf8')
print(f'PASS configured marking region/{len(pins)} pins; signed spans, replacement20/22, primary-vs-secondary test and beam fall-through.')
