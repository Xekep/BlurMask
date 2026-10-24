"""Prospective SAME fixed-input selected-bootstrap DATA check; never an issuer."""
import argparse,copy,hashlib,json
from pathlib import Path
PRIOR='a4af3c56706ba3f7701fd7d4d25fbf2e7473fd5995c970b7847f2ef6f94d3a3b'
RAW='d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99'
OLD_FRONTIER='Original-selected INPUT/C20 bootstrap,01FD40/020000 and document apply remain unissued.'
NEW_FRONTIER='Original-selected bootstrap retained;01FD40/020000 and document apply remain unissued.'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def strict(a,b):
 assert type(a)is type(b),(type(a),type(b))
 if isinstance(b,dict):
  assert a.keys()==b.keys()
  for k in b:strict(a[k],b[k])
 elif isinstance(b,list):
  assert len(a)==len(b)
  for x,y in zip(a,b,strict=True):strict(x,y)
 else:assert a==b,(a,b)
def setup(prior,raw):
 assert sha(prior)==PRIOR and sha(raw)==RAW
 p=json.loads(prior.read_bytes());r=json.loads(raw.read_bytes())[4]
 assert r['Raw']==[[[0,0],[7,40],[14,0]]]and type(r['Resolution'])is int and r['Resolution']==96
 assert p['Completed']is True and p['DocumentApplied']is False and p['PostText']['RemainingFrontier']==OLD_FRONTIER
 #Library bootstrap source protocol: fresh C51 has no keys; ONE field causes
 #ONE actual INPUT insert. Raw-object ordinals are a different namespace.
 #OriginalSelected clone copies that exact registry and Next, not P/N.
 fields=1;input_id=0;original_next=fields;selected_next=original_next
 pending=p['Text']['PendingCommands'];unique_chars=len(p['Word']['FinalCharacters'])
 assert len(pending)==unique_chars+2 and [c['Name']for c in pending]==['CHAR']*unique_chars+['WORD','TEXT']
 assert [c['SelectionId']for c in pending]==list(range(selected_next,selected_next+len(pending)))
 assert p['Text']['NextSelectionId']==selected_next+len(pending)
 #Exactly10 DOM nodes from the actual one-field source schedule; selected
 #new PAGE6 + cloned field/id/input/tag4. No node count is read from new DATA.
 early=dict(FieldId='english-field',OriginalObjects=len(r['Raw']),OriginalPoints=sum(map(len,r['Raw'])),OriginalInputId=input_id,
  OriginalNext=original_next,SelectedNextBeforeRegistration=selected_next,PendingNextAfterText=selected_next+len(pending),
  OriginalDomNodes=6+4*fields,SelectedDomNodes=6+4,SelectedRegistryCount=fields,DocumentApplied=False,
  Scope='EARLY_SAME_DOCUMENT_ORIGINAL_PAGE_INPUT_SELECTED_C20_BEFORE_CHAR_WORD_TEXT_NO_DOCUMENT_APPLY')
 return p,early
def audit(x,state):
 p,early=state
 assert type(x)is dict and x.keys()==p.keys()|{'EarlySelected'}
 for name,expected in p.items():
  if name!='PostText':strict(x[name],expected);continue
  #The only changed EXISTING field is the exact frontier literal. Every
  #other nested PostText scalar/array/mask/range is still compared verbatim.
  assert type(x[name])is dict and x[name].keys()==expected.keys()
  for field,value in expected.items():strict(x[name][field],NEW_FRONTIER if field=='RemainingFrontier'else value)
 strict(x['EarlySelected'],early)
