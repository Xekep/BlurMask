from pathlib import Path
import json,sys,hashlib
cap=json.loads(Path(sys.argv[1]).read_bytes());old=json.loads(Path(sys.argv[2]).read_bytes());assert len(cap['Unchanged'])==len(old['Actual'])==6
for a,b in zip(cap['Unchanged'],old['Actual']):
 for key in ['Width','Reversed','Prior','Prepared','Finished','NewNn']:assert a[key]==b[key],(a['Width'],a['Reversed'],key)
print('PASS6 exact unchanged prior/C23/NEWNN snapshots vs authenticated old6 reference; new current route separate')
