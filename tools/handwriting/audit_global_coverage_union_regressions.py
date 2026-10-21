"""Exact old11 per-case and unchanged prior13 producer regression evidence.
Independent numerical outputs are checked by the six composed stage oracles.
This comparator never supplies recognized labels to runtime inference.
"""
from pathlib import Path
import argparse,json,hashlib
p=argparse.ArgumentParser(description=__doc__);p.add_argument('artifacts',type=Path);p.add_argument('output',type=Path);p.add_argument('--native',action='store_true');a=p.parse_args()
sha=lambda b:hashlib.sha256(b).hexdigest();kind='native'if a.native else'managed'
def read(name):
 path=a.artifacts/name;data=path.read_bytes();return json.loads(data),dict(file=str(path),sha256=sha(data))
reports=[]
for short,oldname in [('wrapper','timed-geometry-old-LegacyGlobalTextSourceWrapper-managed.json'),('parent','global-timed-geometry-managed-parent13-v9.json')]:
 old,oldfile=read(oldname);new,newfile=read(f'union-{short}-{kind}-v1.json');assert old==new
 assert oldfile['sha256']==newfile['sha256'];reports.append(dict(stage=short,old=oldfile,new=newfile,actual13ByteIdentical=True))
names=[('coverage','LegacyGlobalDocumentCoverage'),('selected','LegacyGlobalDocumentSelectedSources'),('indices','LegacyGlobalDocumentSourceIndices'),('cleanup','LegacyGlobalControlCleanup'),('transforms','LegacyGlobalInputTransforms'),('counter','LegacyGlobalCounterHandoff')]
for short,name in names:
 baseline='root-global-'+short+'-managed-20261021.json'if short in('cleanup','transforms','counter')else'timed-geometry-old-'+name+'-managed.json'
 old,oldfile=read(baseline);new,newfile=read(f'union-{short}-{kind}-v1.json')
 if a.native:
  managed,managedfile=read(f'union-{short}-managed-v1.json');assert new==managed and newfile['sha256']==managedfile['sha256']
 byname={x['name']:x for x in new['samples']};count=0;aux=0
 for before in old['samples']:
  if not before.get('completed',False):
   if 'completed'not in before:assert byname[before['name']]==before;aux+=1
   continue
  after=byname[before['name']].copy()
  if short=='coverage':after.pop('normalizedWord')
  assert after==before,(short,before['name']);count+=1
 assert count==11
 # Existing failure/alias/cancellation/control arrays are byte-value equivalent.
 for key,value in old.items():
  if key!='samples'and isinstance(value,(list,dict)):assert new[key]==value,(short,key);aux+=1
 reports.append(dict(stage=short,old=oldfile,new=newfile,old11Unchanged=count,preparedCollectionsUnchanged=aux))
a.output.write_text(json.dumps(dict(reports=reports,mode=kind,noVendorExecution=True,status='PASS'),indent=2)+'\n')
print(f'PASS {kind}:2 prior13 producers byte-identical;6x11 prior successful cases and historical prepared/failure collections unchanged.')
