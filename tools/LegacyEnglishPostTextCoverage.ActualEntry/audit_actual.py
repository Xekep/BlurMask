"""Prospective detached DATA replay, not a live owner or document-apply oracle."""
import argparse,ast,copy,hashlib,json
from pathlib import Path
HERE=Path(__file__).resolve().parent
BASELINE='1bb645b6c029541fc9ce345d74718b836f404694ee9bf7ab646312a4bfc577d9'
RAW='d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def strict(a,b):
 assert type(a)is type(b),(type(a),type(b))
 if isinstance(b,dict):
  assert a.keys()==b.keys(),(a.keys(),b.keys())
  for k in b:strict(a[k],b[k])
 elif isinstance(b,list):
  assert len(a)==len(b)
  for x,y in zip(a,b,strict=True):strict(x,y)
 else:assert a==b,(a,b)
def setup(prior,raw):
 assert sha(prior)==BASELINE and prior.stat().st_size==28374 and sha(raw)==RAW
 p=json.loads(prior.read_bytes());r=json.loads(raw.read_bytes())[4]
 assert r['Raw']==[[[0,0],[7,40],[14,0]]]and type(r['Resolution'])is int and r['Resolution']==96
 assert p['Completed']is True and p['TEXTCreated']is True and p['DocumentApplied']is False
 f=HERE/'reference_ranges.py';assert sha(f)=='dc5e998a42e4dcedbfda714d97dd58d06136af608c5f4aa5bc03bf70b89fb750'
 tree=ast.parse(f.read_text(encoding='utf-8-sig'));g={'MASK':2**64-1}
 exec(compile(ast.Module([n for n in tree.body if isinstance(n,ast.FunctionDef)],[]),str(f),'exec'),g)
 return p,[len(s)for s in r['Raw']],g
def pairs(flat):
 assert type(flat)is list and len(flat)%2==0
 assert all(type(x)is int and 0<=x<2**64 for x in flat)
 return [flat[i:i+2]for i in range(0,len(flat),2)]
def flatten(rows):return [n for row in rows for n in row]
def expected(p,counts,g):
 word=p['Word'];choices=word['Choices'];assert 1<=len(choices)<=4095
 assert type(word['SelectedOrdinal'])is int and 0<=word['SelectedOrdinal']<len(choices)
 #Original source and early NULL TEXT key are separate authenticated baseline
 #inputs. No producer output from the new PostText channel is used.
 union=g['union']([],pairs(word['OriginalPairs']))
 assert union==pairs(p['Text']['NullSelectedPairs'])
 all_characters={n['SourceNode']:n for n in word['FinalCharacters']}
 assert len(all_characters)==len(word['FinalCharacters'])
 out=[None]*len(choices);links=0
 for ordinal in range(len(choices)-1,-1,-1):
  c=choices[ordinal];assert c['Ordinal']==ordinal and len(c['Links'])>0
  start=pairs(c['OrdinaryPairs'])if c['OrdinaryPairs']else pairs(word['OriginalPairs'])
  remaining=copy.deepcopy(start);nodes=[]
  for link in c['Links']:
   n=all_characters[link['SourceNode']]
   assert n['LineId']==0 and n['AssignedDefaults']['Field40']==-1
   #Prior-vector producer authority is source-reviewed; scalar Prior is not
   #used as a surrogate for vector emptiness. Retained key is baseline INPUT.
   remaining=g['subtract'](remaining,pairs(n['SourcePairs']),counts)
   nodes.append(link['SourceNode']);links+=1
  assert not remaining or len(choices)==1,'Native024A90 unsupported'
  out[ordinal]=dict(Ordinal=ordinal,CharacterNodes=nodes,InitialPairs=flatten(start),RemainingPairs=flatten(remaining))
 return dict(SelectedOrdinal=word['SelectedOrdinal'],LineCount=1,CheckedChoices=len(choices),CheckedLinks=links,
  WordUnionPairs=flatten(union),Choices=out,NextSelectionId=p['Text']['NextSelectionId'],PendingCount=len(p['Text']['PendingCommands']),
  DocumentApplied=False,Scope='SAME_EN_LIVE_POST_TEXT_025E40_027670_0248D0_01C740_ORDINARY_LINE0',
  RemainingFrontier='Original-selected INPUT/C20 bootstrap,01FD40/020000 and document apply remain unissued.')
