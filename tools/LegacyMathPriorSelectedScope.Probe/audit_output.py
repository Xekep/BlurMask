"""Independent structural replay of selected clone from actual applied capture.
Not a proof of native nonfresh edit production or restored source authority.
"""
from pathlib import Path
import copy,json,sys
p=Path(sys.argv[1]);data=json.loads(p.read_bytes());original=data['Original'];selected=data['Scope']
assert data['ActualNN'] and not data['PriorNN'] and not data['NonfreshDiff']
assert original['Complete'] and original['DomEntered'] and original['SourceExecutionSucceeded'] and original['JournalComplete']
assert not original['Failed'] and not original['RolledBack']
assert selected['Epoch']==original['LibraryEpoch']==1
assert selected['Next']==original['ActiveNext']>max(x['Id'] for x in original['ActiveRegistry'])
active={n['Id']:n for n in original['ActivePage']};children={}
for n in original['ActivePage']:children.setdefault(n['Parent'],[]).append(n)
expected=[]
def clone(identity,parent=0):
 n=copy.deepcopy(active[identity]);n['Id']=len(expected)+1;n['Parent']=parent;n['PayloadOrigin']=0;expected.append(n)
 for attributes in (True,False):
  for c in children.get(identity,[]):
   if (c['Kind']==0)==attributes:clone(c['Id'],n['Id'])
clone(original['OriginalField'])
assert selected['SelectedField']==expected
assert selected['Registry']==original['ActiveRegistry']
aliases={}
for row in selected['Registry']:aliases.setdefault(row['CanonicalC22Id'],[]).append(row['Id'])
assert any(len(ids)>1 for ids in aliases.values())
assert sum(n['Kind']==3 and n['Name']=='mathResult' for n in expected)==1
assert data['Controls']==61
if len(sys.argv)>3:
 assert p.read_bytes()==Path(sys.argv[3]).read_bytes(), 'Managed/native capture differs'
print(f"PASS exact active-field subtree replay ({len(expected)} nodes), current registry/Next and alias groups; {data['Controls']} actual ownership controls.")
