"""Former12 exact per-case and prepared regressions across the six stages."""
from pathlib import Path
import argparse,json,hashlib
p=argparse.ArgumentParser(description=__doc__);p.add_argument('artifacts',type=Path);p.add_argument('output',type=Path);a=p.parse_args();sha=lambda b:hashlib.sha256(b).hexdigest()
reports=[]
for stage in ['selected','indices','cleanup','transforms','counter','formatter']:
 oldname='root-global-range-union-managed-20261021.json'if stage=='formatter'else'union-'+stage+'-managed-v1.json'
 oldpath=a.artifacts/oldname;old=json.loads(oldpath.read_bytes());before={x['name']:x for x in old['samples']}
 captures=[]
 for mode in ['managed','native']:
  path=a.artifacts/('raw-kind-'+stage+'-'+mode+'-v1.json');data=path.read_bytes();after=json.loads(data);now={x['name']:x for x in after['samples']};kept=0;prepared=0
  for name,value in before.items():
   complete=value.get('completed',False)if stage!='formatter'else'refusal'not in value
   if not complete:
    if 'completed'not in value and stage!='formatter':assert now[name]==value;prepared+=1
    continue
   assert now[name]==value,(stage,mode,name);kept+=1
  assert kept==12
  for key,value in old.items():
   if key!='samples'and isinstance(value,(list,dict)):assert after[key]==value,(stage,mode,key);prepared+=1
  captures.append(dict(mode=mode,path=str(path),sha256=sha(data),former12Exact=kept,preparedCollectionsExact=prepared))
 assert captures[0]['sha256']==captures[1]['sha256']
 reports.append(dict(stage=stage,baseline=str(oldpath),baselineSha256=sha(oldpath.read_bytes()),captures=captures))
a.output.write_text(json.dumps(dict(status='PASS',reports=reports,scope='All six Managed/Native exact;6xformer12 objects/prepared collections unchanged; no inference quality claim'),indent=2)+'\n')
print('PASS6 Managed/Native byte-identical captures and6xformer12 per-case outputs +historical prepared/failure collections unchanged.')