def tampers(x,state):
 changes=[]
 for k,v in [('FieldId','other'),('OriginalObjects',True),('OriginalPoints',3.0),('OriginalInputId',1),('OriginalNext',0),('SelectedNextBeforeRegistration',2),('PendingNextAfterText',5),('OriginalDomNodes',9),('SelectedDomNodes',11),('SelectedRegistryCount',0),('DocumentApplied',True),('Scope','NATIVE_FRONTEND_SELECTED_PROVEN')]:
  changes.append((k,lambda q,k=k,v=v:q['EarlySelected'].update({k:v})))
 changes.extend([
  ('missing-bootstrap',lambda q:q.pop('EarlySelected')),
  ('extra-bootstrap-field',lambda q:q['EarlySelected'].update(Extra=False)),
  ('old-frontier-not-updated',lambda q:q['PostText'].update(RemainingFrontier=OLD_FRONTIER)),
  ('arbitrary-frontier-waiver',lambda q:q['PostText'].update(RemainingFrontier='applied')),
  ('old-posttext-key',lambda q:q['PostText'].update(WordUnionPairs=[])),
  ('old-posttext-count',lambda q:q['PostText'].update(CheckedLinks=0)),
  ('old-scorer',lambda q:q['Vector10'].pop()),
  ('old-source',lambda q:q['SourceCompletion']['Source'].update(Appended=0)),
  ('old-metrics',lambda q:q.update(Metrics={})),
  ('old-WORD-flags',lambda q:q['Word'].update(Flags=1)),
  ('old-WORD-ids',lambda q:q['Word'].update(SelectionId=3)),
  ('old-CHAR-mask',lambda q:q['Word']['FinalCharacters'][0].update(RecordKnown40=[])),
  ('old-pending-counter',lambda q:q['Text'].update(NextSelectionId=3)),
  ('old-pending-name',lambda q:q['Text']['PendingCommands'][0].update(Name='INPUT')),
  ('old-pending-id',lambda q:q['Text']['PendingCommands'][0].update(SelectionId=0)),
  ('old-TEXT-line',lambda q:q['Text'].update(SourceLines56='')),
  ('old-scope',lambda q:q.update(Scope='SELECTED')),
  ('claim-apply',lambda q:q.update(DocumentApplied=True)),
  ('old-canonical-shape',lambda q:q.pop('FirstChoiceCharacterPrefixes')),
  ('unexpected-root-field',lambda q:q.update(Extra=0))])
 names=[]
 for name,mut in changes:
  q=copy.deepcopy(x);mut(q)
  try:audit(q,state)
  except(AssertionError,KeyError,TypeError,ValueError):names.append(name)
  else:raise AssertionError('accepted '+name)
 return names
def main():
 p=argparse.ArgumentParser();p.add_argument('actual',type=Path);p.add_argument('prior_posttext',type=Path);p.add_argument('raw',type=Path);p.add_argument('report',type=Path);p.add_argument('--tamper',action='store_true');a=p.parse_args()
 assert not a.report.exists()and a.actual.stat().st_size<=32*1024*1024
 state=setup(a.prior_posttext,a.raw);x=json.loads(a.actual.read_bytes());audit(x,state)
 report=dict(Passed=True,ActualSha256=sha(a.actual),Scope='ONE_FIXED_INPUT_LIBRARY_SELECTED_BOOTSTRAP_DATA_ONLY',AllOldRecognitionSourceScorerMetricsFlagsExact=True,
  PermittedExistingChanges=['PostText.RemainingFrontier exact old-to-new literal only'],AdditiveFields=['EarlySelected'],NN=False,DocumentApplied=False,
  NotIndependentlyVerified=['Earlier native frontend-selectedC20 remains unproved; this library bootstrap occurs after recognition/before registration.',
   'Detached scalars do not prove actual private owner/thread/epoch/lifetime or mutation call chronology.',
   'Raw/DOM table topology is source + model-free helper evidence; owning export only exposes cardinality/IDs, not all DOM bytes.',
   'No new source-guide FIRST producer, complete CLR/CPU/RSS or owning ledger-total proof.',
   '01FD40/020000 and later application/quality/public admission remain absent.'])
 if a.tamper:report['TamperRefused']=tampers(x,state)
 a.report.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
if __name__=='__main__':main()
