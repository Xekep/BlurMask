from pathlib import Path
import copy,hashlib,json,sys
from late_replay import replay
BASE=Path(__file__).resolve().parent/'fixtures/root83-before-late.json'
BASE_SHA='643289fecfbbb6682d6ad7ddd09384566ee29044d6b4495137c1a31ac8a598f1'
STOPS={0:'Registered',1:'TagWritten',2:'Removed',3:'Completed',4:'Registered',5:'Claimed'}

def strip_counts(snapshot):
 out=copy.deepcopy(snapshot)
 assert 0<out.pop('Visits') and 0<out.pop('Reservation')<=16*1024*1024
 return out

def expected_failure(events,mode):
 snap=copy.deepcopy(next(e['Snapshot']for e in events if e['Stage']==STOPS[mode]));snap['Failed']=True
 return snap

def audit(path):
 assert hashlib.sha256(BASE.read_bytes()).hexdigest()==BASE_SHA
 base=json.loads(BASE.read_bytes());actual=json.loads(path.read_bytes());summaries=[]
 assert actual['Unchanged']==base['Unchanged'];assert len(actual['Actual'])==len(base['Actual'])==6
 for got,old in zip(actual['Actual'],base['Actual'],strict=True):
  assert all(got[k]==old[k]for k in old),('pre-late data changed',old['Case'])
  want,tree,events=replay(old)
  assert strip_counts(got['Late'])==want,('late',old['Case'])
  assert got['AfterLateTree']==tree,('tree',old['Case'])
  summaries.append(dict(case=old['Case'],registrations=want['Registrations'],written=want['TagsWritten'],newTags=want['NewTags'],removed=want['Removed'],next=want['Observed']['Next']))
 assert sorted(f['Mode']for f in actual['Failures'])==list(STOPS)
 _,_,events=replay(base['Actual'][0]);initial=base['Actual'][0]['Candidate']
 for f in actual['Failures']:
  mode=f['Mode'];want=expected_failure(events,mode);assert strip_counts(f['Snapshot'])==want,('failure',mode)
  candidate=copy.deepcopy(initial);observed=want['Observed'];candidate.update(RegistryNext=observed['Next'],Registry=observed['Registry'],Pending=observed['Pending'],Temporary=observed['Temporary'])
  assert f['Candidate']==candidate,('candidate prefix',mode)
 return dict(PASS=True,baselineSha256=BASE_SHA,evidenceSha256=hashlib.sha256(path.read_bytes()).hexdigest(),cases=summaries,failureModes=list(STOPS),scope='Independent native late ordering/cache-before-write/currentTHIRD registry+Next+canonical aliases/live tree/stale removal/failure prefixes. No original document/public publication claim.')
if __name__=='__main__':
 r=audit(Path(sys.argv[1]));Path(sys.argv[2]).write_text(json.dumps(r,indent=2)+'\n',encoding='utf-8');print(json.dumps(r))