def audit(x,state):
 p,counts,g=state
 assert type(x)is dict and x.keys()==p.keys()|{'PostText'}
 #Exactly ONE additive field; all historical fields retain exact recursive
 #types, shapes and scalar values. No changed existing field is excluded.
 strict({k:v for k,v in x.items()if k!='PostText'},p)
 strict(x['PostText'],expected(p,counts,g))
def tampers(x,state):
 changes=[]
 def add(name,f):changes.append((name,f))
 for k,v in [('SelectedOrdinal',True),('LineCount',1.0),('CheckedChoices',0),('CheckedLinks',1),('WordUnionPairs',[]),('NextSelectionId',5),('PendingCount',2),('DocumentApplied',True),('Scope','APPLIED'),('RemainingFrontier','')]:
  add(k,lambda q,k=k,v=v:q['PostText'].update({k:v}))
 add('extra-new-field',lambda q:q['PostText'].update(Extra=0))
 add('missing-new-field',lambda q:q['PostText'].pop('Scope'))
 add('missing-choice',lambda q:q['PostText']['Choices'].pop())
 add('duplicate-choice',lambda q:q['PostText']['Choices'].append(copy.deepcopy(q['PostText']['Choices'][0])))
 add('reversed-choices',lambda q:q['PostText']['Choices'].reverse())
 for k,v in [('Ordinal',True),('CharacterNodes',[2]),('InitialPairs',[]),('RemainingPairs',[3,196357])]:
  add('choice-'+k,lambda q,k=k,v=v:q['PostText']['Choices'][0].update({k:v}))
 add('old-word-flag',lambda q:q['Word'].update(Flags=1))
 add('old-command-identity',lambda q:q['Text']['PendingCommands'][2].update(CommandIdentity=4))
 add('old-known-mask',lambda q:q['Text'].update(WordLinkKnown24=''))
 add('old-NN-vector',lambda q:q['Vector10'].pop())
 add('old-metrics',lambda q:q.update(Metrics={}))
 add('old-scope',lambda q:q.update(Scope='POSTTEXT'))
 add('old-DocumentApplied',lambda q:q.update(DocumentApplied=True))
 add('old-field-omitted',lambda q:q.pop('FirstChoiceCharacterPrefixes'))
 add('extra-root-field',lambda q:q.update(Unexpected=False))
 names=[]
 for name,mut in changes:
  q=copy.deepcopy(x);mut(q)
  try:audit(q,state)
  except(AssertionError,KeyError,TypeError,ValueError):names.append(name)
  else:raise AssertionError('accepted '+name)
 return names
def main():
 p=argparse.ArgumentParser();p.add_argument('actual',type=Path);p.add_argument('prior_text',type=Path);p.add_argument('raw',type=Path);p.add_argument('report',type=Path);p.add_argument('--tamper',action='store_true');a=p.parse_args()
 assert not a.report.exists()and a.actual.stat().st_size<=32*1024*1024
 state=setup(a.prior_text,a.raw);x=json.loads(a.actual.read_bytes());audit(x,state)
 report=dict(Passed=True,ActualSha256=sha(a.actual),Scope='ONE_FIXED_INPUT_POSTTEXT_DATA_ONLY',WholePriorCanonicalExact=True,
  Choices=x['PostText']['CheckedChoices'],Links=x['PostText']['CheckedLinks'],DocumentApplied=False,NN=False,
  NotIndependentlyVerified=['Live owner/thread/epoch and prior-vector emptiness are source premises, not derived from detached DATA.',
   'Equal line0 branch and fresh22A=false are source/native premises; no new whole machine-call proof.',
   'Guide FIRST producer, owning lifecycle/cancel/poison, actual ledger totals and document apply are not certified.',
   'No quality/admission/public transcript claim.'])
 if a.tamper:report['TamperRefused']=tampers(x,state)
 a.report.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print(json.dumps(report))
if __name__=='__main__':main()
