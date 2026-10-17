"""Independent field-level96-to56 projection and linked-selector traversal oracle."""
import json,sys
from pathlib import Path
p=json.loads(Path(sys.argv[1]).read_text());groups=arcs=dead=0
for case in p['cases']+p['actual']:
 assert len(case['groups'])==len(case['inputGroups'])
 for i,(old,new) in enumerate(zip(case['inputGroups'],case['groups'])):
  f=old['fields'];assert new['fields']==[i,f[0],f[1],f[6],f[7],f[5],0]
  assert new['bounds']==old['bounds'] and new['ranges']==old['ranges']
  dead+=f[3]==0;groups+=1
 for arc in [case['root']]+[a for row in case['nodes'] for a in row]:
  assert arc['header']==arc['inputHeader'];assert len(arc['selectors'])==case['cap']+1
  #Native walks selector0..cap and each existing linked list in order.
  flat=[];ends=[];read=[]
  for linked in arc['selectors']:
   values=[v&65535 for v in linked];flat.extend(values);ends.append(len(flat));read.append(values)
  assert len(flat)<=2048
  assert arc['ends']==ends and arc['indices']==flat and arc['read']==read
  arcs+=1
assert p['prepared']==66 and p['actualCount']==7 and p['actualCross']==5 and p['guards']==22
assert dead>=64
assert any(a['root']['ends'][-1]==2048 for a in p['cases'])
assert any(a['root']['ends'][-1]==2047 for a in p['cases'])
print(f'PASS independent{len(p["cases"])+len(p["actual"])} compact states/{groups} groups/{arcs} selector tables;{dead} dead groups retained, exact flags/bboxes/ranges/order and2048 boundary')
