"""Prospective strict DATA-only01FD40/020000 replay. Never an owner/issuer."""
import argparse,base64,copy,hashlib,json,struct
from pathlib import Path
PRIOR='beaef516f78e14ff9f01a0fdb7d5a9bb8746766b24858d7f6836218e6696cb91';RAW='d1c9284dbe4f79301f2a0d860e14b3c00e5a0ba02430b98a6bac5f0c43fa6b99'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def b(x):return base64.b64encode(bytes(x)).decode()
def strict(a,e):
 assert type(a)is type(e),(type(a),type(e))
 if isinstance(e,dict):
  assert a.keys()==e.keys()
  for k in e:strict(a[k],e[k])
 elif isinstance(e,list):
  assert len(a)==len(e)
  for x,y in zip(a,e,strict=True):strict(x,y)
 else:assert a==e,(a,e)
def setup(prior,raw):
 assert sha(prior)==PRIOR and sha(raw)==RAW
 p=json.loads(prior.read_bytes());r=json.loads(raw.read_bytes())[4];assert r['Raw']==[[[0,0],[7,40],[14,0]]]and type(r['Resolution'])is int and r['Resolution']==96
 assert p['Completed']is True and p['DocumentApplied']is False
 strict(p['Word']['FinalCharacters'],p['LastChoiceCharacters']['FinalNodes'])
 tree=p['LastChoiceCharacters']['CharacterTree'];root=p['LastChoiceCharacters']['CharacterTreeRoot'];nodes={n['Identity']:n for n in tree};payload={c['SourceNode']:c for c in p['Word']['FinalCharacters']}
 assert len(nodes)==len(tree)==len(payload)>0 and nodes.keys()==payload.keys()
 order=[];entered=set()
 def walk(i,depth):
  if not i:return
  assert depth<64 and i in nodes and i not in entered;entered.add(i);walk(nodes[i]['Left'],depth+1);order.append(i);walk(nodes[i]['Right'],depth+1)
 walk(root,0);assert len(order)==len(nodes)
 states={}
 for i in order:
  c=payload[i];flags=c['Flags'];assert type(flags)is int and c['Written']&2 and c['Written']&64
  keys=c['SourcePairs'];assert len(keys)%2==0
  lookup=False;concrete=reported=None;meta=flags
  if not flags&0x10600000:
   if flags&0x20000000:assert c['SelectionId']==-1,'reached unsupported prior'
   elif len(keys)==2:
    a,z=keys
    if a>>32==z>>32 and (a>>16)&65535==0 and (a>>8)&255==0 and (z>>16)&65535==3 and (z>>8)&255==255:
     original=a>>32;assert original<len(r['Raw']);lookup=True;concrete=0xc49;reported=0xc1e;meta=flags&0xfeffffff
  before=struct.pack('<q',c['AssignedDefaults']['Field40']);states[i]=dict(SourceNode=i,MetadataFlags=meta,Flags=meta,FlagsKnown4=b(b'\xff'*4),Field40Before8=b(before),KnownBefore8=b(bytes(8)),Field40After8=b(before),KnownAfter8=b(bytes(8)),RawKindLookedUp=lookup,ConcreteKind=concrete,ReportedKind=reported)
 word=p['Word'];selected=word['SelectedOrdinal'];choice=word['Choices'][selected];assert choice['Ordinal']==selected
 links=choice['Links'];allbits=all(states[l['SourceNode']]['MetadataFlags']&0x11004000 for l in links)
 wm=(word['Flags']&0xfeffffff)|(0x01000000 if allbits else 0);writes=[]
 for l in links:
  i=l['SourceNode'];marker=l['Marker'];assert type(i)is int and type(marker)is int and 0<=marker<len(payload[i]['Records40'])
  state=states[i];state['Flags']|=0x08000000;before=base64.b64decode(state['Field40After8']);state['Field40After8']=b(before[:4]+struct.pack('<i',marker));state['KnownAfter8']=b(bytes(4)+b'\xff'*4);writes.append(dict(SourceNode=i,Marker=marker))
 expected=dict(CharacterRoot=root,Tree=tree,Characters=[states[i]for i in order],Writes=writes,WordMetadataFlags=wm,WordFlags=wm|0x08000000,WordFlagsKnown4=b(b'\xff'*4),WordUnknownField40Template8=b(b'\xff'*8),WordKnownBefore8=b(bytes(8)),WordField40After8=b(b'\xff'*4+struct.pack('<i',selected)),WordKnownAfter8=b(bytes(4)+b'\xff'*4),PendingNext=p['Text']['NextSelectionId'],PendingCount=len(p['Text']['PendingCommands']),DocumentApplied=False,Scope='SAME_EN_EARLY_SELECTED_LIVE_01FD40_020000_NO_020920_NO_DOCUMENT_APPLY')
 return p,expected

