"""Native query keys/epoch schedule plus independent raw NN→case dictionary oracle."""
import json,runpy,sys
from pathlib import Path
if len(sys.argv)!=5:raise ValueError('AK LK case-preparation-controls capture')
p=Path(sys.argv[4]);got=json.loads(p.read_text(encoding='utf8'))
assert got['epochs']==[-1,0,1,2,3]
seen=set()
for row in got['spans']:
 key=(row['node'],row['span'])
 assert key not in seen and row['parameter1c']==key[0] and row['parameter20']==key[1]
 seen.add(key)
assert len(seen)==10
runpy.run_path(str(Path(__file__).resolve().parents[1]/'LegacyCaseMainSpan.Probe/audit_output.py'),run_name='__main__')
print('PASS independent source keys: ten distinct(node,span), ONE node-boundary epoch sequence; same195cost->permission->case-map->397records oracle')
