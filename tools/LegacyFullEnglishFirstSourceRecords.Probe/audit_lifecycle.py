"""Independent exact typed/reached/prefix/backing mutation lifecycle checks.
Requires the same capture to pass audit_output.py's raw all720 oracle first.
"""
import argparse,base64,copy,hashlib,json,struct,sys
from pathlib import Path
sys.dont_write_bytecode=True;sys.path.insert(0,str(Path(__file__).resolve().parent))
from raw_profile import read_profile
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--numeric-report',type=Path,required=True);p.add_argument('--report',type=Path,required=True);p.add_argument('--tamper',action='store_true');a=p.parse_args()
raw=a.capture.read_bytes();data=json.loads(raw);numeric=json.loads(a.numeric_report.read_bytes());assert numeric['Status']=='PASS_INDEPENDENT_FULL_SOURCE720' and numeric['CaptureSHA256']==hashlib.sha256(raw).hexdigest()
profile=read_profile(a.models/'en-US/en_US-ak-cur.res')
def profile_hash(mutated):
 h=hashlib.sha256();h.update(bytes([255]*220) if mutated else bytes(x&255 for x in profile['StartMap']));h.update(bytes([255]*220) if mutated else bytes(x&255 for x in profile['EndMap']))
 for i,(first,second,third,flag) in enumerate(profile['Prototypes']):
  if mutated and i==0:first,second=17.25,23.5
  h.update(struct.pack('<fffB',first,second,third,int(flag!=0)))
 return h.hexdigest()
def rows_hash(rows):
 h=hashlib.sha256()
 def u(x):h.update(struct.pack('<Q',x&0xffffffffffffffff))
 u(len(rows))
 for row in rows:
  for k in ('Descriptor','Word','Ordinal','Reference'):u(row[k])
  u(int(row['Flat']));h.update(base64.b64decode(row['Storage'],validate=True))
  for name in ('ChildRanges','Projected'):
   u(len(row[name]))
   for r in row[name]:
    for x in r:u(x)
 return h.hexdigest()
def expected_proofs(multi):
 values=[]
 def r(label,kind,reached=0,message=''):
  cancellation='|tokenMatch=1|actualCancelable=1|actualRequested=1|expectedCancelable=1|expectedRequested=1' if kind=='OperationCanceledException' else ''
  values.append(f'{label}|{kind}|{reached}|{message}'+cancellation)
 def yes(label):values.append(label+'|PASS')
 for token in ('default','foreign','unrequested'):yes('prepared-token-validator-'+token)
 for name in ('peak','copy','visits'):r(name+'-one-short','ArgumentException')
 r('prepare-canceled','OperationCanceledException');r('run-preentry-canceled','OperationCanceledException');yes('preentry-retryable-empty');yes('retry-after-preentry-success');r('second-run','InvalidOperationException')
 for stage in ('entry','row:created','row:published'):
  r('entered-cancel-'+stage,'OperationCanceledException',1);yes('poison-'+stage);yes('historical-prefix-'+stage);r('postfault-run-'+stage,'InvalidOperationException')
 r('injected-row-failure','InvalidDataException',1,'Injected');yes('injected-historical-row')
 if multi:r('multi-child-close-cancel','OperationCanceledException',1);yes('multi-child-historical-prefix')
 r('incomplete-upstream','ArgumentException');r('upstream-injected-entry','InvalidDataException',1,'Injected');r('faulted-upstream','ArgumentException')
 yes('offlock-prefix-readable');r('concurrent-run-loser','InvalidOperationException',1);yes('concurrent-one-winner')
 for label in ('defensive-actual-returned-ROM','defensive-origin-ranges','defensive-descriptors','defensive-current-source','defensive-chronology','canonical-row-source-owner','cached-lifecycle-noNN'):yes(label)
 return ['actual-category-ROM-mutated-before-and-after-Prepare/raw-source-snapshot-stable/restored/noNN']+values