def audit(x,state):
 p,e=state;assert x.keys()==p.keys()|{'SelectedSources'}
 for k in p:strict(x[k],p[k])
 strict(x['SelectedSources'],e)
def tampers(x,state):
 changes=[('missing-character',lambda q:q['SelectedSources']['Characters'].pop()),('missing-write',lambda q:q['SelectedSources']['Writes'].pop()),('char-flag',lambda q:q['SelectedSources']['Characters'][0].update(Flags=0)),('word-flag',lambda q:q['SelectedSources'].update(WordFlags=0)),('word-metadata',lambda q:q['SelectedSources'].update(WordMetadataFlags=0x01000000)),('char-promote-lower',lambda q:q['SelectedSources']['Characters'][0].update(KnownAfter8=b(b'\xff'*8))),('word-promote-lower',lambda q:q['SelectedSources'].update(WordKnownAfter8=b(b'\xff'*8))),('word-template-authority',lambda q:q['SelectedSources'].update(WordKnownBefore8=b(b'\xff'*8))),('change-lower-data',lambda q:q['SelectedSources']['Characters'][0].update(Field40After8=b(bytes(8)))),('wrong-marker',lambda q:q['SelectedSources']['Writes'][0].update(Marker=1)),('fake-c25',lambda q:q['SelectedSources']['Characters'][0].update(ReportedKind=0xc25)),('lookup-claim',lambda q:q['SelectedSources']['Characters'][0].update(RawKindLookedUp=True)),('tree-root',lambda q:q['SelectedSources'].update(CharacterRoot=0)),('next',lambda q:q['SelectedSources'].update(PendingNext=0)),('count-bool',lambda q:q['SelectedSources'].update(PendingCount=True)),('scope',lambda q:q['SelectedSources'].update(Scope='TEXT_ACCEPTED')),('apply',lambda q:q['SelectedSources'].update(DocumentApplied=True)),('old-metrics',lambda q:q.update(Metrics={})),('old-flags',lambda q:q['Word'].update(Flags=1)),('old-bootstrap',lambda q:q['EarlySelected'].update(OriginalInputId=1)),('missing-old',lambda q:q.pop('Vector10'))]
 names=[]
 for n,f in changes:
  q=copy.deepcopy(x);f(q)
  try:audit(q,state)
  except AssertionError:names.append(n)
  else:raise AssertionError('accepted '+n)
 return names

def main():
 a=argparse.ArgumentParser();a.add_argument('actual',type=Path);a.add_argument('prior',type=Path);a.add_argument('raw',type=Path);a.add_argument('report',type=Path);a.add_argument('--tamper',action='store_true');a=a.parse_args();assert not a.report.exists();assert a.actual.stat().st_size<=32*1024*1024
 state=setup(a.prior,a.raw);x=json.loads(a.actual.read_bytes());audit(x,state)
 report=dict(Passed=True,Scope='ONE_FIXED_PRIOR_SELECTED_SOURCE_DATA_ONLY',ActualSha256=sha(a.actual),EntirePriorExact=True,Characters=len(state[1]['Characters']),Writes=len(state[1]['Writes']),ActualRawKindLookups=sum(c['RawKindLookedUp']for c in state[1]['Characters']),NN=False,DocumentApplied=False,NotIndependentlyVerified=['Private owner/thread/epoch/lifetime and call chronology are source evidence, not inferred from DATA.','Unknown WORD template has no live before-image authority; lower Field40 mask remains unknown.','No native frontend-selectedC20,020920/DOM/apply/quality/global readiness acceptance.','No owning CLR/RSS/CPU or complete transitive meter proof.'])
 if a.tamper:report['TamperRefused']=tampers(x,state)
 a.report.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
if __name__=='__main__':main()
