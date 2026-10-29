"""Observation-schema adapter and comparison, no engine input or owner authority."""
from pathlib import Path
import base64,hashlib,json,struct,sys
from temporary_replay import replay

def scalar(n):
 t=n['ValueTag'] if n['Kind'] in (0,1) else 0
 data=bytearray(16);heap=b''
 if t in (1,2):
  b=n['Text'].encode('utf-8',errors='strict')
  if t==1:
   assert len(b)>=16;heap=b;struct.pack_into('<I',data,8,len(b))
  else:
   assert 1<=len(b)<=15;data[:len(b)]=b;data[15]=len(b) if len(b)<15 else 0
 elif t==4:struct.pack_into('<q',data,0,n['Number'])
 elif t==7:struct.pack_into('<i',data,0,n['Number'])
 else:
  assert t in (0,3,10)
  if t==10:assert n['Number']==0
 return dict(ValueTag=t,Storage=base64.b64encode(data).decode(),Heap=base64.b64encode(heap).decode())

def normalized(tree,nodes):
 byid={n['Id']:n for n in nodes};out=[]
 for row in tree:
  n=byid[row['Id']]
  assert row==dict(Id=n['Id'],Kind=n['Kind'],Name=n['Name'],**scalar(n),Attributes=row['Attributes'],Children=row['Children'])
  out.append({k:row[k]for k in ('Id','Attributes','Children')})
 return out

def audit(path):
 cap=json.loads(path.read_bytes());summaries=[]
 for case in cap['Actual']:
  before=case['BeforeApply'];candidate=case['Candidate'];actions=case['Diff']['Actions'];application=case['Application']
  prefix=candidate['Temporary'];assert before[:len(prefix)]==prefix
  # Every additional allocation must be a complete detached clone belonging to
  # an actually produced insertion, including scalar fields and source origin.
  newids={n['Id']for n in before[len(prefix):]};covered=set()
  def visit(i):
   assert i not in covered;covered.add(i)
   for n in before:
    if n['Parent']==i:visit(n['Id'])
  for a in actions:
   if a['Operation']==2:visit(a['NodeAt20'])
  assert covered==newids
  bt=normalized(case['BeforeTree'],before)
  want,tree,count=replay(before,bt,actions,candidate['Candidate'])
  after=application['ObservedTemporary'];assert after==want
  assert normalized(case['AfterTree'],after)==tree
  assert application['Complete'] and application['Ready'] and not(application['Failed']or application['Released']or application['PartialActionMayExist'])
  assert application['CompletedActions']==application['ObservedAfterActions']==count==sum(not a['Filtered']for a in actions)
  assert application['HeaderConsumed'] and not case['DiffHeaderLiveAfter']
  assert case['Diff']['HeaderLive']
  assert case['SameOriginalArenaAfter'] and case['OriginalCurrentAfter']==case['CurrentInput']
  summaries.append(dict(case=case['Case'],completed=count,nodes=len(after),reachable=len(tree)))
 return dict(PASS=True,cases=summaries,evidenceSha256=hashlib.sha256(path.read_bytes()).hexdigest(),scope='Independent actual2/4/7 temporary replay/full after snapshots/ordered live tree; unchanged original current snapshot and arena identity observation. No original-document apply claim.')
if __name__=='__main__':
 r=audit(Path(sys.argv[1]));Path(sys.argv[2]).write_text(json.dumps(r,indent=2)+'\n',encoding='utf-8');print(json.dumps(r))
