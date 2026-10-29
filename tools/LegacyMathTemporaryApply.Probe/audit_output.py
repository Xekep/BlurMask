from pathlib import Path
import json,sys
c=json.loads(Path(sys.argv[1]).read_bytes());assert c['Controls']==203;assert len(c['Actual'])==6 and len(c['Faults'])==8 and len(c['Closures'])==9
assert [(x['Width'],x['Reversed'])for x in c['Actual']]==[(w,r)for w in [1,3,16]for r in [False,True]]
for case in c['Actual']:
 a=case['Application'];d=case['Diff'];p=case['Candidate']
 assert a['Complete']and a['Ready']and not(a['Failed']or a['Released']or a['PartialActionMayExist'])
 assert a['HeaderConsumed']and a['CompletedActions']==a['ObservedAfterActions']==0
 assert d['Produced']==d['Filtered']==3 and d['Kept']==0 and not d['HeaderLive']
 assert a['ObservedTemporary']==p['Temporary']
for f in c['Faults']:
 a=f['Application'];consumed=f['Stage']in ['HeaderConsumed','Completed']
 assert a['Failed']and not a['Ready']and a['HeaderConsumed']==consumed and f['DiffHeaderLive']!=consumed
 assert a['CompletedActions']==0 and not a['PartialActionMayExist']
assert sorted((f['Stage'],f['Cancel'])for f in c['Faults'])==sorted((s,b)for s in ['Claimed','BeforeTraversal','HeaderConsumed','Completed']for b in [False,True])
assert sorted(f['Closed']for f in c['Closures'])==sorted(['scope','freeze','current','outgoing','next','candidate','diff','original','self'])
for f in c['Closures']:assert f['Application']['Failed']and not f['Application']['Ready']and f['Application']['HeaderConsumed']
print('PASS independent actual empty traversal/header and revoked-owner boundaries; no kept-action proof')
