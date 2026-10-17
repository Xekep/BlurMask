"""Independent raw two-M source chronology for the genuine reorder regression."""
import json,sys
from pathlib import Path
if len(sys.argv)!=2:raise SystemExit('output.json')
r=json.loads(Path(sys.argv[1]).read_bytes())
# Both original five-point raw M inputs are distinct adjacent coordinates;
# no source deduplication occurs. Native F8 therefore remains this original
# input chronology even though the model reverses current physical order.
expected=[[0,physical,point] for physical in range(2) for point in range(5)]
assert r['physicalOrder']==[1,0]
assert r['originalKeyTable']==expected
assert r['currentKeys']==expected[5:]+expected[:5]
assert not r['failed'] and len(r['queries'])>0
for q in r['queries']:
    assert q['expectedFromOriginalF8']==[expected[0],expected[-1],0,0]
    assert q['actual']==[[expected[0],expected[-1],0,0]]
print(f"PASS independent original raw-key table/model reversal/{len(r['queries'])} source-adjacency regression queries; no text labels.")