def check(data):
 assert data['GuardMode'] is True and 1<=len(data['Actual'])<=3
 cases=proofs=prefixes=0
 for case in data['Actual']:
  assert case['Status']=='Accepted' and case['NetworkBefore']==case['NetworkAfter']
  full=case['Variants'][0]['Rows'];multi=len(case['Variants'][0]['B7Children'])>1
  assert case['GuardProofs']==expected_proofs(multi)
  stages=['entry','row:created','row:published','injected-row']+(['multi-child'] if multi else [])
  assert [p['Stage'] for p in case['GuardPrefixes']]==stages
  for prefix in case['GuardPrefixes']:
   assert set(prefix)=={'Stage','Complete','Faulted','ReservedRows','Rows','Descriptors'} and prefix['Complete'] is False and prefix['Faulted'] is True and prefix['ReservedRows']==len(full)
   stage=prefix['Stage'];n=0 if stage in ('entry','row:created') else sum(r['Descriptor']==full[0]['Descriptor'] for r in full) if stage=='multi-child' else 1
   assert prefix['Rows']==full[:n]
   desc=[bytearray(base64.b64decode(x,validate=True)) for x in case['Parent']['Descriptors']]
   if stage!='entry':struct.pack_into('<ii',desc[full[0]['Descriptor']],52,0,n if stage=='multi-child' else 0)
   assert prefix['Descriptors']==[base64.b64encode(x).decode() for x in desc];prefixes+=1
  mutation=case['CategoryMutation'];assert set(mutation)=={'Before','Mutated','Restored','BaselineRows','PreparedBeforeRows','PreparedAfterRows'}
  assert mutation['Before']==mutation['Restored']==profile_hash(False) and mutation['Mutated']==profile_hash(True)!=mutation['Before']
  assert mutation['BaselineRows']==mutation['PreparedBeforeRows']==mutation['PreparedAfterRows']==rows_hash(full)
  cases+=1;proofs+=len(case['GuardProofs'])
 return dict(Cases=cases,Proofs=proofs,HistoricalPrefixes=prefixes,ActualCategoryMutations=cases)
metrics=check(data);negative=[]
if a.tamper:
 def refuse(label,change):
  d=copy.deepcopy(data);change(d)
  try:check(d)
  except(AssertionError,ValueError,KeyError,TypeError,IndexError):negative.append(label);return
  raise AssertionError('Undetected '+label)
 c=lambda d:d['Actual'][0]
 def alter_proof(d,label,old,new):
  values=c(d)['GuardProofs'];i=next(i for i,v in enumerate(values) if v.startswith(label+'|'));assert old in values[i];values[i]=values[i].replace(old,new,1)
 refuse('wrong-cancel-exception',lambda d:alter_proof(d,'prepare-canceled','OperationCanceledException','InvalidDataException'))
 refuse('missing-reached',lambda d:alter_proof(d,'entered-cancel-entry','|1|','|0|'))
 refuse('wrong-token',lambda d:alter_proof(d,'prepare-canceled','tokenMatch=1','tokenMatch=0'))
 refuse('default-token',lambda d:alter_proof(d,'run-preentry-canceled','actualCancelable=1','actualCancelable=0'))
 refuse('foreign-token',lambda d:alter_proof(d,'entered-cancel-entry','tokenMatch=1','tokenMatch=foreign'))
 refuse('actual-token-not-requested',lambda d:alter_proof(d,'entered-cancel-row:created','actualRequested=1','actualRequested=0'))
 refuse('expected-token-not-requested',lambda d:alter_proof(d,'entered-cancel-row:published','expectedRequested=1','expectedRequested=0'))
 refuse('expected-default-token',lambda d:alter_proof(d,'prepare-canceled','expectedCancelable=1','expectedCancelable=0'))
 refuse('missing-token-witness',lambda d:alter_proof(d,'prepare-canceled','|tokenMatch=1',''))
 refuse('extra-proof',lambda d:c(d)['GuardProofs'].append('unproved|PASS'))
 refuse('missing-prefix',lambda d:c(d)['GuardPrefixes'].pop())
 refuse('prefix-accepted',lambda d:c(d)['GuardPrefixes'][0].__setitem__('Complete',True))
 refuse('prefix-row-lost',lambda d:c(d)['GuardPrefixes'][2]['Rows'].clear())
 refuse('prefix-descriptor-lost',lambda d:c(d)['GuardPrefixes'][2]['Descriptors'].__setitem__(0,base64.b64encode(bytes(88)).decode()))
 refuse('prefix-reservation',lambda d:c(d)['GuardPrefixes'][0].__setitem__('ReservedRows',9999))
 refuse('category-was-not-mutated',lambda d:c(d)['CategoryMutation'].__setitem__('Mutated',c(d)['CategoryMutation']['Before']))
 refuse('category-was-not-restored',lambda d:c(d)['CategoryMutation'].__setitem__('Restored','0'*64))
 refuse('private-numeric-snapshot-changed',lambda d:c(d)['CategoryMutation'].__setitem__('PreparedAfterRows','0'*64))
 refuse('guard-NN-replay',lambda d:c(d).__setitem__('NetworkAfter',c(d)['NetworkAfter']+1))
report=dict(Status='PASS_B8_TYPED_LIFECYCLE_PREFIX_AND_ACTUAL_BACKING_MUTATION',Metrics=metrics,TamperRefusals=negative,PreparedOnly=False,NNEvaluated=False,SDKExecuted=False,TargetsRead=False,CaptureSHA256=hashlib.sha256(raw).hexdigest())
a.report.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
