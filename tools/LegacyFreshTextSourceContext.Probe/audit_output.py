"""Independent actual fresh selection transaction check; no expected labels."""
import json,sys
from pathlib import Path
a=json.loads(Path(sys.argv[1]).read_text());n=a['originalObjects'];rows=a['actual']
assert n>0 and len(rows)>0
for i,row in enumerate(rows):
 assert row['id']==n+i and row['name']=='CHAR' and row['command']==i+1 and row['previous']==i
 assert all(0<=v<2**64 for r in row['ranges'] for v in r)
 if i%2:assert row['ranges']==rows[i-1]['ranges']
assert a['next']==n+len(rows)
print('PASS actual source ranges/IDs/pending command order, duplicate positions remain distinct registrations:',len(rows))
