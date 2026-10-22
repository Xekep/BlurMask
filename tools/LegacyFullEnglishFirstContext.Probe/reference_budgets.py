"""Admission/reference-only whole B4 budgets from genuine frozen B3 rows. No NN/labels."""
from pathlib import Path
import argparse,hashlib,json,base64,struct
p=argparse.ArgumentParser();p.add_argument('--capture',action='append',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
def envelope(g,ar,n,e,k):
 assert 0<=g<=4096 and 0<=ar<=4096 and 0<=n<=4096 and 0<=e<=1000000 and 0<=k<=32768
 return dict(peak=4*1048576+4096*ar+4096*n+512*g+16*e+320*k,byteWork=4*1048576+24576*ar+16384*n+1024*g+24*e+1536*k)
maximum=envelope(4096,4096,4096,1000000,32768);assert maximum['peak']<=64*1048576 and maximum['byteWork']<=256*1048576
reports=[]
for path in a.capture:
 raw=path.read_bytes();assert hashlib.sha256(raw).hexdigest()in{'84ce82204df3574f897438abed14cd29192c623ebd646d0e10dc61010420d6a7','c82d59d4a854a181d16f142a83ff9a9ca135c957961a441d676940fd9dfb9a07'}
 d=json.loads(raw);assert d['TargetsRead']is False;rows=[]
 for row in d['Actual']:
  assert row['Status']=='AcceptedPrimary';g=ar=n=e=k=visits=records=0;children=[]
  for c in row['Children']:
   nodes=c['Nodes'];tables=c['Primary'];assert len(nodes)==len(tables);counts=[x['Count']for x in tables];cn=len(nodes);ca=sum(map(len,nodes));cg=len(c['Groups']);ce=0;D=P=0;T=R=K=0
   assert 0<cg<=ca and 1<=cn<=4096 and all(0<=v<=5 for v in counts)
   for ni,node in enumerate(nodes):
    root=0
    for arc in node:
     ce+=len(arc['Successors']);D=max(D,len(arc['Successors']));P=max(P,len(arc['Pred']))
     if arc['Span']==ni and -1 in arc['Pred']:ce+=1;root+=1
    D=max(D,root)
    storage=base64.b64decode(tables[ni]['Bytes']);assert len(storage)==200
    for j in range(counts[ni]):
     off=j*40;packed=struct.unpack_from('<H',storage,off+14)[0];span=(packed>>1)&7;parent=ni-span-1;R+=1
     assert parent>=-1
     if parent==-1:K+=2
     else:T+=counts[parent];K+=4*counts[parent]
   #128 is an actual private AVL bound allowance (32-direction path plus rotations), not a search tune.
   V=1024*cn+512*ca+320*R+2*T*(cn+D+32)+6*T*(P+128)+128*K
   g+=cg;ar+=ca;n+=cn;e+=ce;k+=K;visits+=V;records+=R;children.append(dict(descriptor=c['Descriptor'],nodes=cn,groups=cg,arcs=ca,rootSuccessors=ce,retainedPrimaryRecords=R,priorPairs=T,maximumResolveAttempts=K,visits=V))
  reason=[]
  if not(g<=ar<=4096 and n<=4096 and e<=1000000 and k<=32768):reason.append('structural/context')
  if visits>128*1048576:reason.append('visit')
  env=envelope(g,ar,n,e,k)if not reason else None
  rows.append(dict(name=row['Name'],accepted=not reason,refusal=reason,groups=g,arcs=ar,nodes=n,links=e,resolveAttempts=k,visits=visits,envelope=env,children=children))
 reports.append(dict(path=str(path),sha256=hashlib.sha256(raw).hexdigest(),actual=len(rows),accepted=sum(x['accepted']for x in rows),refused=sum(not x['accepted']for x in rows),maximumPeak=max(x['envelope']['peak']for x in rows if x['accepted']),maximumByteWork=max(x['envelope']['byteWork']for x in rows if x['accepted']),maximumResolveAttempts=max(x['resolveAttempts']for x in rows),maximumVisits=max(x['visits']for x in rows),rows=rows))
report=dict(status='PASS',scope='STATIC proposal arithmetic from genuine completed B3 only; no runtime/context computation/NN/target',independentMaxima=maximum,captures=reports,vendorExecuted=False,targetsRead=False);a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps({k:v for k,v in report.items()if k!='captures'}));print(json.dumps([{k:v for k,v in x.items()if k!='rows'}for x in reports]))
