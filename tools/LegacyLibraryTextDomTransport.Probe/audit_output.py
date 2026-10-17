"""Independent native C8F row/ownership-prefix and actual C4D checks."""
import base64,json,struct,sys,hashlib
from pathlib import Path
a=json.loads(Path(sys.argv[1]).read_bytes());assert len(a['actual'])==4
rows=0;commands=0
for index,sample in enumerate(a['actual']):
 assert sample['sample']==index and sample['selectedTag']==0 and sample['initialPending']==1
 source=sample['commands'];assert source and source[-1]['operation']==7
 assert [x['id']for x in source]==list(range(1,len(source)+1))
 for i,c in enumerate(source):
  assert c['next']==(i+2 if i+1<len(source)else 0)
  if c['operation']==4:
   assert c['category']==100 and c['tag']==i+1
   assert c['name']in('CHAR','WORD','TEXT')
   raw=base64.b64decode(c['ranges16']);assert raw and len(raw)%16==0
  else:
   assert i==len(source)-1 and c['old']==1 and c['new']==sample['finalPending']
   assert base64.b64decode(c['body56'])==struct.pack('<qq',c['new'],c['old'])+bytes(40)
 commands+=len(source)
 states=sample['states'];stages=[x['stage']for x in states]
 expected=['prepared','cancelled-before-entry','failed-reserve']+(['transferred']if index!=3 else[])+['disposed-incoming','disposed-cache']
 assert stages==expected
 for state in states:
  stage=state['stage'];transferred=index!=3 and stage in('transferred','disposed-incoming','disposed-cache')
  disposedIncoming=stage in('disposed-incoming','disposed-cache');disposedCache=stage=='disposed-cache'
  pending=not transferred and not disposedIncoming
  assert state['pendingEdit']==state['pendingTransaction']==pending
  assert state['transferred']==transferred and state['disposedIncoming']==disposedIncoming and state['disposedCache']==disposedCache
  assert state['count']==state['capacity']==int(transferred and not disposedCache)
 if index==3:assert sample['row40']is None
 else:
  #11B860 explicitly clears40B before storing five qwords. Actual call's
  #two scalar versions are-1; three pointer lanes stay typed, never invented.
  assert base64.b64decode(sample['row40'])==struct.pack('<qq',-1,-1)+bytes(24);rows+=1
 assert sample['sourceUnchanged']and sample['targetEpoch']==0 and sample['targetChildCount']==0
sha=hashlib.sha256(json.dumps(a,sort_keys=True,separators=(',',':')).encode()).hexdigest()
Path(str(sys.argv[1])+'.stable.sha256').write_text(sha+'\n',encoding='ascii')
print(f'PASS independent4 actual C4D prefixes/{commands} commands/{rows} full40B rows and all transfer/failure/disposal states;SHA={sha}')
